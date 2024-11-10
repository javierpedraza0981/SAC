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
    public class ClientConsController : Controller
    {
        Validaciones mod = new Validaciones();
        cPersona Cliente = new cPersona();
        private sfiinternationalEntities Entity = new sfiinternationalEntities();

        public ActionResult Index(string ntoken)
        {

            return RedirectToAction("Index", "Home", new { ntoken = ntoken, psAction = "ClientCons" });

        }




        public ActionResult ClientCons()
        {
            string msg = "";
            int cveCliente = 0;

            try
            {
                if (Session["cveCliente"] == null) return RedirectToAction("Error", "Home");
                cveCliente = int.Parse(Session["cveCliente"].ToString());

                ViewBag.Name = Session["Name"].ToString().Trim();

                if (msg != "")
                {
                    mod.Log_Diario("ClientCons()", "Cve: " + cveCliente + "\nError: " + msg);
                    return RedirectToAction("Error", "Home", new { psError = msg });
                }

                return View();
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("ClientCons(Ex)", "Cve: " + cveCliente + "\nError: " + sEx);
                return RedirectToAction("Error", "Home", new { psError = "ClientFiles" });
            }
        }

        [HttpPost]
        public JsonResult RegistraBitacoraPM(string psVista)
        {
            bool bSuccess = true;
            string sResult = "";
            try
            {
                this.RegistraBitacora_PopupMeesage(psVista, Session["correo"].ToString(), Session["cveCliente"].ToString());
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
            DataTable tbl = new DataTable();

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

        public ActionResult SeleccionarPopMessage(string cve, string psAction)
        {
            bool bSuccess = true;

            PopMessageResponse response = new PopMessageResponse();
            string msg = "";
            response.lstMessages = ObtenerMessPopup("ClientFiles", ref msg);
            response.Message = msg;
            response.Success = true;

            return Json(new { success = bSuccess, lstPop = response.lstMessages }, JsonRequestBehavior.AllowGet);
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
    }
}