using NavistarPagos.Data;
using NavistarPagos.Entity;
using NavistarPagos.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace NavistarPagos.Controllers
{
    public class ClientPolizasController : Controller
    {

        bool _varPruebas = System.Configuration.ConfigurationManager.AppSettings["ambientePP"].ToString() == "1" ? true : false;

        // GET: ClientPolizas
        public ActionResult Index(string ntoken)
        {
            return RedirectToAction("Index", "Home", new { ntoken = ntoken, psAction = "ClientPolizas" });
        }

        Validaciones mod = new Validaciones();
        cPersona Cliente = new cPersona();
        private sfiinternationalEntities Entity = new sfiinternationalEntities();

        public ActionResult ClientPolizas()
        {
            string msg = "";
            int cveCliente = int.Parse(Session["cveCliente"].ToString());
            ViewBag.Action = "ClientPolizas";
            try
            {
                if (Session["cveCliente"] == null) return RedirectToAction("Error", "Home");
                mod.Log_Diario("Imprime session -- ", "Cve: " + Session["cveCliente"] );
                cveCliente = int.Parse(Session["cveCliente"].ToString());
                
                ViewBag.Name = Session["Name"].ToString().Trim();

                var jsonResult = ObtenerNumerosSeriesPorContrato("", false) as JsonResult;
                dynamic data = jsonResult.Data;
                if (data.success)
                {
                    ViewBag.NumerosDeSerie = data.result;
                }




                ViewBag.Contratos = ObtenerContratosPolizas(cveCliente);

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
            //response.cveCliente=cve

            return Json(new { success = bSuccess, lstPop = response.lstMessages, cveClient = response.cveCliente }, JsonRequestBehavior.AllowGet);
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


        public List<string> _getContratosMeses(string contratosFechas)
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



            List<PolizasC> lstContratosC = ObtenerContratosPolizas(cveCliente);

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

        [HttpPost]
        public async Task<ActionResult> DescargarPDFDesdeAPI(string contratosFechas)
        {
            try
            {

                //ViewBag.tokenN = JsonToken<>;
                // Lista de nombres de archivos
                //List<string> listaParametros = new List<string> { "638094701099190053.PNG", "638397972344126676.pdf", "638397972330778348.pdf" };
                string rutaCarpetaZip = "";
                string nombreArchivoZip = "";
                List<string> listaParametros = _getContratosMeses(contratosFechas);

                // URL del método en el servidor
                //string apiUrl = "http://localhost:5041/download-multiple";
                string apiUrl = System.Configuration.ConfigurationManager.AppSettings["apiUrl"].ToString();

                using (HttpClient client = new HttpClient())
                {
                    // Convierte la lista de nombres de archivos a JSON y crea el contenido de la solicitud
                    string jsonContent = JsonConvert.SerializeObject(listaParametros);
                    StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Realiza la solicitud HTTP POST
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Maneja la respuesta
                    if (response.IsSuccessStatusCode)
                    {
                        // Lee el contenido de la respuesta en formato JSON
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // Convierte el JSON de la respuesta a objetos C#
                        List<FileBase64Result> results = JsonConvert.DeserializeObject<List<FileBase64Result>>(jsonResponse);

                        Dictionary<string, List<FileBase64Result>> lstPorMes = agrupaMes(results);
                        //*****************************
                        Dictionary<string, string> base64Files = new Dictionary<string, string>();

                        // Agrega cada resultado al Dictionary
                        foreach (var result in results)
                        {
                            base64Files.Add(result.Nombre, result.ContenidoBase64);
                        }

                        rutaCarpetaZip = System.Configuration.ConfigurationManager.AppSettings["zipFilePath"].ToString();
                        nombreArchivoZip = String.Format("EdoCtaZipArchivo_{0:yyyyMMdd_HHmmss}.zip", DateTime.Now);

                        //// Verifica si la carpeta existe, si no, la crea
                        if (!Directory.Exists(rutaCarpetaZip))
                        {
                            Directory.CreateDirectory(rutaCarpetaZip);
                        }
                        //string zipFilePath = "C:\\Ruta\\EdoCtaZipArchivo.zip";
                        string zipFilePath = rutaCarpetaZip + nombreArchivoZip;

                        //ConvertBase64ToFilesAndZip(base64Files, zipFilePath);
                        ConvertBase64ToFilesAndZip_2(lstPorMes, zipFilePath);
                        //*****************************


                        // Haz algo con los resultados, por ejemplo, mostrarlos en la vista
                        //return View(results);
                        //return File(pdfBytes, "application/pdf", "638397972344126676" + ".pdf");
                        return File(zipFilePath, "application/zip", nombreArchivoZip);
                    }
                    else
                    {
                        // Maneja el caso en que la solicitud no fue exitosa
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)", "Cve: " + "" + "\nError: " + sEx);
                return View("Error");
            }

        }

        [HttpGet]
        public JsonResult ResumenDescargarPolizaPDF(string contratosSerie)
        {
            bool respuesta = false;
            List<PolizasExists> listaPolizas = new List<PolizasExists>();
            try
            {
                listaPolizas = ResumenObtenerPolizasPorContratoySerie(contratosSerie);
                respuesta = true;
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)", "Cve: " + "" + "\nError: " + sEx);
            }

            object Respuesta = new
            {
                success = respuesta,
                result = listaPolizas
            };

            return Json(Respuesta, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DescargarPolizaPDF(string contratosSerie)
        {
            try
            {
                List<string> listaPolizas = ObtenerPolizasPorContratoySerie(contratosSerie);

                string rutaBase = System.Configuration.ConfigurationManager.AppSettings["rutaBasePolizas"].ToString();

                using (MemoryStream ms = new MemoryStream())
                {
                    using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        foreach (string poliza in listaPolizas)
                        {
                            string rutaCarpetaArchivo = Path.Combine(rutaBase, poliza);

                            if (!Directory.Exists(rutaCarpetaArchivo))
                            {
                                //throw new FileNotFoundException("Error: La carpeta con el nombre del archivo no existe ");
                                continue; // Si la carpeta no existe, pasa a la siguiente.
                            }

                            string[] archivosPDF = Directory.GetFiles(rutaCarpetaArchivo, "*.pdf");

                            if (archivosPDF.Length == 0)
                            {
                                //throw new FileNotFoundException("Error: No se encontraron archivos PDF dentro de la carpeta");
                                continue; // Si no hay archivos PDF, pasa a la siguiente carpeta.
                            }

                            foreach (string archivoPDF in archivosPDF)
                            {
                                byte[] contenidoArchivo = System.IO.File.ReadAllBytes(archivoPDF);
                                string nombreArchivoPDF = Path.GetFileName(archivoPDF);
                                ZipArchiveEntry entry = zip.CreateEntry($"{poliza}/{nombreArchivoPDF}");

                                using (Stream zipStream = entry.Open())
                                {
                                    zipStream.Write(contenidoArchivo, 0, contenidoArchivo.Length);
                                }
                            }
                        }
                    }

                    return File(ms.ToArray(), "application/zip", "archivos.zip");
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)", "Cve: " + "" + "\nError: " + sEx);
                return View("Error");
            }
        }


        public Dictionary<string, List<FileBase64Result>> agrupaMes(List<FileBase64Result> results)
        {

            Dictionary<string, List<FileBase64Result>> diccionarioResultados = new Dictionary<string, List<FileBase64Result>>();
            var resultadosAgrupados = results.GroupBy(r => r._Carpeta);

            // Iterar sobre los grupos
            foreach (var grupo in resultadosAgrupados)
            {
                // Agregar la clave (nombre de la carpeta) y la lista de resultados al diccionario
                diccionarioResultados.Add(grupo.Key, grupo.ToList());
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
                //// Crear una carpeta temporal para almacenar los archivos originales
                //string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                //Directory.CreateDirectory(tempFolder);

                //foreach (var entry in base64Files)
                //{
                //    string subfolderPath = Path.Combine(tempFolder, entry.Key);
                //    Directory.CreateDirectory(subfolderPath);

                //    foreach (var fileResult in entry.Value)
                //    {
                //        // Convierte la cadena base64 a bytes
                //        byte[] fileBytes = Convert.FromBase64String(fileResult.ContenidoBase64);

                //        // Guarda los bytes en un archivo dentro de la subcarpeta
                //        string tempFilePath = Path.Combine(subfolderPath, fileResult.Nombre);
                //        System.IO.File.WriteAllBytes(tempFilePath, fileBytes);
                //    }
                //}

                //// Crea el archivo zip y agrega los archivos temporales
                //ZipFile.CreateFromDirectory(tempFolder, zipFilePath);

                //Console.WriteLine($"Archivos guardados en el archivo zip: {zipFilePath}");

                //// Elimina la carpeta temporal
                //Directory.Delete(tempFolder, true);
            }

            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)", "Cve: " + "" + "\nError: " + sEx);
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
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)", "Cve: " + "" + "\nError: " + sEx);
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
                mod.Log_Diario("SP_GetContratos_DescargaSeisMeses(Ex)", "Cve: " + "" + "\nError: " + sEx);
            }
        }


        public List<PolizasC> ObtenerContratosPolizas(int idCliente)
        {
            //Comentar
            if (_varPruebas)
                idCliente = 56914;




            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<PolizasC> listContratosC = new List<PolizasC>();
            try
            {
                string sSQL = "sp_GetPolizasByClienteSACContratos";
                SqlParameter[] paramArray = new SqlParameter[1];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@idCliente", idCliente, "");

                mod.Log_Diario("ClientPolizasController_ObtenerContratosPolizas", "1");

                DataSet ds = conector.RunProcedure(sSQL, paramArray, "1");
                tbl = ds.Tables[0];

                mod.Log_Diario("ClientPolizasController_ObtenerContratosPolizas", tbl.Rows.Count.ToString());

                //PrimerItem Seleccionar
                listContratosC.Add(new PolizasC
                {
                    vContratoValue = "-1",
                    vContrato = "Seleccionar contrato(s)"
                });


                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        listContratosC.Add(new PolizasC
                        {
                            vContratoValue = item["vContrato"].ToString(),
                            vContrato = item["vContrato"].ToString()
                        });
                    }

                    //PrimerItem Seleccionar
                    listContratosC.Add(new PolizasC
                    {
                        vContratoValue = "0",
                        vContrato = "Seleccionar disponibles"
                    });

                }


            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("ObtenerContratos(Ex)", "Cve: " + "" + "\nError: " + sEx);
            }
            return listContratosC;
        }

        [HttpGet]
        public JsonResult ObtenerNumerosSeriesPorContrato(string cto_fl_cve, bool flag)
        {
            int cveCliente = 0;
            int _Opcion = 1;
            //Solo cuando requieren todos los contratos se invoca sp que devuelve numeros de series por cliente
            if (cto_fl_cve == "0")
            {
                mod.Log_Diario("Imprime session ObtenerNumerosSeriesPorContrato get-- ", "Cve: " + Session["cveCliente"]);

                cveCliente = int.Parse(Session["cveCliente"].ToString());
                _Opcion = 2;
            }

            bool bsuccess = false;
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<PolizasC> listNumeroSeries = new List<PolizasC>();
            try
            {
                if (flag)
                {
                    //Definir si son 2 o 3 parametros
                    int LenghtParametros = _Opcion == 1 ? 2 : 3;

                    string sSQL = "sp_GetPolizasByClienteSACNumerosSerie";
                    SqlParameter[] paramArray = new SqlParameter[LenghtParametros];
                    paramArray[0] = BaseDatos.BDatos.NewParameter("@Cto_fl_cve", cto_fl_cve.Trim(), "");
                    paramArray[1] = BaseDatos.BDatos.NewParameter("@Opc", _Opcion, "");


                    mod.Log_Diario("ClientPolizasController_ObtenerNumerosSeriesPorContrato", "1");

                    //Solo para opcion seleccionar disponibles
                    if (_Opcion == 2)
                    {
                        //Comentar
                        if (_varPruebas)
                            cveCliente = 56914;

                        paramArray[2] = BaseDatos.BDatos.NewParameter("@IdCliente", cveCliente, "");
                    }
                    DataSet ds = conector.RunProcedure(sSQL, paramArray, "1");
                    tbl = ds.Tables[0];

                    mod.Log_Diario("ClientPolizasController_ObtenerNumerosSeriesPorContrato", tbl.Rows.Count.ToString());
                    //PrimerItem Seleccionar
                    listNumeroSeries.Add(new PolizasC
                    {
                        numeroSerieValue = "-1",
                        NumeroSerie = "Seleccionar no. serie(s)"
                    });


                    if (tbl != null && tbl.Rows.Count > 0)
                    {
                        foreach (DataRow item in tbl.Rows)
                        {
                            listNumeroSeries.Add(new PolizasC
                            {
                                numeroSerieValue = item["NoSerie"].ToString(),
                                NumeroSerie = item["NoSerie"].ToString()
                            });
                        }
                        //PrimerItem Seleccionar
                        listNumeroSeries.Add(new PolizasC
                        {
                            numeroSerieValue = "0",
                            NumeroSerie = "Seleccionar disponibles"
                        });
                    }


                    bsuccess = true;
                }
                else
                {
                    listNumeroSeries.Add(new PolizasC
                    {
                        numeroSerieValue = "-1",
                        NumeroSerie = "Seleccionar no. serie(s)"
                    });

                    ////PrimerItem Seleccionar
                    //listNumeroSeries.Add(new PolizasC
                    //{
                    //    numeroSerieValue = "0",
                    //    NumeroSerie = "Seleccionar disponibles"
                    //});
                    bsuccess = true;
                }

            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("ObtenerContratos(Ex)", "Cve: " + "" + "\nError: " + sEx);
                bsuccess = false;
            }
            object Respuesta = new
            {
                success = bsuccess,
                result = listNumeroSeries
            };


            return Json(Respuesta, JsonRequestBehavior.AllowGet);

        }

        public List<PolizasExists> ResumenObtenerPolizasPorContratoySerie(string contratoSerie)
        {
            string rutaBase = System.Configuration.ConfigurationManager.AppSettings["rutaBasePolizas"].ToString();

            #region Polizas Listado

            List<string> listP = contratoSerie.Split('|').ToList();
            int _opcion = 1;
            int contadorParametros = 3;
            int cveCliente = 0;

            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<PolizasExists> listPolizas = new List<PolizasExists>();


            if (listP.Count == 0)
            {
                listP.Add("");
                listP.Add("");
            }

            //Si selecciona un contrato y un numero de serie
            if (listP[0].Trim() != "0" && listP[1].Trim() != "0")
            {
                _opcion = 1;
                contadorParametros = 3;
            }

            //Si selecciona un contrato y todos sus numeros de serie
            if (listP[0].Trim() != "0" && listP[1].Trim() == "0")
            {
                _opcion = 2;
                contadorParametros = 3;
            }

            //Si selecciona todos los contratos y solo un numero de serie
            if (listP[0].Trim() == "0" && listP[1].Trim() != "0")
            {
                cveCliente = int.Parse(Session["cveCliente"].ToString());
                _opcion = 3;
                contadorParametros = 3;
            }

            //Selecciona todos los contratos y todos los numeros de serie
            if (listP[0].Trim() == "0" && listP[1].Trim() == "0")
            {
                cveCliente = int.Parse(Session["cveCliente"].ToString());
                _opcion = 4;
                contadorParametros = 4;
            }

            try
            {

                string sSQL = "sp_GetPolizasByContratoySerie";
                SqlParameter[] paramArray = new SqlParameter[contadorParametros];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@cto_fl_cve", listP[0].Trim(), "");
                paramArray[1] = BaseDatos.BDatos.NewParameter("@No_Serie", listP[1].Trim(), "");
                paramArray[2] = BaseDatos.BDatos.NewParameter("@Opc", _opcion, "");

                if (_opcion == 4)
                {
                    //Comentar
                    if (_varPruebas)
                        cveCliente = 56914;

                    paramArray[3] = BaseDatos.BDatos.NewParameter("@pna_fl_persona", cveCliente, "");

                }


                DataSet ds = conector.RunProcedure(sSQL, paramArray, "1");
                tbl = ds.Tables[0];

                mod.Log_Diario("ClientPolizasController_ResumenObtenerPolizasPorContratoySerie" + " Se inicia a registrar Polizas que se descargarón",
                    "Se inicia a registrar Polizas que se descargaran");

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        mod.Log_Diario("ClientPolizasController_ObtenerNumerosSeriesPorContrato", item["Poliza"].ToString());
                        listPolizas.Add(new PolizasExists
                        {
                            Poliza = item["Poliza"].ToString(),
                            Existe = false
                        });
                    }
                }


            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("ObtenerContratos(Ex)", "Cve: " + "" + "\nError: " + sEx);
            }
            #endregion

            foreach (PolizasExists polizaI in listPolizas)
            {
                string rutaCarpetaArchivo = Path.Combine(rutaBase, polizaI.Poliza);

                if (Directory.Exists(rutaCarpetaArchivo))
                {

                    if (Directory.Exists(rutaCarpetaArchivo))
                    {
                        string[] pdfFiles = Directory.GetFiles(rutaCarpetaArchivo, "*.pdf");
                        mod.Log_Diario("Linea 900 -- ", polizaI.Poliza + " - " + rutaCarpetaArchivo + " - " + pdfFiles.Length.ToString());
                        

                        if (pdfFiles.Length > 0)
                        {
                            polizaI.Existe = true;
                            polizaI.MsgPoliza = "Poliza descargada correctamente";
                        }
                        else
                        {
                            polizaI.Existe = false;
                            polizaI.MsgPoliza = "Para obtener tu póliza comunícate con nosotros al 800 7000 123.";
                        }

                    }

                }
                else
                {
                    polizaI.Existe = false;
                    polizaI.MsgPoliza = "Para obtener tu póliza comunícate con nosotros al 800 7000 123.";
                }

            }

            return listPolizas;
        }

        public List<string> ObtenerPolizasPorContratoySerie(string contratoSerie)
        {
            List<string> listP = contratoSerie.Split('|').ToList();
            int _opcion = 1;
            int contadorParametros = 3;
            int cveCliente = 0;

            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<string> listContratosC = new List<string>();


            if (listP.Count == 0)
            {
                listP.Add("");
                listP.Add("");
            }

            //Si selecciona un contrato y un numero de serie
            if (listP[0].Trim() != "0" && listP[1].Trim() != "0")
            {
                _opcion = 1;
                contadorParametros = 3;
            }

            //Si selecciona un contrato y todos sus numeros de serie
            if (listP[0].Trim() != "0" && listP[1].Trim() == "0")
            {
                _opcion = 2;
                contadorParametros = 3;
            }

            //Si selecciona todos los contratos y solo un numero de serie
            if (listP[0].Trim() == "0" && listP[1].Trim() != "0")
            {
                cveCliente = int.Parse(Session["cveCliente"].ToString());
                _opcion = 3;
                contadorParametros = 3;
            }

            //Selecciona todos los contratos y todos los numeros de serie
            if (listP[0].Trim() == "0" && listP[1].Trim() == "0")
            {
                cveCliente = int.Parse(Session["cveCliente"].ToString());
                _opcion = 4;
                contadorParametros = 4;
            }
            try
            {

                string sSQL = "sp_GetPolizasByContratoySerie";
                SqlParameter[] paramArray = new SqlParameter[contadorParametros];
                paramArray[0] = BaseDatos.BDatos.NewParameter("@cto_fl_cve", listP[0].Trim(), "");
                paramArray[1] = BaseDatos.BDatos.NewParameter("@No_Serie", listP[1].Trim(), "");
                paramArray[2] = BaseDatos.BDatos.NewParameter("@Opc", _opcion, "");

                if (_opcion == 4)
                {
                    //Comentar
                    if (_varPruebas)
                        cveCliente = 56914;

                    paramArray[3] = BaseDatos.BDatos.NewParameter("@pna_fl_persona", cveCliente, "");

                }


                DataSet ds = conector.RunProcedure(sSQL, paramArray, "1");
                tbl = ds.Tables[0];

                mod.Log_Diario("ClientPolizasController_ObtenerNumerosSeriesPorContrato" + " Se inicia a registrar Polizas que se descargaran",
                    "Se inicia a registrar Polizas que se descargaran");

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        mod.Log_Diario("ClientPolizasController_ObtenerNumerosSeriesPorContrato", item["Poliza"].ToString());
                        listContratosC.Add(item["Poliza"].ToString());
                    }
                }


            }
            catch (Exception ex)
            {
                string sEx = ex.Message;
                mod.Log_Diario("ObtenerContratos(Ex)", "Cve: " + "" + "\nError: " + sEx);
            }
            return listContratosC;
        }

    }
}