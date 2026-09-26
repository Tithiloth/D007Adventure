using System;
using System.Runtime.InteropServices;
using D007Adventure.Graphics;

namespace D007Adventure.Input
{
    // ============================================================
    // MOUSE
    // ============================================================
    //
    // Diese Klasse verwaltet den Maus-Input unseres Spiels.
    //
    // Wir benutzen direkt die Windows-API und benötigen
    // deshalb keine externe Input-Bibliothek.
    //
    // Die Klasse stellt unter anderem folgende Informationen
    // bereit:
    //
    // - aktuelle Mausposition
    // - linke Maustaste
    // - rechte Maustaste
    // - mittlere Maustaste
    // - Maustaste gerade gedrückt
    // - Maustaste gerade einmal gedrückt
    // - Maustaste gerade losgelassen
    // ============================================================

    public class Mouse
    {
        // ========================================================
        // WINDOWS-KONSTANTEN
        // ========================================================

        // Virtuelle Tasten für die Windows-Maus.
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;
        private const int VK_MBUTTON = 0x04;


        // ========================================================
        // WINDOWS-FUNKTIONEN
        // ========================================================

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(
            int virtualKey);


        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(
            out POINT point);


        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(
            IntPtr window,
            ref POINT point);


        // ========================================================
        // FELDER
        // ========================================================

        private readonly Window window;


        // Aktueller Zustand der Maustasten.
        private bool leftButtonDown;
        private bool rightButtonDown;
        private bool middleButtonDown;


        // Zustand der Maustasten aus dem vorherigen Frame.
        //
        // Dieser Zustand wird benötigt, um erkennen zu können,
        // ob eine Taste gerade neu gedrückt wurde.
        private bool previousLeftButtonDown;
        private bool previousRightButtonDown;
        private bool previousMiddleButtonDown;


        // ========================================================
        // ÖFFENTLICHE EIGENSCHAFTEN
        // ========================================================

        // Aktuelle X-Position der Maus innerhalb unseres Fensters.
        public int X { get; private set; }


        // Aktuelle Y-Position der Maus innerhalb unseres Fensters.
        public int Y { get; private set; }


        // Gibt an, ob die linke Maustaste momentan gedrückt ist.
        public bool LeftButtonDown => leftButtonDown;


        // Gibt an, ob die rechte Maustaste momentan gedrückt ist.
        public bool RightButtonDown => rightButtonDown;


        // Gibt an, ob die mittlere Maustaste momentan gedrückt ist.
        public bool MiddleButtonDown => middleButtonDown;


        // --------------------------------------------------------
        // PRESSED
        // --------------------------------------------------------
        //
        // true nur in dem Frame, in dem die Taste gedrückt wurde.
        //
        // Das ist besonders wichtig für Buttons.
        //
        // Beispiel:
        //
        // if (mouse.LeftButtonPressed)
        // {
        //     // Button ausführen
        // }
        //
        // Der Code wird nicht 60 Frames lang ausgeführt,
        // während die Maustaste gehalten wird.
        // --------------------------------------------------------

        public bool LeftButtonPressed =>
            leftButtonDown && !previousLeftButtonDown;


        public bool RightButtonPressed =>
            rightButtonDown && !previousRightButtonDown;


        public bool MiddleButtonPressed =>
            middleButtonDown && !previousMiddleButtonDown;


        // --------------------------------------------------------
        // RELEASED
        // --------------------------------------------------------
        //
        // true nur in dem Frame, in dem die Taste losgelassen wurde.
        // --------------------------------------------------------

        public bool LeftButtonReleased =>
            !leftButtonDown && previousLeftButtonDown;


        public bool RightButtonReleased =>
            !rightButtonDown && previousRightButtonDown;


        public bool MiddleButtonReleased =>
            !middleButtonDown && previousMiddleButtonDown;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public Mouse(Window window)
        {
            this.window = window;

            Logger.Info(
                "Mouse wurde erstellt."
            );
        }


        // ========================================================
        // UPDATE
        // ========================================================
        //
        // Diese Methode wird einmal pro Game-Loop aufgerufen.
        //
        // Hier werden:
        //
        // 1. die alten Maustasten-Zustände gespeichert
        // 2. die aktuellen Maustasten-Zustände abgefragt
        // 3. die Mausposition aktualisiert
        // ========================================================

        public void Update()
        {
            // ----------------------------------------------------
            // Alte Zustände speichern.
            // ----------------------------------------------------

            previousLeftButtonDown =
                leftButtonDown;

            previousRightButtonDown =
                rightButtonDown;

            previousMiddleButtonDown =
                middleButtonDown;


            // ----------------------------------------------------
            // Aktuelle Zustände der Maustasten abfragen.
            // ----------------------------------------------------

            leftButtonDown =
                IsButtonDown(VK_LBUTTON);

            rightButtonDown =
                IsButtonDown(VK_RBUTTON);

            middleButtonDown =
                IsButtonDown(VK_MBUTTON);


            // ----------------------------------------------------
            // Mausposition aktualisieren.
            // ----------------------------------------------------

            UpdateMousePosition();
        }


        // ========================================================
        // BUTTON DOWN
        // ========================================================

        private bool IsButtonDown(int virtualKey)
        {
            return (
                GetAsyncKeyState(virtualKey) & 0x8000
            ) != 0;
        }


        // ========================================================
        // MOUSE POSITION
        // ========================================================

        private void UpdateMousePosition()
        {
            if (window.Handle == IntPtr.Zero)
            {
                X = 0;
                Y = 0;

                return;
            }


            // ----------------------------------------------------
            // Aktuelle Position der Maus auf dem Bildschirm holen.
            // ----------------------------------------------------

            POINT point;

            if (!GetCursorPos(out point))
            {
                return;
            }


            // ----------------------------------------------------
            // Bildschirmkoordinaten in Fensterkoordinaten
            // umwandeln.
            //
            // Dadurch ist die obere linke Ecke unseres
            // Spielfensters immer:
            //
            // X = 0
            // Y = 0
            // ----------------------------------------------------

            if (!ScreenToClient(
                window.Handle,
                ref point))
            {
                return;
            }

            //---------------------------------------------------------
            // CLientkoordinaten in unsere virtuellen 1920x1080-
            // Koordinaten umwandeln
            // 
            // Dabei weren Skalierung und Letterboxing berücksichtigt
            //---------------------------------------------------------
            window.ScreenToVirtual(
                point.X,
                point.Y,
                out int virtualX,
                out int virtualY
                );

            X = virtualX;
            Y = virtualY;
        }

        // ========================================================
        // POINT
        // ========================================================
        //
        // Struktur für eine zweidimensionale Windows-Position.
        // ========================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
