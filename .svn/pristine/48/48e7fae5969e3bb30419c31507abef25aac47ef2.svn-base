create sequence ips_defect_comments_seq start with 1 increment by 1;

drop trigger ips_defect_comments_bir_trg;

create or replace trigger ips_defect_comments_bir_trg
before insert on ips_defect_comments
for each row
begin
  select ips_defect_comments_seq.nextval
  ,     sysdate
  into :new.id
  ,    :new.comment_date
  from dual;
end;
/
