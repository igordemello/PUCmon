using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The player's account: sign up / sign in with Supabase Auth, a session that survives reloads, and the collection
/// (table "capturas"). Methods that can fail return a message for the player, or null when all went well. If the
/// session is lost for good it signs out and opens the "Login" scene, which shows <see cref="Notice"/>.
/// Players only have a user name: Supabase Auth wants an e-mail, so the name becomes "name@pucmon.invalid" (a domain
/// that can never receive mail; fine while "Confirm email" stays off, since no e-mail is ever sent).
/// </summary>
public static class Account
{
    [Serializable] class Session { public string access_token, refresh_token; public long expires_at; public User user; }
    [Serializable] class User { public string id, email; public Profile user_metadata; }
    [Serializable] class Profile { public string usuario; }
    [Serializable] class SignUpRequest { public string email, password; public Profile data; }
    [Serializable] class SignInRequest { public string email, password; }
    [Serializable] class RefreshRequest { public string refresh_token; }
    [Serializable] class Error { public string error_code; }
    [Serializable] class Row { public string criatura; }
    [Serializable] class Rows { public Row[] items; }

    const string SavedSession = "pucmon.refresh_token", EmailDomain = "@pucmon.invalid";
    static Session session;

    public static bool SignedIn => session != null;
    public static bool HasSavedSession => PlayerPrefs.HasKey(SavedSession);
    public static string Username => session?.user?.user_metadata?.usuario ?? "";
    /// <summary>A message for the login screen (why the player was sent back to it), or null.</summary>
    public static string Notice;
    /// <summary>Names of the captured posters (their image asset names).</summary>
    public static readonly HashSet<string> Collection = new HashSet<string>();

    /// <summary>User names are case-insensitive: "Igor" and "igor" are the same player.</summary>
    public static string Normalize(string username) => username.Trim().ToLowerInvariant();

    /// <summary>What is wrong with a new user name, or null.</summary>
    public static string CheckUsername(string username) =>
        username.Length < 3 ? "O usuário precisa de pelo menos 3 caracteres."
        : !Regex.IsMatch(username, "^[a-z0-9._-]+$") ? "Use só letras, números, ponto, hífen ou _ (sem espaços nem acentos)."
        : null;

    public static Task<string> SignIn(string username, string password) =>
        Open("/auth/v1/token?grant_type=password",
            JsonUtility.ToJson(new SignInRequest { email = Normalize(username) + EmailDomain, password = password }));

    public static async Task<string> SignUp(string username, string password)
    {
        username = Normalize(username);
        var json = JsonUtility.ToJson(new SignUpRequest { email = username + EmailDomain, password = password, data = new Profile { usuario = username } });
        var response = await Supabase.Send("POST", "/auth/v1/signup", json);
        if (!response.Ok)
            return Message(response);
        if (string.IsNullOrEmpty(JsonUtility.FromJson<Session>(response.text).access_token))
            return "Desligue \"Confirm email\" no Supabase (Authentication > Sign In / Providers > Email).";
        return await Keep(response);
    }

    /// <summary>Signs back in with the session saved on this device.</summary>
    public static async Task<string> Resume()
    {
        var response = await RefreshWith(PlayerPrefs.GetString(SavedSession));
        if (response.Ok)
            return await Keep(response);
        if (response.status != 0)
            SignOut();  // expired or revoked: ask for the password again
        return Message(response);
    }

    /// <summary>Ends the session, on this device and in Supabase.</summary>
    public static void SignOut()
    {
        if (session != null)
            _ = Supabase.Send("POST", "/auth/v1/logout?scope=local", token: session.access_token);
        session = null;
        Collection.Clear();
        PlayerPrefs.DeleteKey(SavedSession);
        PlayerPrefs.Save();
    }

    /// <summary>Saves a creature (its poster's image name) into the player's collection.</summary>
    public static async Task<string> Capture(string creature)
    {
        if (Collection.Contains(creature))
            return null;
        var response = await Authorized("POST", "/rest/v1/capturas", JsonUtility.ToJson(new Row { criatura = creature }),
            "resolution=ignore-duplicates,return=minimal");
        if (!response.Ok)
            return response.status == 0 ? Message(response) : "Não deu para salvar a captura. Tente de novo.";
        Collection.Add(creature);
        return null;
    }

