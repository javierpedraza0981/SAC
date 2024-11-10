function chkBtn(chk) {
    var chk = document.getElementById(chk.id);
    var hIdtbOtroMonto = document.getElementById('hIdtbOtroMonto');
    var tbOtroMonto = document.getElementById(hIdtbOtroMonto.value);
    if (chk.checked) {
        tbOtroMonto.disabled = false;
    } else {
        tbOtroMonto.disabled = true;
    }
}
function mensajeAlerta(msg) {
    msg = msg.replace(/&aacute;/g, 'á').replace(/&eacute;/g, 'é').replace(/&iacute;/g, 'í').replace(/&oacute;/g, 'ó').replace(/&uacute;/g, 'ú').replace(/&uuml;/g,'ü');
    alert(msg);
}
function cierraPagos(btn) {
    parent.$('#PopupPagos').modal('hide');
}
function isNumberKey(evt, tb) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) return false;
    return true;
}
function isDecimalKey(evt, tb) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) if (charCode != 46) return false;
    return true;
}
function formateaCampo(evt, tb) {
    var sMonto = formatMonto(tb.value.replace(/,/, ''), 14, 2);
    tb.value = sMonto;
}
function formatMonto(v, maxLength, decimales) {
    var mult = 100;
    var ceros = '00';
    if (decimales != undefined) {
        ceros = '00000000000000000000'.substr(0, decimales);
        mult = parseInt('1' + ceros);
    }
    if (v == null) v = 0;
    if (v.toString() == 'NaN') v = 0;
    v = (Math.round((v - 0) * mult)) / mult;
    v = (v == Math.floor(v)) ? v + '.' + ceros : ((v * 10 == Math.floor(v * 10)) ? v + '0' : v);
    v = String(v);
    var ps = v.split('.');
    var whole = ps[0];
    var sub = ps[1] ? '.' + ps[1] : '.' + ceros;
    var r = /(\d+)(\d{3})/;
    while (r.test(whole)) {
        whole = whole.replace(r, '$1' + ',' + '$2');
    }
    v = whole + sub;
    if (maxLength != null) {
        if (v.length > maxLength) {
            v = v.substring(0, maxLength);
            if (v.substring(v.length - 1) == ".") v = v.substring(0, v.length - 1);
        }
    }
    if (v.charAt(0) == '-') return '-' + v.substr(1);
    return v;
}
