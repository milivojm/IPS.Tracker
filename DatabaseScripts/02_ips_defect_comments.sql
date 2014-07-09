create table ips_defect_comments (
  id number(10,0) not null
, commentator_id number(10,0) not null
, comment_date date not null
, constraint dfc_pk primary key (id)
, constraint dfc_wor_fk foreign key(commentator_id) references ips_workers(id)
);