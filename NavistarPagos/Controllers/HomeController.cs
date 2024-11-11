using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using NavistarPagos.AccountCore;
using NavistarPagos.Entity;
using NavistarPagos.Models;
using System.Data;
using System.Data.SqlClient;
using NavistarPagos.Data;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;

namespace NavistarPagos.Controllers
{
    public class HomeController : Controller
    {
        Validaciones mod = new Validaciones();
        cPersona Cliente = new cPersona();
        private sfiinternationalEntities Entity = new sfiinternationalEntities();

        #region "Seleccionar Cliente"

        public ActionResult Index(string ntoken, string psAction)
        {
            try
            {
                string urlOk = "";
                string urlError = "";
                string urlCancela = "";
                string correo = "";
                int cve = 0;
                string token = ntoken;
                Session["tokenUsr_1"] = ntoken;
                AppEncriptaJWT appEncriptaJWT = new AppEncriptaJWT();
                string llaveJWT = System.Configuration.ConfigurationManager.AppSettings["llaveJWT"];
                List<cPersona> listaPersonas = new List<cPersona>();

                if (token != "" && token != null)
                {
                    appEncriptaJWT.DesencriptaJWT(token, llaveJWT);
                    correo = appEncriptaJWT.GetClaim("email");
                    int.TryParse(appEncriptaJWT.GetClaim("idCliente"), out cve);
                    Session["cveUsuario"] = cve;
                    urlOk = appEncriptaJWT.GetClaim("urlOk");
                    urlError = appEncriptaJWT.GetClaim("urlError");
                    urlCancela = appEncriptaJWT.GetClaim("urlCancela");
                    if (urlOk == "") urlOk = Request.Url.OriginalString;
                    if (urlError == "") urlError = urlOk;
                    Session["urlOk"] = ((urlOk != null) ? urlOk : "");
                    Session["urlError"] = ((urlError != null) ? urlError : "");
                    Session["urlCancela"] = ((urlCancela != null) ? urlCancela : "");
                }

                if (cve == 0 && correo != "")
                {
                    listaPersonas = recuperaPersonaEmail(correo, "1");

                    if (listaPersonas.Count > 0)
                    {
                        foreach (var persona in listaPersonas)
                        {
                            // Lógica o procesamiento con cada persona en listaPersonas
                        }
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home", new { psError = "No hay clientes con esa información" });
                    }

                    if (listaPersonas.Count == 1)
                    {
                        cve = listaPersonas[0].PNA_FL_PERSONA;
                    }
                    else if (listaPersonas.Count > 1)
                    {
                        ViewBag.Clientes = listaPersonas;
                        ViewBag.Action = psAction;
                        return View();
                    }
                }

                if (cve > 0)
                {
                    if (psAction == "ClientFiles" && !ValidaContratos(cve))
                    {
                        ViewBag.Clientes = listaPersonas;
                        ViewBag.Action = psAction;
                        return View();
                    }
                    IniciaSesionExterna(cve);
                    return RedirectToAction(psAction, psAction);
                }

                return View();
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("Index(Ex)", "Token: " + ntoken + "\nAction: " + psAction + "\nError: " + sEx);
                return RedirectToAction("Error", "Home", new { psError = "Index" });
            }
        }



