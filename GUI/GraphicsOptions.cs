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
        private readonly Window window;
        private IntPtr background;
        private IntPtr modus;
        private IntPtr windowed;
        private IntPtr borderless;
        private IntPtr fullscreen;
        private IntPtr resolution;
        private IntPtr res720i;
        private IntPtr res1080p;
        private IntPtr res1440;
        private IntPtr drop;
        public IntPtr pulldown;
        private IntPtr vsync;
        private IntPtr check;
        private IntPtr checkp;
        private readonly ImageLoader imageLoader;
        private readonly D007Adventure.Input.Input input;
        private readonly Renderer2D renderer;

        public bool pullDownModi = false;
        public bool pullDownRes = false;

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

            resolution = imageLoader.Load(
                "Assets/Options/GraphicsOptions/Resolution.png"
                );

            vsync = imageLoader.Load(
                "Assets/Options/GraphicsOptions/VSync.png"
                );

            pulldown = imageLoader.Load(
                "Assets/Options/GraphicsOptions/PullDown.png"
                );

            windowed = imageLoader.Load(
                "Assets/Buttons/Windowed.png"
                );

            borderless = imageLoader.Load(
                "Assets/Buttons/Borderless.png"
                );

            fullscreen = imageLoader.Load(
                "Assets/Buttons/Fullscreen.png"
                );

            res720i = imageLoader.Load(
                "Assets/Buttons/720i.png"
                );

            res1080p = imageLoader.Load(
                "Assets/Buttons/1080p.png"
                );

            res1440 = imageLoader.Load(
                "Assets/Buttons/1440.png"
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

            if (pullDownModi)
            {
                renderer.DrawImage(
                    pulldown,
                    512, 264, 256, 192
                    );

                renderer.DrawImage(
                    windowed,
                    512, 264, 256, 64
                    );

                renderer.DrawImage(
                    borderless,
                    512, 328, 256, 64
                    );

                renderer.DrawImage(
                    fullscreen,
                    512, 392, 256, 64
                    );
            }

            if(pullDownRes)
            {
                renderer.DrawImage(
                pulldown,
                    512, 364, 256, 192
                    );

                renderer.DrawImage(
                    res720i,
                    512, 364, 256, 64
                    );

                renderer.DrawImage(
                    res1080p,
                    512, 428, 256, 64
                    );

                renderer.DrawImage(
                    res1440,
                    512, 492, 256, 64
                    );
            }
        }

        public void Dispose()
        {

        }
    }
}
