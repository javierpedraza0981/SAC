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
    public class ClientCapturasFMController : Controller
    {
        Validaciones mod = new Validaciones();
        cPersona Cliente = new cPersona();
        private sfiinternationalEntities Entity = new sfiinternationalEntities();

        // GET: ClientUpdate

        public ActionResult Index(string ntoken)
        {
            return RedirectToAction("Index", "Home", new { ntoken = ntoken, psAction = "ClientCapturasFM" });
        }


        #region "Actualización de datos del Cliente"
        public ActionResult ClientCapturasFM()
        {
            List<ComboBoxSencillo> lstUpdate = new List<ComboBoxSencillo>();
            List<DireccionClientModel> lstDomicilio = new List<DireccionClientModel>();
            List<TelefonoClientModel> lstTelefono = new List<TelefonoClientModel>();
            int cveCliente = 0;

            try
            {
                if (Session["cveCliente"] == null) return RedirectToAction("Error", "Home");
                cveCliente = int.Parse(Session["cveCliente"].ToString());
                lstUpdate.Add(new ComboBoxSencillo { Value = 1, Text = "NUMERO TELEFONICO" });
                lstUpdate.Add(new ComboBoxSencillo { Value = 2, Text = "DOMICILIO FISCAL" });
                lstUpdate.Add(new ComboBoxSencillo { Value = 3, Text = "DOMICILIO ADMINISTRATIVO" });
                lstUpdate.Add(new ComboBoxSencillo { Value = 4, Text = "CORREO ELECTRÓNICO" });

                lstDomicilio = (List<DireccionClientModel>)this.getDatosCliente(1, cveCliente);
                lstTelefono = (List<TelefonoClientModel>)this.getDatosCliente(2, cveCliente);

                ViewBag.Name = Session["Name"].ToString().Trim();
                ViewBag.CveCliente = cveCliente.ToString();
                ViewBag.RFC = Session["rfc"].ToString().Trim();
                ViewBag.Correo = Session["correo"].ToString().Trim();
                ViewBag.DatosActualizar = lstUpdate;
                ViewBag.DomicilioCliente = lstDomicilio;
                ViewBag.TelefonoCliente = lstTelefono;

                return View();
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("ClientUpdate(Ex)", "Cve: " + cveCliente + "\nError: " + sEx);
                return RedirectToAction("Error", "Home", new { psError = "ClientFiles" });
            }
        }

        private object getDatosCliente(int piTipoCon, int piCveCliente)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            object objResult = null;

            try
            {
                string sSQL = "exec usp_getDatosCliente @iTipoCon=" + piTipoCon + ", @iIdCliente=" + piCveCliente;
                SqlParameter[] paramArray = new SqlParameter[0];

                tbl = conector.RunQueryDT(sSQL, paramArray);

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    if (piTipoCon == 1)
                    {
                        List<DireccionClientModel> lstDirecciones = new List<DireccionClientModel>();

                        foreach (DataRow row in tbl.Rows)
                        {
                            lstDirecciones.Add(new DireccionClientModel
                            {
                                TipoDomicilio = row["TIPO_DOMICILIO"].ToString().Trim(),
                                Calle = row["CALLE"].ToString().Trim(),
                                NoExt = row["NO_EXT"].ToString().Trim(),
                                NoInt = row["NO_INT"].ToString().Trim(),
                                Colonia = row["COLONIA"].ToString().Trim(),
                                DelMun = row["DEL_MUN"].ToString().Trim(),
                                Ciudad = row["CIUDAD"].ToString().Trim(),
                                Entidad = row["ENTIDAD"].ToString().Trim(),
                                CP = row["CP"].ToString().Trim(),
                            });
                        }

                        objResult = lstDirecciones;
                    }
                    if (piTipoCon == 2)
                    {
                        List<TelefonoClientModel> lstTelefono = new List<TelefonoClientModel>();

                        foreach (DataRow row in tbl.Rows)
                        {
                            lstTelefono.Add(new TelefonoClientModel
                            {
                                TipoTelefono = row["tx_TipoTelefono"].ToString().Trim(),
                                Telefono = row["tx_Telefono"].ToString().Trim()
                            });
                        }

                        objResult = lstTelefono;
                    }
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("getDatosCliente(Ex)", "Cve: " + piCveCliente + "\nError: " + sEx);
            }

            return objResult;
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

        [HttpPost]
        public JsonResult ActualizarDatosCliente(int piCveCliente, string psCliente, string psCorreoCliente, string psOpcUpdate, string psTelefono, string psTipoTel,
            string psDomicilioFModel, HttpPostedFileBase fileF, string psDomicilioAModel, HttpPostedFileBase fileA, string psCorreo, string psComAdic)
        {
            DireccionClientModel dirFiscal = new DireccionClientModel();
            DireccionClientModel dirAdmin = new DireccionClientModel();
            bool bSuccess = true;
            string sResult = "";
            int iIdSolicitud = 0;

            try
            {
                if (psOpcUpdate.Contains("2")) dirFiscal = JsonConvert.DeserializeObject<DireccionClientModel>(psDomicilioFModel);
                if (psOpcUpdate.Contains("3")) dirAdmin = JsonConvert.DeserializeObject<DireccionClientModel>(psDomicilioAModel);

                iIdSolicitud = this.RegistraSolicitud(piCveCliente, psOpcUpdate, psTelefono, psTipoTel, dirFiscal, dirAdmin, psCorreo, psComAdic);
                if (iIdSolicitud > 0)
                {
                    if (!this.EnviaCorreoSolicitud(iIdSolicitud, piCveCliente, psCliente, psOpcUpdate, psTelefono, psTipoTel, dirFiscal, fileF, dirAdmin, fileA, psCorreo, psComAdic))
                    {
                        bSuccess = false;
                        sResult = "Error al enviar el correo de solicitud";
                    }
                    if (!this.EnviaCorreoCliente(iIdSolicitud, piCveCliente, psCliente, psCorreoCliente))
                    {
                        bSuccess = false;
                        sResult = "Error al enviar el correo al cliente";
                    }
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("ActualizarDatosCliente(Ex)", "Cve: " + piCveCliente + "\nError: " + sEx);
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

        private bool EnviaCorreoSolicitud(int piIdSolicitud, int piCveCliente, string psCliente, string psOpcUpdate, string psTelefono, string psTipoTel,
            DireccionClientModel pDirFiscal, HttpPostedFileBase fileF, DireccionClientModel pDirAdmin, HttpPostedFileBase fileA, string psCorreo, string psComAdic)
        {
            SmtpClient server = new SmtpClient();
            MailMessage mensaje = new MailMessage();

            try
            {
                string sEnableSsl = ConfigurationManager.AppSettings.Get("EnabledSSL");
                string strHtmlContent = "";

                strHtmlContent = @"<html>
                    <body>
                        <table width=""100%"" align=""left"" style=""text-align: left; font-style: arial, sans-serif; font-size: 13px;"">
                            <tr>
                                <td>
                                    <br>
                                    <span style=""font-style: Calibri, sans-serif; font-size: 15px; font-weight: bold;"">
                                            Id Solicitud: 
                                    </span>                                    
                                    <span> " +
                                        piIdSolicitud +
                                    @"</span>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding-top: 15px;"">
                                    <span style=""font-style: Calibri, sans-serif; font-size: 15px; font-weight: bold;"">
                                            Cliente
                                    </span>
                                    <br>
                                    <span> " +
                                        piCveCliente + " - " + psCliente.ToUpper().Trim() +
                                    @"</span>
                                </td>
                            </tr>";
                if (psOpcUpdate.Contains("1"))
                {
                    strHtmlContent += @"<tr>
                                <td style=""padding-top: 15px;"">
                                    <span style=""font-style: Calibri, sans-serif; font-size: 15px; font-weight: bold;"">
                                            Teléfono
                                    </span>
                                    <br>
                                    <span> " +
                                        psTelefono + " - " + (psTipoTel == "1" ? "Fijo" : "Móvil") +
                                    @"</span>
                                </td>
                            </tr>";
                }
                if (psOpcUpdate.Contains("2"))
                {
                    strHtmlContent += @"<tr>
                                <td style=""padding-top: 15px;"">
                                    <span style=""font-style: Calibri, sans-serif; font-size: 15px; font-weight: bold;"">
                                            Domicilio Fiscal
                                    </span>
                                    <br>
                                    <span> " +
                                        "Calle: " + pDirFiscal.Calle.ToUpper().Trim() + ", No. Ext:" + pDirFiscal.NoExt.ToUpper().Trim() + ", No. Int:" + pDirFiscal.NoInt.ToUpper().Trim() + ", Colonia:" + pDirFiscal.Colonia.ToUpper().Trim() + ", " +
                                        "Delegación/Municipio:" + pDirFiscal.DelMun.ToUpper().Trim() + ", Ciudad:" + pDirFiscal.Ciudad.ToUpper().Trim() + ", Entidad:" + pDirFiscal.Entidad.ToUpper().Trim() + ", CP:" + pDirFiscal.CP.ToUpper().Trim() +
                                    @"</span>
                                </td>
                            </tr>";
                }
                if (psOpcUpdate.Contains("3"))
                {
                    strHtmlContent += @"<tr>
                                <td style=""padding-top: 15px;"">
                                    <span style=""font-style: Calibri, sans-serif; font-size: 15px; font-weight: bold;"">
                                            Domicilio Administrativo
                                    </span>
                                    <br>
                                    <span> " +
                                        "Calle: " + pDirAdmin.Calle.ToUpper().Trim() + ", No. Ext:" + pDirAdmin.NoExt.ToUpper().Trim() + ", No. Int:" + pDirAdmin.NoInt.ToUpper().Trim() + ", Colonia:" + pDirAdmin.Colonia.ToUpper().Trim() + ", " +
                                        "Delegación/Municipio:" + pDirAdmin.DelMun.ToUpper().Trim() + ", Ciudad:" + pDirAdmin.Ciudad.ToUpper().Trim() + ", Entidad:" + pDirAdmin.Entidad.ToUpper().Trim() + ", CP:" + pDirAdmin.CP.ToUpper().Trim() +
                                    @"</span>
                                </td>
                            </tr>";
                }
                if (psOpcUpdate.Contains("4"))
                {
                    strHtmlContent += @"<tr>
                                <td style=""padding-top: 15px;"">
                                    <span style=""font-style: Calibri, sans-serif; font-size: 15px; font-weight: bold;"">
                                            Correo electrónico
                                    </span>
                                    <br>
                                    <span> " +
                                        psCorreo +
                                    @"</span>
                                </td>
                            </tr>";
                }
                if (psComAdic != null && psComAdic != "")
                {
                    strHtmlContent += @"<tr>
                                <td style=""padding-top: 15px;"">
                                    <span style=""font-style: Calibri, sans-serif; font-size: 15px; font-weight: bold;"">
                                            Comentarios adicionales
                                    </span>
                                    <br>
                                    <span> " +
                                        psComAdic.ToUpper().Trim() +
                                    @"</span>
                                </td>
                            </tr>";
                }
                strHtmlContent += @"<br>
                        </table>
                    </body>
                </html> ";

                AlternateView html = AlternateView.CreateAlternateViewFromString(strHtmlContent, null, MediaTypeNames.Text.Html);
                string sCorreoEnviaSolicitud = ConfigurationManager.AppSettings.Get("CorreoEnviaSolicitud");

                mensaje.Subject = "Solicitud de actualización de datos: " + piIdSolicitud;
                mensaje.From = new MailAddress(ConfigurationManager.AppSettings.Get("CorreoQueEnvia"), ConfigurationManager.AppSettings.Get("NombreQueEnvia"));
                mensaje.To.Add(new MailAddress(sCorreoEnviaSolicitud));
                mensaje.IsBodyHtml = true;
                mensaje.AlternateViews.Add(html);

                if (psOpcUpdate.Contains("2")) mensaje.Attachments.Add(new Attachment(fileF.InputStream, "ComprobanteFiscal.pdf"));
                if (psOpcUpdate.Contains("3")) mensaje.Attachments.Add(new Attachment(fileA.InputStream, "ComprobanteAdministrativo.pdf"));

                server.Credentials = new NetworkCredential(ConfigurationManager.AppSettings.Get("CorreoQueEnvia"), ConfigurationManager.AppSettings.Get("ContrasenaQueEnvia"));
                server.Port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Puerto"));
                server.Host = ConfigurationManager.AppSettings.Get("Server");
                server.EnableSsl = (sEnableSsl == "1" ? true : false);

                try
                {
                    server.Send(mensaje);
                }
                catch (SmtpException exMail)
                {
                    string sEx = exMail.Message;

                    mod.Log_Diario("EnviaCorreoCliente(exMail)", "Cve: " + piCveCliente + "\nError: " + sEx);
                    return false;
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("EnviaCorreoSolicitud(Ex)", "Cve: " + piCveCliente + "\nError: " + sEx);
                return false;
            }
            mensaje.Dispose();
            server.Dispose();

            return true;
        }

        private bool EnviaCorreoCliente(int piIdSolicitud, int piCveCliente, string psCliente, string psCorreoCliente)
        {
            SmtpClient server = new SmtpClient();
            MailMessage mensaje = new MailMessage();

            try
            {
                string sEnableSsl = ConfigurationManager.AppSettings.Get("EnabledSSL");
                string strHtmlContent = @"<html>
                            <body>
                                <table width=""100%"" style=""font-style: arial, sans-serif; font-size: 13px;"">
                                    <tr>
                                        <td align=""right"">
                                            <br>
                                            <img width=""211"" height=""53"" src=cid:logoNavistarID>
                                        </td>                                        
                                    </tr>
                                    <tr>                                        
                                        <td align=""left"" style=""font-style: Calibri, sans-serif; font-weight:bold""> " +
                                            psCliente +
                                        @"</td>
                                    </tr>
                                    <tr>
                                        <td align=""left"">
                                            <span lang=""ES - MX"">
                                                Te notificamos que la solicitud para actualizar tus datos a través de Portal Navistar<sup>®</sup> se ha generado correctamente con el folio de seguimiento " + piIdSolicitud + @". En breve un ejecutivo te contactará para concluir la actualización. 
                                            </span>                                            
                                            <br><br>
                                            Gracias.
                                            <br><br>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align=""left"">
                                            <br>
                                            <img src=cid:NavistarATCID>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""font-size: 7.0pt; font-family: Arial, sans-serif; color: #999999;"">
                                            <p>
                                                <u>
                                                    <span>
                                                        Aviso de Confidencialidad:
                                                    </span>
                                                </u>
                                                <span>
                                                    El contenido de esta comunicación es confidencial para uso exclusivo del destinatario.Si Usted lo ha recibido por error, favor de eliminarlo de cualquier medio de almacenamiento electrónico o bien destrúyalo inmediatamente.Gracias.
                                                   <u> Aviso Privacidad:</u> Los datos personales incluidos en el presente correo, están registrados en una base de datos propiedad de Navistar Financial S.A. de C.V., SOFOM, E.R., con el propósito de cumplir con lo requerido mediante la legislación aplicable e informarle lo relativo a sus
                                                    datos personales. Si usted desea ejercer los derechos de acceso, rectificación, cancelación y oposición, por favor, envíe su solicitud a <a href=""mailto:protecciondatosmx@navistar.com"" target= ""_blank""><span>protecciondatosmx@navistar.com</span></a>
                                                    o Ejército Nacional No. 904, Piso 8, Colonia Palmas Polanco, 11560, Ciudad de México, México.Sus datos personales serán tratados de acuerdo con nuestra política de privacidad disponible en
                                                   <a href=""https://nam11.safelinks.protection.outlook.com/?url=http%3A%2F%2Fwww.navistarfinancial.com%2F&data=05%7C01%7COctavio.Avila%40Navistar.com%7Cb0f087d5649940946ff808db6e90b2cc%7Cb5a920d67d3c44febaad4ffed6b8774d%7C0%7C0%7C638225339996513045%7CUnknown%7CTWFpbGZsb3d8eyJWIjoiMC4wLjAwMDAiLCJQIjoiV2luMzIiLCJBTiI6Ik1haWwiLCJXVCI6Mn0%3D%7C3000%7C%7C%7C&sdata=Xzk3zSmzf1VxtdBDWqdpOpPYhAn4vUiKbMCBQvlvuAY%3D&reserved=0"" target= ""_blank"" data-saferedirecturl= ""https://www.google.com/url?hl=es&q=https://nam11.safelinks.protection.outlook.com/?url%3Dhttp%253A%252F%252Fwww.navistarfinancial.com%252F%26data%3D05%257C01%257COctavio.Avila%2540Navistar.com%257Cb0f087d5649940946ff808db6e90b2cc%257Cb5a920d67d3c44febaad4ffed6b8774d%257C0%257C0%257C638225339996513045%257CUnknown%257CTWFpbGZsb3d8eyJWIjoiMC4wLjAwMDAiLCJQIjoiV2luMzIiLCJBTiI6Ik1haWwiLCJXVCI6Mn0%253D%257C3000%257C%257C%257C%26sdata%3DXzk3zSmzf1VxtdBDWqdpOpPYhAn4vUiKbMCBQvlvuAY%253D%26reserved%3D0&source=gmail&ust=1687472102845000&usg=AOvVaw22-DBAh1NWKfsfbCNLczoD"">
                                                         <span>www.navistarfinancial.com</span>
                                                     </a>
                                                     y en su caso, con la Notificación de Privacidad correspondiente según lo dispuesto en su caso. Puedes consultar en cualquier momento términos y condiciones
                                                     <a href=""https://nam11.safelinks.protection.outlook.com/?url=https%3A%2F%2Fservicioscorporativosnfc.com%2FServiciosDigitales%2FDocumentos%2FAvisoDomiciliacion.pdf&data=05%7C01%7COctavio.Avila%40Navistar.com%7Cb0f087d5649940946ff808db6e90b2cc%7Cb5a920d67d3c44febaad4ffed6b8774d%7C0%7C0%7C638225339996513045%7CUnknown%7CTWFpbGZsb3d8eyJWIjoiMC4wLjAwMDAiLCJQIjoiV2luMzIiLCJBTiI6Ik1haWwiLCJXVCI6Mn0%3D%7C3000%7C%7C%7C&sdata=Y4aLr0uHCksRciDvAQ8Ru7RVjPSSTHNGh2bSd4x%2Fu0I%3D&reserved=0"" >
                                                         <span>aquí</span>
                                                     </a>
                                                 </span>
                                             </p>
                                         </td>
                                     </tr>
                                 </table>
                            </body>
                            </html>";

                AlternateView html = AlternateView.CreateAlternateViewFromString(strHtmlContent, null, MediaTypeNames.Text.Html);
                LinkedResource theEmailImage = new LinkedResource(Server.MapPath("/img/Navistar2.png"));
                string sCorreoEnviaCliente = ConfigurationManager.AppSettings.Get("CorreoEnviaCliente");

                if (sCorreoEnviaCliente != null && sCorreoEnviaCliente != "") psCorreoCliente = sCorreoEnviaCliente;
                theEmailImage.ContentId = "logoNavistarID";
                theEmailImage.ContentType.MediaType = "image/png";
                html.LinkedResources.Add(theEmailImage);

                theEmailImage = new LinkedResource(Server.MapPath("/img/NavistarATC.png"));
                theEmailImage.ContentId = "NavistarATCID";
                theEmailImage.ContentType.MediaType = "image/png";
                html.LinkedResources.Add(theEmailImage);

                mensaje.Subject = "Confirmación de solicitud de actualización de datos";
                mensaje.From = new MailAddress(ConfigurationManager.AppSettings.Get("CorreoQueEnvia"), ConfigurationManager.AppSettings.Get("NombreQueEnvia"));
                mensaje.To.Add(new MailAddress(psCorreoCliente));
                mensaje.IsBodyHtml = true;
                mensaje.AlternateViews.Add(html);

                server.Credentials = new NetworkCredential(ConfigurationManager.AppSettings.Get("CorreoQueEnvia"), ConfigurationManager.AppSettings.Get("ContrasenaQueEnvia"));
                server.Port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Puerto"));
                server.Host = ConfigurationManager.AppSettings.Get("Server");
                server.EnableSsl = (sEnableSsl == "1" ? true : false);

                try
                {
                    server.Send(mensaje);
                }
                catch (SmtpException exMail)
                {
                    string sEx = exMail.Message;

                    mod.Log_Diario("EnviaCorreoCliente(exMail)", "Cve: " + piCveCliente + "\nError: " + sEx);
                    return false;
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("EnviaCorreoCliente(Ex)", "Cve: " + piCveCliente + "\nError: " + sEx);
                return false;
            }
            mensaje.Dispose();
            server.Dispose();

            return true;
        }

        private int RegistraSolicitud(int piCveCliente, string psOpcUpdate, string psTelefono, string psTipoTel, DireccionClientModel pDirFiscal, DireccionClientModel pDirAdmin, string psCorreo, string psComAdic)
        {
            BaseDatos.BDatos conector = new BaseDatos.BDatos();
            DataTable tbl = new DataTable();
            string sTipoSol = "";
            string sSolicitud = "";
            int iIdSolicitud = 0;

            try
            {
                List<string> OpcUpdate = JsonConvert.DeserializeObject<List<string>>(psOpcUpdate);
                string sSQL = "usp_SolicitudActualizarCliente";
                SqlParameter[] paramArray = new SqlParameter[3];
                int afecto = 0;

                // Registramos la solicitud
                paramArray[0] = BaseDatos.BDatos.NewParameter("@piIdPersona", piCveCliente, "");
                paramArray[1] = BaseDatos.BDatos.NewParameter("@psComentarios", psComAdic, "");
                paramArray[2] = BaseDatos.BDatos.NewParameter("@iIdSolicitud", psComAdic, "output");

                iIdSolicitud = conector.RunProcedure(sSQL, paramArray, out afecto);

                if (iIdSolicitud > 0)
                {
                    // Registramos el detalle de la solicitud
                    sSQL = "usp_DetalleSolicitudActualizarCliente";

                    foreach (string tipoSol in OpcUpdate)
                    {
                        if (tipoSol == "1")
                        {
                            sTipoSol = "Teléfono";
                            sSolicitud = psTelefono + " - " + (psTipoTel == "1" ? "Fijo" : "Móvil");
                        }
                        else if (tipoSol == "2")
                        {
                            sTipoSol = "Domicilio Fiscal";
                            sSolicitud = "Calle: " + pDirFiscal.Calle.ToUpper().Trim() + ", No. Ext:" + pDirFiscal.NoExt.ToUpper().Trim() + ", No. Int:" + pDirFiscal.NoInt.ToUpper().Trim() + ", Colonia:" + pDirFiscal.Colonia.ToUpper().Trim() + ", ";
                            sSolicitud += "Delegación/Municipio:" + pDirFiscal.DelMun.ToUpper().Trim() + ", Ciudad:" + pDirFiscal.Ciudad.ToUpper().Trim() + ", Entidad:" + pDirFiscal.Entidad.ToUpper().Trim() + ", CP:" + pDirFiscal.CP.ToUpper().Trim();
                        }
                        else if (tipoSol == "3")
                        {
                            sTipoSol = "Domicilio Fiscal";
                            sSolicitud = "Calle: " + pDirAdmin.Calle.ToUpper().Trim() + ", No. Ext:" + pDirAdmin.NoExt.ToUpper().Trim() + ", No. Int:" + pDirAdmin.NoInt.ToUpper().Trim() + ", Colonia:" + pDirAdmin.Colonia.ToUpper().Trim() + ", ";
                            sSolicitud += "Delegación/Municipio:" + pDirAdmin.DelMun.ToUpper().Trim() + ", Ciudad:" + pDirAdmin.Ciudad.ToUpper().Trim() + ", Entidad:" + pDirAdmin.Entidad.ToUpper().Trim() + ", CP:" + pDirAdmin.CP.ToUpper().Trim();
                        }
                        else if (tipoSol == "4")
                        {
                            sTipoSol = "Correo electrónico";
                            sSolicitud = psCorreo;
                        }

                        paramArray = new SqlParameter[3];
                        paramArray[0] = BaseDatos.BDatos.NewParameter("@iIdSolicitud", iIdSolicitud, "");
                        paramArray[1] = BaseDatos.BDatos.NewParameter("@psTipoSolicitud", sTipoSol, "");
                        paramArray[2] = BaseDatos.BDatos.NewParameter("@psSolicitud", sSolicitud, "");

                        conector.RunProcedure(sSQL, paramArray, out afecto);

                        if (afecto <= 0)
                        {
                            mod.Log_Diario("RegistraSolicitud(td)", "IdSolicitud: " + iIdSolicitud + "\nCve: " + piCveCliente + "\nTipo Solicitud: " + sTipoSol + "\nSolicitud: " + sSolicitud);
                        }
                    }
                }
                else
                {
                    mod.Log_Diario("RegistraSolicitud(tb)", "Cve: " + piCveCliente + "\nComentarios: " + psComAdic);
                }
            }
            catch (Exception ex)
            {
                string sEx = ex.Message;

                mod.Log_Diario("RegistraSolicitud(Ex)", "Cve: " + piCveCliente + "\nError: " + sEx + "\nTipo Solicitud: " + sTipoSol + "\nSolicitud: " + sSolicitud + "\nComentarios: " + psComAdic);
            }

            return iIdSolicitud;
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

        //--> Correo al que se va a mandar la info del cliente para actualizar
        //servicioaclientes@navistar.com


        //-- > Correo del que se va enviar
        //serviciosenlinea@navistar.com

        #endregion
    }
}