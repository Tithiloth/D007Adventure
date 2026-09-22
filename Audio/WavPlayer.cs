using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace D007Adventure.Audio
{
    // Spielt WAV-Dateien über die Windows Multimedia API ab.
    // Benötigt keine zusätzlichen Bibliotheken.
    public class WavPlayer
    {
        // Windows Multimedia API.
        // Damit können WAV-Dateien direkt von Windows abgespielt werden.
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern int mciSendString(
            string command,
            StringBuilder returnString,
            int returnLength,
            IntPtr callback);

        private string currentAlias = string.Empty;

        private bool loopEnabled;
        private string currentFilePath = string.Empty;

        private bool isPlaying;

        // Gibt zurück, ob aktuell eine WAV-Datei abgespielt wird.
        public bool IsPlaying
        {
            get { return isPlaying; }
        }

        /// Spielt eine WAV-Datei ab.
        // <param name="filePath">Pfad zur WAV-Datei.</param>
        // <param name="loop">Wenn true, wird die Datei wiederholt.</param>
        public void Play(string filePath, bool loop = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.Error("WAV-Dateipfad ist leer.");
                return;
            }

            if (!File.Exists(filePath))
            {
                Logger.Error("WAV-Datei nicht gefunden: " + filePath);
                Logger.Error("Aktuelles Arbeitsverzeichnis: " +
                             Environment.CurrentDirectory);
                return;
            }

            Stop();

            currentAlias = "wavPlayer";
            currentFilePath = Path.GetFullPath(filePath);
            loopEnabled = loop;

            string command =
                "open \"" +
                currentFilePath +
                "\" type waveaudio alias " +
                currentAlias;

            int result = SendCommand(command);

            if (result != 0)
            {
                Logger.Error(
                    "WAV-Datei konnte nicht geöffnet werden: " +
                    filePath);

                currentAlias = string.Empty;
                currentFilePath = string.Empty;

                return;
            }

            result = SendCommand(
                "play " + currentAlias);

            if (result != 0)
            {
                Logger.Error(
                    "WAV-Datei konnte nicht abgespielt werden: " +
                    filePath);

                CloseCurrentSound();
                return;
            }

            isPlaying = true;

            Logger.Info("WAV wird abgespielt: " + filePath);
        }

        // Führt einen MCI-Befehl aus und gibt bei einem Fehler
        // die genaue Windows-Fehlermeldung aus.
        private int SendCommand(string command)
        {
            int result = mciSendString(
                command,
                null,
                0,
                IntPtr.Zero);

            if (result != 0)
            {
                StringBuilder errorMessage =
                    new StringBuilder(256);

                mciGetErrorString(
                    result,
                    errorMessage,
                    errorMessage.Capacity);

                Logger.Error(
                    "MCI-Fehler bei '" +
                    command +
                    "': " +
                    errorMessage);
            }

            return result;
        }

        // Pausiert die aktuelle WAV-Datei.
        public void Pause()
        {
            if (string.IsNullOrEmpty(currentAlias))
            {
                return;
            }

            int result = SendCommand(
                "pause " + currentAlias);

            if (result == 0)
            {
                isPlaying = false;
                Logger.Info("WAV pausiert.");
            }
        }

        // Setzt eine pausierte WAV-Datei fort.
        public void Resume()
        {
            if (string.IsNullOrEmpty(currentAlias))
            {
                return;
            }

            int result = SendCommand(
                "resume " + currentAlias);

            if (result == 0)
            {
                isPlaying = true;
                Logger.Info("WAV fortgesetzt.");
            }
        }

        // Stoppt die aktuelle WAV-Datei vollständig.
        public void Stop()
        {
            if (string.IsNullOrEmpty(currentAlias))
            {
                return;
            }

            SendCommand(
                "stop " + currentAlias);

            CloseCurrentSound();

            Logger.Info("WAV gestoppt.");
        }

        // Schließt die aktuell geöffnete WAV-Datei.
        private void CloseCurrentSound()
        {
            if (string.IsNullOrEmpty(currentAlias))
            {
                return;
            }

            SendCommand(
                "close " + currentAlias);

            currentAlias = string.Empty;
            isPlaying = false;
        }

        // Gibt die Windows-Ressourcen des Players frei.
        public void Dispose()
        {
            Stop();

            Logger.Info("WavPlayer wurde beendet.");
        }

        // Liefert die genaue Fehlermeldung eines MCI-Fehlercodes.
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern bool mciGetErrorString(
            int errorCode,
            StringBuilder errorText,
            int errorTextLength);
    }
}