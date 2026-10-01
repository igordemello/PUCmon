-- PUCmon: rode uma vez no Supabase (SQL Editor > New query > Run).
-- Contas e senhas ficam no Supabase Auth; aqui só a coleção de cada jogador.

create table public.capturas (
  user_id uuid not null default auth.uid() references auth.users on delete cascade,
  criatura text not null,  -- nome da imagem do cartaz na Unity, ex.: 'image1'
  capturada_em timestamptz not null default now(),
  primary key (user_id, criatura)
);

-- Cada jogador só vê e só grava as próprias capturas.
alter table public.capturas enable row level security;
create policy "ver as próprias capturas" on public.capturas
  for select to authenticated using (user_id = (select auth.uid()));
create policy "capturar para si" on public.capturas
  for insert to authenticated with check (user_id = (select auth.uid()));
grant select, insert on public.capturas to authenticated;
