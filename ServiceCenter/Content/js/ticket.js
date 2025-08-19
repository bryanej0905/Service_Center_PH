$(function () {
    moment.locale('es');

    $.fn.dataTable.moment('DD/MM/YYYY hh:mm:ss a');

    if ($('#ticketTable thead tr').length === 1) {
        $('#ticketTable thead tr').clone(true).appendTo('#ticketTable thead');
    }

    var table = $('#ticketTable').DataTable({
        destroy: true,
        orderCellsTop: true,
        fixedHeader: true,
        scrollCollapse: true,
        paging: false,
        info: false,
        autoWidth: false,
        dom: "<'row mb-3'<'col-sm-12 col-md-6'B><'col-sm-12 col-md-6'f>>" +
            "rt" +
            "<'row mt-2'<'col-sm-12'i>>",
        buttons: [{
            extend: 'excelHtml5',
            text: '<i class="fa-solid fa-file-excel" style="color: white;"></i> Exportar a Excel',
            className: 'btn btn-success',
            title: 'ReporteTickets',
            exportOptions: { columns: ':not(:last-child)' }
        }],
        columnDefs: [{
            targets: 4, 
            render: function (data, type) {
                if (type === 'sort' || type === 'type') {
                    var norm = (data || '')
                        .replace(/\s*a\. m\.\s*/i, ' am')
                        .replace(/\s*p\. m\.\s*/i, ' pm');
                    var m = moment(norm, 'DD/MM/YYYY hh:mm:ss a', true);
                    return m.isValid() ? m.valueOf() : 0; 
                }
                return data;
            }
        }]
    });

    $('#ticketTable thead tr:eq(1) th').each(function (i) {

        var title = $('#ticketTable thead tr:eq(0) th').eq(i).text().trim();
        $(this).html('<input type="text" placeholder="Buscar ' + title + '" class="form-control form-control-sm" />');

        $('input', this).on('keyup change', function () {
            if (table.column(i).search() !== this.value) {
                table.column(i).search(this.value).draw();
            }
        });
    });

    $('#globalSearch').on('keyup change', function () {
        table.search(this.value).draw();
    });
});