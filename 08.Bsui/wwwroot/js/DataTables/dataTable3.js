function initDataTable3(tableId, fixedLeftColumns) {
    var table = $("#" + tableId).DataTable({
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": 0
        }],
        "order": [[1, 'asc']],
        fixedColumns: {
            left: fixedLeftColumns,
            right: 1
        },
        scrollY: 300,
        scrollX: true,
        scrollCollapse: true
    });

    table.on('order.dt search.dt', function () {
        table.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();
}
