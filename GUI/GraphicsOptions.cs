using System;
using System.Collections.Generic;
using System.Text;
using D007Adventure;
using D007Adventure.Graphics;
using D007Adventure.Input;

namespace D007Adventure.GUI
{
    internal class GraphicsOptions
    {
        private IntPtr background;
        private IntPtr modus;
        private IntPtr drop;
        public IntPtr pulldown;
        private IntPtr resolution;
        private IntPtr vsync;
        private IntPtr check;
        private IntPtr checkp;
        private readonly ImageLoader imageLoader;
        private readonly D007Adventure.Input.Input input;
        private readonly Renderer2D renderer;

        public bool pullDown = false;

        public GraphicsOptions(
            ImageLoader imageLoader,
            D007Adventure.Input.Input input,
            Renderer2D renderer)
        {
            this.imageLoader = imageLoader;
            this.input = input;
            this.renderer = renderer;

            Logger.Info(
                "Grafikoptionen initialisiert"
                );

            background = imageLoader.Load(
                "Assets/Backgrounds/BGMain.png"
                );

            modus = imageLoader.Load(
                "Assets/Options/GraphicsOptions/Modus.png"
                );

            drop = imageLoader.Load(
                "Assets/Options/GraphicsOptions/Drop.png"
                );

            pulldown = imageLoader.Load(
                "Assets/Options/GraphicsOptions/PullDown.png"
                );

            resolution = imageLoader.Load(
                "Assets/Options/GraphicsOptions/Resolution.png"
                );

            vsync = imageLoader.Load(
                "Assets/Options/GraphicsOptions/VSync.png"
                );

            check = imageLoader.Load(
                "Assets/Buttons/Check.png"
                );

            checkp = imageLoader.Load(
                "Assets/Buttons/CheckP.png"
                );

            if (background == IntPtr.Zero)
            {
                Logger.Error(
                    "Fehler beim Laden des Hintergrundbildes für GrafikOptionen"
                    );
            }
            else
            {
                Logger.Info(
                    "Hintergrund für GrafikOptionen erfolgreich geladen"
                    );
            }
        }

        public void Draw()
        {
            if (background == IntPtr.Zero)
            {
                Logger.Error(
                    "Hintergrundbild ist null, kann nicht gezeichnet werden"
                    );
                return;
            }

            // Hintergrund zeichnen
            renderer.DrawImage(
                background,
                0, 0, 1920, 1080);

            // Modus zeichnen
            renderer.DrawImage(
                modus,
                200, 200, 256, 64
                );

            renderer.DrawImage(
                drop,
                512, 200, 330, 64
                );

            renderer.DrawImage(
                drop,
                512, 300, 330, 64
                );

            renderer.DrawImage(
                check,
                512, 400, 64, 64
                );

            if (pullDown)
            {
                renderer.DrawImage(
                    pulldown,
                    512, 264, 256, 256
                    );
                return;
            }

            // Auflösung zeichnen
            renderer.DrawImage(
                resolution,
                200, 300, 256, 64
                );

            // Vsync zeichnen
            renderer.DrawImage(
                vsync,
                200, 400, 256, 64
                );
        }

        public void Dispose()
        {

        }
    }
}
