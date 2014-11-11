alter table ips_defect_comments
add defect_id number(10,0) not null;

alter table ips_defect_comments
add constraint dfc_def_fk foreign key (defect_id) references ips_defects(id);

alter table ips_defect_comments
add text varchar2(1000 char);