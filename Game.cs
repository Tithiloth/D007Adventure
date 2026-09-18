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
        private Window window;

        // Unser 2D-Renderer.
        private Renderer2D renderer;

        // Unser Input-System.
        public D007Adventure.Input.Input input;

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
                1280,
                720,
                "D007 Adventure"
            );

            // Wir erstellen den Renderer.
            renderer = new Renderer2D(window);

            // Wir erstellen unser Input-System.
            input = new D007Adventure.Input.Input();

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
            gameLoop.Run();
        }
    }
}