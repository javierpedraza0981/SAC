var arrayMes = ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];
var montoInicialPagar;

function descargaArchivosCliente() {
    clickBTNResumen('pdf');
    clickBTNResumen('xlsx');
}

function clickBTNResumen(formato) {
    var divProcesando = document.getElementById('divProcesando' + formato);
    divProcesando.style.visibility = "visible";
    var link = urlAccount + "Resumen.aspx?formato=" + formato;
    NewWindowOculta(link, 'eop', 50, 50, 0);
}

function peticionGlobal(dataRq, dataRs) {
    $.ajax({
        data: { data: JSON.stringify(dataRq) },
        type: 'POST',
        dataType: 'JSON',
        url: dataRq.recurso,
        beforeSend: function () {
            dataRq.animado = "s";
            dataRq.clase = "i";
            setAlertaMensaje(dataRq);
        },
        error: function (request, error) {
            dataRq.animado = "n";
            dataRq.clase = "d";
            dataRq.contenido = "<b>Error:</b> verifique que este conectado a la red, he intente de nuevo.";
            setAlertaMensaje(dataRq);
        },
        success: function (dataRs) {
            eval(dataRs.accion + "(dataRq,dataRs);");
        }
    });
}

function setAlertaMensaje(dataRq) {
    var animado = (dataRq.animado == undefined) ? "n" : dataRq.animado;
    var clase = dataRq.clase;
    var c, titulo, contenido;
    switch (animado) {
        case "s":
            switch (clase) {
                case "i":
                    c = "info";
                    titulo = "<h4>Solicitando al información al servidor</h4>";
                    contenido = "\n\
                        <div class='progress'>\n\
                            <div class='progress-bar progress-bar-striped active' role='progressbar' aria-valuenow='100' aria-valuemin='0' aria-valuemax='100' style='width: 100%'>\n\
                                <span class='sr-only'>100%</span>\n\
                            </div>\n\
                        </div>\n\
                    ";
                    break;
            }
            break;
        case "n":
            switch (clase) {
                case "i":
                    c = "info";
                    titulo = "<h4>Tip</h4>";
                    contenido = "<p>" + dataRq.contenido + "</p>";
                    break;
                case "w":
                    c = "warning";
                    titulo = "<h4>¡Precaución!</h4>";
                    contenido = "<p>" + dataRq.contenido + "</p>";
                    break;
                case "d":
                    c = "danger";
                    titulo = "<h4>¡Atención!</h4>";
                    contenido = "<p>" + dataRq.contenido + "</p>";
                    break;
                case "s":
                    c = "success";
                    titulo = "<h4>Completado</h4>";
                    contenido = "<p>" + dataRq.contenido + "</p>";
                    break;
            }
            break;
    }
    $("#" + dataRq.contenedor).html(titulo + contenido).attr("class", "alert alert-" + c);
}

function clickBTNPagoLinea(contrato, referenciaNumerica, saldoporVenc, saldoMes, fecCorte, fecProxPago, nombreCliente) {
    var dataRq = new Object();
    var accion = "mostrarDetallePagos";
    dataRq.recurso = rutaAccount + "Home/" + accion;
    dataRq.accion = accion;
    dataRq.contrato = contrato;
    dataRq.referencia = referenciaNumerica;
    dataRq.saldoporVenc = saldoporVenc;
    dataRq.saldoMes = saldoMes;
    dataRq.fechaCorte = fecCorte;
    dataRq.fecProxPago = fecProxPago;
    dataRq.nombreCliente = nombreCliente;
    peticionGlobal(dataRq);
}
function confirmarDomiciliacion(dataRq, dataRs) {
    var rsp = confirm(dataRs.contenido);
    if (rsp == true) { mostrarDetallePagos(dataRq, dataRs); }
}
function mostrarDetallePagos(dataRq, dataRs) {
    initDetallePagos(dataRq, dataRs);

    $("#optMontoPagar").click(function () {
        $("#lblMontoExcedente").hide();
        getSeleccionadosMontoTotal(false);
    });
    $("#optMontoOtro").click(function () {
        $("#txtMontoOtro").prop("disabled", false).focus();
        $("#lblMontoExcedente").hide();
        getSeleccionadosMontoTotal(false);
    });
    $("#txtMontoOtro").on('input', function (e) {
        if (/^[0-9.]*$/i.test(this.value)) {
            getMontoParcial();
        } else {
            this.value = this.value.replace(/[^0-9.]+/ig, "");
        }
    });
}

