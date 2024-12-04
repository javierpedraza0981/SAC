using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PagoEnLinea;

public class PDFPago
{
    public PDFPago()
    {
    }
    public static BaseColor colorGris = new BaseColor(0, 53, 94);
    //public static BaseColor colorAzul = new BaseColor(128, 130, 132);
    public static BaseColor colorAzul = new BaseColor(0, 53, 95);
    public static MemoryStream GeneraResumen(int claveCliente, string nombre, string rfc, string correo, DataTable dt, string nombreArchivo)
    {
        MemoryStream archivo = new MemoryStream();
        Rectangle pageSize = new Rectangle(PageSize.LETTER.Height, PageSize.LETTER.Width);
        Document document = new Document(pageSize, 30.0f, 30.0f, 70.0f, 50.0f);
        try
        {
            // Le decimos donde queremos escribir
            PdfWriter writer = PdfWriter.GetInstance(document, archivo);

            PdfPTable head = new PdfPTable(1);
            PdfPTable footer = new PdfPTable(1);

            string pie1 = "";
            string pie2 = "";
            string pie3 = "";
            string titulo1 = "";
            string titulo2 = "";
            string titulo3 = "";
            document.AddAuthor("Altum Technologies, S.A. de C.V.");
            document.AddCreationDate();
            document.AddTitle(titulo1 + " " + titulo2);
            LlenaEncabezadoPie(document, ref head, ref footer, "", titulo1, titulo2, titulo3, pie1, pie2, pie3);
            writer.PageEvent = new _events(head, footer);
            document.Open();
            fnEscribeResumenPDF(claveCliente, nombre, rfc, correo, document, dt);
        }
        catch (Exception de)
        {
            throw new DocumentException("Error ." + de.Message + " " + de.StackTrace);
        }
        try
        {
            // Cerramos el fichero
            document.Close();
            fnEscribeArchivo(archivo, nombreArchivo);
        }
        catch (Exception de)
        {
            throw new DocumentException("Error al Cerrar." + de.Message + " " + de.StackTrace);
        }
        return archivo;
    }
    public static MemoryStream GeneraComprobantePago(int clavePagoEnLinea, string nombreArchivo)
    {
        MemoryStream archivo = new MemoryStream();
        Rectangle pageSize = new Rectangle(PageSize.LETTER.Width, PageSize.LETTER.Height);
        Document document = new Document(pageSize, 25.0f, 25.0f, 80.0f, 62.0f);
        try
        {
            // Le decimos donde queremos escribir
            PdfWriter writer = PdfWriter.GetInstance(document, archivo);

            PdfPTable head = new PdfPTable(1);
            PdfPTable footer = new PdfPTable(1);

            string pie1 = "";
            string pie2 = "";
            string pie3 = "";
            string titulo1 = "";
            string titulo2 = "";
            string titulo3 = "";
            document.AddAuthor("Altum Technologies, S.A. de C.V.");
            document.AddCreationDate();
            document.AddTitle(titulo1 + " " + titulo2);
            PagoEnLinea.PagoEnLinea pagoEnLinea = new PagoEnLinea.PagoEnLinea(clavePagoEnLinea);
            LlenaEncabezadoPie(document, ref head, ref footer, "", titulo1, titulo2, titulo3, pie1, pie2, pie3);
            writer.PageEvent = new _events(head, footer);
            document.Open();
            fnEscribePDF(document, pagoEnLinea);
        }
        catch (Exception de)
        {
            throw new DocumentException("Error ." + de.Message + " " + de.StackTrace);
        }
        try
        {
            // Cerramos el fichero
            document.Close();
            fnEscribeArchivo(archivo, nombreArchivo);
        }
        catch (Exception de)
        {
            throw new DocumentException("Error al Cerrar." + de.Message + " " + de.StackTrace);
        }
        return archivo;
    }
    public static void fnEscribeResumenPDF(int claveCliente, string nombre, string rfc, string correo, Document document, DataTable dt)
    {
        float[] anchos = new float[4] { 11.0f, 51.0f, 5.0f, 33.0f };
        PdfPTable datos = new PdfPTable(anchos.Length);
        datos.SetWidths(anchos);
        datos.DefaultCell.Border = 0;
        datos.DefaultCell.BorderWidth = 0.0f;
        datos.WidthPercentage = 100.0f;
        Phrase frase;
        PdfPCell celda;
        BaseFont fuente = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        Font font = new Font(fuente, 8, Font.NORMAL);
        Font fontBold = new Font(fuente, 8, Font.BOLD);
        Font fontTitulo = new Font(fuente, 12, Font.BOLD);
        Font fontBlanco = new Font(fuente, 8, Font.NORMAL, BaseColor.WHITE);
        try
        {
            PdfPCell celdaVacia = CrearCelda(new Phrase("   ", font), 4);
            celdaVacia.Rowspan = 2;

            PdfPCell celdaSubrayada = CrearCelda(new Phrase("", font), 4);
            celdaSubrayada.Border = PdfPCell.BOTTOM_BORDER | PdfPCell.TOP_BORDER;
            celdaSubrayada.BorderWidthTop = 1.5f;       // Gris
            celdaSubrayada.BorderColorTop = colorGris;
            celdaSubrayada.BorderWidthBottom = 1.5f;    // Azul
            celdaSubrayada.BorderColorBottom = PDFPago.colorAzul;

            datos.AddCell(celdaVacia);
            datos.AddCell(celdaSubrayada);
            datos.AddCell(celdaVacia);
            //
            frase = new Phrase("Resumen mensual de contratos", fontTitulo);
            celda = CrearCelda(frase, 4);
            celda.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);
            // Linea 1
            frase = new Phrase("Fecha de emisión: ", fontBlanco);
            celda = CrearCelda(frase, 1);
            celda.BackgroundColor = colorAzul;
            datos.AddCell(celda);

            frase = new Phrase(DateTime.Now.ToString("dd MMM yyyy HH:mm").Replace(".", ""), font);
            celda = CrearCelda(frase, 3);
            celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            datos.AddCell(celda);
            // Linea 2
            frase = new Phrase("Cliente:", fontBlanco);
            celda = CrearCelda(frase, 1);
            celda.BackgroundColor = colorAzul;
            datos.AddCell(celda);
            frase = new Phrase(nombre, font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);
            frase = new Phrase("R.F.C.:", fontBlanco);
            celda = CrearCelda(frase, 1);
            celda.BackgroundColor = colorAzul;
            datos.AddCell(celda);
            frase = new Phrase(rfc, font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);
            // linea 3
            frase = new Phrase("Id Cliente:", fontBlanco);
            celda = CrearCelda(frase, 1);
            celda.BackgroundColor = colorAzul;
            datos.AddCell(celda);
            frase = new Phrase(claveCliente.ToString(), font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);
            frase = new Phrase("Email:", fontBlanco);
            celda = CrearCelda(frase, 1);
            celda.BackgroundColor = colorAzul;
            datos.AddCell(celda);
            frase = new Phrase(correo, font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);
            //
            datos.AddCell(celdaVacia);
            document.Add(datos);
            // Ahora la tabla
            fontBlanco.Size = 7.0f;
            anchos = new float[12] { 6.0f, 6.4f, 5.0f, 6.2f, 6.2f, 13.0f, 12.0f, 12.0f, 14.0f, 7.0f, 7.0f, 6.0f };
            string[] titulos = { "Contrato", "Monto a pagar", "Moneda", "Fecha de pago", "Servicio de domiciliación", "Beneficiario", "Pago a cuenta BANCO CITI MÉXICO", "Pago a cuenta BBVA", "Transferencia electrónica a BBVA", "Referencia Bancaria", "Status Contrato", "FechaFinMovimiento" };

            datos = new PdfPTable(anchos.Length);
            datos.SetWidths(anchos);
            datos.WidthPercentage = 100.0f;
            datos.DefaultCell.Border = 0;
            datos.DefaultCell.BorderWidth = 0.0f;
            int bordes = PdfPCell.BOTTOM_BORDER | PdfPCell.TOP_BORDER | PdfPCell.RIGHT_BORDER | PdfPCell.LEFT_BORDER;
            for (int i = 0; i < titulos.Length; i++)
            {
                celda = CrearCelda(titulos[i], fontBlanco, 1, PdfPCell.ALIGN_CENTER, bordes);
                celda.BackgroundColor = colorAzul;
                datos.AddCell(celda);
            }
            double montoTotal = 0;
            double montoAPagar = 0;
            DateTime fechaExigible = DateTime.Now;
            bool resp_Fecha = false;
            foreach (DataRow row in dt.Rows)
            {
                double.TryParse(row[1].ToString(), out montoAPagar);

                resp_Fecha = DateTime.TryParse(row[3].ToString(), out fechaExigible);
                montoTotal += montoAPagar;
                // 1
                celda = CrearCelda(row[0].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
                // 2
                celda = CrearCelda(montoAPagar.ToString("###,###,###,##0.00"), font, 1, PdfPCell.ALIGN_RIGHT, bordes);
                datos.AddCell(celda);
                //3
                celda = CrearCelda(row[2].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
                //4
                if (resp_Fecha)
                {
                    celda = CrearCelda(fechaExigible.ToString("dd/MM/yyyy"), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                    datos.AddCell(celda);
                }
                else 
                {
                    celda = CrearCelda(row[3].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                    datos.AddCell(celda);
                }

                
                //5 Domiciliado
                celda = CrearCelda(row[4].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
                //6
                celda = CrearCelda(row[5].ToString(), font, 1, PdfPCell.ALIGN_LEFT, bordes);
                datos.AddCell(celda);
                //7
                celda = CrearCelda(row[6].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
                //8
                celda = CrearCelda(row[7].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
                //9
                celda = CrearCelda(row[8].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
                //10
                celda = CrearCelda(row[9].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
                //11
                celda = CrearCelda(row[10].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
                //12
                celda = CrearCelda(row[11].ToString(), font, 1, PdfPCell.ALIGN_CENTER, bordes);
                datos.AddCell(celda);
            }
            //celda = CrearCelda("Total", fontBold, 1, PdfPCell.ALIGN_RIGHT, bordes);
            //datos.AddCell(celda);
            //celda = CrearCelda(montoTotal.ToString("###,###,###,##0.00"), fontBold, 1, PdfPCell.ALIGN_RIGHT, bordes);
            //datos.AddCell(celda);
            //frase = new Phrase("", font);
            //celda = CrearCelda(frase, 8);
            //datos.AddCell(celda);

            document.Add(datos);
        }
        catch (Exception ex)
        {
            string msg = ex.Message + ((ex.InnerException != null) ? "-" + ex.InnerException.Message : "");
            frase = new Phrase("Error: " + msg, new Font(font.BaseFont, 7.0f));
            celda = CrearCelda(frase, 3);
            datos.AddCell(celda);
            document.Add(datos);
            throw new Exception(msg);
        }
    }
    public static void fnEscribeArchivo(MemoryStream ms, string nombreArchivo)
    {
        Byte[] barchivo = ms.ToArray();
        FileStream fs = File.Open(nombreArchivo, FileMode.Create, System.IO.FileAccess.Write);
        try
        {
            fs.Write(barchivo, 0, barchivo.Length);
        }
        catch (Exception ex)
        {
            throw (new Exception(ex.Message));
        }
        finally
        {
            fs.Close();
        }
    }
    public static void fnEscribePDF(Document document, PagoEnLinea.PagoEnLinea pagoEnLinea)
    {
        float[] anchos = new float[3] { 30.0f, 45.0f, 25.0f };
        PdfPTable datos = new PdfPTable(anchos.Length);
        datos.SetWidths(anchos);
        datos.DefaultCell.Border = 0;
        datos.DefaultCell.BorderWidth = 0.0f;
        Phrase frase;
        PdfPCell celda;
        BaseFont fuente = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        Font font = new Font(fuente, 10, Font.NORMAL);
        Font fontBold = new Font(fuente, 11, Font.BOLD);
        Font fontBlanco = new Font(fuente, 11, Font.BOLD, BaseColor.WHITE);
        try
        {
            //datos.Border = 0;
            //datos.Cellpadding = 2;
            //datos.DefaultCell.Leading = 5.0f;
            //datos.Cellspacing = 0;
            //datos.DefaultCellBorder = Cell.NO_BORDER;
            //datos.DefaultCell.Leading = 10;

            PdfPCell celdaVacia = CrearCelda(new Phrase("   ", font), 3);
            celdaVacia.Rowspan = 2;

            PdfPCell celdaSubrayada = CrearCelda(new Phrase("", font), 3);
            celdaSubrayada.Border = PdfPCell.BOTTOM_BORDER | PdfPCell.TOP_BORDER;
            celdaSubrayada.BorderWidthTop = 1.5f;       // Gris
            celdaSubrayada.BorderColorTop = colorGris;
            celdaSubrayada.BorderWidthBottom = 1.5f;    // Azul
            celdaSubrayada.BorderColorBottom = PDFPago.colorAzul;

            datos.AddCell(celdaVacia);
            datos.AddCell(celdaSubrayada);
            datos.AddCell(celdaVacia);
            //
            frase = new Phrase("Comprobante de pago", fontBold);
            celda = CrearCelda(frase, 3);
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);
            //
            frase = new Phrase("ID: " + pagoEnLinea.ClavePagoEnLinea.ToString(), font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            frase = new Phrase(DateTime.Now.ToString("dd MMM yyyy HH:mm").Replace(".", ""), font);
            celda = CrearCelda(frase, 2);
            celda.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);
            //
            frase = new Phrase(pagoEnLinea.Cliente.nombre, font);
            celda = CrearCelda(frase, 3);
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);
            datos.AddCell(celdaSubrayada);
            datos.AddCell(celdaVacia);

            // linea 1
            frase = new Phrase("Tu pago ha sido aplicado.", fontBold);
            celda = CrearCelda(frase, 3);
            datos.AddCell(celda);

            frase = new Phrase("Número de autorización " + pagoEnLinea.Autorizacion, fontBold);
            celda = CrearCelda(frase, 3);
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);

            datos.AddCell(celdaSubrayada);
            datos.AddCell(celdaVacia);

            // linea 1
            frase = new Phrase("Contrato:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            frase = new Phrase(pagoEnLinea.Contrato, font);
            celda = CrearCelda(frase, 2);
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);
            // linea 2
            frase = new Phrase("Cuenta de retiro:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            frase = new Phrase(pagoEnLinea.CuentaPago, font);
            celda = CrearCelda(frase, 1);
            celda.Colspan = 2;
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);

            // linea 3
            frase = new Phrase("Titular:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            frase = new Phrase(pagoEnLinea.NombreClientePago, font);
            celda = CrearCelda(frase, 1);
            celda.Colspan = 2;
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);

            // linea 3
            frase = new Phrase("Cuenta de depósito:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            frase = new Phrase("NAVISTAR BANCOMER", font);
            //frase = new Phrase(pagoEnLinea.CuentaDeposito, font);
            celda = CrearCelda(frase, 2);
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);

            //
            frase = new Phrase("Importe:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            frase = new Phrase("$ " + pagoEnLinea.MontoAPagar.ToString("#,###,###,###,##0.00") + " (" + fnNumerosALetras(pagoEnLinea.MontoAPagar).ToUpper() + ")", font);
            celda = CrearCelda(frase, 2);
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);

            //
            frase = new Phrase("Forma de pago:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            string metodoPago = pagoEnLinea.MetodoPago;
            switch (metodoPago.ToUpper())
            {
                case "TDX":
                    metodoPago = "Visa/Mastercard";
                    break;
                case "CIE":
                    metodoPago = "Cheque electrónico Bancomer";
                    break;
                case "CLABE":
                    metodoPago = "CLABE Interbancaria";
                    break;
                case "SUC":
                    metodoPago = "Sucursal";
                    break;
                case "AMEX":
                    metodoPago = "American Express";
                    break;
                case "PCB":
                    metodoPago = "Practicaja Bancomer";
                    break;
                case "CIE_INTER":
                    metodoPago = "Transferencia interbancaria";
                    break;
            }
            frase = new Phrase(metodoPago, font);
            celda = CrearCelda(frase, 1);
            celda.Colspan = 2;
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);

            //
            frase = new Phrase("Banco emisor de la cuenta:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            string[] nombreBanco = pagoEnLinea.NombreBancoEmisor.Split('-');
            frase = new Phrase(nombreBanco[nombreBanco.Length-1], font);
            celda = CrearCelda(frase, 1);
            celda.Colspan = 2;
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);

            // linea 3
            frase = new Phrase("Fecha de aplicación:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            frase = new Phrase(pagoEnLinea.FechaAplicacion.ToString("dd MMM yyyy").Replace(".", ""), font);
            celda = CrearCelda(frase, 2);
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);

            // linea 4
            frase = new Phrase("Hora de aplicación:", font);
            celda = CrearCelda(frase, 1);
            datos.AddCell(celda);

            frase = new Phrase(pagoEnLinea.FechaAplicacion.ToString("HH:mm"), font);
            celda = CrearCelda(frase, 2);
            datos.AddCell(celda);

            datos.AddCell(celdaVacia);
            datos.AddCell(celdaVacia);
            datos.AddCell(celdaVacia);

            datos.AddCell(celdaSubrayada);
            frase = new Phrase("Este recibo es sólo de carácter informativo. No tiene validez oficial como comprobante legal o fiscal. Todos los Derechos Reservados Navistar Financial, S.A. de C.V., SOFOM, E.R. Para cualquier duda o aclaración comunícate con nosotros 018007000123.", new Font(font.BaseFont, 7.0f));
            celda = CrearCelda(frase, 3);
            datos.AddCell(celda);

            document.Add(datos);
        }
        catch(Exception ex)
        {
            string msg = ex.Message + ((ex.InnerException != null) ? "-" + ex.InnerException.Message:"");
            pagoEnLinea.Respuesta = msg;
            PagoEnLinea.PagoEnLinea.Actualizar(pagoEnLinea);

            frase = new Phrase("Error: " + msg, new Font(font.BaseFont, 7.0f));
            celda = CrearCelda(frase, 3);
            datos.AddCell(celda);
            document.Add(datos);
            throw new Exception(msg);
        }
    }
    public static PdfPCell CrearCelda(Phrase frase, int colspan)
    {
        PdfPCell celda = new PdfPCell(frase);
        celda.Colspan = colspan;
        celda.Border = 0;
        return celda;
    }
    public static PdfPCell CrearCelda(string texto, Font font, int colspan, int alineacion, int bordes)
    {
        Phrase frase = new Phrase(texto, font);
        PdfPCell celda = new PdfPCell(frase);
        celda.Colspan = colspan;
        celda.Border = bordes;
        celda.HorizontalAlignment = alineacion;
        return celda;
    }
    private class _events : PdfPageEventHelper
    {
        PdfPTable head = new PdfPTable(1);
        PdfPTable footer = new PdfPTable(1);

        public _events(PdfPTable head, PdfPTable footer)
        {
            this.head = head;
            this.footer = footer;
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            Rectangle page = document.PageSize;
            float aleft = 80.0f;
            float atop = page.Height - document.TopMargin + head.TotalHeight + 20.0f;
            //if (page.Width < page.Height)
            //{
            //    aleft = 25.0f;
            //    atop -= 5.0f;
            //}
            head.WriteSelectedRows(
                0, -1,          // first/last row; -1 flags all write all rows
                aleft, // left offset
                       // ** bottom** yPos of the table
                atop,
                writer.DirectContent
            );

            footer.WriteSelectedRows(
                0, -1,          // first/last row; -1 flags all write all rows
                aleft, // left offset
                       // ** bottom** yPos of the table
                document.BottomMargin - 20,
                writer.DirectContent
            );
        }
    }

    public static void LlenaEncabezadoPie(Document document, ref PdfPTable head, ref PdfPTable footer, string folio, string titulo1, string titulo2, string titulo3, string pie1, string pie2, string pie3)
    {
        BaseColor colorAzul = new BaseColor(0, 53, 95);

        Rectangle page = document.PageSize;

        int[] tablaWidths = new int[2] { 30, 70 };

        head = new PdfPTable(tablaWidths.Length);
        head.TotalWidth = page.Width - 160.0f;
        head.SetWidths(tablaWidths);

        string ruta = System.AppDomain.CurrentDomain.BaseDirectory;
        string nombreLogo = ruta + @"img\logonavistar.png";
        PdfPCell c = null;
        BaseFont fuente = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        if (File.Exists(nombreLogo))
        {
            Image imagen = Image.GetInstance(nombreLogo);
            c = new PdfPCell(imagen, true);
            c.HorizontalAlignment = Element.ALIGN_LEFT;
            c.VerticalAlignment = Element.ALIGN_TOP;
            c.Border = Rectangle.NO_BORDER;
            head.AddCell(c);
        }
        else
        {
            c = new PdfPCell(new Phrase(nombreLogo, new Font(fuente, 7, Font.BOLD)));
            c.HorizontalAlignment = Element.ALIGN_LEFT;
            c.VerticalAlignment = Element.ALIGN_TOP;
            c.Border = Rectangle.NO_BORDER;
            head.AddCell(c);
        }
        Font font = new Font(fuente, 4, Font.NORMAL);
        PdfPCell celdaSubrayada = CrearCelda(new Phrase(" ", font), 3);
        celdaSubrayada.Border = PdfPCell.BOTTOM_BORDER | PdfPCell.TOP_BORDER;
        celdaSubrayada.BorderWidthTop = 2.5f;       // Gris
        celdaSubrayada.BorderColorTop = colorGris;
        celdaSubrayada.BorderWidthBottom = 2.5f;    // Azul
        celdaSubrayada.BorderColorBottom = PDFPago.colorAzul;
        celdaSubrayada.HorizontalAlignment = Element.ALIGN_CENTER;
        celdaSubrayada.VerticalAlignment = Element.ALIGN_CENTER;

        font = new Font(fuente, 13, Font.NORMAL);
        // encabezado del centro
        PdfPTable headCentro = new PdfPTable(1);
        // add header text
        c = new PdfPCell(new Phrase("  ", font));
        c.Border = Rectangle.NO_BORDER;
        c.HorizontalAlignment = Element.ALIGN_CENTER;
        c.VerticalAlignment = Element.ALIGN_TOP;
        headCentro.AddCell(c);

        headCentro.AddCell(celdaSubrayada);

        // add header text
        c = new PdfPCell(new Phrase(" ", font));
        c.Border = Rectangle.NO_BORDER;
        c.HorizontalAlignment = Element.ALIGN_CENTER;
        c.VerticalAlignment = Element.ALIGN_TOP;
        //c.BackgroundColor = colorAzul;
        headCentro.AddCell(c);

        c = new PdfPCell(headCentro);
        c.Border = PdfPCell.NO_BORDER;
        head.AddCell(c);
        //// ahora el de la derecha
        //PdfPTable headDerecho = new PdfPTable(1);
        //headDerecho.SetWidths(new int[1] { 100 });
        //c = new PdfPCell(new Phrase("", font));
        //c.Border = Rectangle.NO_BORDER;
        //c.HorizontalAlignment = Element.ALIGN_RIGHT;
        //headDerecho.AddCell(c);
        //c = new PdfPCell(headDerecho);
        //c.Border = PdfPCell.NO_BORDER;
        //head.AddCell(c);

        // pie de pógina
        footer = new PdfPTable(1);
        footer.TotalWidth = head.TotalWidth;
    }
    public static string fnNumerosALetras(decimal valorConvertir)
    {
        //format the incoming number to guarantee six digits'to the left of the decimal point and two to the right'
        //and then separate the dollars from the cents
        string valor = valorConvertir.ToString("####000,000,000,000.00");
        int pos = valor.IndexOf(".");
        string sEnteros = valor.Substring(0, pos);
        long enteros = long.Parse(sEnteros.Replace(",", ""));
        string sCentavos = valor.Substring(pos + 1, 2);
        int centavos = int.Parse(sCentavos);
        string mensaje = "";
        string letras = "";
        int numero = 0;
        if (sEnteros.Length > 15)
        {
            mensaje = "El monto es muy grande";
        }
        else
        {
            string de = "de";
            if (enteros == 0)
            {
                letras = "Cero";
                de = "";
            }
            else
            {
                //Miles de Millon
                int numeroMilesMillon = int.Parse(sEnteros.Substring(0, 3));
                if (numeroMilesMillon > 0)
                {
                    letras = ConvierteGrupo(numeroMilesMillon);
                    letras += " mil ";
                }
                //Millones
                numero = int.Parse(sEnteros.Substring(4, 3));
                if (numero > 0)
                {
                    letras += ConvierteGrupo(numero);
                    if (numero > 1)
                        letras += " millones ";
                    else
                        letras += " millón ";
                }
                else if (numeroMilesMillon > 0)
                {
                    letras += " millones ";
                }
                //Miles
                numero = int.Parse(sEnteros.Substring(8, 3));
                if (numero > 0)
                {
                    de = "";
                    letras += ConvierteGrupo(numero) + " mil ";
                }
                //centenas
                numero = int.Parse(sEnteros.Substring(12, 3));
                if (numero > 0)
                {
                    de = "";
                    letras += ConvierteGrupo(numero);
                }
            }
            if (Math.Truncate(valorConvertir).ToString("#################0") == "1")
                letras += " " + de + " peso ";
            else
                letras += " " + de + " pesos ";
            //centavos
            if (centavos == 0)
                letras += "00";
            else
                letras += centavos.ToString("00");
            letras = letras.Trim().Replace("  ", " ");
            letras += "/100 M.N.";
            return letras.Substring(0, 1).ToUpper() + letras.Substring(1).Replace("  ", " ");
        }
        return mensaje;
    }
    public static string ConvierteGrupo(int numero)
    {
        ArrayList grandes = new ArrayList();
        ArrayList aDecenas = new ArrayList();
        ArrayList aCentenas = new ArrayList();
        ArrayList chicos = new ArrayList();
        grandes.Add("");
        grandes.Add("Diez");
        grandes.Add("Veinte");
        grandes.Add("Treinta");
        grandes.Add("Cuarenta");
        grandes.Add("Cincuenta");
        grandes.Add("Sesenta");
        grandes.Add("Setenta");
        grandes.Add("Ochenta");
        grandes.Add("Noventa");

        aDecenas.Add("");
        aDecenas.Add("Dieci");
        aDecenas.Add("Veinti");
        aDecenas.Add("Treinta y ");
        aDecenas.Add("Cuarenta y ");
        aDecenas.Add("Cincuenta y ");
        aDecenas.Add("Sesenta y ");
        aDecenas.Add("Setenta y ");
        aDecenas.Add("Ochenta y ");
        aDecenas.Add("Noventa y ");

        aCentenas.Add("");
        aCentenas.Add("Ciento");
        aCentenas.Add("Doscientos");
        aCentenas.Add("Trescientos");
        aCentenas.Add("Cuatrocientos");
        aCentenas.Add("Quinientos");
        aCentenas.Add("Seiscientos");
        aCentenas.Add("Setecientos");
        aCentenas.Add("Ochocientos");
        aCentenas.Add("Novecientos");

        for (int i = 0; i < 20; i++) chicos.Add("");
        chicos[1] = "Un";
        chicos[2] = "Dos";
        chicos[3] = "Tres";
        chicos[4] = "Cuatro";
        chicos[5] = "Cinco";
        chicos[6] = "Seis";
        chicos[7] = "Siete";
        chicos[8] = "Ocho";
        chicos[9] = "Nueve";
        chicos[10] = "Diez";
        chicos[11] = "Once";
        chicos[12] = "Doce";
        chicos[13] = "Trece";
        chicos[14] = "Catorce";
        chicos[15] = "Quince";
        chicos[16] = "Dieciseis";
        chicos[17] = "Diecisiete";
        chicos[18] = "Dieciocho";
        chicos[19] = "Diecinueve";

        string parte = numero.ToString("000");
        int centenas = int.Parse(parte.Substring(0, 1));
        string letras = "";
        int digitoDecena = 0;
        int digitoUnidad = 0;
        int decenas = int.Parse(parte.Substring(1, 2));
        // centenas
        if (centenas > 0)
        {
            if (centenas == 1 && decenas == 0)
                letras = "cien";
            else
                letras = aCentenas[centenas].ToString() + " ";
        }
        // decenas
        digitoDecena = int.Parse(parte.Substring(1, 1));
        if (decenas > 19)
        {
            digitoUnidad = int.Parse(parte.Substring(2, 1));
            if (digitoUnidad == 0) letras += " " + grandes[digitoDecena];
            else letras += " " + aDecenas[digitoDecena] + (chicos[digitoUnidad]).ToString().ToLower();
        }
        else
        {
            if (decenas == 1) letras += "Un";
            else if (decenas == 10) letras += "Diez";
            else letras += chicos[decenas];
        }
        return letras;
    }
}