        public bool IniciaSesionExterna(int cve = 0)
        {
            string paso = "";
            try
            {
                int cveCliente = 0;
                paso = "1.";
                var homeConfiguracion = new HomeConfiguracion();
                paso += ", A";
                cveCliente = ((homeConfiguracion.TipoServidor == 1 || homeConfiguracion.TipoServidor == 2) ? 335 : cve);
                paso += ", 2: " + cveCliente.ToString();

                //Cliente = Entity.cPersonas.Where(x => x.PNA_FL_PERSONA == cveCliente).FirstOrDefault();
                string nCadena = "";
                List<cPersona> cliente = recuperaPersona(cve, ref nCadena);

                paso += ". Recupero Cliente";

                if (Cliente == null)
                {
                    paso = "No encontró el cliente " + cveCliente;
                }
                else
                {
                    paso = " Inicia actualiza sesión";
                    try
                    {
                        Session["Name"] = cliente.FirstOrDefault().PNA_DS_NOMBRE;
                        Session["cveCliente"] = cliente.FirstOrDefault().PNA_FL_PERSONA;
                        Session["rfc"] = cliente.FirstOrDefault().PNA_CL_RFC;
                        Session["correo"] = cliente.FirstOrDefault().PNA_DS_EMAIL;
                        paso = " Actualiza ViewBag";

                        if (ViewBag != null)
                        {
                            ViewBag.Name = Session["Name"];
                            ViewBag.IdCliente = Session["cveCliente"];
                            paso += ". ViewBag";
                        }
                        paso += ". Actualiza sesión B";
                    }
                    catch (Exception ex)
                    {
                        string sEx = ex.Message;

                        throw new Exception("En IniciaSesionExterna., " + paso + ", Error: [" + ex.Message + ((ex.InnerException != null) ? "- " + ex.InnerException.Message : "") + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                throw new Exception("En IniciaSesionExterna, " + paso + ", Error: [" + ex.Message + ((ex.InnerException != null) ? "- " + ex.InnerException.Message : "") + "]");
            }

            return true;
        }


        public ActionResult SeleccionarCliente(int cve, string psAction)
        {
            bool bSuccess = true;

            if (psAction == "ClientFiles") bSuccess = ValidaContratos(cve);
            mod.Log_Diario("IniciaSesionExterna -> ", "182");
            if (bSuccess) { IniciaSesionExterna(cve); }
            Session["cveUsuarioLog"] = cve;

            return Json(new { success = bSuccess }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "Archivos del Cliente"

        public ActionResult ClientFiles()
        {
            string msg = "";
            int cveCliente = 0;

            try
            {
                if (Session["cveCliente"] == null) return RedirectToAction("Error", "Home");
                cveCliente = int.Parse(Session["cveCliente"].ToString());

                ViewBag.Name = Session["Name"].ToString().Trim();
                ViewBag.CveCliente = cveCliente.ToString();
                ViewBag.RFC = Session["rfc"].ToString().Trim();
                ViewBag.Correo = Session["correo"].ToString().Trim();
                ViewBag.Contracts = RecuperaDatos(cveCliente, ref msg);
                //ViewBag.lstMessagePopup = SeleccionarPopMessage(); 

                if (msg != "")
                {
                    mod.Log_Diario("ClientFiles(RecuperaDatos)", "Cve: " + cveCliente + "\nError: " + msg);
                    return RedirectToAction("Error", "Home", new { psError = msg });
                }

                return View();
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("ClientFiles(Ex)", "Cve: " + cveCliente + "\nError: " + sEx);
                return RedirectToAction("Error", "Home", new { psError = "ClientFiles" });
            }
        }

        private bool ValidaContratos(int piCve)
        {
            List<ResumenContratos> contratos = new List<ResumenContratos>();
            string msg = "";

            contratos = RecuperaDatos(piCve, ref msg);
            if (contratos == null || contratos.Count <= 0)
            {
                return false;
            }

            return true;
        }


        public List<ResumenContratos> RecuperaDatos1(int claveCliente, ref string msg, bool bPagosIniciales = true)
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

                mod.Log_Diario("RecuperaDatos(Ex)", "Cve: " + claveCliente + "\nError: " + sEx);
            }

            return contratos;
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
        public List<ResumenContratos> RecuperaDatos(int claveCliente, ref string msg, bool bPagosIniciales = true)
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
                                FechaPago = "De inmediato",
                                Domiciliado = item["Domiciliado"].ToString().Trim(),
                                Beneficiario = item["Beneficiario"].ToString().ToUpper().Trim(),
                                PagoCuentaBanamex = item["PagoACuentaBanamex"].ToString().Trim(),
                                PagoCuentaBBVA = item["PagoACuentaBBVA"].ToString().Trim(),
                                TransferenciaBBVA = item["TransferenciaBBVA"].ToString().Trim(),
                                Referencia = item["Referencia"].ToString().Trim(),
                                Status_Contrato = "PAGOS INICIALES",
                                Fechafinmov = item["Fechafinmov"].ToString().Trim()

                            });
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
                            Status_Contrato = "ACTIVO"//,
                            //Paperless = item["Paperless"].ToString().Trim()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("RecuperaDatos(Ex)", "Cve: " + claveCliente + "\nError: " + sEx);
            }

            return contratos;
        }

