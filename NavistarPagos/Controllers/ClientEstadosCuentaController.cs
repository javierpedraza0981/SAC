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
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.IO;
using System.IO.Compression;
using NavistarPagos.Servicios;


namespace NavistarPagos.Controllers
{
    public class ClientEstadosCuentaController : Controller
    {
        Validaciones mod = new Validaciones();
        cPersona Cliente = new cPersona();
        private sfiinternationalEntities Entity = new sfiinternationalEntities();

        public ActionResult Index(string ntoken)
        {
            mod.Log_Diario("ClientEstadosCuentaController", "Inicio Index");
            return RedirectToAction("Index", "Home", new { ntoken = ntoken, psAction = "ClientEstadosCuenta" });

        }


        public ActionResult ClientEstadosCuenta()
        {
            string msg = "";
            int cveCliente = int.Parse(Session["cveCliente"].ToString());
            ViewBag.Action = "ClientEstadosCuenta";
            try
            {
                if (Session["cveCliente"] == null) return RedirectToAction("Error", "Home");
                cveCliente = int.Parse(Session["cveCliente"].ToString());

                ViewBag.Name = Session["Name"].ToString().Trim();
                ViewBag.Meses = ObtenerMesUltimos6Meses();
                ViewBag.Contratos = ObtenerContratos(cveCliente);

                if (msg != "")
                {
                    mod.Log_Diario("ClientEstadosCuenta()", "Cve: " + cveCliente + "\nError: " + msg);
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

        public List<MesesC> ObtenerMesUltimos6Meses()
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<MesesC> listMesesC = new List<MesesC>();

            try
            {
                // Contratos PAGOS INICIALES
                string sSQL = "Sp_GetLastMeses6";
                SqlParameter[] paramArray = new SqlParameter[1];

                DataSet ds = conector.RunProcedure(sSQL, paramArray, "1");
                tbl = ds.Tables[0];

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        listMesesC.Add(new MesesC
                        {
                            AnioMes = int.Parse(item["AnioMes"].ToString()),
                            Mes = item["Mes"].ToString()
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("ObtenerMesUltimos6Meses(Ex)", "Cve: " + "" + "\nError: " + sEx);
            }
            return listMesesC;
        }


        public ActionResult SeleccionarContratos(int cve, string psAction)
        {
            bool bSuccess = true;

            return Json(new { success = bSuccess }, JsonRequestBehavior.AllowGet);
        }


        public List<string> _getEmpresaMeses(string contratosFechas)
        {
            int cveCliente = int.Parse(Session["cveCliente"].ToString());
            List<string> listP = contratosFechas.Split('|').ToList();
            List<string> _listaParametros = new List<string>();
            List<MesesC> lstMeses = ObtenerMesUltimos6Meses();
            if (listP[0] == "190001")
                _listaParametros = lstMeses.Where(x => x.Mes != "Seleccionar Mes").ToList().Where(y => y.Mes != "Todos").ToList().Select(z =>
               {
                   return "M" + z.AnioMes;
               })
                .ToList();
            else
                _listaParametros = lstMeses.Where(x => x.AnioMes == int.Parse(listP[0])).ToList().Select(z =>
                {
                    return "M" + z.AnioMes;
                })
                .ToList();



            List<MesesC> lstContratosC = ObtenerContratos(cveCliente);
            if (listP[1] == "Todos")
                _listaParametros.AddRange(lstContratosC.Where(x => x.ePersona != 1).ToList().Where(y => y.ePersona != -2).ToList().Select(z =>
                {
                    return z.vContrato;
                })
               .ToList());
            else
                _listaParametros.AddRange(lstContratosC.Where(x => x.vContrato == listP[1]).ToList().Select(z =>
               {
                   return z.vContrato;
               })
              .ToList());


            return _listaParametros;
        }

        private List<FileBase64Result> _IntegracionResumenesContrato(List<FileBase64Result> results)
        {
            string fechaInicioMovimientos = DateTime.Now.Date.AddDays(-28).Date.ToString("yyyy/MM/dd");
            string fechaFinalMovimientos = DateTime.Now.Date.ToString("yyyy/MM/dd");

            mod.Log_Diario("Inicia _IntegracionResumenesContrato", "_IntegracionResumenesContrato");
            string _carpetaMesEnCurso = @"M" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00");
            //List<FileBase64Result> listaCarpetaMes = results.Where(r => r._Carpeta == _carpetaMesEnCurso).ToList();
            mod.Log_Diario("Obtiene _carpetaMesEnero previo a foreach", "_IntegracionResumenesContrato");
            mod.Log_Diario("Previo a foreach", "_IntegracionResumenesContrato");
          
            foreach (FileBase64Result _FileBase64Result in results)
            {
                mod.Log_Diario("Ingresa a foreach", "_IntegracionResumenesContrato");
                mod.Log_Diario("Recorre foreach", _FileBase64Result._Carpeta + "  -  " + _carpetaMesEnCurso);
                //Integramos resumenes de contratos solo mes en curso
                if (_FileBase64Result._Carpeta == _carpetaMesEnCurso)
                {
                    mod.Log_Diario("Ingresa a iF", "_IntegracionResumenesContrato");
                    //Inicia proceso de consulta a servicio web estados de cuenta
                    _FileBase64Result.ContenidoBase64 = _ObtieneResumenContratoBase64(_FileBase64Result.Contrato);
                    //results.Remove()
                }
                mod.Log_Diario("Sale  iF", "_IntegracionResumenesContrato");
            }

            return results;
        }

        private string _ObtieneResumenContratoBase64(string _Contrato)
        {
            mod.Log_Diario("inicia _ObtieneResumenContratoBase64", "_ObtieneResumenContratoBase64");
            //Obtener pdf mes actual
            EstadosCuentaService estadosCuentaService = new EstadosCuentaService();
            mod.Log_Diario("inicia _RecuperaEmpresaPorContrato", "_ObtieneResumenContratoBase64");
            int _Empresa = _RecuperaEmpresaPorContrato(_Contrato);
            mod.Log_Diario("obtiene emprsa contrato servicio wsdl", "_Empresa " + _Empresa.ToString());
            List<string> resultd = estadosCuentaService.ObtenerResultado(_Contrato, _Empresa);
            mod.Log_Diario("lista de 5 items servicio wsdl", "resultd" + resultd.Count.ToString());
            if (resultd.Count == 0)
            {
                //mod.Log_Diario("*Mes Actual Correcto", "_Contrato: " + _Contrato + "_Empresa: " + _Empresa);
                resultd.Add("SinPDFEstadoCuenta");
                resultd.Add("Error");
            }
            else 
            {
                mod.Log_Diario("sin pdf servicio wsdl", "resultd" + resultd.Count.ToString());
            }
            mod.Log_Diario("revisamos base 64", "resultd" + resultd[1].ToString());
            //Solo se regresa la bse 64 del archivo (resumen de contrato del contrato en turno ) para reeemplazarlo por la fotografia de este contrato en el mes en curso            
            return resultd[1];
            //La posicion 1 corresponde al base 64 que nos da el servicio estados de cuenta
        }

        public int _RecuperaEmpresaPorContrato(string _Contrato)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<ResumenContratos> contratos = new List<ResumenContratos>();
            string sSQL = "";
            SqlParameter[] paramArray;
            DataSet ds = new DataSet();
            int _Empresa = 0;

            try
            {
                // Contratos PAGOS INICIALES

                sSQL = "spObtenerEmpresaPorContrato";
                paramArray = new SqlParameter[1];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@CTO_FL_CVE", _Contrato, "");
                ds = conector.RunProcedure(sSQL, paramArray, "tblEmpresa");
                tbl = ds.Tables[0];

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        _Empresa = int.Parse(item["Empresa"].ToString().Trim());
                    }
                }


            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("_RecuperaEmpresaPorContrato(Ex)", "_Contrato: " + _Contrato + " _Empresa: " + _Empresa.ToString() + "\nError: " + sEx);
            }

            return _Empresa;
        }


        [HttpPost]
        public async Task<ActionResult> DescargarPDFDesdeAPI(string contratosFechas)
        {
            try
            {
                mod.Log_Diario("Ingresa a DescargarPDFDesdeAPI", "368");
                string rutaCarpetaZip = "";
                string nombreArchivoZip = "";
                mod.Log_Diario("contratosFechas", contratosFechas);
                List<string> listaParametros = _getEmpresaMeses(contratosFechas);
                mod.Log_Diario("Ingresa a DescargarPDFDesdeAPI", "372");

                // URL del método en el servidor
                //string apiUrl = "http://localhost:5041/download-multiple";
                string apiUrl = System.Configuration.ConfigurationManager.AppSettings["apiUrl"].ToString();
                mod.Log_Diario("apiUrl a donde apunta", apiUrl);


                using (HttpClient client = new HttpClient())
                {
                    mod.Log_Diario("Ingresa ", "Ingresa posterior a apiUrl");
                    // Convierte la lista de nombres de archivos a JSON y crea el contenido de la solicitud
                    string jsonContent = JsonConvert.SerializeObject(listaParametros);
                    mod.Log_Diario("Ingresa ", listaParametros.Count.ToString());

                    StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    
                    mod.Log_Diario("content ", content.ToString());

                    // Realiza la solicitud HTTP POST
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    mod.Log_Diario("Ingresa a servicio 394", "DescargarPDFDesdeAPI's");
                    // Maneja la respuesta
                    if (response.IsSuccessStatusCode)
                    {
                        mod.Log_Diario("Recibe respuesta de servicio", "DescargarPDFDesdeAPI");
                        // Lee el contenido de la respuesta en formato JSON
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        mod.Log_Diario("Asigna a jsonResponse", "DescargarPDFDesdeAPI");
                        // Convierte el JSON de la respuesta a objetos C#
                        List<FileBase64Result> results = JsonConvert.DeserializeObject<List<FileBase64Result>>(jsonResponse);
                        mod.Log_Diario("Asigna results", "DescargarPDFDesdeAPI");
                        //Pasamos results para iniciar proceso de resumenes de contratos
                        mod.Log_Diario("Results", results.Count.ToString());
                        
                        mod.Log_Diario("ResultsValue", results.Count>0 ? results[0]._Carpeta : "No aplica" );
                        mod.Log_Diario("ResultsValue", results.Count > 0 ? results[0].Nombre : "No aplica");
                        mod.Log_Diario("ResultsValue", results.Count > 0 ? results[0].Contrato : "No aplica");

                        results = _IntegracionResumenesContrato(results);

                        mod.Log_Diario("Sale  _IntegracionResumenesContrato", "_IntegracionResumenesContrato");
                        Dictionary<string, List<FileBase64Result>> lstPorMes = agrupaMes(results);

                        rutaCarpetaZip = System.Configuration.ConfigurationManager.AppSettings["zipFilePath"].ToString();
                        nombreArchivoZip = String.Format("EdoCtaZipArchivo_{0:yyyyMMdd_HHmmss}.zip", DateTime.Now);

                        //// Verifica si la carpeta existe, si no, la crea
                        if (!Directory.Exists(rutaCarpetaZip))
                        {
                            Directory.CreateDirectory(rutaCarpetaZip);
                        }

                        string zipFilePath = rutaCarpetaZip + nombreArchivoZip;

                        ConvertBase64ToFilesAndZip_2(lstPorMes, zipFilePath);

                        return File(zipFilePath, "application/zip", nombreArchivoZip);
                    }
                    else
                    {

                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)-ln427", "Cve: " + "" + "\nError: " + sEx  + "- StackTrace - " + ex.StackTrace + " - InnerException - " + ex.InnerException);
                return View("Error");
            }

        }

        public Dictionary<string, List<FileBase64Result>> agrupaMes(List<FileBase64Result> results)
        {
            mod.Log_Diario("ResultsValue", results.Count > 0 ? results[0]._Carpeta : "No aplica");
            mod.Log_Diario("ResultsValue", results.Count > 0 ? results[0].Nombre : "No aplica");
            mod.Log_Diario("ResultsValue", results.Count > 0 ? results[0].Contrato : "No aplica");
            mod.Log_Diario("ResultsValue", results.Count > 0 ? results[0].RutaBase64 : "No aplica");



            Dictionary<string, List<FileBase64Result>> diccionarioResultados = new Dictionary<string, List<FileBase64Result>>();
            var resultadosAgrupados = results.GroupBy(r => r._Carpeta);

            // Iterar sobre los grupos
            foreach (var grupo in resultadosAgrupados)
            {
                // Agregar la clave (nombre de la carpeta) y la lista de resultados al diccionario
                diccionarioResultados.Add(grupo.Key, grupo.ToList());
                mod.Log_Diario("ResultsKey", grupo.Key);
            }

            return diccionarioResultados;

        }

        public void ConvertBase64ToFilesAndZip_2(Dictionary<string, List<FileBase64Result>> base64Files, string zipFilePath)
        {


            try
            {
                // Crear una carpeta temporal para almacenar los archivos originales
                string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);

                // Agrupar los archivos PDF por mes
                var archivosPorMes = base64Files.GroupBy(entry => entry.Key.Substring(0, 7)); // Suponiendo que el formato de la clave es "AAAA-MM"

                foreach (var mesGroup in archivosPorMes)
                {
                    string mesFolder = Path.Combine(tempFolder, mesGroup.Key);
                    Directory.CreateDirectory(mesFolder);

                    // Agrupar los archivos PDF por contrato dentro del mes
                    var archivosPorContrato = mesGroup.SelectMany(entry => entry.Value).GroupBy(fileResult => fileResult.Contrato);

                    foreach (var contratoGroup in archivosPorContrato)
                    {
                        string contratoFolder = Path.Combine(mesFolder, contratoGroup.Key);
                        Directory.CreateDirectory(contratoFolder);

                        // Guardar cada archivo PDF correspondiente al contrato
                        foreach (var fileResult in contratoGroup)
                        {
                            // Suponemos que los archivos PDF tienen una extensión .pdf
                            if (fileResult.Nombre.EndsWith(".pdf"))
                            {
                                // Convierte la cadena base64 a bytes
                                byte[] fileBytes = Convert.FromBase64String(fileResult.ContenidoBase64);

                                // Guarda los bytes en un archivo dentro de la carpeta del contrato
                                string tempFilePath = Path.Combine(contratoFolder, fileResult.Nombre);
                                System.IO.File.WriteAllBytes(tempFilePath, fileBytes);
                            }
                        }
                    }
                }

                // Crea el archivo zip y agrega los archivos temporales
                ZipFile.CreateFromDirectory(tempFolder, zipFilePath);

                Console.WriteLine($"Archivos guardados en el archivo zip: {zipFilePath}");

                // Elimina la carpeta temporal
                Directory.Delete(tempFolder, true);
            }

            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)-ln505", "Cve: " + "" + "\nError: " + sEx);
            }
        }

