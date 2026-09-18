using System;
using System.Runtime.InteropServices;

namespace D007Adventure.Graphics
{
    // ============================================================
    // RENDERER 2D
    // ============================================================
    //
    // Der Renderer ist dafür verantwortlich, Dinge auf den
    // Bildschirm zu zeichnen.
    //
    // "Rendern" bedeutet vereinfacht:
    //
    //     Daten -> Bild
    //
    // Zum Beispiel:
    //
    //     Spieler X = 100
    //     Spieler Y = 200
    //
    // wird zu:
    //
    //     Zeichne Spieler bei (100, 200)
    //
    // ============================================================

    public class Renderer2D
    {
        private readonly Window window;

        private IntPtr deviceContext;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public Renderer2D(Window window)
        {
            this.window = window;
        }


        // ========================================================
        // BEGIN
        // ========================================================
        //
        // Hier wird ein neuer Frame vorbereitet.
        //
        // Der Bildschirm wird zuerst mit der Hintergrundfarbe
        // Hellblau gefüllt.
        //
        // ========================================================

        public void Begin()
        {
            deviceContext = GetDC(window.Handle);

            IntPtr backgroundBrush = CreateSolidBrush(
                RGB(173, 216, 230)
            );

            RECT rectangle = new RECT
            {
                Left = 0,
                Top = 0,
                Right = window.Width,
                Bottom = window.Height
            };

            FillRect(
                deviceContext,
                ref rectangle,
                backgroundBrush
            );

            DeleteObject(backgroundBrush);
        }


        // ========================================================
        // END
        // ========================================================
        //
        // Hier wird der aktuelle Frame beendet.
        //
        // Der Device Context wird wieder freigegeben.
        //
        // ========================================================

        public void End()
        {
            if (deviceContext != IntPtr.Zero)
            {
                ReleaseDC(
                    window.Handle,
                    deviceContext
                );

                deviceContext = IntPtr.Zero;
            }
        }


        // ========================================================
        // DRAW RECTANGLE
        // ========================================================
        //
        // Zeichnet ein Rechteck.
        //
        // x      = Position horizontal
        // y      = Position vertikal
        // width  = Breite
        // height = Höhe
        //
        // Das Test-Rechteck ist momentan rot.
        //
        // ========================================================

        public void DrawRectangle(
            int x,
            int y,
            int width,
            int height)
        {
            IntPtr brush = CreateSolidBrush(
                RGB(255, 0, 0)
            );

            RECT rectangle = new RECT
            {
                Left = x,
                Top = y,
                Right = x + width,
                Bottom = y + height
            };

            FillRect(
                deviceContext,
                ref rectangle,
                brush
            );

            DeleteObject(brush);
        }

        public void DrawImage(
    IntPtr image,
    int x,
    int y,
    int width,
    int height)
        {
            if (image == IntPtr.Zero)
            {
                return;
            }

            if (deviceContext == IntPtr.Zero)
            {
                return;
            }

            IntPtr memoryDC = CreateCompatibleDC(
                deviceContext
            );

            if (memoryDC == IntPtr.Zero)
            {
                return;
            }

            IntPtr oldObject = SelectObject(
                memoryDC,
                image
            );

            StretchBlt(
                deviceContext,
                x,
                y,
                width,
                height,
                memoryDC,
                0,
                0,
                width,
                height,
                SRCCOPY
            );

            SelectObject(
                memoryDC,
                oldObject
            );

            DeleteDC(memoryDC);
        }


        // ========================================================
        // RGB
        // ========================================================
        //
        // Wandelt Rot, Grün und Blau in einen Windows-Farbwert um.
        //
        // Beispiel:
        //
        //     RGB(255, 0, 0)
        //
        // ergibt Rot.
        //
        // ========================================================

        private static uint RGB(
            byte red,
            byte green,
            byte blue)
        {
            return (uint)(
                red |
                (green << 8) |
                (blue << 16)
            );
        }


        // ========================================================
        // RECT
        // ========================================================
        //
        // Beschreibt ein Rechteck auf dem Bildschirm.
        //
        // Left   = linke Seite
        // Top    = obere Seite
        // Right  = rechte Seite
        // Bottom = untere Seite
        //
        // ========================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        // ========================================================
        // WINDOWS API
        // ========================================================

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(
            IntPtr window);


        [DllImport("user32.dll")]
        private static extern int ReleaseDC(
            IntPtr window,
            IntPtr deviceContext);


        [DllImport("user32.dll")]
        private static extern int FillRect(
            IntPtr deviceContext,
            ref RECT rectangle,
            IntPtr brush);


        // ========================================================
        // GDI
        // ========================================================

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateSolidBrush(
            uint color);


        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(
            IntPtr objectHandle);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr deviceContext);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(
            IntPtr deviceContext,
            IntPtr objectHandle
        );

        [DllImport("gdi32.dll")]
        private static extern bool StretchBlt(
            IntPtr destination,
            int x,
            int y,
            int width,
            int height,
            IntPtr source,
            int sourceX,
            int sourceY,
            int sourceWidth,
            int sourceHeight,
            uint rasterOperation
        );

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(
            IntPtr deviceContext
        );

        private const uint SRCCOPY = 0x00CC0020;
    }
}