create sequence ips_defects_seq start with 1 increment by 1;

create or replace trigger ips_defects_bir_trg
before insert on ips_defects
for each row
begin
  select ips_defects_seq.nextval
  ,      sysdate
  into :new.id
  ,    :new.defect_date
  from dual;
end;
/
