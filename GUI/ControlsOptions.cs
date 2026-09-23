using System;
using System.Collections.Generic;
using System.Text;
using D007Adventure;
using D007Adventure.Graphics;
using D007Adventure.Input;

namespace D007Adventure.GUI
{
    internal class ControlsOptions
    {
        private IntPtr background;
        private IntPtr keys;
        private IntPtr Cancel;

        private readonly ImageLoader imageLoader;
        private readonly D007Adventure.Input.Input input;
        private readonly Renderer2D renderer;

        public ControlsOptions(
            ImageLoader imageLoader,
            D007Adventure.Input.Input input,
            Renderer2D renderer)
        {
            this.imageLoader = imageLoader;
            this.input = input;
            this.renderer = renderer;

            Logger.Info(
                "Steuerungsoptionen initialisiert"
                );

            background = imageLoader.Load(
                "Assets/Backgrounds/BGMain.png"
                );

            keys = imageLoader.Load(
                "Assets/Options/ControlsOptions/Keys.png"
                );

            Cancel = imageLoader.Load(
                "Assets/Buttons/Cancel.png"
                );
        }

        public void Draw()
        {
            renderer.DrawImage(
                background,
                0, 0, 1920, 1080
            );

            renderer.DrawImage(
                keys,
                359, 180, 1600, 720
            );

            renderer.DrawImage(
                Cancel,
                1302, 958, 256, 64
            );
        }
    }
}
