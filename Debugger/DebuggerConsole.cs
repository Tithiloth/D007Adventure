using System;
using System.Collections.Generic;
using D007Adventure.Graphics;

namespace D007Adventure.Debugger
{
    public class DebugConsole
    {
        private const int ConsoleHeight = 540;
        private const int TextX = 20;
        private const int TextStartY = 20;
        private const int LineHeight = 26;
        private const int InputY = 500;

        private readonly Renderer2D renderer;
        private readonly List<string> outputLines;

        private string inputText = "";

        public bool IsOpen { get; private set; }

        public DebugConsole(Renderer2D renderer)
        {
            this.renderer = renderer;
            outputLines = new List<string>();

            Logger.Info("DebugConsole initialisiert.");

            AddLine("D007 Adventure Debug Console");
            AddLine("Tippe 'help' für eine Liste der Befehle.");
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;

            Logger.Info($"DebugConsole {(IsOpen ? "geöffnet" : "geschlossen")}.");
        }

        public void HandleCharacter(char character)
        {
            if (!IsOpen)
                return;

            // Enter
            if (character == '\r')
            {
                ExecuteCommand();
                return;
            }

            // Backspace
            if (character == '\b')
            {
                if (inputText.Length > 0)
                {
                    inputText = inputText[..^1];
                }

                return;
            }

            // Steuerzeichen ignorieren.
            if (char.IsControl(character))
                return;

            inputText += character;
        }

        public void Render()
        {
            if (!IsOpen)
                return;

            // Konsolenbereich
            renderer.DrawFilledRectangle(
                0,
                0,
                1920,
                ConsoleHeight,
                10,
                10,
                10);

            int y = TextStartY;

            foreach (string line in outputLines)
            {
                renderer.DrawText(line, TextX, y);
                y += LineHeight;

                if (y >= InputY - LineHeight)
                    break;
            }

            // Eingabezeile
            renderer.DrawText($"] {inputText}", TextX, InputY);
        }

        public void AddLine(string text)
        {
            outputLines.Add(text);

            // Vorläufige Begrenzung der Konsolen-Historie.
            if (outputLines.Count > 20)
            {
                outputLines.RemoveAt(0);
            }
        }

        public void Clear()
        {
            outputLines.Clear();
        }

        private void ExecuteCommand()
        {
            string command = inputText.Trim();

            if (command.Length == 0)
                return;

            AddLine($"] {command}");

            switch (command.ToLowerInvariant())
            {
                case "help":
                    AddLine("Verfügbare Befehle:");
                    AddLine("help  - Zeigt diese Hilfe.");
                    AddLine("clear - Leert die Konsole.");
                    break;

                case "clear":
                    Clear();
                    break;

                default:
                    AddLine($"Unbekannter Befehl: {command}");
                    break;
            }

            inputText = "";
        }
    }
}