        public List<cPersona> recuperaPersona(int cvePersona, ref string msg)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<cPersona> cPersona = new List<cPersona>();
            string sSQL = "";
            SqlParameter[] paramArray;
            DataSet ds = new DataSet();

            try
            {
                sSQL = "spConsultaPersonaPorFlPersona";
                paramArray = new SqlParameter[1];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@pna_fl_persona", cvePersona, "");
                ds = conector.RunProcedure(sSQL, paramArray, "tblcPersona");
                tbl = ds.Tables[0];
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        cPersona.Add(new cPersona
                        {
                            PNA_DS_NOMBRE = item["PNA_DS_NOMBRE"].ToString().Trim(),
                            PNA_FL_PERSONA = int.Parse(item["PNA_FL_PERSONA"].ToString().Trim()),
                            PNA_CL_RFC = item["PNA_CL_RFC"].ToString().Trim(),
                            PNA_DS_EMAIL = item["PNA_DS_EMAIL"].ToString().Trim(),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mod.Log_Diario("Clase HomeController Metodo recuperaPersona Error", ex.Message);
                string sEx = ex.Message;
            }

            return cPersona;
        }

        public List<cPersona> recuperaPersonaEmail(string email, string status)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<cPersona> cPersona = new List<cPersona>();
            string sSQL = "";
            SqlParameter[] paramArray;
            DataSet ds = new DataSet();

            try
            {
                // Contratos PAGOS INICIALES
                //if (bPagosIniciales)
                //{
                sSQL = "sp_Obtener_cPersona";
                paramArray = new SqlParameter[2];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@PNA_DS_EMAIL", email, "");
                paramArray[1] = BaseDatos.BDatos.NewParameter("@PNA_FG_STATUS", status, "");
                ds = conector.RunProcedure(sSQL, paramArray, "tblcPersona");
                tbl = ds.Tables[0];

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        cPersona.Add(new cPersona
                        {
                            //Contrato = item["cto_fl_cve"].ToString().Trim(),
                            //MontoAPagar = decimal.Parse(item["MONTOAPAGAR"].ToString()),
                            //Moneda = item["MONEDA"].ToString().Trim(),
                            //FechaPago = "De inmediato",
                            //Domiciliado = item["Domiciliado"].ToString().Trim(),
                            //Beneficiario = item["Beneficiario"].ToString().ToUpper().Trim(),
                            //PagoCuentaBanamex = item["PagoACuentaBanamex"].ToString().Trim(),
                         //PagoCuentaBBVA = item["PagoACuentaBBVA"].ToString().Trim(),
                            //TransferenciaBBVA = item["TransferenciaBBVA"].ToString().Trim(),
                            //Referencia = item["Referencia"].ToString().Trim(),
                            //Status_Contrato = "PAGOS INICIALES",
                            //Fechafinmov = item["Fechafinmov"].ToString().Trim()

                            PNA_DS_NOMBRE = item["PNA_DS_NOMBRE"].ToString().Trim(),
                            PNA_FL_PERSONA = int.Parse(item["PNA_FL_PERSONA"].ToString().Trim()),
                            PNA_CL_RFC = item["PNA_CL_RFC"].ToString().Trim(),
                            PNA_DS_EMAIL = item["PNA_DS_EMAIL"].ToString().Trim(),
                        });
                    }
                }



            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("RecuperaDatos(Ex)", "Email: " + email + "\nError: " + sEx);
            }

