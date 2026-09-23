using D007Adventure.Graphics;
using D007Adventure.Input;
using D007Adventure.GUI;
using D007Adventure.Audio;
using System;
using System.Threading;
using System.ComponentModel.Design;

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

        // Unser OptionsMenu.
        private readonly Options options;

        // Unser GraphicsOptionsMenu.
        private readonly GraphicsOptions graphicsOptions;

        // Unser SoundOptionsMenu.
        private readonly SoundOptions soundOptions;

        // Unser KeybindingsMenu.

        // Unser WavPlayer für Soundeffekte.
        private readonly WavPlayer wavPlayer;

        // Unser OggPlayer für Musik.
        private OggPlayer oggPlayer = new D007Adventure.Audio.OggPlayer();

        // Unser GameLoop-Zustand.
        private bool gameStarted;

        // Weitere Zustände, die wir später für Optionen oder andere Menüs nutzen können.
        private bool showMainMenu = true;
        private bool showOptionsMenu = false;
        private bool showGraphicsMenu = false;
        private bool showSoundOptionsMenu = false;
        private bool showKeybindingsMenu = false;
        private bool isClicked = false;

        // Maus-Input-System.
        private readonly Mouse mouse;

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

            // Wir erstellen den WavPlayer.
            wavPlayer = new WavPlayer();

            gameStarted = false;

            // Wir erstellen unser MainMenu.
            mainMenu = new MMenu(
                imageLoader,
                input,
                renderer
                );

            Logger.Info(
                "HauptMenü initialisiert"
                );

            options = new Options(
                imageLoader,
                input,
                renderer
                );

            Logger.Info(
                "OptionsMenü initialisiert"
                );

            graphicsOptions = new GraphicsOptions
                (imageLoader,
                input,
                renderer
                );

            Logger.Info(
                "GrafikOptionsMenü initialisiert"
                );

            soundOptions = new SoundOptions(
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
            // Wir starten die Hintergrundmusik. true nur wenn es im loop laufen soll.
            if(input.IsKeyPressed(ConsoleKey.W))
            {
                wavPlayer.Stop();
                oggPlayer.Stop();
            }
            else
            {
                wavPlayer.Play("Assets/Audio/MenuMusic.wav", true);
            }

            if(input.IsKeyPressed(ConsoleKey.O))
            {
                wavPlayer.Stop();
                oggPlayer.Stop();
            }
            else
            {
                oggPlayer.Play("Assets/Audio/MenuMusic.ogg", true);
            }

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

                if (input.IsKeyPressed(ConsoleKey.Escape))
                {
                    // Zurück zum MainMenu, wenn das Spiel gestartet ist.
                    if (gameStarted)
                    {
                        gameStarted = false;
                        Logger.Info(
                            "Game verlassen, zurück zum MainMenü."
                            );
                    }
                    else if (showOptionsMenu)
                    {
                        showOptionsMenu = false;
                        showSoundOptionsMenu = false;
                        showMainMenu = true;
                        Logger.Info(
                            "Einstellungen verlassen, zurück zum MainMenü."
                            );
                    }
                    else if (showGraphicsMenu)
                    {
                        showGraphicsMenu = false;
                        showOptionsMenu = true;
                        showMainMenu = false;
                        Logger.Info(
                            "Grafikeisntellungen verlassen, zurück zu den Einstellungen."
                            );
                    }
                    else if (showSoundOptionsMenu)
                    {
                        showSoundOptionsMenu = false;
                        showOptionsMenu = true;
                        showMainMenu = false;
                        Logger.Info(
                            "Soundeinstellungen verlassen, zurück zu den Einstellungen."
                            );
                    }
                    else if (showKeybindingsMenu)
                    {
                        showKeybindingsMenu = false;
                        showOptionsMenu = true;
                        Logger.Info(
                            "Tastatureinstellungen verlassen, zurück zu den Einstellungen."
                            );
                    }
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
                        // Prüfen, oder der Spieler die Grafikeinstellungen öffnen möchte.
                        if(mouse.LeftButtonPressed && 
                            mouse.X >= 832 &&
                            mouse.X <= 1086 &&
                            mouse.Y >= 400 &&
                            mouse.Y <= 464)
                        {
                            showOptionsMenu = false;
                            showGraphicsMenu = true;
                            Logger.Info(
                                "Grafikeinstellungen geöffnet."
                                );
                        }
                        // Prüfen, ob der Spieler die Soundeinstellungen öffnen möchte.
                        else if(mouse.LeftButtonPressed &&
                            mouse.X >= 832 &&
                            mouse.X <= 1086 &&
                            mouse.Y >= 500 &&
                            mouse.Y <= 564)
                        {
                            showSoundOptionsMenu = true;
                            showOptionsMenu = false;
                            Logger.Info(
                                "Soundeinstellungen geöffnet."
                                );
                        }
                        // Prüfen, ob der Spieler die Tastatureinstellungen öffnen möchte.
                        else if (mouse.LeftButtonPressed &&
                                mouse.X >= 832 &&
                                mouse.X <= 1086 &&
                                mouse.Y >= 600 &&
                                mouse.Y <= 664)
                        {
                            showOptionsMenu = false;
                            showMainMenu = false;
                            showKeybindingsMenu = true;

                            Logger.Info(
                                "Tastatureinstellungen geöffnet."
                                );
                        }
                        options.Draw();
                    }
                    else if (showGraphicsMenu)
                    {
                        graphicsOptions.Draw();
                       
                        if(mouse.LeftButtonDown &&
                            mouse.X >= 768 &&
                            mouse.X <= 842 &&
                            mouse.Y >= 201 &&
                            mouse.Y <= 264)
                        {

                            isClicked = true;

                            if (isClicked)
                            {
                                graphicsOptions.pullDown = true;
                                graphicsOptions.Draw();
                                isClicked = true;
                            }
                            isClicked = false;
                        }
                        else
                        {
                           graphicsOptions.pullDown = false;
                        }
                    }
                    else if (showSoundOptionsMenu)
                    {
                        // Hier steht der Code für das Zeichnen des Sound-Options-Menüs.
                        soundOptions.Draw();
                    }
                    else if (showKeybindingsMenu)
                    {
                        // Hier könnte später der Code für das Zeichnen des Keybindings-Menüs stehen.
                        // Zum Beispiel:
                        // keybindingsMenu.Draw();
                    }
                    else if (showMainMenu)
                    {
                        // Hier steht der Code für das Zeichnen des Hauptmenüs.
                        mainMenu.Draw();
                    }

                    if (showMainMenu)
                    {
                        // Prüfen, ob der Spieler das Spiel starten
                        // möchte.
                        if (mouse.LeftButtonPressed &&
                            mouse.X >= 832 &&
                            mouse.X <= 1086 &&
                            mouse.Y >= 300 &&
                            mouse.Y <= 364)
                        {
                            gameStarted = true;

                            Logger.Info(
                                "MainMenü verlassen, Game gestartet."
                                );
                        }

                        // Prüfen, ob der Spieler die Einstellungen öffnen möchte.
                        if (mouse.LeftButtonPressed &&
                            mouse.X >= 832 &&
                            mouse.X <= 1086 &&
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
                            mouse.X >= 832 &&
                            mouse.X <= 1086 &&
                            mouse.Y >= 600 &&
                            mouse.Y <= 664)
                        {
                            window.Quit();
                        }
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