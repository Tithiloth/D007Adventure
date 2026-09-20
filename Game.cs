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

        // Weitere Zustände, die wir später für Optionen oder andere Menüs nutzen können.
        private bool showMainMenu = true;
        private bool showOptionsMenu = false;

        // Maus-Input-System.
        private Mouse mouse;

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

            // Wir erstellen das Maus-Input-System.
            mouse = new Mouse(window);

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
                mouse.Update();

                // ------------------------------------------------
                // 3. ESCAPE PRÜFEN
                // ------------------------------------------------

                if (input.IsKeyDown(ConsoleKey.Escape))
                {
                    // Zurück zum MainMenu, wenn das Spiel gestartet ist.
                    if (gameStarted)
                    {
                        gameStarted = false;
                        Logger.Info(
                            "Game verlassen, zurück zum MainMenü."
                        );
                    }
                    mainMenu.Draw();
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
                    // Menü-Zustand prüfen
                    if (showOptionsMenu)
                    {
                        // Hier würden die Einstellungen gezeichnet werden.
                        // Zum Beispiel:
                        // - Lautstärke
                        // - Auflösung
                        // - Steuerung
                        // - Zurück-Button
                        renderer.DrawRectangle(
                            100,
                            100,
                            200,
                            200
                        );
                    }
                    else if (showMainMenu)
                    {
                        mainMenu.Draw();
                    }

                    // Prüfen, ob der Spieler das Spiel starten
                    // möchte.
                    if (mouse.LeftButtonPressed && 
                        mouse.X >= 1600 && 
                        mouse.X <= 1856 && 
                        mouse.Y >= 400 && 
                        mouse.Y <= 464)
                    {
                        gameStarted = true;

                        Logger.Info(
                            "MainMenü verlassen, Game gestartet."
                        );
                    }

                    // Prüfen, ob der Spieler die Einstellungen öffnen möchte.
                    if (mouse.LeftButtonPressed &&
                            mouse.X >= 1600 &&
                            mouse.X <= 1856 &&
                            mouse.Y >= 500 &&
                            mouse.Y <= 564)
                    {
                        showOptionsMenu = true;
                        showMainMenu = false;
                        Logger.Info(
                            "Einstellungen geöffnet."
                        );
                    }

                    // Prüfen, ob der Spieler das Spiel beenden
                    if (mouse.LeftButtonPressed &&
                            mouse.X >= 1600 &&
                            mouse.X <= 1856 &&
                            mouse.Y >= 600 &&
                            mouse.Y <= 664)
                    {
                        window.Quit();
                    }  
                    
                    //// Optionen-Zustand prüfen.
                    //if (options == 1)
                    //{
                    //    MainMenu = 0;
                    //    // Hier würden die Einstellungen gezeichnet werden.
                    //    // Zum Beispiel:
                    //    // - Lautstärke
                    //    // - Auflösung
                    //    // - Steuerung
                    //    // - Zurück-Button
                    //    renderer.DrawRectangle(
                    //        100,
                    //        100,
                    //        200,
                    //        200
                    //    );
                    //}
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