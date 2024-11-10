using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Data;
using NavistarPagos.Entity;
using NavistarPagos.Models;

namespace NavistarPagos.AccountCore
{
    public partial class resumen : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int claveCliente = 0;
            string formato = Request.Params["formato"];
            int.TryParse(Session["cveCliente"].ToString(), out claveCliente);
            string nombre = Session["Name"].ToString();
            string rfc = Session["rfc"].ToString();
            string correo = Session["correo"].ToString();

            string nombreArchivo = "~/AccountCore/pdf/resumen-" + claveCliente + "." + formato;
            string nombreFisico = Server.MapPath(nombreArchivo);
            string msg = "";
            DataTable dt = RecuperaDatos1(claveCliente, ref msg);
            string script = "<script type='text/javascript'>";
            MemoryStream ms = new MemoryStream();
            if (msg == "")
            {
                if (formato == "pdf")
                {
                    ms = PDFPago.GeneraResumen(claveCliente, nombre, rfc, correo, dt, nombreFisico);
                }
                else if (formato == "xlsx")
                {
                    ms = AppExcel.GeneraResumen(claveCliente, nombre, rfc, correo, dt, ref msg);
                    if (msg == "") PDFPago.fnEscribeArchivo(ms, nombreFisico);
                }
                else
                {
                    msg = "Formato no conocido, verifique por favor " + formato;
                }
                if (msg == "")
                {
                    string urlArchivo = Page.ResolveUrl(nombreArchivo);
                    //script += "var wo = window.opener;window.close();wo.abrirResumen('" + urlArchivo + "','" + Path.GetFileName(nombreFisico) + "');";
                    script += "abrirResumen('" + urlArchivo + "','" + Path.GetFileName(nombreFisico) + "');";
                }
            }
            if (msg != "")
            {
                script += "activaBTNResumen();alert('" + msg.Replace("'", "") + "');";
            }
            //if (formato == "xlsx" && msg == "")
            //{
            //    byte[] archivo = AppExcel.ConvierteStreamToByte(ms);
            //    string type = "application/vnd.ms-excel";
            //    nombreArchivo = Path.GetFileName(nombreFisico);
            //    Response.Clear();
            //    Response.Buffer = true;
            //    Response.ContentType = type;
            //    Response.AppendHeader("content-disposition", "Attachment;filename=" + nombreArchivo);
            //    Response.BinaryWrite(archivo);
            //    Response.End();
            //}
            //else
            {
                script += "</script>";
                ClientScript.RegisterStartupScript(Page.GetType(), "Mensaje", script);
            }
        }

        public DataTable RecuperaDatos(int claveCliente, ref string msg)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = null;
            DataTable tbl_ACTIVOS = null;
            try
            {
                // Contratos PAGOS INICIALES
                string sSQL = "spFechaPagoContratosPagosIniciales";
                SqlParameter[] paramArray = new SqlParameter[1];

                paramArray[0] = BaseDatos.BDatos.NewParameter("@id_cliente", claveCliente, "");
                DataSet ds = conector.RunProcedure(sSQL, paramArray, "tblResumen");
                tbl = ds.Tables[0];
                tbl.Columns.Add("Status_Contrato", typeof(System.String));

                foreach (DataRow dr in tbl.Rows)
                {
                    dr["Status_Contrato"] = "PAGOS INICIALES";
                }


                sSQL = "spFechaPagoContratosVigentes";
                paramArray = new SqlParameter[1];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@id_cliente", claveCliente, "");
                ds = conector.RunProcedure(sSQL, paramArray, "tblResumen");
                tbl_ACTIVOS = ds.Tables[0];
                tbl_ACTIVOS.Columns.Add("Status_Contrato", typeof(System.String));

                foreach (DataRow dr in tbl_ACTIVOS.Rows)
                {
                    dr["Status_Contrato"] = "ACTIVO";
                }
                tbl.Merge(tbl_ACTIVOS);

            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message + ((ex.InnerException != null) ? " - " + ex.InnerException.Message : "");
            }
            return tbl;
        }

        public DataTable RecuperaDatos1(int claveCliente, ref string msg, bool bPagosIniciales = true)
        {

            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<ResumenContratos> contratos = new List<ResumenContratos>();
            string sSQL = "";
            SqlParameter[] paramArray;
            DataSet ds = new DataSet();

            try
            {
                // Contratos PAGOS INICIALES
                if (bPagosIniciales)
                {
                    sSQL = "spFechaPagoContratosPagosIniciales";
                    paramArray = new SqlParameter[1];
                    paramArray[0] = BaseDatos.BDatos.NewParameter("@id_cliente", claveCliente, "");
                    ds = conector.RunProcedure(sSQL, paramArray, "tblResumen");
                    tbl = ds.Tables[0];

                    if (tbl != null && tbl.Rows.Count > 0)
                    {
                        foreach (DataRow item in tbl.Rows)
                        {
                            contratos.Add(new ResumenContratos
                            {
                                Contrato = item["cto_fl_cve"].ToString().Trim(),
                                MontoAPagar = decimal.Parse(item["MONTOAPAGAR"].ToString()),
                                Moneda = item["MONEDA"].ToString().Trim(),
                                FechaPago = "DE INMEDIATO",
                                Domiciliado = item["Domiciliado"].ToString().Trim(),
                                Beneficiario = item["Beneficiario"].ToString().ToUpper().Trim(),
                                PagoCuentaBanamex = item["PagoACuentaBanamex"].ToString().Trim(),
                                PagoCuentaBBVA = item["PagoACuentaBBVA"].ToString().Trim(),
                                TransferenciaBBVA = item["TransferenciaBBVA"].ToString().Trim(),
                                Referencia = item["Referencia"].ToString().Trim(),
                                Status_Contrato = "PAGOS INICIALES",
                                Fechafinmov = item["Fechafinmov"].ToString() == "" ? "" : _FillFecha(item["Fechafinmov"].ToString()), //DateTime.Parse(item["Fechafinmov"].ToString()).ToString("dd/MM/yyyy"),
                                EstatusContrato = item["Estatus"].ToString().Trim()
                            });
                        }
                    }
                    if (contratos.Count > 0)
                    {
                        foreach (ResumenContratos rsContratos in contratos)
                        {


                            //if (rsContratos.Fechafinmov != "" || rsContratos.Fechafinmov.Length > 0)
                            //{
                            //    rsContratos.FechaPago = "";
                            //    rsContratos.Status_Contrato = "Terminado";
                            //}

                            if (rsContratos.EstatusContrato == "1" || rsContratos.EstatusContrato == "2")
                            {
                                //rsContratos.FechaPago = "";
                                rsContratos.Fechafinmov = "";
                                rsContratos.Status_Contrato = "SIN ACTIVAR ";
                            }

                            if (rsContratos.EstatusContrato == "5")
                            {
                                rsContratos.FechaPago = "";
                                rsContratos.Status_Contrato = "TERMINADO";
                            }

                        }
                    }
                }


                // Contratos Activos
                sSQL = "spFechaPagoContratosVigentes";
                paramArray = new SqlParameter[1];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@id_cliente", claveCliente, "");
                ds = conector.RunProcedure(sSQL, paramArray, "tblResumen");
                tbl = ds.Tables[0];

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        contratos.Add(new ResumenContratos
                        {
                            Contrato = item["cto_fl_cve"].ToString().Trim(),
                            MontoAPagar = decimal.Parse(item["MONTOAPAGAR"].ToString()),
                            Moneda = item["MONEDA"].ToString().Trim(),
                            FechaPago = DateTime.Parse(item["FECHA_EXIGIBILIDAD"].ToString()).ToString("dd/MM/yyyy"),
                            Domiciliado = item["Domiciliado"].ToString().Trim(),
                            Beneficiario = item["Beneficiario"].ToString().ToUpper().Trim(),
                            PagoCuentaBanamex = item["PagoACuentaBanamex"].ToString().Trim(),
                            PagoCuentaBBVA = item["PagoACuentaBBVA"].ToString().Trim(),
                            TransferenciaBBVA = item["TransferenciaBBVA"].ToString().Trim(),
                            Referencia = item["Referencia"].ToString().Trim(),
                            Status_Contrato = "ACTIVO",
                            Fechafinmov = ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                //mod.Log_Diario("RecuperaDatos(Ex)", "Cve: " + claveCliente + "\nError: " + sEx);
            }
            DataTable dataTable =  CreateDataTable();

            foreach (var contrato in contratos)
            {
                DataRow row = dataTable.NewRow();
                row["Contrato"] = contrato.Contrato;
                row["MontoAPagar"] = contrato.MontoAPagar;
                row["Moneda"] = contrato.Moneda;
                row["FechaPago"] = contrato.FechaPago;
                row["Domiciliado"] = contrato.Domiciliado;
                row["Beneficiario"] = contrato.Beneficiario;
                row["PagoCuentaBanamex"] = contrato.PagoCuentaBanamex;
                row["PagoCuentaBBVA"] = contrato.PagoCuentaBBVA;
                row["TransferenciaBBVA"] = contrato.TransferenciaBBVA;
                row["Referencia"] = contrato.Referencia;
                row["Status_Contrato"] = contrato.Status_Contrato;
                row["Fechafinmov"] = contrato.Fechafinmov;
                row["EstatusContrato"] = contrato.EstatusContrato;

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public string _FillFecha(string fecha)
        {

            var arrFecha = fecha.Split('/');

            if (arrFecha.Length > 0)
            {
                string _opc = arrFecha[1];
                switch (_opc)
                {
                    case "1":
                        arrFecha[1] = "ene";
                        break;
                    case "2":
                        arrFecha[1] = "feb";
                        break;
                    case "3":
                        arrFecha[1] = "mar";
                        break;
                    case "4":
                        arrFecha[1] = "abr";
                        break;
                    case "5":
                        arrFecha[1] = "may";
                        break;
                    case "6":
                        arrFecha[1] = "jun";
                        break;
                    case "7":
                        arrFecha[1] = "jul";
                        break;
                    case "8":
                        arrFecha[1] = "ago";
                        break;
                    case "9":
                        arrFecha[1] = "sep";
                        break;
                    case "10":
                        arrFecha[1] = "oct";
                        break;
                    case "11":
                        arrFecha[1] = "nov";
                        break;
                    case "12":
                        arrFecha[1] = "dic";
                        break;
                }

            }

            return string.Format("{0} / {1} / {2}", arrFecha[0], arrFecha[1], arrFecha[2]);
        }

        static DataTable CreateDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Contrato", typeof(string));
            dataTable.Columns.Add("MontoAPagar", typeof(decimal));
            dataTable.Columns.Add("Moneda", typeof(string));
            dataTable.Columns.Add("FechaPago", typeof(string));
            dataTable.Columns.Add("Domiciliado", typeof(string));
            dataTable.Columns.Add("Beneficiario", typeof(string));
            dataTable.Columns.Add("PagoCuentaBanamex", typeof(string));
            dataTable.Columns.Add("PagoCuentaBBVA", typeof(string));
            dataTable.Columns.Add("TransferenciaBBVA", typeof(string));
            dataTable.Columns.Add("Referencia", typeof(string));
            dataTable.Columns.Add("Status_Contrato", typeof(string));
            dataTable.Columns.Add("Fechafinmov", typeof(string));
            dataTable.Columns.Add("EstatusContrato", typeof(string));

            return dataTable;
        }


    }
}