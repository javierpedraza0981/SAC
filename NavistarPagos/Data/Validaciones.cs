using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavistarPagos.Data
{
    public class Validaciones
    {

        public void Log_Diario(string psModulo, string psMensaje)
        {
            try
            {
                string sFecha = DateTime.Now.ToString();
                string sFecLog = DateTime.Now.ToString("yyyy-MM-dd");
                string sPath = Path.Combine(ConfigurationManager.AppSettings.Get("rutaLog"), $"Log");
                TextWriter sw;

                if (!Directory.Exists(sPath)) { Directory.CreateDirectory(sPath); }
                sPath += "\\NavistarPagos_" + sFecLog + ".log";
                sw = File.AppendText(sPath);
                sw.WriteLine(sFecha + " -->  " + psModulo);
                sw.WriteLine(psMensaje);
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch { }
        }        

    }
}
