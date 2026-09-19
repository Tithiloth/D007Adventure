using D007Adventure.Graphics;
using D007Adventure.Input;
using D007Adventure.GUI;
using System;
using System.Threading;

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

        // Unser ImageLoader.
        private readonly ImageLoader imageLoader;

        // Unser MainMenu.
        private readonly MMenu mainMenu;

        // Unser GameLoop-Zustand.
        private bool gameStarted;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public Game()
        {
            // Wir erstellen das Fenster.
            window = new Window();

            // Wir erstellen den Renderer.
            renderer = new Renderer2D(window);

            // Wir erstellen unser Input-System.
            input = new D007Adventure.Input.Input();

            // Wir erstellen den ImageLoader.
            imageLoader = new ImageLoader();

            gameStarted = false;

            // Wir erstellen unser MainMenu.
            mainMenu = new MMenu(
                imageLoader,
                input,
                renderer
            );

            Logger.Info(
                "Game initialisiert"
            );
        }


        // ========================================================
        // RUN
        // ========================================================
        //
        // Diese Methode startet die Game Loop.
        // ========================================================

        public void Run()
        {
            while (!window.ShouldClose)
            {
                // ------------------------------------------------
                // 1. WINDOWS-NACHRICHTEN VERARBEITEN
                // ------------------------------------------------

                window.ProcessMessages();

                // Wenn Windows während ProcessMessages()
                // bereits das Ende signalisiert hat, dürfen wir
                // keinen weiteren Frame mehr zeichnen.
                if (window.ShouldClose)
                {
                    break;
                }


                // ------------------------------------------------
                // 2. INPUT AKTUALISIEREN
                // ------------------------------------------------

                input.Update();


                // ------------------------------------------------
                // 3. ESCAPE PRÜFEN
                // ------------------------------------------------

                if (input.IsKeyDown(ConsoleKey.Escape))
                {
                    Logger.Info(
                        "Escape gedrückt, Spiel wird beendet."
                    );

                    window.Quit();

                    // Ganz wichtig:
                    //
                    // Nach dem Zerstören des Fensters darf in diesem
                    // Frame nicht mehr gerendert werden.
                    break;
                }


                // ------------------------------------------------
                // 4. RENDERER STARTEN
                // ------------------------------------------------

                renderer.Begin();


                // ------------------------------------------------
                // 5. SPIEL ZEICHNEN
                // ------------------------------------------------

                if (!gameStarted)
                {
                    // MainMenu zeichnen.
                    mainMenu.Draw();


                    // Prüfen, ob der Spieler das Spiel starten
                    // möchte.
                    if (input.IsKeyDown(ConsoleKey.M))
                    {
                        gameStarted = true;

                        Logger.Info(
                            "MainMenü verlassen, Game gestartet."
                        );
                    }
                }
                else
                {
                    DrawGame();
                }


                // ------------------------------------------------
                // 6. FRAME AUF DEN BILDSCHIRM KOPIEREN
                // ------------------------------------------------

                renderer.End();


                // ------------------------------------------------
                // 7. FPS BEGRENZEN
                // ------------------------------------------------

                Thread.Sleep(16);
            }
        }


        // ========================================================
        // DRAW GAME
        // ========================================================

        private void DrawGame()
        {
            // Hier würde später das eigentliche Spiel laufen.
            //
            // Zum Beispiel:
            // - Spieler zeichnen
            // - Gegner zeichnen
            // - Level zeichnen
            // - Punkte anzeigen
            // - Sound abspielen

            renderer.DrawRectangle(
                100,
                100,
                200,
                200
            );
        }


        // ========================================================
        // DISPOSE
        // ========================================================
        //
        // Beim Beenden müssen die Ressourcen in der richtigen
        // Reihenfolge freigegeben werden.
        //
        // 1. MainMenu -> HBITMAP freigeben
        // 2. Renderer -> Backbuffer freigeben
        // 3. ImageLoader -> GDI+ beenden
        // 4. Window -> Fenster endgültig schließen
        //
        // ========================================================

        public void Dispose()
        {
            // Das geladene Hintergrundbild freigeben.
            mainMenu.Dispose();

            // Backbuffer und GDI-Ressourcen freigeben.
            renderer.Dispose();

            // GDI+ beenden.
            imageLoader.Dispose();

            // Fenster endgültig schließen.
            window.Close();

            Logger.Info(
                "Game wurde beendet."
            );
        }
    }
}