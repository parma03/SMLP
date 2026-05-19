function initDataTable2(tableId) {
    $("#" + tableId).DataTable({
        fixedColumns: {
            left: 1,
            right: 1
        },
        scrollY: 300,
        scrollX: true,
        scrollCollapse: true
    });
}
