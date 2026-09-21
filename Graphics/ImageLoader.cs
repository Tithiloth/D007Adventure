using System;
using System.Runtime.InteropServices;

namespace D007Adventure.Graphics
{
    // ============================================================
    // IMAGE LOADER
    // ============================================================
    //
    // Diese Klasse ist dafür zuständig, Bilder über GDI+
    // zu laden und daraus HBITMAPs für den Renderer zu erstellen.
    //
    // ============================================================

    public class ImageLoader
    {
        // Token, das GDI+ beim Start zurückgibt.
        private IntPtr gdiplusToken;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public ImageLoader()
        {
            GdiplusStartupInput startupInput =
                new GdiplusStartupInput
                {
                    // GDI+ Version 1 verwenden.
                    //
                    // Ohne diesen Wert kann GDI+ mit
                    // Status 17 fehlschlagen.
                    GdiplusVersion = 1,

                    DebugEventCallback = IntPtr.Zero,

                    SuppressBackgroundThread = false,

                    SuppressExternalCodecs = false
                };


            int result = GdiplusStartup(
                out gdiplusToken,
                ref startupInput,
                IntPtr.Zero
            );


            if (result != 0)
            {
                throw new Exception(
                    $"GDI+ konnte nicht gestartet werden. Fehlercode: {result}"
                );
            }


            Logger.Info(
                "GDI+ wurde erfolgreich gestartet."
            );
        }


        // ========================================================
        // LOAD
        // ========================================================
        //
        // Lädt eine Bilddatei und erstellt daraus ein HBITMAP.
        //
        // Das zurückgegebene HBITMAP muss später mit Unload()
        // wieder freigegeben werden.
        //
        // ========================================================

        public IntPtr Load(string filePath)
        {
            IntPtr bitmap = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;


            // ----------------------------------------------------
            // Bild mit GDI+ laden.
            // ----------------------------------------------------

            int result = GdipCreateBitmapFromFile(
                filePath,
                out bitmap
            );


            if (result != 0 || bitmap == IntPtr.Zero)
            {
                throw new Exception(
                    $"Bild konnte nicht geladen werden: {filePath} " +
                    $"(GDI+ Fehlercode: {result})"
                );
            }


            // ----------------------------------------------------
            // GDI+-Bitmap in HBITMAP umwandeln.
            // ----------------------------------------------------

            result = GdipCreateHBITMAPFromBitmap(
                bitmap,
                out hBitmap,
                0
            );


            // Die ursprüngliche GDI+-Bitmap wird danach nicht
            // mehr benötigt.
            GdipDisposeImage(
                bitmap
            );


            if (result != 0 || hBitmap == IntPtr.Zero)
            {
                throw new Exception(
                    $"HBITMAP konnte nicht erstellt werden: {filePath} " +
                    $"(GDI+ Fehlercode: {result})"
                );
            }


            return hBitmap;
        }


        // ========================================================
        // UNLOAD
        // ========================================================
        //
        // Gibt ein zuvor geladenes HBITMAP wieder frei.
        //
        // ========================================================

        public void Unload(IntPtr image)
        {
            if (image != IntPtr.Zero)
            {
                DeleteObject(
                    image
                );
            }
        }


        // ========================================================
        // DISPOSE
        // ========================================================
        //
        // Beendet GDI+.
        //
        // Wichtig:
        //
        // Alle geladenen HBITMAPs müssen VORHER freigegeben
        // worden sein.
        //
        // ========================================================

        public void Dispose()
        {
            if (gdiplusToken != IntPtr.Zero)
            {
                GdiplusShutdown(
                    gdiplusToken
                );

                gdiplusToken = IntPtr.Zero;

                Logger.Info(
                    "GDI+ wurde beendet."
                );
            }
        }


        // ========================================================
        // GDI+ STRUKTUREN
        // ========================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct GdiplusStartupInput
        {
            public uint GdiplusVersion;

            public IntPtr DebugEventCallback;

            [MarshalAs(UnmanagedType.Bool)]
            public bool SuppressBackgroundThread;

            [MarshalAs(UnmanagedType.Bool)]
            public bool SuppressExternalCodecs;
        }


        // ========================================================
        // GDI+ API
        // ========================================================

        [DllImport("gdiplus.dll")]
        private static extern int GdiplusStartup(
            out IntPtr token,
            ref GdiplusStartupInput input,
            IntPtr output
        );


        [DllImport("gdiplus.dll")]
        private static extern void GdiplusShutdown(
            IntPtr token
        );


        [DllImport(
            "gdiplus.dll",
            CharSet = CharSet.Unicode)]
        private static extern int GdipCreateBitmapFromFile(
            string filename,
            out IntPtr bitmap
        );


        [DllImport("gdiplus.dll")]
        private static extern int GdipCreateHBITMAPFromBitmap(
            IntPtr bitmap,
            out IntPtr hBitmap,
            int background
        );


        [DllImport("gdiplus.dll")]
        private static extern int GdipDisposeImage(
            IntPtr image
        );


        // ========================================================
        // GDI API
        // ========================================================

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(
            IntPtr objectHandle
        );
    }
}