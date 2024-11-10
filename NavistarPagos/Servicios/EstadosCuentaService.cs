using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NavistarPagos.EstadoCuentaWs;
using NavistarPagos.Data;

namespace NavistarPagos.Servicios
{
    public class EstadosCuentaService
    {
        Validaciones mod = new Validaciones();
        public List<string> ObtenerResultado(string _Contrato, int _Empresa)
        {
            // Crea una instancia del cliente del servicio web
            WSEstadosDeCuentaSoapClient cliente = new WSEstadosDeCuentaSoapClient();
            List<string> lstResumenInfo = new List<string>();
            // Define los parámetros necesarios para llamar al método del servicio
            string usuario = "1";
            string clave = "23075";
            int claveEmpresa = _Empresa; // Reemplaza con el valor adecuado
            string contrato = _Contrato;
            string fechaInicioMovimientos = DateTime.Now.Date.AddDays(-28).Date.ToString("yyyy/MM/dd");
            string fechaFinalMovimientos = DateTime.Now.Date.ToString("yyyy/MM/dd");
            string fechaInicioVencimientos = "1900/01/01";
            string fechaFinalVencimientos = "1900/01/01";

            try
            {
                mod.Log_Diario("EstadosCuentaService ln 30", "Cve: " + "" + "\nError: " );
                lstResumenInfo =  cliente.ObtenEstadoDeCuentaNavistarPagos(usuario, clave, claveEmpresa, contrato, fechaInicioMovimientos, fechaFinalMovimientos, fechaInicioVencimientos, fechaFinalVencimientos);
                mod.Log_Diario("**EstadoCuentaMesActualGeneradoOk", "_Contrato: " + _Contrato);
            }
            catch (Exception ex)
            {
                mod.Log_Diario("**EstadoCuentaMesActualGeneradoError", "_Contrato: " + _Contrato);
            }
            
            return lstResumenInfo;
        }
    }
}