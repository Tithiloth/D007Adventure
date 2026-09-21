using System;
using System.Runtime.InteropServices;

namespace D007Adventure.Graphics
{
    // ============================================================
    // WINDOW
    // ============================================================
    //
    // Diese Klasse erstellt und verwaltet unser Spielfenster.
    //
    // Das Fenster wird nicht durch eine externe Bibliothek
    // erstellt.
    //
    // Wir benutzen direkt die Windows-API.
    // ============================================================

    public class Window
    {
        // ========================================================
        // WINDOWS-KONSTANTEN
        // ========================================================

        private const int CS_HREDRAW = 0x0002;
        private const int CS_VREDRAW = 0x0001;

        // Fullscreen-Fenster ohne Rahmen und Titelleiste.
        private const uint WS_POPUP = 0x80000000;

        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;

        private const uint PM_REMOVE = 0x0001;

        private const uint WM_QUIT = 0x0012;
        private const uint WM_CLOSE = 0x0010;
        private const uint WM_DESTROY = 0x0002;

        private const int IDC_ARROW = 32512;

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;


        // ========================================================
        // WINDOWS-FUNKTIONEN
        // ========================================================

        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(
            string? lpModuleName);


        [DllImport(
            "user32.dll",
            CharSet = CharSet.Ansi)]
        private static extern ushort RegisterClassEx(
            ref WNDCLASSEX windowClass);


        [DllImport(
            "user32.dll",
            CharSet = CharSet.Ansi)]
        private static extern IntPtr CreateWindowEx(
            int exStyle,
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


        // ========================================================
        // FELDER
        // ========================================================

        private IntPtr handle;

        // Sehr wichtig:
        //
        // Der Delegate muss als Feld erhalten bleiben.
        //
        // Windows besitzt einen Verweis auf unsere WindowProcedure.
        // Würde der Delegate vom Garbage Collector eingesammelt,
        // könnte Windows später in ungültigen Speicher springen.
        private readonly WndProcDelegate wndProc;


        // ========================================================
        // ÖFFENTLICHE EIGENSCHAFTEN
        // ========================================================

        public int Width { get; }

        public int Height { get; }

        public bool ShouldClose { get; private set; }


        // Der Windows-Handle unseres Spielfensters.
        //
        // Der Renderer benötigt diesen Handle, um in unser
        // Fenster zeichnen zu können.

        public IntPtr Handle => handle;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public Window()
        {
            // ----------------------------------------------------
            // Die aktuelle Bildschirmauflösung ermitteln.
            // ----------------------------------------------------

            Width = GetSystemMetrics(
                SM_CXSCREEN
            );

            Height = GetSystemMetrics(
                SM_CYSCREEN
            );

            if (Width <= 0 || Height <= 0)
            {
                throw new Exception(
                    "Die Bildschirmauflösung konnte nicht ermittelt werden."
                );
            }

            Logger.Info(
                $"Fullscreen-Auflösung: {Width}x{Height}"
            );


            // ----------------------------------------------------
            // Unsere eigene Windows-Funktion speichern.
            // ----------------------------------------------------

            wndProc = WindowProcedure;


            // ----------------------------------------------------
            // Die Instanz unseres Programms holen.
            // ----------------------------------------------------

            IntPtr instance = GetModuleHandle(null);

            if (instance == IntPtr.Zero)
            {
                throw new Exception(
                    "Die Programm-Instanz konnte nicht ermittelt werden."
                );
            }


            // ----------------------------------------------------
            // Windows-Fensterklasse erstellen.
            // ----------------------------------------------------

            WNDCLASSEX windowClass = new WNDCLASSEX();

            windowClass.cbSize =
                (uint)Marshal.SizeOf<WNDCLASSEX>();

            windowClass.style =
                CS_HREDRAW | CS_VREDRAW;

            windowClass.lpfnWndProc =
                wndProc;

            windowClass.hInstance =
                instance;

            windowClass.hCursor =
                LoadCursor(
                    IntPtr.Zero,
                    IDC_ARROW
                );

            windowClass.lpszClassName =
                "D007AdventureWindow";


            // ----------------------------------------------------
            // Fensterklasse bei Windows registrieren.
            // ----------------------------------------------------

            ushort result = RegisterClassEx(
                ref windowClass
            );

            if (result == 0)
            {
                throw new Exception(
                    "Die Fensterklasse konnte nicht registriert werden."
                );
            }


            // ----------------------------------------------------
            // Das eigentliche Fullscreen-Fenster erstellen.
            // ----------------------------------------------------

            handle = CreateWindowEx(
                0,

                // Interner Name der Fensterklasse.
                "D007AdventureWindow",

                // Sichtbarer Fenstertitel.
                "D007 Adventure",

                // WS_POPUP bedeutet:
                //
                // Kein Rahmen
                // Keine Titelleiste
                // Kein Fensterrand
                //
                // Dadurch erhalten wir ein randloses
                // Fullscreen-Fenster.
                WS_POPUP,

                // Position des Fensters.
                0,
                0,

                // Größe des Fensters.
                Width,
                Height,

                IntPtr.Zero,
                IntPtr.Zero,
                instance,
                IntPtr.Zero
            );


            // ----------------------------------------------------
            // Prüfen, ob das Fenster erfolgreich erstellt wurde.
            // ----------------------------------------------------

            if (handle == IntPtr.Zero)
            {
                throw new Exception(
                    "Das Fullscreen-Fenster konnte nicht erstellt werden."
                );
            }


            // ----------------------------------------------------
            // Fenster sichtbar machen.
            // ----------------------------------------------------

            ShowWindow(
                handle,
                SW_SHOW
            );


            // ----------------------------------------------------
            // Fenster aktualisieren.
            // ----------------------------------------------------

            UpdateWindow(
                handle
            );


            Logger.Info(
                "Fullscreen-Fenster wurde erstellt."
            );
        }


        // ========================================================
        // PROCESS MESSAGES
        // ========================================================
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
        // ========================================================

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
                // ------------------------------------------------
                // WM_QUIT ist keine normale Fensternachricht.
                //
                // Sie wird nicht an WindowProcedure geschickt.
                // Deshalb dürfen wir sie hier nicht mit
                // TranslateMessage() und DispatchMessage()
                // weiterverarbeiten.
                // ------------------------------------------------

                if (message.message == WM_QUIT)
                {
                    ShouldClose = true;
                    continue;
                }


                // ------------------------------------------------
                // Nachricht übersetzen.
                // ------------------------------------------------

                TranslateMessage(
                    ref message
                );


                // ------------------------------------------------
                // Nachricht an Windows zurückgeben.
                // ------------------------------------------------

                DispatchMessage(
                    ref message
                );
            }
        }