function initDetallePagos(dataRq, dataRs) {
    var contrato = dataRq.contrato;
    var referencia = dataRq.referencia;
    var saldoTotal = dataRq.saldoporVenc;
    var saldoMes = dataRq.saldoMes;
    var fechaPago = dataRq.fecProxPago;
    var fechaCorte = dataRq.fechaCorte;
    var nombreCliente = dataRq.nombreCliente;
    if (dataRs.listaDetallePagos == 'true') {
        alert(dataRs.leyendaPendiente);
    } else {
        var objJSON = JSON.parse(dataRs.listaDetallePagos);
        var cadTDBDetallePagos = "";

        $("#bloqueModalDetallePagos").show();
        $("#divPagos").hide();

        $("input[name=chkTipoMonto]:first").prop("checked", true);
        $("#lblMontoPagar").html("");
        $("#txtMontoOtro").val("").prop("disabled", true);
        $("#lblMontoExcedente").hide();

        $("#lblFechaCorte").html(fechaCorte);

        $("#lblDetallePagoContrato").html(contrato);
        $("#lblDetallePagoNombreCliente").html(nombreCliente);

        $("#txtMontoOtro").blur(function () {
            $(this).val(accounting.formatMoney($(this).val()));
        });

        $("#btnDetallePagoResumen").click(function () {
            var montoFinal = 0;
            if ($("#optMontoPagar").is(":checked")) {
                montoFinal = accounting.unformat($("#lblMontoPagar").html());
            } else if ($("#optMontoOtro").is(":checked")) {
                montoFinal = accounting.unformat($("#txtMontoOtro").val());;
            }

            if (montoFinal == 0) {
                alert("No debe dejarlo vacio o en ceros.");
            } else {
                $("#bloqueModalDetallePagos").hide();
                $("#divPagos").show();

                saldoTotal = ($("#optMontoPagar").is(":checked"))?accounting.unformat($("#lblMontoPagar").html()):accounting.unformat($("#txtMontoOtro").val());
                saldoMes = 0;

                var link = "../AccountCore/Aplicapago.aspx?lcontrato=Contrato&lreferencia=Referencia&contrato=" + contrato + "&referencia=" + referencia + "&saldoTotal=" + saldoTotal + "&saldoMes=" + saldoMes + "&otroPago=false&fechaCorte=" + fechaCorte + "&fechaPago=" + fechaPago + "&origen=1&nombre=" + nombreCliente;
                $("#divPagos").html("<iframe id='idFrmPagos' src='" + link + "' style='zoom:0.60' width='99.5%' height='600' frameborder='0' />");
            }
        });

        var i = 1;

        cadTDBDetallePagos =
            "<tr>" +
            "<td>&nbsp;</td>" +
            "<td>" +
            "<input id='chkDetallePagosTodos' type='checkbox' checked/>" +
            "</td>" +
            "<td colspan='3'><label for='chkDetallePagosTodos'>Todos</label></td>" +
            "</tr>"
            ;

        $.each(objJSON, function (kAnio, vAnio) {
            $.each(vAnio, function (kMes, vMes) {
                var cadMes = "";
                cadTDBDetallePagos +=
                    "<tr>" +
                    "<td style='text-align:center;font-weight:bold;'>" + i + "</td>" +
                    "<td><input id='opt" + i + "' name='chkDetallePagosMes' type='checkbox' data-no='" + i + "' value='[saldoTotal]' checked='checked'/></td>" +
                    "<td><label for='opt" + i + "' style='cursor: pointer; '>" + arrayMes[parseInt(kMes) - 1] + " " + kAnio + "</label></td>" +
                    "<td>&nbsp;</td>" +
                    "<td>&nbsp;</td>" +
                    "</tr>"
                    ;
                var saldoTotalMes = 0;
                $.each(vMes, function (kDescripcion, vDescripcion) {
                    var saldo = parseFloat(vDescripcion.saldo);
                    cadTDBDetallePagos = cadTDBDetallePagos
                        .replace(/\[descripcionMovimiento]/g, arrayMes[parseInt(kMes) - 1])
                        .replace(/\[saldo]/g, accounting.formatMoney(saldo));
                    cadTDBDetallePagos +=
                        "<tr>" +
                        "<td>&nbsp;</td>" +
                        "<td>&nbsp;</td>" +
                        "<td>" + vDescripcion.descripcionMovimiento + "</td>" +
                        "<td class='text-right'>" + accounting.formatMoney(saldo) + "</td>" +
                        "<td>&nbsp;</td>" +
                        "</tr>";
                    saldoTotalMes += saldo;
                });
                var cMes = 1;
                cadTDBDetallePagos +=
                    "<tr>" +
                    "<td colspan='3' style='text-align:right;font-weight:bold;'>Total:</td>" +
                    "<td id='lblTotalSaldo" + i + "' class='text-right'>" + accounting.formatMoney(saldoTotalMes) + "</td>" +
                    "<td id='lblTotalParcial" + i + "' class='text-right bc-totalParcial'></td>" +
                    "</tr>";
                i++;
            });
        });
        $("#lblLeyendaPendiente").html(dataRs.leyendaPendiente);

        $("#tbdDetallePagos").html(cadTDBDetallePagos);

        (saldoTotal == 0) ? $("#bloqueDetallePagoCalculos").hide() : $("#bloqueDetallePagoCalculos").show();

        getSeleccionadosMontoTotal(true);

        $("#chkDetallePagosTodos").click(function () {
            var seleccionado = $(this).is(":checked");
            if (seleccionado) {
                $("input[name=chkDetallePagosMes]").prop({ "checked": true });
            } else {
                $("input[name=chkDetallePagosMes]").prop({ "checked": false });
            }
            getSeleccionadosMontoTotal(false);
            verificarCHKTodos();
        });

        $("input[name=chkDetallePagosMes]").click(function () {
            var chkSeleccionado = $(this).is(":checked");

            var no = 0;
            var chkSeleccionados = $("input[name=chkDetallePagosMes]:checked").length;
            var mensaje = "Los pagos son aplicados a aquellos cargos con fecha de vencimiento m&aacute;s antigua";

            if (chkSeleccionado) {
                no = $("input[name=chkDetallePagosMes]:checked").last().attr("data-no");
                if (chkSeleccionados == no) {
                    getSeleccionadosMontoTotal(false);
                } else {
                    mensajeAlerta(mensaje);
                    $(this).prop("checked", false);
                }
            } else {
                no = parseInt($(this).attr("data-no"));
                chkSeleccionados = chkSeleccionados + 1;

                if (chkSeleccionados == no) {
                    getSeleccionadosMontoTotal(false);
                } else {
                    mensajeAlerta(mensaje);
                    $(this).prop("checked", true);
                }
            }
            verificarCHKTodos();
        });

        $('#PopupPagos').modal({
            show: true
            , backdrop: 'static'
        });
    }
}