            return cPersona;
        }

        #endregion



        #region "Descarga Masiva de estados de cuenta de contratos"

        public ActionResult DLAccountStatements()
        {
            ViewBag.Name = Session["Name"].ToString().Trim();
            ViewBag.Fecha = DateTime.Now.ToString("yyyy-MM-dd");

            return View();
        }

        #endregion

        #region "Error" 

        public ActionResult Error(string psError)
        {
            ViewBag.Exception = psError;

            return View();
        }


        public List<PopupMessage> ObtenerMessPopup(string vista, ref string msg)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<PopupMessage> listPopupMessage = new List<PopupMessage>();

            try
            {
                // Contratos PAGOS INICIALES
                string sSQL = "Sp_PopMessage";
                SqlParameter[] paramArray = new SqlParameter[1];

                paramArray[0] = BaseDatos.BDatos.NewParameter("@Vista", vista, "");
                DataSet ds = conector.RunProcedure(sSQL, paramArray, "tblPopMessage");
                tbl = ds.Tables[0];

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        listPopupMessage.Add(new PopupMessage
                        {
                            ID = int.Parse(item["Id"].ToString()),
                            Url = item["Url"].ToString(),
                            Texto = item["Texto"].ToString(),
                            Texto2 = item["Texto2"].ToString(),
                            Vista = item["Vista"].ToString()
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("RecuperaDatos(Ex)", "Cve: " + vista + "\nError: " + sEx);
            }
            return listPopupMessage;
        }

        [HttpGet]
        public JsonResult GetPopMessages(int pPaisID)
        {

            PopMessageResponse response = new PopMessageResponse();
            string msg = "";
            response.lstMessages = ObtenerMessPopup("ClientFiles", ref msg); ;
            response.Message = msg;
            response.Success = true;

            return Json(response);
        }

        public ActionResult SeleccionarPopMessage(string psAction)
        {
            bool bSuccess = true;

            PopMessageResponse response = new PopMessageResponse();
            string msg = "";
            response.lstMessages = ObtenerMessPopup(psAction, ref msg);
            response.Message = msg;
            response.Success = true;
            response.cveCliente = Session["cveUsuarioLog"].ToString();

            return Json(new { success = bSuccess, lstPop = response.lstMessages , cCliente = response.cveCliente }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RegistraBitacoraPM(string psVista)
        {
            bool bSuccess = true;
            string sResult = "";
            try
            {
                this.RegistraBitacora_PopupMeesage(psVista, Session["correo"].ToString(), Session["cveUsuario"].ToString());
                bSuccess = true;
                sResult = "Bitacora Registrada correctamente";
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("RegistraBitacora_PopupMeesage(Ex)", "Cve: " + Session["cveCliente"].ToString() + "\nError: " + sEx);
                bSuccess = false;
                sResult = sEx;
            }

            object Respuesta = new
            {
                success = bSuccess,
                result = sResult
            };

            return Json(Respuesta, JsonRequestBehavior.AllowGet);
        }

        private int RegistraBitacora_PopupMeesage(string vista, string email, string cveCliente)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            int iIdSolicitud = 0;

            try
            {
                string sSQL = "Sp_Add_Bitacora_PopupMeesage";
                SqlParameter[] paramArray = new SqlParameter[3];

                int afecto = 0;
                // Registramos la solicitud
                paramArray[0] = BaseDatos.BDatos.NewParameter("@Vista", vista, "");
                paramArray[1] = BaseDatos.BDatos.NewParameter("@UsuarioMail", email, "");
                paramArray[2] = BaseDatos.BDatos.NewParameter("@CveCliente", cveCliente, "");

                iIdSolicitud = conector.RunProcedure(sSQL, paramArray, out afecto);
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("RegistraBitacora_PopupMeesage(Ex)", "Cve: " + cveCliente + "\nError: " + sEx + "\nEmail: " + email + "\nVista: " + vista);
            }

            return iIdSolicitud;
        }


        #endregion

    }
}