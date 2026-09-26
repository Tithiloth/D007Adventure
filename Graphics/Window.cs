using System;
using System.Runtime.InteropServices;

namespace D007Adventure.Graphics
{
    //============================================================
    // WINDOW Mode
    //============================================================
    // 
    // Diese Emumeration beschreibt die drei möglichen
    // Darstellungsmodi unseres Spielensters.
    //
    // Windowed: Normales Fenster mit Rahmen und Titelleiste.
    //
    // Borderless: Fenster ohne Rahmen und Titelleiste.
    //
    // Fullscreen: Das Fenster nimmt den gesamten Bildschirm ein.
    //=============================================================

    public enum WindowMode
    {
        Windowed,
        Borderless,
        Fullscreen
    }

    //============================================================
    // WINDOW
    //============================================================
    //
    // Diese Klasse erstellt und verwaltet unser Spielfenster.
    //
    // Das Fenster wird nicht durch eine externe Bibliothek
    // erstellt.
    //
    // Wir benutzen direkt die Windows-API.
    //============================================================

    public class Window
    {
        //========================================================
        // WINDOWS-KONSTANTEN
        //========================================================

        private const int CS_HREDRAW = 0x0002;
        private const int CS_VREDRAW = 0x0001;

        //--------------------------------------------------------
        // Windows Konstanten für Fensterstile.
        //--------------------------------------------------------

        // Normales Windowsfenster
        //
        // Titelleiste, Rahmen, Fensterrand.
        // Minimieren, Maximieren, Schließen.
        private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;

        // Fenster ohne Rahmen, Titelleiste und Fensterrand.
        //
        // Dieses Fenster wird für den Borderless-Modus und Vollbild
        // verwendet.
        private const uint WS_POPUP = 0x80000000;

        //--------------------------------------------------------
        // ShowWindow() Konstanten.
        //--------------------------------------------------------
        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;

        //--------------------------------------------------------
        // PeekMessage() Konstanten.
        //--------------------------------------------------------
        private const uint PM_REMOVE = 0x0001;

        //--------------------------------------------------------
        // Windows Nachrichten.
        //--------------------------------------------------------
        private const uint WM_QUIT = 0x0012;
        private const uint WM_CLOSE = 0x0010;
        private const uint WM_DESTROY = 0x0002;

        //--------------------------------------------------------
        // Windows Cursor Konstanten.
        //--------------------------------------------------------
        private const int IDC_ARROW = 32512;

        //--------------------------------------------------------
        // Windows System Metrics Konstanten.
        // (Bildschirmauflösung)
        //--------------------------------------------------------
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        //--------------------------------------------------------
        // SetWindowPos() Konstanten.
        //--------------------------------------------------------

        // SWP_NOMOVE bedeutet:
        // Die Position des Fensters wird nicht verändert.
        private const uint SWP_NOMOVE = 0x0002;

        // SWP_NOSIZE bedeutet:
        // Die Größe des Fensters wird nicht verändert.
        private const uint SWP_NOSIZE = 0x0001;

        // SWP_NOZORDER bedeutet:
        // Die Z-Reihenfolge des Fensters wird nicht verändert.
        private const uint SWP_NOZORDER = 0x0004;

        // Windows soll Änderungen am Fensterrahmen sofort berrechnen.
        private const uint SWP_FRAMECHANGED = 0x0020;

        // Fenster nach der Änderung anzeigen.
        private const uint SWP_SHOWWINDOW = 0x0040;

        //--------------------------------------------------------
        // SetWindowLongPtr() Konstanten.
        //--------------------------------------------------------

        // GWL_STYLE bedeutet:
        // Wir wollen den Fensterstil ändern.
        private const int GWL_STYLE = -16;

        //--------------------------------------------------------
        // HWND_TOP
        //--------------------------------------------------------
        private static readonly IntPtr HWND_TOP =
            IntPtr.Zero;

        //========================================================
        // WINDOWS-FUNKTIONEN
        //========================================================

        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(
            string? lpModuleName);


