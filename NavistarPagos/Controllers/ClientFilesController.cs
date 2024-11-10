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
    public class ClientFilesController : Controller
    {
        Validaciones mod = new Validaciones();
        cPersona Cliente = new cPersona();
        private sfiinternationalEntities Entity = new sfiinternationalEntities();

        
        public ActionResult Index(string ntoken)
        {
               
                return RedirectToAction("Index","Home", new { ntoken = ntoken , psAction = "ClientFiles" });
            
        }

        
        public ActionResult ClientFiles()
        {
            string msg = "";
            int cveCliente = 0;

            try
            {
                HomeController _HomeController = new HomeController();

                if (Session["cveCliente"] == null) return RedirectToAction("Error", "Home");
                cveCliente = int.Parse(Session["cveCliente"].ToString());

                ViewBag.Name = Session["Name"].ToString().Trim();
                ViewBag.CveCliente = cveCliente.ToString();
                ViewBag.RFC = Session["rfc"].ToString().Trim();
                ViewBag.Correo = Session["correo"].ToString().Trim();
                ViewBag.Contracts = _HomeController.RecuperaDatos1(cveCliente, ref msg);
                List<PopupMessage> lstMessage = ObtenerMessagePopup("ClientFilesPage");
                ViewBag.lstBanner = lstMessage.First().Texto;
                
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

        public List<PopupMessage> ObtenerMessagePopup(string vista)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<PopupMessage> listPopupMessage = new List<PopupMessage>();
            try
            {   
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