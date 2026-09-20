using System;
using System.IO;
using System.Text.Json;

namespace D007Adventure.Utils
{
    // ============================================================
    // JSON PARSER
    // ============================================================
    //
    // Diese Klasse übernimmt das Laden und Speichern von
    // Objekten im JSON-Format.
    //
    // Wir verwenden System.Text.Json.
    // Dadurch benötigen wir keine externe Bibliothek.
    //
    // Die Klasse kann später zum Beispiel für:
    //
    // - Einstellungen
    // - Spielstände
    // - Charakterdaten
    // - Item-Daten
    // - Gegner-Daten
    // - Konfigurationen
    //
    // verwendet werden.
    // ============================================================

    public static class JsonParser
    {
        // ========================================================
        // JSON OPTIONEN
        // ========================================================

        // Diese Optionen gelten für alle JSON-Dateien.
        //
        // WriteIndented sorgt dafür, dass die JSON-Datei
        // übersichtlich formatiert gespeichert wird.
        private static readonly JsonSerializerOptions options =
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

        // ========================================================
        // LOAD
        // ========================================================
        //
        // Lädt eine JSON-Datei und wandelt sie in den angegebenen
        // Datentyp um.
        //
        // Beispiel:
        //
        // Settings settings =
        //     JsonParser.Load<Settings>(
        //         "Assets/Config/settings.json"
        //     );
        // ========================================================

        public static T Load<T>(string path)
        {
            // Prüfen, ob der Pfad überhaupt angegeben wurde.
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "Der JSON-Pfad darf nicht leer sein.",
                    nameof(path)
                );
            }


            // Prüfen, ob die Datei existiert.
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"JSON-Datei wurde nicht gefunden: {path}",
                    path
                );
            }

            try
            {
                // Die komplette JSON-Datei einlesen.
                string json = File.ReadAllText(path);


                // JSON in unser gewünschtes Objekt umwandeln.
                T? result =
                    JsonSerializer.Deserialize<T>(
                        json,
                        options
                    );


                // Falls die JSON-Datei keinen gültigen Wert
                // erzeugt hat, geben wir einen Fehler aus.
                if (result == null)
                {
                    throw new Exception(
                        $"Die JSON-Datei konnte nicht geladen werden: {path}"
                    );
                }

                Logger.Info(
                    $"JSON geladen: {path}"
                );

                return result;
            }
            catch (JsonException exception)
            {
                Logger.Error(
                    $"Ungültiges JSON in '{path}': {exception.Message}"
                );

                throw;
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"Fehler beim Laden von JSON '{path}': {exception.Message}"
                );

                throw;
            }
        }

        // ========================================================
        // SAVE
        // ========================================================
        //
        // Speichert ein Objekt als JSON-Datei.
        //
        // Beispiel:
        //
        // JsonParser.Save(
        //     "Assets/Config/settings.json",
        //     settings
        // );
        // ========================================================

        public static void Save<T>(
            string path,
            T data)
        {
            // Prüfen, ob der Pfad angegeben wurde.
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "Der JSON-Pfad darf nicht leer sein.",
                    nameof(path)
                );
            }

            // ----------------------------------------------------
            // Übergeordneten Ordner ermitteln.
            // ----------------------------------------------------

            string? directory =
                Path.GetDirectoryName(path);

            // ----------------------------------------------------
            // Falls der Ordner noch nicht existiert,
            // wird er automatisch erstellt.
            // ----------------------------------------------------

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(
                    directory
                );
            }


            try
            {
                // Objekt in einen JSON-String umwandeln.
                string json =
                    JsonSerializer.Serialize(
                        data,
                        options
                    );


                // JSON-Datei speichern.
                File.WriteAllText(
                    path,
                    json
                );


                Logger.Info(
                    $"JSON gespeichert: {path}"
                );
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"Fehler beim Speichern von JSON '{path}': {exception.Message}"
                );

                throw;
            }
        }

        // ========================================================
        // EXISTS
        // ========================================================
        //
        // Prüft, ob eine JSON-Datei vorhanden ist.
        //
        // Das ist besonders praktisch für Spielstände.
        // ========================================================

        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        // ========================================================
        // DELETE
        // ========================================================
        //
        // Löscht eine JSON-Datei, falls sie existiert.
        // ========================================================

        public static void Delete(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            File.Delete(path);

            Logger.Info(
                $"JSON gelöscht: {path}"
            );
        }
    }
}