// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

console.clear();

var navExpand = [].slice.call(document.querySelectorAll('.nav-expand'));
var backLink = `<li class="nav-item">
	<a class="nav-link nav-back-link" href="javascript:;">
		<i class="fa fa-angle-left "></i>  &nbsp; &nbsp;Regresar
	</a>
</li>`;

navExpand.forEach(item => {
    item.querySelector('.nav-expand-content').insertAdjacentHTML('afterbegin', backLink);
    item.querySelector('.nav-link').addEventListener('click', () => item.classList.add('active'));
    item.querySelector('.nav-back-link').addEventListener('click', () => item.classList.remove('active'));
});

var ham = document.getElementById('nav-menu');

var hmenuCanvasBackground = document.getElementById('hmenu-canvas-background');

if (ham && hmenuCanvasBackground) {

    ham.addEventListener('click', function () {
        document.body.classList.toggle('nav-is-toggled');
        document.getElementById('hmenu-canvas-background').classList.toggle('hmenu-transparent');
        document.getElementById('hmenu-canvas-background').classList.toggle('hmenu-opaque');
    });
}
else {
    console.error('Element not found');
}

//Funcion para mandar un mensaje personalizado
function sweetalerts(tit, tex, ico, butt) {
    swal({
        title: tit,
        text: tex,
        icon: ico,
        button: 'Aceptar',
    });
}

//Funcion para mandar un mensaje de confirmacion , devulve la respuesta, usa promesa para que el js espera la respuesta
function sweetalertsAsync(tit, txt, ico, booldanger) {
    return new Promise(resolve => {
        swal({
            title: tit,
            text: txt,
            icon: ico,
            buttons: {
                confirm: "Aceptar"
            },
            dangerMode: booldanger,
        })
            .then(() => {
                resolve(true);
            });
    });
}

//Funcion para mandar un mensaje de confirmacion , devulve la respuesta, usa promesa para que el js espera la respuesta
function sweetAlertConfirm(tit, txt, ico, booldanger) {
    return new Promise(resolve => {
        swal({
            title: tit,
            text: txt,
            icon: ico,
            buttons: {
                confirm: "Aceptar"
            },
            dangerMode: booldanger,
        })
            .then((willDelete) => {
                resolve(willDelete);
            });
    });
}

//Funcion para mandar un mensaje de confirmacion , devulve la respuesta, usa promesa para que el js espera la respuesta
function sweetAlertConfirm(tit, txt, ico, booldanger, btnCancel) {
    return new Promise(resolve => {
        swal({
            title: tit,
            text: txt,
            icon: ico,
            buttons: {                
                confirm: "Aceptar",
                cancel: "Cancelar"
            },
            dangerMode: booldanger,
        })
            .then((willDelete) => {
                resolve(willDelete);
            });
    });
}


function _isNumber(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
};

const _formatterPeso = new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN',
    minimumFractionDigits: 2
})

const _formatterPiezas = new Intl.NumberFormat('es-MX')

// DataTables

function _cargaTabla(_idTable, _columnsDefs, _isLengthChange, _isFilter, _isSort, _isPaginate, _isAutoWidth, _sScrollY , _isScrollColapse, _isInfo, _result, _columns, _createRow) {
    var _table = $(_idTable).DataTable(
        {
            'select': true,
            'columnDefs': _columnsDefs,
            'destroy': true,
            'bLengthChange': _isLengthChange,
            'bFilter': _isFilter,
            'bSort': _isSort,
            'bPaginate': _isPaginate,
            'bAutoWidth': _isAutoWidth,
            'sScrollX': '100%',
            'sScrollY': _sScrollY,
            'bScrollColapse': _isScrollColapse,
            'bInfo': _isInfo,
            'data': _result,
            'columns': _columns,
            'createdRow': function (nRow, aData, iDisplayIndex) {
                _createRow(nRow, aData, iDisplayIndex)
            }
        });

    return _table;
}

function _limpiaTabla(idTabla) {
    $(idTabla).DataTable().clear().draw();
}

function _limpiaElemento(idElemento) {
    var node = document.getElementById(idElemento);
    while (node.hasChildNodes()) {
        node.removeChild(node.lastChild);
    }
}