function getMontoParcial() {
    var txtMontoPagar = accounting.unformat($("#lblMontoPagar").html());
    var txtMontoOtro = accounting.unformat($("#txtMontoOtro").val());
    var totalExcedente = (txtMontoOtro - txtMontoPagar);

    if (totalExcedente == 0) {
        $(".bc-totalParcial").html("");
        $("#chkDetallePagosTodos").prop("checked", true);
        $("input[name=chkDetallePagosMes]").prop("checked", true);
        $("#bloqueExcedente").hide();
        $("#lblMontoExcedente").hide();
    } else if (totalExcedente > 0) {
        $(".bc-totalParcial").html("");
        $("#chkDetallePagosTodos").prop("checked", true);
        $("input[name=chkDetallePagosMes]").prop("checked", true);
        $("#lblMontoExcedente").html("Aplicar&#225; un excendente de: " + accounting.formatMoney(totalExcedente)).show();
        $("#bloqueExcedente").show();
    } else if (totalExcedente < txtMontoPagar) {
        var tmpTotal = txtMontoOtro;
        if (txtMontoOtro == "" || txtMontoOtro == 0) {
            $(".bc-totalParcial").html("");
            $("#chkDetallePagosTodos").prop("checked", false);
            $("input[name=chkDetallePagosMes]").prop("checked", false);
        } else {
            var totalMontoCHK = $("input[name=chkDetallePagosMes]");
            var total = 0;
            var no = 0;
            var tmpNo = 0;
            $.each(totalMontoCHK, function (k, v) {
                no = $(this).attr("data-no");
                total = accounting.unformat($("#lblTotalSaldo" + no).html());

                tmpTotal -= total;
                if (tmpTotal < 0) {
                    return false;
                }
            });

            $(".bc-totalParcial").html("");
            $("#chkDetallePagosTodos").prop("checked", false);
            $("input[name=chkDetallePagosMes]").prop("checked", false);

            for (var i = 1; i <= no; i++) {
                $("#opt" + i).prop("checked", true);
            }
            
            tmpTotal = 0;
            tmpNo = no;
            total = 0;
            if (no == 1) {
                total = accounting.formatMoney(txtMontoOtro);
                $("#lblTotalParcial" + no).html(total);
            } else {
                $.each(totalMontoCHK, function (k, v) {
                    no = $(this).attr("data-no");
                    total = accounting.unformat($("#lblTotalSaldo" + no).html());
                    
                    if (no <= (tmpNo-1)) {
                        tmpTotal += total;
                    }
                });
                total = tmpTotal;
                
                var montoParcial = txtMontoOtro - total;
                $("#lblTotalParcial" + tmpNo).html(accounting.formatMoney(montoParcial));
                $("#bloqueExcedente").hide();
            }
        }
        $("#lblMontoExcedente").hide();
    }
}

