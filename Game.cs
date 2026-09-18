using D007Adventure.Graphics;
using D007Adventure.Input;
using System;

namespace D007Adventure
{
    // ============================================================
    // GAME
    // ============================================================
    //
    // Diese Klasse ist sozusagen der "Chef" unseres Spiels.
    //
    // Sie kümmert sich darum, dass die einzelnen Bestandteile
    // unseres Spiels zusammenarbeiten.
    //
    // Später könnten hier zum Beispiel stehen:
    //
    // - Spieler
    // - Gegner
    // - Level
    // - Menüs
    // - Punkte
    // - Sound
    //
    // Im Moment halten wir es noch sehr einfach.
    // ============================================================

    public class Game
    {
        // Unser Fenster.
        private readonly Window window;

        // Unser 2D-Renderer.
        private readonly Renderer2D renderer;

        // Unser Input-System.
        private readonly D007Adventure.Input.Input input;

        //Unser ImageLoader.
        private readonly ImageLoader imageLoader;

        //unser MainMenu.
        private readonly GUI.MMenu mainMenu;

        // Unsere Game Loop.
        private GameLoop gameLoop;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================
        //
        // Dieser wird automatisch aufgerufen, wenn wir schreiben:
        //
        //     new Game();
        //
        // ========================================================

        public Game()
        {
            // Wir erstellen das Fenster.
            window = new Window(
                1920,
                1080,
                "D007 Adventure"
            );

            // Wir erstellen den Renderer.
            renderer = new Renderer2D(window);

            // Wir erstellen unser Input-System.
            input = new D007Adventure.Input.Input();

            imageLoader = new ImageLoader();

            mainMenu = new GUI.MMenu(imageLoader, input, renderer);

            // Wir erstellen die Game Loop.
            gameLoop = new GameLoop(
                window,
                renderer,
                input
            );
        }


        // ========================================================
        // RUN
        // ========================================================
        //
        // Diese Methode startet das Spiel.
        // ========================================================

        public void Run()
        {
            while(!window.ShouldClose)
            {
                // Wir starten den Frame.
                window.ProcessMessages();

                //Input aktualisieren
                input.Update();

                // Wir zeichnen das MainMenu.
                mainMenu.Draw();

                // Wir aktualisieren die Game Loop.
                gameLoop.Run();

                // Wir rendern den Frame.
                renderer.End();
            }
        }
    }
}