    /// <summary>Reloads the collection from Supabase (it may have changed on another device).</summary>
    public static async Task<string> LoadCollection()
    {
        var response = await Authorized("GET", "/rest/v1/capturas?select=criatura");
        if (!response.Ok)
            return Message(response);
        Collection.Clear();
        foreach (var row in JsonUtility.FromJson<Rows>("{\"items\":" + response.text + "}").items)
            Collection.Add(row.criatura);
        return null;
    }

    static async Task<string> Open(string path, string json)
    {
        var response = await Supabase.Send("POST", path, json);
        return response.Ok ? await Keep(response) : Message(response);
    }

    // A new session: save it on this device and load the player's collection.
    static async Task<string> Keep(Supabase.Response response)
    {
        Save(response);
        await LoadCollection();
        return null;
    }

    static void Save(Supabase.Response response)
    {
        session = JsonUtility.FromJson<Session>(response.text);
        PlayerPrefs.SetString(SavedSession, session.refresh_token);
        PlayerPrefs.Save();
    }

    // A Supabase call as the player: renews the session first when it is about to expire, and once more if Supabase
    // still says it expired.
    static async Task<Supabase.Response> Authorized(string method, string path, string json = null, string prefer = null)
    {
        if (session != null && DateTimeOffset.UtcNow.ToUnixTimeSeconds() > session.expires_at - 60)
            await Refresh();
        var response = await Supabase.Send(method, path, json, session?.access_token, prefer);
        if (response.status == 401 && await Refresh())
            response = await Supabase.Send(method, path, json, session?.access_token, prefer);
        return response;
    }

    // Each refresh token works once, so calls that need a new session at the same time share one renewal.
    static Task<bool> refreshing;
    static Task<bool> Refresh() => session == null ? Task.FromResult(false) : refreshing ??= RefreshOnce();

    static async Task<bool> RefreshOnce()
    {
        try
        {
            var response = await RefreshWith(session.refresh_token);
            if (response.Ok)
            {
                Save(response);
                return true;
            }
            if (response.status != 0)
            {
                // Revoked or expired for good (signed out elsewhere, password changed): back to the login screen.
                SignOut();
                Notice = "Sua sessão expirou. Entre de novo.";
                SceneManager.LoadScene("Login");
            }
            return false;
        }
        finally
        {
            refreshing = null;
        }
    }

    static Task<Supabase.Response> RefreshWith(string refreshToken) =>
        Supabase.Send("POST", "/auth/v1/token?grant_type=refresh_token", JsonUtility.ToJson(new RefreshRequest { refresh_token = refreshToken }));

    static string Message(Supabase.Response response)
    {
        if (response.status == 0)
            return "Sem conexão. Confira a internet e tente de novo.";
        string code = null;
        try { code = JsonUtility.FromJson<Error>(response.text)?.error_code; } catch (ArgumentException) { }
        switch (code)
        {
            case "invalid_credentials": return "Usuário ou senha incorretos.";
            case "user_already_exists":
            case "email_exists": return "Esse nome de usuário já existe. Escolha outro.";
            case "weak_password": return "Senha fraca. Use pelo menos 6 caracteres.";
            case "email_address_invalid":
            case "validation_failed": return "Nome de usuário inválido.";
            case "email_not_confirmed": return "Desligue \"Confirm email\" no Supabase (Authentication > Sign In / Providers > Email).";
            case "over_request_rate_limit":
            case "over_email_send_rate_limit": return "Muitas tentativas seguidas. Espere um pouco e tente de novo.";
            case "refresh_token_not_found":
            case "refresh_token_already_used":
            case "session_not_found":
            case "session_expired": return "Sua sessão expirou. Entre de novo.";
        }
        return $"Algo deu errado ({response.status}). Tente de novo.";
    }
}
