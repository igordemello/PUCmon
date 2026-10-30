-- PUCmon, Coleção (tamagochi): rode uma vez no Supabase (SQL Editor > New query > Run), depois do schema.sql.
-- Cada PUCmon capturado ganha afinidade (XP de amizade), fome e humor. O jogo nunca grava esses números direto:
-- só pela função cuidar(), que decide quanto cada cuidado rende.
-- Os números de agora ficam no 4-economia.sql (gerado pelo asset Economia, no Unity), que substitui esta cuidar().

alter table public.capturas
  add column afinidade int not null default 0,
  add column fome real not null default 60,     -- 100 = satisfeito; um recém-capturado chega com um pouco de fome
  add column humor real not null default 20,    -- 100 = feliz; e bem bravo com quem o apedrejou
  add column atualizado_em timestamptz not null default now(),  -- fome e humor valem neste instante e caem com o tempo
  add column carinho_em timestamptz;            -- último carinho que rendeu afinidade

-- O jogo só escolhe qual PUCmon capturou; o resto começa nos valores padrão acima.
revoke insert on public.capturas from authenticated;
grant insert (criatura) on public.capturas to authenticated;

-- Um cuidado com o PUCmon p_criatura: 'alimentar', 'brincar' (p_desempenho de 0 a 1 no minijogo) ou 'carinho'.
-- Fome e humor caem de 100 a 0 em 24 h. A afinidade só sobe quando o cuidado era preciso (alimentar quem está com
-- fome, brincar com quem está desanimado, um carinho a cada 10 minutos), então tocar sem parar não adianta.
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
  c.fome := greatest(0, c.fome - horas * 100 / 24);
  c.humor := greatest(0, c.humor - horas * 100 / 24);
  p_desempenho := least(1, greatest(0, coalesce(p_desempenho, 0)));

  if p_tipo = 'alimentar' then
    if c.fome < 70 then ganho := 15; end if;
    c.fome := least(100, c.fome + 40);
  elsif p_tipo = 'brincar' then
    if c.humor < 80 then ganho := round(10 + 15 * p_desempenho); end if;
    c.humor := least(100, c.humor + 20 + 20 * p_desempenho);
  elsif p_tipo = 'carinho' then
    if c.carinho_em is null or now() - c.carinho_em >= interval '10 minutes' then
      ganho := 5;
      c.carinho_em := now();
    end if;
    c.humor := least(100, c.humor + 5);
  else
    raise exception 'Cuidado desconhecido: %', p_tipo;
  end if;

  update capturas
     set afinidade = c.afinidade + ganho, fome = c.fome, humor = c.humor, atualizado_em = now(), carinho_em = c.carinho_em
   where user_id = c.user_id and criatura = c.criatura;
  return json_build_object('criatura', c.criatura, 'afinidade', c.afinidade + ganho, 'fome', c.fome, 'humor', c.humor,
                           'atualizado_em', now(), 'carinho_em', c.carinho_em, 'ganho', ganho);
end $$;

revoke execute on function public.cuidar(text, text, real) from public, anon;
grant execute on function public.cuidar(text, text, real) to authenticated;