        // ========================================================
        // QUIT
        // ========================================================
        //
        // Beendet unser Fenster sofort.
        //
        // Das Fenster wird zuerst versteckt.
        // Dadurch kann Windows den Desktop dahinter neu zeichnen.
        //
        // Danach wird das eigentliche Fenster zerstört.
        // ========================================================

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


        // ========================================================
        // CLOSE
        // ========================================================
        //
        // Wird beim endgültigen Aufräumen des Spiels verwendet.
        //
        // Close() ist absichtlich sicher mehrfach aufrufbar.
        // ========================================================

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


        // ========================================================
        // WINDOW PROCEDURE
        // ========================================================
        //
        // Windows ruft diese Methode automatisch auf,
        // wenn etwas mit unserem Fenster passiert.
        // ========================================================

        private IntPtr WindowProcedure(
            IntPtr hwnd,
            uint message,
            IntPtr wParam,
            IntPtr lParam)
        {
            // ----------------------------------------------------
            // Der Benutzer möchte das Fenster schließen.
            // ----------------------------------------------------

            if (message == WM_CLOSE)
            {
                ShouldClose = true;

                // Fenster zerstören.
                //
                // Dadurch entsteht anschließend WM_DESTROY.
                DestroyWindow(hwnd);

                return IntPtr.Zero;
            }


            // ----------------------------------------------------
            // Das Fenster wurde tatsächlich zerstört.
            // ----------------------------------------------------

            if (message == WM_DESTROY)
            {
                ShouldClose = true;

                // Die Game Loop muss über WM_QUIT erfahren,
                // dass das Fenster nicht mehr existiert.
                PostQuitMessage(0);

                return IntPtr.Zero;
            }


            // ----------------------------------------------------
            // Alle Nachrichten, die wir nicht selbst behandeln,
            // werden an Windows weitergegeben.
            // ----------------------------------------------------

            return DefWindowProc(
                hwnd,
                message,
                wParam,
                lParam
            );
        }


        // ========================================================
        // WINDOW PROCEDURE DELEGATE
        // ========================================================

        [UnmanagedFunctionPointer(
            CallingConvention.Winapi)]
        private delegate IntPtr WndProcDelegate(
            IntPtr hwnd,
            uint message,
            IntPtr wParam,
            IntPtr lParam);


        // ========================================================
        // WINDOWS WINDOW CLASS
        // ========================================================

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

            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszMenuName;

            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszClassName;

            public IntPtr hIconSm;
        }


        // ========================================================
        // WINDOWS MESSAGE
        // ========================================================

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