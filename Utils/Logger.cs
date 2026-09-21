using System;
using System.IO;

namespace D007Adventure
{
    // ============================================================
    // LOGGER
    // ============================================================
    //
    // Der Logger ist dafür zuständig, Meldungen über den
    // Programmablauf zu speichern.
    //
    // Ein Logger hilft uns später herauszufinden:
    //
    // - Was ist im Programm passiert?
    // - Wo ist ein Fehler aufgetreten?
    // - Wann ist ein Fehler aufgetreten?
    // - Welche Klasse hat den Fehler verursacht?
    //
    // Die Meldungen werden gleichzeitig:
    //
    // 1. in der Console
    // 2. in einer Log-Datei
    //
    // gespeichert.
    //
    // ============================================================

    public static class Logger
    {
        // ========================================================
        // LOG-ORDNER
        // ========================================================
        //
        // Hier werden unsere Log-Dateien gespeichert.
        //
        // Beispiel:
        //
        //     D007Adventure/
        //         Logs/
        //             2026-09-17.log
        //
        // ========================================================

        private static readonly string logDirectory =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Logs"
            );


        // ========================================================
        // LOG-DATEI
        // ========================================================
        //
        // Für jeden Tag wird eine eigene Log-Datei erstellt.
        //
        // Beispiel:
        //
        //     2026-09-17.log
        //
        // ========================================================

        private static readonly string logFile =
            Path.Combine(
                logDirectory,
                $"{DateTime.Now:yyyy-MM-dd}.log"
            );


        // ========================================================
        // INITIALISIERUNG
        // ========================================================
        //
        // Diese Methode sollte einmal beim Start des Programms
        // aufgerufen werden.
        //
        // Beispiel:
        //
        //     Logger.Initialize();
        //
        // Danach kann der Logger überall verwendet werden.
        //
        // ========================================================

        public static void Initialize()
        {
            try
            {
                // Erstellt den Logs-Ordner, falls er noch nicht
                // existiert.
                Directory.CreateDirectory(logDirectory);

                // Schreibt eine Trennlinie in die Log-Datei,
                // damit man einen neuen Programmstart erkennen kann.
                WriteToFile("");
                WriteToFile("============================================================");
                WriteToFile($"Programmstart: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                WriteToFile("============================================================");

                Info("Logger wurde gestartet.");
            }
            catch (Exception exception)
            {
                // Falls bereits das Erstellen des Logs fehlschlägt,
                // können wir den Fehler zumindest in der Console
                // anzeigen.
                Console.WriteLine(
                    $"[LOGGER FEHLER] {exception.Message}"
                );
            }
        }


        // ========================================================
        // INFO
        // ========================================================
        //
        // Für normale Informationen über den Programmablauf.
        //
        // Beispiel:
        //
        //     Logger.Info("Spiel wurde gestartet.");
        //
        // ========================================================

        public static void Info(string message)
        {
            Write(
                "INFO",
                message
            );
        }


        // ========================================================
        // WARNING
        // ========================================================
        //
        // Für Situationen, die kein richtiger Fehler sind,
        // aber trotzdem wichtig sein können.
        //
        // Beispiel:
        //
        //     Logger.Warning("Datei wurde nicht gefunden.");
        //
        // ========================================================

        public static void Warning(string message)
        {
            Write(
                "WARNING",
                message
            );
        }


        // ========================================================
        // ERROR
        // ========================================================
        //
        // Für Fehler, bei denen das Programm eventuell
        // weiterlaufen kann.
        //
        // Beispiel:
        //
        //     Logger.Error("Renderer konnte nicht gestartet werden.");
        //
        // ========================================================

        public static void Error(string message)
        {
            Write(
                "ERROR",
                message
            );
        }


        // ========================================================
        // EXCEPTION
        // ========================================================
        //
        // Diese Methode verwenden wir für echte C#-Exceptions.
        //
        // Beispiel:
        //
        //     catch (Exception exception)
        //     {
        //         Logger.Exception(exception);
        //     }
        //
        // Dadurch wird neben der Fehlermeldung auch der komplette
        // StackTrace gespeichert.
        //
        // Der StackTrace zeigt uns später, wo genau der Fehler
        // im Programm aufgetreten ist.
        //
        // ========================================================

        public static void Exception(Exception exception)
        {
            string message =
                $"Exception: {exception.Message}" +
                Environment.NewLine +
                $"StackTrace: {exception.StackTrace}";

            Write(
                "ERROR",
                message
            );
        }


        // ========================================================
        // WRITE
        // ========================================================
        //
        // Diese Methode kümmert sich um das eigentliche Schreiben.
        //
        // Alle anderen Methoden wie Info(), Warning() und Error()
        // benutzen diese Methode.
        //
        // ========================================================

        private static void Write(
            string type,
            string message)
        {
            string timestamp =
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

            string logMessage =
                $"[{timestamp}] [{type}] {message}";


            // ====================================================
            // CONSOLE
            // ====================================================
            //
            // Die Meldung wird sofort in der Console angezeigt.
            //
            // ====================================================

            Console.WriteLine(logMessage);


            // ====================================================
            // LOG-DATEI
            // ====================================================
            //
            // Zusätzlich wird die Meldung in die Log-Datei
            // geschrieben.
            //
            // ====================================================

            WriteToFile(logMessage);
        }


        // ========================================================
        // WRITE TO FILE
        // ========================================================
        //
        // Schreibt eine einzelne Zeile in die Log-Datei.
        //
        // Append bedeutet:
        //
        //     Die vorhandenen Einträge bleiben erhalten.
        //
        // Die neue Meldung wird hinten angehängt.
        //
        // ========================================================

        private static void WriteToFile(string message)
        {
            try
            {
                File.AppendAllText(
                    logFile,
                    message + Environment.NewLine
                );
            }
            catch (Exception exception)
            {
                // Wenn die Log-Datei nicht geschrieben werden kann,
                // geben wir den Fehler trotzdem in der Console aus.
                //
                // Wichtig:
                //
                // Wir dürfen hier nicht wieder Logger.Error()
                // verwenden.
                //
                // Sonst würde Logger.Error() wieder versuchen,
                // in die kaputte Log-Datei zu schreiben.
                Console.WriteLine($"[LOGGER FEHLER] Log-Datei konnte nicht geschrieben werden: {exception.Message}");
            }
        }
    }
}