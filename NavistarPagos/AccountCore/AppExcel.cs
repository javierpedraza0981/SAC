using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;

public class AppExcel
{
    public AppExcel()
    {
    }
    public static MemoryStream GeneraResumen(int claveCliente, string nombre, string rfc, string correo, DataTable dt, ref string mensaje)
    {
        MemoryStream ms = new MemoryStream();
        int[] acols = new int[10];
        using (ExcelPackage xlApp = new ExcelPackage(ms))
        {
            ExcelWorkbook wb = null;
            ExcelWorksheet ws = null;
            try
            {
                wb = xlApp.Workbook;
                if (wb.Worksheets.Count == 0)   // se inserta una hoja en blanco para que no devuelva error
                {
                    wb.Worksheets.Add("Resumen");
                }
                ws = wb.Worksheets[1];
                fnConfiguraHoja(wb, ws, 1);
                float ancho = 15.0f;
                for (int i = 1; i <= 11; i++)
                {
                    if (i < 6) ancho = 15.0f;
                    else if (i == 6) ancho = 35.0f;
                    else if (i < 10) ancho = 30.0f;
                    else ancho = 20.0f;
                    ws.Column(i).Width = ancho;
                }
                ws.SetValue(1, 1, "Resumen mensual de contratos");
                fnFormateaTitulo(ws, "A1", "K1");
                ws.SetValue(2, 1, "Fecha de emisión:");
                ws.SetValue(2, 2, DateTime.Now.ToString("dd/MMM/yyyy HH:mm"));

                ws.SetValue(3, 1, "Cliente:");
                fnMezclaCeldas(ws, "B3", "D3");
                ws.SetValue(3, 2, nombre);

                ws.SetValue(3, 6, "R.F.C.:");
                ws.SetValue(3, 7, rfc);

                ws.SetValue(4, 1, "Id Cliente:");
                ws.SetValue(4, 2, claveCliente);

                ws.SetValue(4, 6, "Email:");
                ws.SetValue(4, 7, correo);
                fnMezclaCeldas(ws, "G4", "J4");

                int rng = 6;
                int col = 0;
                string[] titulos = { "Contrato", "Monto a pagar", "Moneda", "Fecha de pago", "Servicio de domiciliación", "Beneficiario", "Pago a cuenta BANAMEX", "Pago a cuenta BBVA", "Transferencia electrónica a BBVA", "Referencia Bancaria", "Status Contrato", "Fecha FinMov" };
                for (int i = 0; i < titulos.Length; i++)
                {
                    ws.SetValue(rng, col + 1, titulos[i]);
                    fnFormateaTitulo(ws, (Char)('A' + col) + rng.ToString(), (Char)('A' + col) + (rng).ToString());
                    col++;
                }
                double montoTotal = 0;
                double montoAPagar = 0;
                DateTime fechaExigible = DateTime.Now;
                bool resp_Fecha = false;

                foreach (DataRow row in dt.Rows)
                {
                    rng++;
                    double.TryParse(row[1].ToString(), out montoAPagar);

                    resp_Fecha = DateTime.TryParse(row[3].ToString(), out fechaExigible);


                    montoTotal += montoAPagar;
                    ws.SetValue(rng, 1, row[0].ToString());
                    ws.SetValue(rng, 2, montoAPagar.ToString("###,###,###,##0.00"));
                    ws.SetValue(rng, 3, row[2].ToString());
                    if (resp_Fecha) ws.SetValue(rng, 4, fechaExigible.ToString("dd/MM/yyyy"));
                    else ws.SetValue(rng, 4, row[3].ToString());
                    ws.SetValue(rng, 5, row[4].ToString());
                    ws.SetValue(rng, 6, row[5].ToString());
                    ws.SetValue(rng, 7, row[6].ToString());
                    ws.SetValue(rng, 8, row[7].ToString());
                    ws.SetValue(rng, 9, row[8].ToString());
                    ws.SetValue(rng, 10, row[9].ToString());
                    ws.SetValue(rng, 11, row[10].ToString());
                    ws.SetValue(rng, 12, row[11].ToString());
                }
                rng ++;
                //ws.SetValue(rng, 1, "Total");
                //ws.SetValue(rng, 2, montoTotal.ToString("###,###,###,##0.00"));
                //fnFormateaTitulo(ws, 'A' + rng.ToString(), 'A' + rng.ToString());
                //fnFormateaTitulo(ws, 'B' + rng.ToString(), 'B' + rng.ToString());
                // alinea a la derecha el monto
                ExcelRange rango = ws.Cells["B7:B"+rng];
                rango.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                xlApp.Save();
                CierraTodo(xlApp, wb, ws);
            }
        }
        return ms;
    }
    public static void CierraTodo(ExcelPackage xlApp, ExcelWorkbook wb, ExcelWorksheet ws)
    {
        if (xlApp != null) xlApp.Dispose();
        ws = null;
        wb = null;
        xlApp = null;
        System.GC.Collect();
    }
    public static void fnConfiguraHoja(ExcelWorkbook wb, ExcelWorksheet ws, int hoja)
    {
        //ws.Select(Type.Missing);
        ws.Cells.Style.Font.Name = "Arial";
        ws.Cells.Style.Font.Size = 9.0f;
        ws.Cells.Worksheet.DefaultColWidth = 20.0;
    }
    public static void fnMezclaCeldas(ExcelWorksheet ws, string celda1, string celda2)
    {
        ExcelRange rango = ws.Cells[celda1];
        ws.Select(celda2);
        rango = ws.SelectedRange;
        if (celda1 != celda2)
        {
            rango.Merge = true;
            rango.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            rango.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
        }
        rango.Style.WrapText = true;
    }
    public static void fnFormateaTitulo(ExcelWorksheet ws, string celda1, string celda2)
    {
        ExcelRange rango = null;
        try
        {
            rango = ws.Cells[celda1 + ":" + celda2];
            rango.Style.Font.Name = "Arial";
            rango.Style.Font.Size = 11.0f;
            rango.Style.Font.Bold = true;
            rango.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            rango.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            rango.Style.Font.Color.SetColor(System.Drawing.Color.White);
            rango.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            rango.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 53, 95));
            if (celda1 != celda2)
            {
                rango.Merge = true;
            }
            rango.Style.WrapText = true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    public static void fnFormateaRango(ExcelWorksheet ws, int caso, int rng1, int col1, int rng2, int col2)
    {
        ExcelRange rango = ws.Cells[rng1, col1, rng2, col2];
        rango.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        rango.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 53, 95));
        rango.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium, System.Drawing.Color.Black);
        rango.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
        rango.Style.Font.Bold = true;
        rango.Style.Font.Color.SetColor(System.Drawing.Color.White);
        if (caso == 1)
        {
            rango.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            rango.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        }
        rango.Style.WrapText = true;
    }
    public static void fnBordeRango(ExcelWorksheet ws, string celda1, string celda2)
    {
        ExcelRange rango = ws.Cells[celda1];
        ws.Select(celda2);
        rango = ws.SelectedRange;
        rango.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium, System.Drawing.Color.Black);
        rango.Style.WrapText = true;
    }
    public string FmtDecimal(object valor)
    {
        decimal dValor = 0;
        decimal.TryParse(valor.ToString(), out dValor);
        return dValor.ToString("###,###,###,##0.00");
    }
    public string FmtDecimal(object valor, string formato)
    {
        decimal dValor = 0;
        decimal.TryParse(valor.ToString(), out dValor);
        return dValor.ToString(formato);
    }
    public static byte[] ConvierteStreamToByte(Stream str)
    {
        MemoryStream ms = new MemoryStream();
        try
        {
            str.Position = 0;
            str.CopyTo(ms);
        }
        catch { }
        return ms.ToArray();
    }
}