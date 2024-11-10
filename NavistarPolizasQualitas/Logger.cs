using System;
using System.IO;

namespace NavistarPolizasQualitas
{
    public class Logger
    {
        private readonly string logFilePath;

        public Logger(string logFileName = "log.txt")
        {
            // Establece la ruta del archivo de log
            logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);
        }

        public void Log(string message)
        {
            try
            {
                // Agrega el mensaje al archivo de log con la fecha y hora actual
                using (StreamWriter sw = File.AppendText(logFilePath))
                {
                    sw.WriteLine($"{DateTime.Now} - {message}");
                }
            }
            catch (Exception ex)
            {
                // Si hay un error al escribir en el log, puedes manejarlo de alguna manera.
                Console.WriteLine($"Error al escribir en el log: {ex.Message}");
            }
        }


    }
}
