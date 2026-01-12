using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ApiFacturacion.Utils {
    public static class Logger {
        private static readonly string logDirectory = "/var/www/serviciosBackendCsharp";
        private static readonly string logFile = Path.Combine(logDirectory, "backend.log");

        public static void Log(string message, Exception ex = null) {
            try {
                // Solo loguea si está en Linux
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    return; // en Windows no hace nada
                }

                if (!Directory.Exists(logDirectory)) {
                    Directory.CreateDirectory(logDirectory);
                }

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                if (ex != null) {
                    logMessage += Environment.NewLine + $"Error: {ex.Message}" +
                                  Environment.NewLine + $"StackTrace: {ex.StackTrace}";
                }

                using (var writer = new StreamWriter(logFile, append: true)) {
                    writer.WriteLine(logMessage);
                    writer.WriteLine(new string('-', 80));
                }
            } catch {
                // Nunca dejar que un fallo del logger rompa la API
            }
        }
    }
}
