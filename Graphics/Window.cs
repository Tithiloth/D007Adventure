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

        private const int WS_OVERLAPPEDWINDOW = 0x00CF0000;

        private const int SW_SHOW = 5;

        private const uint PM_REMOVE = 0x0001;

        private const uint WM_QUIT = 0x0012;
        private const uint WM_CLOSE = 0x0010;

        private const int IDC_ARROW = 32512;


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
            int style,
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


        // ========================================================
        // FELDER
        // ========================================================

        private IntPtr handle;

        private WndProcDelegate wndProc;


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

        public Window(
            int width,
            int height,
            string title)
        {
            Width = width;
            Height = height;

            // ----------------------------------------------------
            // Unsere eigene Windows-Funktion speichern.
            // ----------------------------------------------------

            wndProc = WindowProcedure;


            // ----------------------------------------------------
            // Die Instanz unseres Programms holen.
            // ----------------------------------------------------

            IntPtr instance = GetModuleHandle(null);


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

            RegisterClassEx(
                ref windowClass
            );


            // ----------------------------------------------------
            // Das eigentliche Fenster erstellen.
            // ----------------------------------------------------

            handle = CreateWindowEx(
                0,

                // Interner Name der Fensterklasse.
                "D007AdventureWindow",

                // Sichtbarer Titel des Fensters.
                title,

                WS_OVERLAPPEDWINDOW,

                // Position des Fensters.
                100,
                100,

                // Größe des Fensters.
                width,
                height,

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
                    "Das Fenster konnte nicht erstellt werden."
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
                // Wenn Windows sagt:
                //
                // "Beende das Programm"
                // ------------------------------------------------

                if (message.message == WM_QUIT)
                {
                    ShouldClose = true;
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
        // CLOSE
        // ========================================================

        public void Close()
        {
            ShouldClose = true;

            if (handle != IntPtr.Zero)
            {
                DestroyWindow(
                    handle
                );

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

                DestroyWindow(
                    hwnd
                );

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