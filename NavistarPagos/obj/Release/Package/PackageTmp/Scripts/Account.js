var urlBase = "https://servicioscorporativosnfc.com/NavistarPagos/"
urlBase = "https://localhost:44327/";
var urlAccount = urlBase + "AccountCore/";

function abrirPDFPago(urlPDF) {
    if (urlPDF.length > 1) NewWindow(urlPDF, 'eop', 800, 550, 0);
    window.location.href = urlBase + 'home/contracts';
}
function abrirResumen(urlResumen, nombre) {
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = urlResumen;
    a.download = nombre;
    document.body.appendChild(a);
    a.click();
    activaBTNResumen();
    window.URL.revokeObjectURL(url);
}
function activaBTNResumen() {
    var divProcesando = parent.document.getElementById('divProcesandopdf');
    divProcesando.style.visibility = "hidden";
    var divProcesando = parent.document.getElementById('divProcesandoxlsx');
    divProcesando.style.visibility = "hidden";
}
function abrirDomiciliar() {
    $.ajax({
        type: "GET",
        url: '../../NavistarPagos/Home/OpenDomiciliation',
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        data: { },
        success: function (result) {
            var liga = result;
            var divPagos = $("#divPagos");
            var ifrmPagos = document.getElementById('idFrmPagos');
            $("#bloqueModalDetallePagos").hide();
            if (ifrmPagos) {
                ifrmPagos.src = liga;
            } else {
                divPagos.html("<iframe id='idFrmPagos' src='" + liga + "' style='zoom:0.60' width='99.5%' height='600' frameborder='0' />");
            }
            divPagos.show();
            $('#PopupPagos').modal('show');
        },
        error: function (error) {
            alert('error');
        }
    });
}
function aplicarPagoPortal(contrato, referencia, saldoTotal, saldoMes, fechaPago, nombreCliente) {
    var link = "../AccountCore/Aplicapago.aspx?lcontrato=Contrato&lreferencia=Referencia&contrato=" + contrato + "&referencia=" + referencia + "&saldoTotal=" + saldoTotal + "&saldoMes=" + saldoMes + "&otroPago=true&fechaPago=" + fechaPago + "&origen=1&nombre=" + nombreCliente;
    var ifrmPagos = parent.document.getElementById('idFrmPagos');
    if (ifrmPagos) {
        ifrmPagos.src = link;
    } else {
        document.getElementById('divPagos').innerHTML = "<iframe id='idFrmPagos' src='" + link + "' style='zoom:0.60' width='99.5%' height='600' frameborder='0' />";
    }
    $('#PopupPagos').modal('show');
}

function buscapdf() {
    var Contract = $('#Contract').val();
    if (Contract != "-1") {
        var link = urlAccount + "Nuevo_estado.aspx?formato=pdf&contrato=" + Contract + "&base=0";
        NewWindow(link, 'eop', 800, 550, 0);
    } else {
        alert("Debe seleccionar un contrato.");
    }
}

function buscapdfresumen() {
    var Contract = $('#Contract').val();
    if (Contract != "-1") {
        var link = urlAccount + "Nuevo_estado.aspx?formato=pdfResumen&contrato=" + Contract + "&base=0";
        NewWindow(link, 'eop', 800, 550, 0);
    } else {
        alert("Debe seleccionar un contrato.");
    }
}

function ChangeContract(contract) {
    document.getElementById('Contract').value = contract;
}

function NewWindow(mypage, myname, w, h, scroll) {
    var winl = (screen.width - w) / 2;
    var wint = (screen.height - h) / 2;
    winprops = 'height=' + h + ',width=' + w + ',top=' + wint + ',left=' + winl + ',scrollbars=' + scroll;
    win = window.open(mypage, myname, winprops);
    if (parseInt(navigator.appVersion) >= 4) { win.window.focus(); }
}
function NewWindowOculta(mypage, myname, w, h, scroll) {
    var element = document.createElement("iframe");
    element.setAttribute('idFrmNW', 'myframe');
    element.src = mypage;
    document.body.appendChild(element);
    //document.getElementById('idFrmNW').setAttribute('src', newloc);
    //var winl = 100000; //(screen.width - w) / 2;
    //var wint = 100000; //(screen.height - h) / 2;
    //winprops = 'height=' + h + ',width=' + w + ',top=' + wint + ',left=' + winl + ',scrollbars=' + scroll + 'visible=none';
    //win = window.open(mypage, myname, winprops);
    //if (parseInt(navigator.appVersion) >= 4) { win.hide(); }
}
function GetPolicy(Policy) {
    $.ajax({
        type: "GET",
        url: '../../NavistarPagos/Home/OpenPolicy',
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        data: {
            Policy: Policy
        },
        success: function (result) {
            window.open(result, '_blank');
        },
        error: function (error) {
            alert('error');
        }
    });
}

function searchVin() {
    var vin = document.getElementById('vin');
    window.location.href = urlAccount + "/Home/Policy" + "?vin=" + vin.value;
}

function sweetalerts(tit, tex, ico, butt) {
    swal({
        title: tit,
        text: tex,
        icon: ico,
        button: 'Aceptar',
    });
}