        public void ConvertBase64ToFilesAndZip_3(Dictionary<string, List<FileBase64Result>> base64Files, string zipFilePath)
        {
            try
            {
                // Crear una carpeta temporal para almacenar los archivos originales
                string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);

                foreach (var entry in base64Files)
                {
                    string subfolderPath = Path.Combine(tempFolder, entry.Key);
                    Directory.CreateDirectory(subfolderPath);

                    foreach (var fileResult in entry.Value)
                    {
                        // Convierte la cadena base64 a bytes
                        byte[] fileBytes = Convert.FromBase64String(fileResult.ContenidoBase64);

                        // Guarda los bytes en un archivo dentro de la subcarpeta
                        string tempFilePath = Path.Combine(subfolderPath, fileResult.Nombre);
                        System.IO.File.WriteAllBytes(tempFilePath, fileBytes);
                    }
                }

                // Crea el archivo zip y agrega los archivos temporales
                ZipFile.CreateFromDirectory(tempFolder, zipFilePath);

                Console.WriteLine($"Archivos guardados en el archivo zip: {zipFilePath}");

                // Elimina la carpeta temporal
                Directory.Delete(tempFolder, true);
            }

            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)-ln545", "Cve: " + "" + "\nError: " + sEx);
            }
        }

        public void ConvertBase64ToFilesAndZip(Dictionary<string, string> base64Files, string zipFilePath)
        {
            try
            {
                // Crear una carpeta temporal para almacenar los archivos originales
                string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);

                foreach (var entry in base64Files)
                {
                    // Convierte la cadena base64 a bytes
                    byte[] fileBytes = Convert.FromBase64String(entry.Value);

                    // Guarda los bytes en un archivo temporal
                    string tempFilePath = Path.Combine(tempFolder, entry.Key);
                    System.IO.File.WriteAllBytes(tempFilePath, fileBytes);
                }

                // Crea el archivo zip y agrega los archivos temporales
                ZipFile.CreateFromDirectory(tempFolder, zipFilePath);

                Console.WriteLine($"Archivos guardados en el archivo zip: {zipFilePath}");

                // Elimina la carpeta temporal
                Directory.Delete(tempFolder, true);
            }

            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)-ln579", "Cve: " + "" + "\nError: " + sEx);
            }
        }

        public List<MesesC> ObtenerContratos(int ePersona)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<MesesC> listContratosC = new List<MesesC>();

            try
            {
                // Contratos PAGOS INICIALES
                string sSQL = "SP_GetContratos_DescargaSeisMeses";
                SqlParameter[] paramArray = new SqlParameter[1];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@EPERSONA", ePersona, "");

                DataSet ds = conector.RunProcedure(sSQL, paramArray, "1");
                tbl = ds.Tables[0];

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        listContratosC.Add(new MesesC
                        {
                            ePersona = int.Parse(item["ePersona"].ToString()),
                            vContrato = item["vContrato"].ToString()
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)-ln616", "Cve: " + "" + "\nError: " + sEx);
            }
            return listContratosC;
        }


        public string obtenerEmpresaPorContrato(string _Contrato)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<ResumenContratos> contratos = new List<ResumenContratos>();
            string sSQL = "";
            SqlParameter[] paramArray;
            DataSet ds = new DataSet();
            string _Empresa = "";

            try
            {

                sSQL = "spObtenerEmpresaPorContrato";
                paramArray = new SqlParameter[1];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@CTO_FL_CVE", _Contrato, "");
                ds = conector.RunProcedure(sSQL, paramArray, "tblEmpresa");
                tbl = ds.Tables[0];

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    _Empresa = tbl.Rows[0].ItemArray[0].ToString();
                }

            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("obtenerEmpresaPorContrato(Ex)", "\nError: " + sEx);
            }

            return _Empresa;
        }

    }


}