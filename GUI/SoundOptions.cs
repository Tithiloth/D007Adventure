using System;
using System.Collections.Generic;
using System.Text;
using D007Adventure;
using D007Adventure.Graphics;
using D007Adventure.Input;

namespace D007Adventure.GUI
{
    internal class SoundOptions
    {
        private IntPtr background;

        private readonly ImageLoader imageLoader;
        private readonly D007Adventure.Input.Input input;
        private readonly Renderer2D renderer;

        public SoundOptions(
            ImageLoader imageLoader,
            D007Adventure.Input.Input input,
            Renderer2D renderer)
        {
            this.imageLoader = imageLoader;
            this.input = input;
            this.renderer = renderer;
            Logger.Info(
                "SoundOptions initialisiert"
            );
            background = imageLoader.Load(
                "Assets/Backgrounds/BGMain.png"
            );
            if (background == IntPtr.Zero)
            {
                Logger.Error(
                    "Fehler beim Laden des Hintergrundbildes für SoundOptions"
                );
            }
            else
            {
                Logger.Info(
                    "Hintergrundbild für SoundOptions erfolgreich geladen"
                );
            }
        }
       
        public void Draw()
        {
            if(background == IntPtr.Zero)
            {
                Logger.Error(
                    "Fehler beim Zeichnen des Hintergrundbildes für SoundOptions"
                );
                return;
            }
            // Hintergrundbild zeichnen
            renderer.DrawImage(
                background,
                0, 0, 1920, 1080
            );
        }
    }
}
