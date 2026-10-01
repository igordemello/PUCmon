using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

/// <summary>
/// The few Supabase HTTP calls the game needs (Auth + REST), over UnityWebRequest so they also work in WebGL builds.
/// </summary>
public static class Supabase
{
    // Supabase dashboard > Project Settings > API. The publishable key is made to ship inside the app: what each
    // player may read or write is decided by the row level security policies in Supabase/schema.sql.
    public const string Url = "https://vlyoxfpqrpnhytuxkibu.supabase.co";
    public const string Key = "sb_publishable_2fB10ysS92BeHPG4Onveuw_dOB0odUy";

    public struct Response
    {
        public long status;  // 0 = no connection
        public string text;
        public bool Ok => status >= 200 && status < 300;
    }

    public static Task<Response> Send(string method, string path, string json = null, string token = null, string prefer = null)
    {
        var request = new UnityWebRequest(Url + path, method) { downloadHandler = new DownloadHandlerBuffer() };
        if (json != null)
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.SetRequestHeader("Content-Type", "application/json");
        }
        request.SetRequestHeader("apikey", Key);
        if (token != null)
            request.SetRequestHeader("Authorization", "Bearer " + token);
        if (prefer != null)
            request.SetRequestHeader("Prefer", prefer);

        var done = new TaskCompletionSource<Response>();
        request.SendWebRequest().completed += _ =>
        {
            done.SetResult(new Response { status = request.responseCode, text = request.downloadHandler.text });
            request.Dispose();
        };
        return done.Task;
    }
}
