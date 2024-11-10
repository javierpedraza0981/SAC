function GetFacturation() {
    var Factures = document.getElementById('Factures');
    var contract = document.getElementById('contract');
    var year = document.getElementById('year');
    var month = document.getElementById('month');

    $.ajax({
        type: "GET",
        url: '../../NavistarPagos/Home/GetFacturation',
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        data: {
            contract: contract.value,
            year: year.value,
            month: month.value
        },
        success: function (result) {
            Factures.innerHTML = '';
            for (var i = 0; i < result.length; i++) {
                Factures.innerHTML +=
                    '<tr>' +
                        '<td>' + result[i].eFolio + '</td>' +
                        '<td>' + result[i].vSerie + '</td>' +
                        '<td>' + result[i].fFiscal + '</td>' +
                        '<td>' + result[i].vContrato + '</td>' +
                        '<td>' +
                            '<a target="_blank" href="../AccountCore/consultafactura.aspx?tipo=xml&compania=' + result[i].eCompania + '&documento=' + result[i].eDocumento + '"><img src="../img/xml.png" alt="xml" border="0"/></a>' +
                            '<a target="_blank" href="../AccountCore/consultafactura.aspx?tipo=pdf&compania=' + result[i].eCompania + '&documento=' + result[i].eDocumento + '"><img src="../img/pdf.jpg" alt="pdf" border="0"/></a>' +
                        '</td>' +
                    '</tr>';
            }
        },
        error: function (reponse) {
            alert('error');
        }
    });
}

function factura(tipo, compania, documento) {
    NewWindow('consultafactura.aspx?tipo=' + tipo + '&compania=' + compania + '&documento=' + documento, 'cmp', 800, 550, 0);
}