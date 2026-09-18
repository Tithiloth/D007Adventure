using System;
using System.Runtime.InteropServices;

namespace D007Adventure.Graphics
{
    public class ImageLoader
    {
        private IntPtr gdiplusToken;

        public ImageLoader()
        {
            GdiplusStartupInput startupInput = new GdiplusStartupInput
            {
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
        }

        public IntPtr Load(string filePath)
        {
            IntPtr bitmap = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;

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

            result = GdipCreateHBITMAPFromBitmap(
                bitmap,
                out hBitmap,
                0
            );

            GdipDisposeImage(bitmap);

            if (result != 0 || hBitmap == IntPtr.Zero)
            {
                throw new Exception(
                    $"HBITMAP konnte nicht erstellt werden: {filePath} " +
                    $"(GDI+ Fehlercode: {result})"
                );
            }

            return hBitmap;
        }

        public void Unload(IntPtr image)
        {
            if (image != IntPtr.Zero)
            {
                DeleteObject(image);
            }
        }

        public void Dispose()
        {
            if (gdiplusToken != IntPtr.Zero)
            {
                GdiplusShutdown(gdiplusToken);
                gdiplusToken = IntPtr.Zero;
            }
        }

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
            CharSet = CharSet.Unicode
        )]
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

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(
            IntPtr objectHandle
        );
    }
}