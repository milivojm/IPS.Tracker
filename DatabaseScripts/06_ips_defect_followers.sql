create table ips_defect_followers (
  id number(10,0) not null
, constraint dfl_pk primary key (id)
, defect_id number(10,0) not null
, constraint dfl_def_fk foreign key (defect_id) references ips_defects (id)
, follower_id number(10,0) not null
, constraint dfl_wor foreign key (follower_id) references ips_workers (id)
);

create sequence ips_defect_followers_seq start with 1 increment by 1;

create or replace trigger ips_defect_followers_bir_trg
before insert on ips_defect_followers
for each row
begin
  select ips_defect_followers_seq.nextval
  into :new.id
  from dual;
end;
/