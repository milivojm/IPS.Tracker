create table ips_release (
  id number(10,0) not null,
  release_version varchar2(3000 char) not null,
  release_date date,
  constraint rel_pk primary key (id)
  )

create sequence ips_release_seq start with 1 increment by 1;

  create or replace trigger ips_release_trig
    before insert on ips_release
    for each row
    begin
        select ips_release_seq.nextval
        into :new.ID
        from dual;
    end;

alter table ips_defects add release_id int;
alter table ips_defects add constraint fk_release_id foreign key (release_id) references ips_release(id);
