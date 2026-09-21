using System;
using System.Collections.Generic;
using System.Text;
using D007Adventure;
using D007Adventure.Graphics;
using D007Adventure.Input;

namespace D007Adventure.GUI
{
    internal class Options
    {
        // ========================================================
        // Sprites
        // ========================================================
        private IntPtr background;
        private IntPtr graphicsbutton;
        private IntPtr soundbutton;
        private IntPtr keybindingsbutton;

        // ========================================================
        // Abhängigkeiten
        // ========================================================
        readonly ImageLoader imageLoader;
        readonly D007Adventure.Input.Input input;
        readonly Renderer2D renderer;

        public Options(
            ImageLoader imageLoader,
            D007Adventure.Input.Input input,
            Renderer2D renderer)
        {
            this.imageLoader = imageLoader;
            this.input = input;
            this.renderer = renderer;
            Logger.Info(
                "Options initialisiert"
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
                    "Fehler beim Laden des Hintergrundbildes für Options"
                );
            }
            else
            {
                Logger.Info(
                    "Hintergrundbild für Options erfolgreich geladen"
                );
            }

            graphicsbutton = imageLoader.Load(
                "Assets/Options/Graphics.png"
            );

            soundbutton = imageLoader.Load(
                "Assets/Options/Sound.png"
            );

            keybindingsbutton = imageLoader.Load(
                "Assets/Options/Keybindings.png"
            );

            // ----------------------------------------------------
            // Buttons laden.
            // ----------------------------------------------------
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

            // Buttons zeichnen.
            renderer.DrawImage(
                graphicsbutton,
                1600,
                400,
                256,
                64
            );

            renderer.DrawImage(
                soundbutton,
                1600,
                500,
                256,
                64
            );

            renderer.DrawImage(
                keybindingsbutton,
                1600,
                600,
                256,
                64
            );
        }
    }
}
