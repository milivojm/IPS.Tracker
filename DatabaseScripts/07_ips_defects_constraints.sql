alter table ips_defects drop constraint def_ck1;

alter table ips_defects
    add constraint def_ck1 check ( defect_state in (
        'OPN',
        'PRO',
        'NRE',
        'REV',
        'INV',
        'CLS'
    ) );