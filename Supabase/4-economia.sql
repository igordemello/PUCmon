-- PUCmon, Economia: os números do cuidado no Laboratório, para o servidor. GERADO PELO UNITY, não edite aqui:
-- mude no asset Assets/Jogo/Resources/Economia, clique em "Atualizar o servidor" e rode este arquivo no Supabase
-- (SQL Editor > New query > Run), depois do 2-colecao.sql. Pode rodar quantas vezes precisar.

-- Com quanto de fome e humor um PUCmon chega ao laboratório.
alter table public.capturas
  alter column fome set default 60,
  alter column humor set default 20;

-- Um cuidado com o PUCmon p_criatura: 'alimentar', 'brincar' (p_desempenho de 0 a 1 no minijogo) ou 'carinho'.
-- A fome zera em 4 h e o humor em 4 h. A afinidade só sobe quando o cuidado era preciso
-- (alimentar quem está com fome, brincar com quem está desanimado, um carinho a cada 10 min).
create or replace function public.cuidar(p_criatura text, p_tipo text, p_desempenho real default 1)
returns json language plpgsql security definer set search_path = public as $$
declare
  c capturas;
  horas real;
  ganho int := 0;
begin
  select * into c from capturas where user_id = auth.uid() and criatura = p_criatura for update;
  if not found then
    raise exception 'PUCmon não capturado: %', p_criatura;
  end if;
  horas := extract(epoch from now() - c.atualizado_em) / 3600;
  c.fome := greatest(0, c.fome - horas * 100 / 4);
  c.humor := greatest(0, c.humor - horas * 100 / 4);
  p_desempenho := least(1, greatest(0, coalesce(p_desempenho, 0)));

  if p_tipo = 'alimentar' then
    if c.fome < 80 then ganho := 30; end if;
    c.fome := least(100, c.fome + 30);
  elsif p_tipo = 'brincar' then
    if c.humor < 80 then ganho := round(20 + 20 * p_desempenho); end if;
    c.humor := least(100, c.humor + 20 + 20 * p_desempenho);
  elsif p_tipo = 'carinho' then
    if c.carinho_em is null or now() - c.carinho_em >= 10 * interval '1 minute' then
      ganho := 5;
      c.carinho_em := now();
    end if;
    c.humor := least(100, c.humor + 2);
  else
    raise exception 'Cuidado desconhecido: %', p_tipo;
  end if;

  update capturas
     set afinidade = c.afinidade + ganho, fome = c.fome, humor = c.humor, atualizado_em = now(), carinho_em = c.carinho_em
   where user_id = c.user_id and criatura = c.criatura;
  return json_build_object('criatura', c.criatura, 'afinidade', c.afinidade + ganho, 'fome', c.fome, 'humor', c.humor,
                           'atualizado_em', now(), 'carinho_em', c.carinho_em, 'ganho', ganho);
end $$;
