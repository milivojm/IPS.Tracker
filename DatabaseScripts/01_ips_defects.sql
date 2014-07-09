create table ips_defects (
  id number(10,0) not null
, priority number(1,0) not null
, summary varchar2(50 char) not null
, description varchar2(4000 char) not null
, work_order_id number(10,0)
, reporter_id number(10,0) not null
, assignee_id number(10,0) not null
, defect_file blob
, defect_date date not null
, defect_state varchar2(3 char) not null
, constraint def_pk primary key (id)
, constraint def_wo_fk foreign key (work_order_id) references ips_work_orders (id)
, constraint def_wor_fk1 foreign key (reporter_id) references ips_workers (id)
, constraint def_wor_fk2 foreign key (assignee_id) references ips_workers (id)
, constraint def_ck1 check (defect_state in ('OPN','PRO','TST','INV','CLS'))
);