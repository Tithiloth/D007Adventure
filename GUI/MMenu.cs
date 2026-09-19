using System;
using D007Adventure.Graphics;
using D007Adventure.Input;

namespace D007Adventure.GUI
{
    public class MMenu
    {
        private IntPtr background;

        private readonly ImageLoader imageLoader;
        private readonly D007Adventure.Input.Input input;
        private readonly Renderer2D renderer;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public MMenu(
            ImageLoader imageLoader,
            D007Adventure.Input.Input input,
            Renderer2D renderer)
        {
            this.imageLoader = imageLoader;
            this.input = input;
            this.renderer = renderer;

            Logger.Info(
                "MainMenü initialisiert"
            );


            // ----------------------------------------------------
            // Hintergrundbild laden.
            // ----------------------------------------------------

            background = imageLoader.Load(
                "Assets/Backgrounds/BGMain.png"
            );


            if (background == IntPtr.Zero)
            {
                Logger.Error(
                    "Fehler beim Laden des Hintergrundbildes für das MainMenü"
                );
            }
            else
            {
                Logger.Info(
                    "Hintergrundbild für das MainMenü erfolgreich geladen"
                );
            }
        }


        // ========================================================
        // DRAW
        // ========================================================

        public void Draw()
        {
            if (background == IntPtr.Zero)
            {
                Logger.Error(
                    "Hintergrundbild ist null, kann nicht gezeichnet werden"
                );

                return;
            }

            // Hintergrundbild zeichnen.
            renderer.DrawImage(
                background,
                0,
                0,
                1920,
                1080
            );
        }


        // ========================================================
        // DISPOSE
        // ========================================================
        //
        // Das Hintergrundbild wurde mit GDI+ geladen und danach
        // in ein natives HBITMAP umgewandelt.
        //
        // Dieses HBITMAP muss explizit freigegeben werden.
        //
        // Wichtig:
        //
        // Dispose() von MMenu muss VOR Dispose() von ImageLoader
        // aufgerufen werden.
        //
        // ========================================================

        public void Dispose()
        {
            if (background != IntPtr.Zero)
            {
                imageLoader.Unload(
                    background
                );

                background = IntPtr.Zero;
            }
        }
    }
}