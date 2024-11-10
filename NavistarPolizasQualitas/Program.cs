using iTextSharp.text;
using iTextSharp.text.pdf;
using NavistarPagos.Data;
using NavistarPolizasQualitas.ServiceQualitasProd;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;

namespace NavistarPolizasQualitas
{
    public class Program
    {
        static Logger logger = new Logger();

        Validaciones mod = new Validaciones();
        static void Main(string[] args)
        {

            logger.Log($"1");
            ServiceQualitasProd.entradaSoapClient client = new entradaSoapClient();
            logger.Log($"2");
            List<string> _PolizasProcesar = new List<string>();
            logger.Log($"3");
            int ImpAnexo = 0;            
            string URLPoliza = "";
            string URLRecibo = "";
            string URLTextos = "";
            int Inciso = 0;
            int ImpPol = 0;
            int ImpRec = 0;
            string Ramo = "";
            string formaPol = "";
            string formaRec = "";
            string formaAnexo = "";
            string Endoso = "";
            string Usuario = "";
            string Password = "";
            string byName1 = "18859";///parametrosBo.GetByName("QualitasWs Agente");
            string byName2 = "00465";//parametrosBo.GetByName("QualitasWs NoNegocio");
            logger.Log($"4");
            string msg = "";
            int cveCliente = 0;
            logger.Log($"5");
            _PolizasProcesar = RecuperaDatos(ref msg);
            logger.Log($"6");
            logger.Log($"longitud " + _PolizasProcesar.Count);
            try
            {
                logger.Log($"7");
                // Llamar al método del servicio
                logger.Log($"inicio forech");
                foreach (string polizaPrincipal in _PolizasProcesar)
                {
                    try
                    {
                        logger.Log($"inicio forech con poliza " + polizaPrincipal);
                        string ruta = "";
                        string result = client.RecuperaImpresionM15(polizaPrincipal, ref URLPoliza, ref URLRecibo, ref URLTextos, Inciso, ImpPol, ImpRec, ImpAnexo, Ramo, formaPol, formaRec, formaAnexo, Endoso, byName2, byName1, Usuario, Password);
                        if (result != "0065-- No existe el numero de poliza a afectar")
                        {
                            //validar carpeta inicio
                            ruta = CreateFolderPoliza(polizaPrincipal);
                            List<string> _Polizas = result.Split('|').ToList();
                            foreach (string polizaSecundaria in _Polizas)
                            {
                                string fileName = System.IO.Path.GetFileName(polizaSecundaria);
                                if (fileName.StartsWith("P") || fileName.StartsWith("p"))
                                {
                                    logger.Log($"Move PDF - " + polizaSecundaria + " - " + ruta);
                                    MovePdf(polizaSecundaria, ruta);
                                    InsertControlPoliza(polizaPrincipal, polizaSecundaria, ruta, result);
                                    //Logo Polizas
                                    AgregarLogoPdf(Path.Combine(ruta, fileName));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Controlar la excepción y continuar iterando
                        logger.Log($"Error en foreach al procesar la póliza {polizaPrincipal}:: " + ex.Message + " InnerException:: " + ex.InnerException);
                        InsertControlPoliza(polizaPrincipal, "Error", "Error", "Poliza No Descargada");
                        InsertControlPoliza("ERROR", polizaPrincipal, "ERROR", "Error al procesar la póliza " + polizaPrincipal + ":: " + ex.Message + " InnerException:: " + ex.InnerException);
                    }
                }
                logger.Log($"fin forech");
                client.Close();
                logger.Log($"close");
                Console.ReadLine();                
            }
            catch (Exception ex)
            {
                logger.Log($"Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
                InsertControlPoliza("ERROR", "ERROR", "ERROR", "Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
            }
        }
        public static void AgregarLogoPdf(string fullPath)
        {
            try 
            {
                // Ruta del archivo PDF existente
                string inputPdfPath = fullPath;
                // Ruta donde se guardará el archivo PDF modificado
                string tempPdfPath = Path.GetTempFileName();


                
                string logoPath = System.Configuration.ConfigurationManager.AppSettings["RouteLogo"];
                logger.Log(logoPath);

                if (!File.Exists(inputPdfPath))
                {
                    Console.WriteLine("El archivo PDF de entrada no existe en la ruta especificada.");
                    return;
                }

                if (!File.Exists(logoPath))
                {
                    Console.WriteLine("El archivo de logo no existe en la ruta especificada.");
                    return;
                }

                using (FileStream inputPdfStream = new FileStream(inputPdfPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream tempPdfStream = new FileStream(tempPdfPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Leer el PDF de entrada
                    PdfReader reader = new PdfReader(inputPdfPath);
                    // Crear el stamper para escribir en el PDF
                    PdfStamper stamper = new PdfStamper(reader, tempPdfStream);

                    // Cargar la imagen del logo
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);

                    // Escalar la imagen al tamaño deseado (opcional)
                    logo.ScaleToFit(100f, 100f);

                    // Obtener el número total de páginas
                    int totalPages = reader.NumberOfPages;

                    for (int i = 1; i <= totalPages; i++)
                    {
                        // Obtener el tamaño de la página actual
                        Rectangle pageSize = reader.GetPageSizeWithRotation(i);

                        // Establecer la posición de la imagen en la parte superior izquierda
                        float x = 35f; // Margen izquierdo
                        float y = pageSize.Height - 35f - logo.ScaledHeight; // Margen superior

                        // Agregar la imagen a la página actual
                        PdfContentByte content = stamper.GetOverContent(i);
                        logo.SetAbsolutePosition(x, y);
                        content.AddImage(logo);

                    }
 
                    // Cerrar el stamper
                    stamper.Close();
                    reader.Close();
                }
                // Sobrescribir el archivo original con el archivo temporal
                File.Delete(inputPdfPath); // Eliminar el archivo original
                File.Move(tempPdfPath, inputPdfPath); // Renombrar el archivo temporal como el archivo original
                Console.WriteLine("PDF modificado exitosamente con el logo en la parte superior izquierda.");
            }
            catch (Exception ex)
            {
                logger.Log($"AgregarLogoPdf - Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
                InsertControlPoliza("ERROR", "ERROR", "ERROR", "Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
            }
            
        }
        public static string CreateFolderPoliza(string polizasProcesar)
        {
            string text = polizasProcesar;
            string rutaDestino = ConfigurationManager.AppSettings["RouteRepository"];
            string _rutaFullFill = "";

            try
            {
                // Combinar la ruta base con el nombre de la carpeta
                _rutaFullFill = Path.Combine(rutaDestino, text);

                // Verificar si la carpeta ya existe
                if (!Directory.Exists(_rutaFullFill))
                {
                    // Crear la carpeta si no existe
                    Directory.CreateDirectory(_rutaFullFill);
                    Console.WriteLine("Carpeta creada exitosamente en: " + _rutaFullFill);
                }
                else
                {
                    Console.WriteLine("La carpeta ya existe en: " + _rutaFullFill);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"CreateFolderPoliza - Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
                InsertControlPoliza("ERROR", "ERROR", "ERROR", "Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
            }

            return _rutaFullFill;
        }
        public static void MovePdf(string poliza, string ruta)
        {
            string url = poliza;
            string rutaDestino = ruta; // Cambia esta ruta por la carpeta donde quieres guardar el archivo

            try
            {
                // Crear una solicitud web
                WebRequest solicitud = WebRequest.Create(url);
                // Obtener la respuesta
                using (WebResponse respuesta = solicitud.GetResponse())
                {
                    // Obtener el flujo de datos de la respuesta
                    using (Stream flujoRespuesta = respuesta.GetResponseStream())
                    {
                        // Crear el archivo en la ruta especificada                       
                        string nombreArchivo = Path.GetFileName(url);
                        string rutaCompleta = Path.Combine(rutaDestino, nombreArchivo);
                        using (FileStream archivoLocal = File.Create(rutaCompleta))
                        {
                            // Leer el flujo de respuesta y escribirlo en el archivo local
                            flujoRespuesta.CopyTo(archivoLocal);
                        }

                        Console.WriteLine("Archivo descargado exitosamente en: " + rutaCompleta);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log($"MovePdf - Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
                InsertControlPoliza("ERROR", "ERROR", "ERROR", "Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
            }
        }
        public static List<string> RecuperaDatos(ref string msg)
        {
            logger.Log("088");
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            logger.Log("099");
            string cadConex="";
            logger.Log("010");
            DataTable tbl = new DataTable();
            logger.Log("011");
            List<string> _PolizasList = new List<string>();
            logger.Log("012");
            string sSQL = "";
            logger.Log("013");
            SqlParameter[] paramArray;
            logger.Log("014");
            DataSet ds = new DataSet();
            logger.Log("015");
            try
            {
                logger.Log("8");
                sSQL = "sp_GetPolizasAllForConsola";
                logger.Log("9");
                paramArray = new SqlParameter[1];
                logger.Log("10");
                ds = conector.RunProcedure(sSQL, paramArray, "tblPolizas", out cadConex);

                logger.Log(cadConex + " cadconex");
                tbl = ds.Tables[0];

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow item in tbl.Rows)
                    {
                        _PolizasList.Add(item[0].ToString());
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Log($"RecuperaDatos - Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
                InsertControlPoliza("ERROR", "ERROR", "ERROR", "Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
            }

            return _PolizasList;
        }
        public static void InsertControlPoliza(string poliza, string polizaSecundaria, string ruta, string mensaje)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            List<string> _PolizasList = new List<string>();
            string sSQL = "";
            SqlParameter[] paramArray;
            DataSet ds = new DataSet();

            try
            {
                int _respuestaSp = 0;
                sSQL = "sp_InsertarControlPoliza";
                paramArray = new SqlParameter[4];

                paramArray[0] = BaseDatos.BDatos.NewParameter("@PolizaPrincipal", poliza, "");
                paramArray[1] = BaseDatos.BDatos.NewParameter("@PolizaSecundaria", polizaSecundaria, "");
                paramArray[2] = BaseDatos.BDatos.NewParameter("@Ruta", ruta, "");
                paramArray[3] = BaseDatos.BDatos.NewParameter("@Mensaje", mensaje, "");
                
                conector.RunProcedure(sSQL, paramArray, out _respuestaSp);

                if (_respuestaSp == 0)
                {
                    //ok
                }
                else {
                    //no inserto seguimiento a base de datos
                }

            }
            catch (Exception ex)
            {
                logger.Log($"InsertControlPoliza - Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
                InsertControlPoliza("ERROR", "ERROR", "ERROR", "Error:: " + ex.Message + " InnerException:: " + ex.InnerException);
            }
        }

    }
}