function verificarCHKTodos() {
    var chkTodos = $("input[name=chkDetallePagosMes]").length;
    var chkTodosSeleccionados = $("input[name=chkDetallePagosMes]:checked").length;
    var todos = true;
    if (chkTodos == chkTodosSeleccionados) {
        $("#optMontoPagar").prop("checked", true);
        $("#txtMontoOtro").val("").prop("disabled", true);
    } else {
        todos = false;
        $("#optMontoOtro").prop("checked", true);
        $("#txtMontoOtro").prop("disabled", false);
    }
    $("#chkDetallePagosTodos").prop("checked", todos);
    getSeleccionadosMontoTotal(false);
}

function getSeleccionadosMontoTotal(mostrarTotal) {
    var total = accounting.formatMoney(getMontoTotakCHKs());
    
    if (mostrarTotal) {
        $("#lblMontoPagar").html(total);
        $("#txtMontoOtro").val("").prop("disabled", true);
    }

    if ($("#optMontoOtro").is(":checked")) {
        $("#txtMontoOtro").val(total).prop("disabled", false);
    }
}

function getMontoTotakCHKs(){
    var seleccionado = $("input[name=chkDetallePagosMes]:checked");
    var total = 0;
    $.each(seleccionado, function (k, v) {
        var no = $(this).attr("data-no");
        var totalSaldo = accounting.unformat($("#lblTotalSaldo" + no).html());
        total += totalSaldo;
    });
    return (total);
}