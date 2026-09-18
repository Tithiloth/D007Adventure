using System;
using System.Runtime.InteropServices;

namespace D007Adventure.Graphics
{
    // ============================================================
    // IMAGE LOADER
    // ============================================================
    //
    // Lädt Bilder direkt über die Windows API.
    //
    // Dadurch benötigen wir keine zusätzliche Bibliothek
    // und auch kein System.Drawing.Common.
    //
    // ============================================================

    public class ImageLoader
    {
        private const uint IMAGE_BITMAP = 0;
        private const uint IMAGE_ICON = 1;
        private const uint IMAGE_CURSOR = 2;

        private const uint LR_LOADFROMFILE = 0x00000010;
        private const uint LR_CREATEDIBSECTION = 0x00002000;


        // ========================================================
        // LOAD
        // ========================================================
        //
        // Lädt ein Bild von der Festplatte.
        //
        // Beispiel:
        //
        //     IntPtr image = loader.Load("Assets/player.jpg");
        //
        // ========================================================

        public IntPtr Load(string filePath)
        {
            IntPtr image = LoadImage(
                IntPtr.Zero,
                filePath,
                IMAGE_BITMAP,
                0,
                0,
                LR_LOADFROMFILE | LR_CREATEDIBSECTION
            );

            if (image == IntPtr.Zero)
            {
                throw new Exception(
                    $"Bild konnte nicht geladen werden: {filePath}"
                );
            }

            return image;
        }


        // ========================================================
        // UNLOAD
        // ========================================================
        //
        // Gibt das geladene Bild wieder frei.
        //
        // ========================================================

        public void Unload(IntPtr image)
        {
            if (image != IntPtr.Zero)
            {
                DeleteObject(image);
            }
        }


        // ========================================================
        // WINDOWS API
        // ========================================================

        [DllImport(
            "user32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true
        )]
        private static extern IntPtr LoadImage(
            IntPtr hInstance,
            string name,
            uint type,
            int width,
            int height,
            uint loadFlags
        );


        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(
            IntPtr objectHandle
        );
    }
}