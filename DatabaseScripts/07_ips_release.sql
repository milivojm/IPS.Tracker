create table ips_release (
  id int not null,
  release_date date,
  constraint rel_pk primary key (id)
  )

alter table ips_defects add release_id int;
alter table ips_defects add constraint fk_release_id foreign key (release_id) references ips_release(id);
