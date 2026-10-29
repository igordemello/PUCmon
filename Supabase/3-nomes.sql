-- PUCmon: rode uma vez no Supabase (SQL Editor > New query > Run), junto com a publicação do build em que os cartazes
-- passaram a ter o nome do PUCmon (antes "1".."6"). O nome do cartaz é o id do PUCmon em "capturas", então os
-- registros já feitos mudam junto. Rodar de novo não faz nada. PUCmons novos já nascem com o nome do cartaz e não
-- precisam de nada aqui.

update public.capturas
   set criatura = case criatura
     when '1' then 'Habeas Corvus'
     when '2' then 'Compatudo'
     when '3' then 'Psoneco'
     when '4' then 'Guaxineasta'
     when '5' then 'Ventintoso'
     when '6' then 'Santídeo'
   end
 where criatura in ('1', '2', '3', '4', '5', '6');