        [DllImport(
            "user32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern ushort RegisterClassEx(
            ref WNDCLASSEX windowClass);


        [DllImport(
            "user32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            uint exStyle,
            string className,
            string windowName,
            uint style,
            int x,
            int y,
            int width,
            int height,
            IntPtr parent,
            IntPtr menu,
            IntPtr instance,
            IntPtr param);


        [DllImport("user32.dll")]
        private static extern bool ShowWindow(
            IntPtr window,
            int command);


        [DllImport("user32.dll")]
        private static extern bool UpdateWindow(
            IntPtr window);


        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(
            IntPtr window);


        [DllImport("user32.dll")]
        private static extern bool PeekMessage(
            out MSG message,
            IntPtr window,
            uint min,
            uint max,
            uint remove);


        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(
            ref MSG message);


        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(
            ref MSG message);


        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(
            IntPtr hwnd,
            uint message,
            IntPtr wParam,
            IntPtr lParam);


        [DllImport(
            "user32.dll",
            CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursor(
            IntPtr instance,
            int cursor);


        [DllImport("user32.dll")]
        private static extern void PostQuitMessage(
            int exitCode);


        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(
            int index);

        //---------------------------------------------------------
        // Ändert den Fensterstil.
        //
        // SetWindowLongPtr ist die 64-Bit-Version von
        // SetWindowLong
        //---------------------------------------------------------
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(
            IntPtr hwnd,
            int index,
            IntPtr newStyle);

        //---------------------------------------------------------
        // Ändert Die Position und Größe.
        //---------------------------------------------------------
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hwnd,
            IntPtr hwndInsertAfter,
            int x,
            int y,
            int width,
            int height,
            uint flags);

        //========================================================
        // FELDER
        //========================================================

        private IntPtr handle;

        // Sehr wichtig:
        //
        // Der Delegate muss als Feld erhalten bleiben.
        //
        // Windows besitzt einen Verweis auf unsere WindowProcedure.
        // Würde der Delegate vom Garbage Collector eingesammelt,
        // könnte Windows später in ungültigen Speicher springen.
        private readonly WndProcDelegate wndProc;


        //========================================================
        // ÖFFENTLICHE EIGENSCHAFTEN
        //========================================================

        // Aktuelle Breite des Spielefensters
        public int Width { get; private set; }

        // Agtuelle Höhe des Spielefensters
        public int Height { get; private set; }

        // Gibt an, ob das Fenster geschlossen werden soll.
        public bool ShouldClose { get; private set; }

        // Aktueller Darstellungmodus
        public WindowMode Mode { get; private set; }

        // Aktuell verwendete Auflösungs-ID (PrivateKey(xD))
        //
        // Diese ID wird durch SetResolution() gesetzt.
        public int Resolution { get; private set; }

        // Der Windows-Handle unseres Spielfensters.
        //
        // Der Renderer benötigt diesen Handle, um in unser
        // Fenster zeichnen zu können.
        public IntPtr Handle => handle;


        //========================================================
        // KONSTRUKTOR
        //========================================================
        //
        // Beim erstellen können wir direkt angeben:
        //
        // resolution:
        //Welche Auflösung verwendet werden soll.
        //
        //mode:
        //Windowed, Borderless oder Fullscreen
        //
        // Beispiel:
        //
        // new Window(1, WindowMode.Windowed);
        //
        //========================================================

        public Window(int resolution = 1,
            WindowMode mode = WindowMode.Windowed)
        {
            // ----------------------------------------------------
            // Die aktuelle Bildschirmauflösung setzen
            //
            // Die eigentliche Auswahl passiert in SetResolution().
            // ----------------------------------------------------
            SetResolution(resolution);

            //----------------------------------------------------
            //Gewünschten FensterModus speichern
            //----------------------------------------------------
            Mode = mode;

            //----------------------------------------------------
            // Unsere eigene Windows-Funktion speichern.
            //----------------------------------------------------

            wndProc = WindowProcedure;


            //----------------------------------------------------
            // Die Instanz unseres Programms holen.
            //----------------------------------------------------

            IntPtr instance = GetModuleHandle(null);

            if (instance == IntPtr.Zero)
            {
                throw new Exception(
                    "Die Programm-Instanz konnte nicht ermittelt werden."
                );
            }


            //----------------------------------------------------
            // Windows-Fensterklasse erstellen.
            // Wir erstellen zun#chst ein normales Fenster.
            //
            // Danach wird SetWindowMode() verwendet, um den Modus
            // einzustellen
            //----------------------------------------------------
            WNDCLASSEX windowClass = new WNDCLASSEX();

            windowClass.cbSize =
                (uint)Marshal.SizeOf<WNDCLASSEX>();

            windowClass.style = CS_HREDRAW | CS_VREDRAW;

            windowClass.lpfnWndProc = wndProc;

            windowClass.hInstance = instance;

            windowClass.hCursor = LoadCursor(
                IntPtr.Zero,
                IDC_ARROW
                );

            // Interner Fensterklassenname
            windowClass.lpszClassName =
                "D007AdventureWindow";

            //----------------------------------------------------
            // Windows-Fensterklasse registrieren.
            //----------------------------------------------------

            // Sichtbarer Fenstertitel
            ushort result = RegisterClassEx(
                ref windowClass
                );

            if(result == 0)
            {
                throw new Exception(
                    $"Die Fensterklasse konnte nicht erstellt werden.");
            }

            //----------------------------------------------------
            // Fenster erstellen.
            //----------------------------------------------------
            handle = CreateWindowEx(
                0,
                // Interner Name der Fensterklasse
                "D007AdventureWindow",

                // Sichtbarer Fenstertitel
                "D007 Adventure",

                // Standardmäßig erstllen wir ein normales Fenster
                WS_OVERLAPPEDWINDOW,

                // Startposition
                100,
                100,

                // Größe
                Width,
                Height,

                IntPtr.Zero,
                IntPtr.Zero,
                instance,
                IntPtr.Zero
                );

            //----------------------------------------------------
            // Prüfen ob das Fenster erfolgreich erstellt wurde
            //----------------------------------------------------
            if (handle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Exception(
                    $"Das Fenster konnte nicht Erstellt werden." +
                    $"Win32-Fehlercode: {errorCode}"
                    );
            }

            //----------------------------------------------------
            // Den gewünschten Fenstermodus setzten
            //----------------------------------------------------
            SetWindowMode(mode);

            //----------------------------------------------------
            // Fenster Sichtbar machen
            //----------------------------------------------------
            ShowWindow(
                handle,
                SW_SHOW
                );

            //----------------------------------------------------
            // Fenster aktualisieren
            //----------------------------------------------------
            UpdateWindow(handle);

            Logger.Info($"Fenster wurde erstellt: " +
                        $"{Width}x{Height} - {Mode}");
        }

            //====================================================
            // SET RESOLUTION
            //====================================================
            //
            // Hier wird die Auflösung über einen switch
            // ausgewählt.
            //
            // Dadurch müssen wir im restlichen Programm nicht 
            // überall mit festen Werten wie 1920 oder 1080
            // arbeiten.
            //
            // Auflösung:
            //
            // 0 = 1280x720
            // 1 = 1920x1080
            // 2 = 2560x1440
            // 3 = ScreenRes
            //
            // Weitere Auflösungen können hier ergänzt werden.
            //====================================================
            public void SetResolution(int resolution)
            {
                switch(resolution)
                {
                //----------------------------------------------------
                // 1280x720
                //----------------------------------------------------
                case 0:
                    Width = 1280;
                    Height = 720;
                    break;
                //----------------------------------------------------
                // 1920x1080
                //----------------------------------------------------
                case 1:
                    Width = 1920;
                    Height = 1080;
                    break;

                //----------------------------------------------------
                // 2560x1440
                //----------------------------------------------------
                case 2:
                    Width = 2560;
                    Height = 1440;
                    break;

                //----------------------------------------------------
                // Native Auflösung
                //----------------------------------------------------
                case 3:
                    Width = GetSystemMetrics(SM_CXSCREEN);
                    Height = GetSystemMetrics(SM_CYSCREEN);
                    break;

                //----------------------------------------------------
                // Default
                //
                //Ungültige Auflösung.
                //
                //Wir verwenden als Standard
                // 800x600
                //----------------------------------------------------
                default:
                    Logger.Info($"Unbekannte Auflösung: {resolution}." +
                        "800x600 wird verwendet"
                        );

                    Width = 800;
                    Height = 600;

                    break;
                }

            //--------------------------------------------------------
            // Speichern, welche Auflösung gewählt wurde.
            //--------------------------------------------------------
            Resolution = resolution;

            //--------------------------------------------------------
            // Wenn bereits ein Fenster existiert, müssen wir
            // dessen tatsächliche Größe ebenfalls ändern.
            //
            // Beim Konstruktor existiert das Fenster noch nicht
            // deshalb wird dieser Teil übersprungen
            //--------------------------------------------------------
                if (handle != IntPtr.Zero)
                {
                    ApplyWindowSize();
                }

                Logger.Info($"Auflösung geändert: {Width}x{Height}");
            }

        //========================================================
        //SET WINDOW MODE
        //========================================================
        // 
        // Diese Methode schaltet zwischen
        //
        // Windowed
        // Borderless
        // Fullscreen
        //
        // um.
        //
        // Die Methode kann jederzeit während des Spiels
        // aufgerufen werden.
        //========================================================
        public void SetWindowMode(
            WindowMode mode)
        {
            if (handle == IntPtr.Zero)
            {
                return;
            }

            //--------------------------------------------------------
            // Den gewünschten Modus wählen
            //--------------------------------------------------------
            switch (mode)
            {
                //========================================================
                // NORMALES FENSTER
                //========================================================
                case WindowMode.Windowed:
                    // WS_OVERLAPPEDWINDOW aktiviert:
                    //
                    // Titelleiste
                    // Rahmen
                    // Minimieren
                    // Maximieren
                    // Schließen
                    SetWindowLongPtr(
                        handle,
                        GWL_STYLE,
                        new IntPtr(
                            unchecked(
                            (int)WS_OVERLAPPEDWINDOW
                            )
                        )
                    );

                    //--------------------------------------------------------
                    // Fenstergröße aus der gewählten Auflösung verwenden
                    //--------------------------------------------------------
                    ApplyWindowSize();

                    break;

                //========================================================
                // RAHMENLOSES FENSTER
                //========================================================
                case WindowMode.Borderless:

                    // WS_POPUP entfernt:
                    //
                    // Rahmen
                    // Titelleiste
                    //
                    // Das Fenster bleibt aber weiterhin ein normales
                    // Windows-Fenster und kann kleiner als der Screen sein.
                    SetWindowLongPtr(
                        handle,
                        GWL_STYLE,
                        new IntPtr(
                            unchecked(
                            (int)WS_POPUP
                            )
                        )
                    );

                    //----------------------------------------------------
                    // Fenster auf die gewünschte Größe setzten
                    // und mittig auf dem Bildschirm ausgeben.
                    //----------------------------------------------------
                    ApplyWindowSize();
                    break;

                //========================================================
                // FULLSCREEN FENSTER
                //========================================================
                case WindowMode.Fullscreen:
                    // Fullscreen verwendet ebenfalls WS_POPUP:
                    //
                    // Der Unterschied zu Rahmenlos ist:
                    //
                    // Das Fenster wird auf der komplette Bildschirmgrüße
                    // ausgegeben.
                    SetWindowLongPtr(
                        handle,
                        GWL_STYLE,
                        new IntPtr(
                            unchecked(
                            (int)WS_POPUP
                            )
                        )
                    );

                    //----------------------------------------------------
                    // Native Bildschirmgröße ermitteln.
                    //----------------------------------------------------
                    int screenWidth =
                        GetSystemMetrics(
                            SM_CXSCREEN
                            );

                    int screenHeight =
                        GetSystemMetrics(
                            SM_CYSCREEN
                            );

                    //----------------------------------------------------
                    // Fullscreen auf dem kompletten Bildschirm ausgeben.
                    //----------------------------------------------------
                    SetWindowPos(
                        handle,
                        HWND_TOP,
                        0,
                        0,
                        screenWidth,
                        screenHeight,
                        SWP_FRAMECHANGED |
                        SWP_SHOWWINDOW
                        );

                    //----------------------------------------------------
                    // Width und Heigth müssen ebenfalls die tatsächliche
                    // Fenstergröße ausgeben.
                    //----------------------------------------------------
                    Width = screenWidth;
                    Height = screenHeight;

                    break;

                //========================================================
                // UNBEKANNTER MODUS
                //========================================================
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(mode)
                        );
            }

            //----------------------------------------------------
            // Den aktuellen Modus speichern.                    
            //----------------------------------------------------
            Mode = mode;

            //----------------------------------------------------
            // Windows soll das Fenster nach Änderungen neu
            // zeichnen.
            //----------------------------------------------------
            ShowWindow(
                handle,
                SW_SHOW
                );

            UpdateWindow(
                handle
                );

            Logger.Info(
                $"Fenstermodus geändert: " +
                $"{Mode} - {Width}x{Height}"
                );
            }

        //========================================================
        // APPLY WINDOW SIZE
        //========================================================
        //
        // Diese Methode setzt die aktuelle Width/Height auf das 
        // Windows-Fenster.
        // 
        // Bei Windowed und Borderless wird das Fenster außerdem 
        // in der mitte des Bildschirms ausgegeben
        //========================================================
        private void ApplyWindowSize()
        {
            if (handle == IntPtr.Zero)
            {
                return;
            }

            //----------------------------------------------------
            // Aktuelle Bildschirmgrößer ermitteln.
            //----------------------------------------------------
            int screenWidth =
                GetSystemMetrics(
                    SM_CXSCREEN
                    );

            int screenHeight =
                GetSystemMetrics(
                    SM_CYSCREEN
                    );

            //----------------------------------------------------
            // Fenster mittig auf dem Bildschirm ausgeben
            //----------------------------------------------------
            int x = (screenWidth - Width) / 2;
            int y = (screenHeight - Height) / 2;

            //----------------------------------------------------
            // Fenstergröße und Position übergeben
            //----------------------------------------------------
            SetWindowPos(
                handle,
                HWND_TOP,
                x,
                y,
                Width,
                Height,
                SWP_FRAMECHANGED |
                SWP_SHOWWINDOW
                );
        }

        //========================================================
        //PROCESS MESSAGES
        //========================================================
        //
        // Windows schickt Nachrichten an unser Fenster.
        //
        // Zum Beispiel:
        //
        // - Fenster schließen
        // - Tastatur
        // - Maus
        // - Fenster bewegen
        //
        // Diese Nachrichten müssen wir regelmäßig verarbeiten.
        //========================================================
        public void ProcessMessages()
        {
            MSG message;

            // Solange Windows noch Nachrichten für uns hat...

            while (PeekMessage(
                out message,
                IntPtr.Zero,
                0,
                0,
                PM_REMOVE))
            {
                //------------------------------------------------
                // WM_QUIT ist keine normale Fensternachricht.
                //
                // Sie wird nicht an WindowProcedure geschickt.
                // Deshalb dürfen wir sie hier nicht mit
                // TranslateMessage() und DispatchMessage()
                // weiterverarbeiten.
                //------------------------------------------------

                if (message.message == WM_QUIT)
                {
                    ShouldClose = true;
                    continue;
                }


                //------------------------------------------------
                // Nachricht übersetzen.
                //------------------------------------------------

                TranslateMessage(
                    ref message
                    );


                //------------------------------------------------
                // Nachricht an Windows zurückgeben.
                //------------------------------------------------

                DispatchMessage(
                    ref message
                );
            }
        }

        //========================================================
        // QUIT
        //========================================================
        //
        // Beendet unser Fenster sofort.
        //
        // Das Fenster wird zuerst versteckt.
        // Dadurch kann Windows den Desktop dahinter neu zeichnen.
        //
        // Danach wird das eigentliche Fenster zerstört.
        //========================================================

        public void Quit()
        {
            if (ShouldClose)
            {
                return;
            }

            ShouldClose = true;

            if (handle != IntPtr.Zero)
            {
                // Das Fenster zuerst unsichtbar machen.
                ShowWindow(
                    handle,
                    SW_HIDE
                );

                // Danach das native Windows-Fenster zerstören.
                DestroyWindow(handle);

                handle = IntPtr.Zero;
            }

            // Dadurch bekommt unsere Message Queue
            // eine WM_QUIT-Nachricht.
            PostQuitMessage(0);
        }

        //========================================================
        // CLOSE
        //========================================================
        //
        // Wird beim endgültigen Aufräumen des Spiels verwendet.
        //
        // Close() ist absichtlich sicher mehrfach aufrufbar.
        //========================================================

        public void Close()
        {
            ShouldClose = true;

            if (handle != IntPtr.Zero)
            {
                // Fenster zuerst verstecken.
                ShowWindow(
                    handle,
                    SW_HIDE
                );

                // Fenster zerstören.
                DestroyWindow(handle);

                handle = IntPtr.Zero;
            }
        }

        //========================================================
        // WINDOW PROCEDURE
        //========================================================
        //
        // Windows ruft diese Methode automatisch auf,
        // wenn etwas mit unserem Fenster passiert.
        //
        // Zum Beispiel:
        //
        // WM_CLOSE:
        // 
        //========================================================

        private IntPtr WindowProcedure(
            IntPtr hwnd,
            uint message,
            IntPtr wParam,
            IntPtr lParam)
        {
            switch (message)
            {
                //----------------------------------------------------
                // Der Benutzer möchte das Fenster schließen.
                //----------------------------------------------------

                case WM_CLOSE:
                    {
                        ShouldClose = true;

                        // Fenster zerstören.
                        //
                        // Dadurch entsteht anschließend WM_DESTROY.
                        DestroyWindow(hwnd);

                        return IntPtr.Zero;
                    }

                //----------------------------------------------------
                // Das Fenster wurde tatsächlich zerstört.
                //----------------------------------------------------

                case WM_DESTROY:
                    {
                        ShouldClose = true;

                        // Die Game Loop muss über WM_QUIT erfahren,
                        // dass das Fenster nicht mehr existiert.
                        PostQuitMessage(0);

                        return IntPtr.Zero;
                    }
            }

            //----------------------------------------------------
            // Alle Nachrichten, die wir nicht selbst behandeln,
            // werden an Windows weitergegeben.
            //----------------------------------------------------
            return DefWindowProc(
                hwnd,
                message,
                wParam,
                lParam
                );
        }

        //========================================================
        // WINDOW PROCEDURE DELEGATE
        //========================================================

        [UnmanagedFunctionPointer(
            CallingConvention.Winapi)]
        private delegate IntPtr WndProcDelegate(
            IntPtr hwnd,
            uint message,
            IntPtr wParam,
            IntPtr lParam);

        //========================================================
        // WINDOWS WINDOW CLASS
        //========================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct WNDCLASSEX
        {
            public uint cbSize;

            public uint style;

            public WndProcDelegate lpfnWndProc;

            public int cbClsExtra;

            public int cbWndExtra;

            public IntPtr hInstance;

            public IntPtr hIcon;

            public IntPtr hCursor;

            public IntPtr hbrBackground;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;

            public IntPtr hIconSm;
        }

        //========================================================
        // WINDOWS MESSAGE
        //========================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;

            public uint message;

            public IntPtr wParam;

            public IntPtr lParam;

            public uint time;

            public int pointX;

            public int pointY;
        }
    }
}