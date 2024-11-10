using NavistarPagos.Data;
using NavistarPagos.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NavistarPagos.Controllers
{
    public class ClientPaperlessController : Controller
    {
        Validaciones mod = new Validaciones();

        // GET: ClientPaperless
        public ActionResult Index(string ntoken)
        {
            return RedirectToAction("Index", "Home", new { ntoken = ntoken, psAction = "ClientPaperless" });
        }

        public ActionResult ClientPaperless()
        {
            string msg = "";
            int cveCliente = 0;

            try
            {
                HomeController _HomeController = new HomeController();

                if (Session["cveCliente"] == null) return RedirectToAction("Error", "Home");
                cveCliente = int.Parse(Session["cveCliente"].ToString());

                ViewBag.Action = "ClientPaperless";
                ViewBag.Name = Session["Name"].ToString().Trim();
                ViewBag.CveCliente = cveCliente.ToString();
                ViewBag.RFC = Session["rfc"].ToString().Trim();
                ViewBag.Correo = Session["correo"].ToString().Trim();
                ViewBag.Contracts = _HomeController.RecuperaDatos(cveCliente, ref msg, false);

                return View();
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("ClientUpdate(Ex)", "Cve: " + cveCliente + "\nError: " + sEx);
                return RedirectToAction("Error", "Home", new { psError = "ClientFiles" });
            }            
        }

        [HttpPost]
        public JsonResult RegistraContratoPaperless(int piCveCliente, string psCorreo, string psContratos)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            List<ContratoPaperless> contratos = JsonConvert.DeserializeObject<List<ContratoPaperless>>(psContratos);
            bool bSuccess = true;
            string sResult = "";
            int afecto = 0;

            try
            {
                string sSQL = "";
                int iPaperless = 0;
                string sContrato = "";

                foreach (ContratoPaperless contrato in contratos)
                {                    
                    try
                    {
                        sSQL = "sp_Insertar_Consentimiento";
                        SqlParameter[] paramArray = new SqlParameter[4];
                        iPaperless = contrato.Paperless;
                        sContrato = contrato.Contrato;

                        paramArray[0] = BaseDatos.BDatos.NewParameter("@peClaveCliente", piCveCliente, "");
                        paramArray[1] = BaseDatos.BDatos.NewParameter("@pvCorreo", psCorreo, "");
                        paramArray[2] = BaseDatos.BDatos.NewParameter("@piPaperless", iPaperless, "");
                        paramArray[3] = BaseDatos.BDatos.NewParameter("@psContrato", sContrato, "");

                        conector.RunProcedure(sSQL, paramArray, out afecto);
                        if (afecto <= 0)
                        {
                            if (sResult != "") { sResult += ","; }
                            sResult += sContrato;
                        }
                    }
                    catch (Exception ex)
                    {
                        string sEx = ex.Message;

                        mod.Log_Diario("RegistraContratoPaperless(Ex)", "Error: " + sEx + "\nCve: " + piCveCliente + "\nCorreo: " + psCorreo + "\nContrato: " + sContrato + "\nPaperless: " + iPaperless);
                    }                    
                }                
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("RegistraContratoPaperless(Ex)", "Cve: " + piCveCliente + "\nError: " + sEx + "\nCorreo: " + psCorreo);
            }

            object Respuesta = new
            {
                success = bSuccess,
                result = sResult
            };

            return Json(Respuesta, JsonRequestBehavior.AllowGet);
        }

    }
}