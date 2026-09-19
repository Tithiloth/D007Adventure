using System;
using D007Adventure;
using D007Adventure.Graphics;
using D007Adventure.Input;

namespace D007Adventure.GUI
{
    public class MMenu
    {
        // ========================================================
        // Sprites
        // ========================================================
        private IntPtr background;
        private IntPtr startbutton;
        private IntPtr optionsbutton;
        private IntPtr exitbutton;

        // ========================================================
        // Abhängigkeiten
        // ========================================================

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

            startbutton = imageLoader.Load(
                "Assets/Buttons/StartButton.png"
            );

            optionsbutton = imageLoader.Load(
                "Assets/Buttons/OptionsButton.png"
            );

            exitbutton = imageLoader.Load(
                "Assets/Buttons/ExitButton.png"
            );
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

            // Startbutton zeichnen.
            renderer.DrawImage(
                startbutton,
                1600,
                400,
                256,
                64
            );

            // Optionsbutton zeichnen.
            renderer.DrawImage(
                optionsbutton,
                1600,
                500,
                256,
                64
            );

            // Exitbutton zeichnen.
            renderer.DrawImage(
                exitbutton,
                1600,
                600,
                256,
                64
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