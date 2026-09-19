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

        // Device Context des eigentlichen Fensters.
        private IntPtr windowDC;

        // Device Context unseres Backbuffers.
        private IntPtr backBufferDC;

        // Bitmap, auf die unser Backbuffer zeichnet.
        private IntPtr backBufferBitmap;

        // Bitmap, die vor unserem Backbuffer im DC ausgewählt war.
        //
        // Diese muss vor DeleteDC wieder eingesetzt werden.
        private IntPtr oldBackBufferBitmap;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public Renderer2D(Window window)
        {
            this.window = window;

            CreateBackBuffer();
        }


        // ========================================================
        // BEGIN
        // ========================================================
        //
        // Hier wird ein neuer Frame vorbereitet.
        //
        // Der Backbuffer wird zuerst mit der Hintergrundfarbe
        // Hellblau gefüllt.
        //
        // ========================================================

        public void Begin()
        {
            if (backBufferDC == IntPtr.Zero)
            {
                return;
            }

            IntPtr brush = CreateSolidBrush(
                RGB(173, 216, 230)
            );

            if (brush == IntPtr.Zero)
            {
                return;
            }

            RECT rectangle = new RECT
            {
                Left = 0,
                Top = 0,
                Right = window.Width,
                Bottom = window.Height
            };

            FillRect(
                backBufferDC,
                ref rectangle,
                brush
            );

            DeleteObject(brush);
        }


        // ========================================================
        // END
        // ========================================================
        //
        // Hier wird der aktuelle Frame beendet.
        //
        // Der Inhalt des Backbuffers wird auf das Fenster
        // kopiert.
        //
        // ========================================================

        public void End()
        {
            if (backBufferDC == IntPtr.Zero)
            {
                return;
            }

            // Falls das Fenster bereits zerstört wurde,
            // dürfen wir nicht mehr versuchen, einen DC zu holen.
            if (window.Handle == IntPtr.Zero)
            {
                return;
            }

            windowDC = GetDC(
                window.Handle
            );

            if (windowDC == IntPtr.Zero)
            {
                return;
            }

            BitBlt(
                windowDC,
                0,
                0,
                window.Width,
                window.Height,
                backBufferDC,
                0,
                0,
                SRCCOPY
            );

            ReleaseDC(
                window.Handle,
                windowDC
            );

            // Der DC wurde bereits freigegeben.
            windowDC = IntPtr.Zero;
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
            if (backBufferDC == IntPtr.Zero)
            {
                return;
            }

            IntPtr brush = CreateSolidBrush(
                RGB(255, 0, 0)
            );

            if (brush == IntPtr.Zero)
            {
                return;
            }

            RECT rectangle = new RECT
            {
                Left = x,
                Top = y,
                Right = x + width,
                Bottom = y + height
            };

            FillRect(
                backBufferDC,
                ref rectangle,
                brush
            );

            DeleteObject(brush);
        }


        // ========================================================
        // DRAW IMAGE
        // ========================================================
        //
        // Zeichnet ein geladenes HBITMAP in den Backbuffer.
        //
        // ========================================================

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

            if (backBufferDC == IntPtr.Zero)
            {
                return;
            }

            IntPtr memoryDC = CreateCompatibleDC(
                backBufferDC
            );

            if (memoryDC == IntPtr.Zero)
            {
                return;
            }

            IntPtr oldObject = SelectObject(
                memoryDC,
                image
            );

            if (oldObject == IntPtr.Zero)
            {
                DeleteDC(memoryDC);
                return;
            }

            StretchBlt(
                backBufferDC,
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

            // Ursprüngliches Objekt wieder einsetzen.
            SelectObject(
                memoryDC,
                oldObject
            );

            // Temporären Memory-DC freigeben.
            DeleteDC(memoryDC);
        }


        // ========================================================
        // CREATE BACK BUFFER
        // ========================================================
        //
        // Der Backbuffer sorgt dafür, dass wir nicht direkt
        // auf das sichtbare Fenster zeichnen.
        //
        // Dadurch können wir einen kompletten Frame vorbereiten
        // und ihn anschließend mit BitBlt auf einmal anzeigen.
        //
        // ========================================================

        private void CreateBackBuffer()
        {
            if (window.Handle == IntPtr.Zero)
            {
                throw new Exception(
                    "Das Fenster besitzt keinen gültigen Handle."
                );
            }

            IntPtr screenDC = GetDC(
                window.Handle
            );

            if (screenDC == IntPtr.Zero)
            {
                throw new Exception(
                    "Screen Device Context konnte nicht erstellt werden."
                );
            }

            backBufferDC = CreateCompatibleDC(
                screenDC
            );

            if (backBufferDC == IntPtr.Zero)
            {
                ReleaseDC(
                    window.Handle,
                    screenDC
                );

                throw new Exception(
                    "Back Buffer Device Context konnte nicht erstellt werden."
                );
            }

            backBufferBitmap = CreateCompatibleBitmap(
                screenDC,
                window.Width,
                window.Height
            );

            if (backBufferBitmap == IntPtr.Zero)
            {
                DeleteDC(backBufferDC);
                backBufferDC = IntPtr.Zero;

                ReleaseDC(
                    window.Handle,
                    screenDC
                );

                throw new Exception(
                    "Back Buffer Bitmap konnte nicht erstellt werden."
                );
            }

            oldBackBufferBitmap = SelectObject(
                backBufferDC,
                backBufferBitmap
            );

            if (oldBackBufferBitmap == IntPtr.Zero)
            {
                DeleteObject(backBufferBitmap);
                backBufferBitmap = IntPtr.Zero;

                DeleteDC(backBufferDC);
                backBufferDC = IntPtr.Zero;

                ReleaseDC(
                    window.Handle,
                    screenDC
                );

                throw new Exception(
                    "Alte Back Buffer Bitmap konnte nicht ausgewählt werden."
                );
            }

            ReleaseDC(
                window.Handle,
                screenDC
            );
        }


        // ========================================================
        // DISPOSE
        // ========================================================
        //
        // Hier werden alle nativen GDI-Ressourcen des Renderers
        // freigegeben.
        //
        // Die Reihenfolge ist wichtig:
        //
        // 1. Alte Bitmap wieder auswählen
        // 2. Backbuffer-DC löschen
        // 3. Backbuffer-Bitmap löschen
        //
        // ========================================================

        public void Dispose()
        {
            if (windowDC != IntPtr.Zero)
            {
                if (window.Handle != IntPtr.Zero)
                {
                    ReleaseDC(
                        window.Handle,
                        windowDC
                    );
                }

                windowDC = IntPtr.Zero;
            }


            if (backBufferDC != IntPtr.Zero)
            {
                // Die vorherige Bitmap wieder einsetzen.
                //
                // Eine Bitmap darf nicht gelöscht werden,
                // solange sie noch im DC ausgewählt ist.
                if (oldBackBufferBitmap != IntPtr.Zero)
                {
                    SelectObject(
                        backBufferDC,
                        oldBackBufferBitmap
                    );

                    oldBackBufferBitmap = IntPtr.Zero;
                }

                DeleteDC(
                    backBufferDC
                );

                backBufferDC = IntPtr.Zero;
            }


            if (backBufferBitmap != IntPtr.Zero)
            {
                DeleteObject(
                    backBufferBitmap
                );

                backBufferBitmap = IntPtr.Zero;
            }
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
        private static extern IntPtr CreateCompatibleDC(
            IntPtr deviceContext);


        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(
            IntPtr deviceContext,
            int width,
            int height);


        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(
            IntPtr deviceContext,
            IntPtr objectHandle);


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
            uint rasterOperation);


        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(
            IntPtr deviceContext);


        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr destination,
            int x,
            int y,
            int width,
            int height,
            IntPtr source,
            int sourceX,
            int sourceY,
            uint rasterOperation);


        private const uint SRCCOPY = 0x00CC0020;
    }
}