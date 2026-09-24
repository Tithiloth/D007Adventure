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
        private IntPtr player;

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

            player = imageLoader.Load(
                "Assets/Player.png"
            );
        }
       
        public void Draw()
        {
            if(background == IntPtr.Zero)
            {
                Logger.Error(
                    "Hintergrundbild ist null, kann nicht gezeichnet werden"
                );
                return;
            }
            // Hintergrundbild zeichnen
            renderer.DrawImage(
                background,
                0, 0, 1920, 1080
            );

            renderer.DrawImage(
                player,
                359, 180, 256, 128
            );
        }

        public void Dispose()
        {

        }
    }
}
