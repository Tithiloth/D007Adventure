using D007Adventure.Graphics;
using D007Adventure.Input;
using D007Adventure.GUI;
using D007Adventure.Audio;
using D007Adventure.Tools;
using System;
using System.Threading;
using System.ComponentModel.Design;
using D007Adventure.Debugger;
using D007Adventure.DatabaseManager;

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

        //Unsere DebugConole
        private readonly DebugConsole debugConsole;

        // Unser Input-System.
        private readonly D007Adventure.Input.Input input;

        // Unser ImageLoader.
        private readonly ImageLoader imageLoader;

        // Unser Datenbank Manager
        private readonly D007Adventure.DatabaseManager.DatabaseManager databaseManager;

        // Unsere Tools aus der Datenbank.
        private readonly List<Tool> tools;

        // Unser MainMenu.
        private readonly MMenu mainMenu;

        // Unser OptionsMenu.
        private readonly Options options;

        // Unser GraphicsOptionsMenu.
        private readonly GraphicsOptions graphicsOptions;

        // Unser SoundOptionsMenu.
        private readonly SoundOptions soundOptions;

        // Unser ControlsOptionsMenu.
        private readonly ControlsOptions controlsOptions;

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
        private bool showControlsOptionsMenu = false;

        // Maus-Input-System.
        private readonly Mouse mouse;

        // Unser FrameLimiter für die gewünschte Bildrate.
        private readonly FrameLimiter frameLimiter;
        // Und der dazuhörige Frame Counter
        private readonly FpsCounter fpsCounter;

        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public Game()
        {
            // Wir erstellen das Fenster.
            window = new Window(
                1,
                WindowMode.Fullscreen);

            // Wir erstellen den Renderer.
            renderer = new Renderer2D(window);

            // Wir erstellen die Konsole
            debugConsole = new DebugConsole(renderer);
            window.CharacterReceived += debugConsole.HandleCharacter;

            // Wir erstellen unser Input-System.
            input = new D007Adventure.Input.Input();

            // Wir erstellen den ImageLoader.
            imageLoader = new ImageLoader();

            // Wir erstellen das Maus-Input-System.
            mouse = new Mouse(window);

            // Wir erstellen den FrameLimiter.
            // Standardmäßig läuft das Spiel mit 60 FPS.
            frameLimiter = new FrameLimiter(
                FpsLimit.Fps60
                );

            // Wir erstellen den FrameCounter
            fpsCounter = new FpsCounter();

            // Wir erstellen den WavPlayer.
            wavPlayer = new WavPlayer();

            gameStarted = false;

            // Wir erstellen unseren DatabaseManager
            databaseManager = new D007Adventure.DatabaseManager.DatabaseManager();

            databaseManager.TestDatabase();
            databaseManager.TestToolsDatabase();
            tools = databaseManager.LoadTools();

            Logger.Info(
                "Datenbanktest abgeschlossen."
                );

            List<Dictionary<string, object?>> items =
            databaseManager.ExecuteReader(
                "game",
                "SELECT * FROM Items;"
                );

            List<string> tables =
                databaseManager.GetTables("game");

            foreach (string table in tables)
            {
                Logger.Info(
                    $"Tabelle gefunden: {table}"
                );

                List<Dictionary<string, object?>> columns =
                    databaseManager.GetColumns(
                        "game",
                        table
                    );

                foreach (Dictionary<string, object?> column in columns)
                {
                    Logger.Info(
                        $"  Spalte: {column["name"]} | " +
                        $"Typ: {column["type"]} | " +
                        $"PrimaryKey: {column["pk"]}"
                    );
                }
            }

            foreach (Dictionary<string, object?> item in items)
            {
                Logger.Info(
                    $"Item: {item["ItemID"]} | " +
                    $"Name: {item["Name"]} | " +
                    $"Damage: {item["Damage"]} | " +
                    $"Skin: {item["Skin"]}"
                );
            }

            foreach (Tool tool in tools)
            {
                Logger.Info(
                    $"C# Tool: {tool.GetType().Name} | " +
                    $"Name: {tool.Name}" +
                    $"Damage: {tool.Damage} | " +
                    $"Tier: {tool.Tier} | " +
                    $"MaxTier: {tool.MaxTier}"
                );
            }

            tools = databaseManager.LoadTools();

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
                "SoundOptionsMenü initialisiert"
                );

            controlsOptions = new ControlsOptions(
                imageLoader,
                input,
                renderer
                );

            Logger.Info(
                "SteuerungsOptionsMenü initialisiert"
                );

            Logger.Info(
                "Game initialisiert"
                );
        }

        //========================================================
        // RUN
        //========================================================
        //
        // Diese Methode startet die Game Loop.
        //========================================================

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
                //wavPlayer.Play("Assets/Audio/MenuMusic.wav", true);
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
                //------------------------------------------------
                // DATABASEMANAGER UPDATEN
                //------------------------------------------------
                databaseManager.Update();
                //------------------------------------------------
                // 1. WINDOWS-NACHRICHTEN VERARBEITEN
                //------------------------------------------------

                window.ProcessMessages();

                // Wenn Windows während ProcessMessages()
                // bereits das Ende signalisiert hat, dürfen wir
                // keinen weiteren Frame mehr zeichnen.
                if (window.ShouldClose)
                {
                    break;
                }

                //------------------------------------------------
                // 2. INPUT UND MAUS AKTUALISIEREN
                //------------------------------------------------

                input.Update();
                mouse.Update();

                //------------------------------------------------
                // 3. ESCAPE PRÜFEN
                //------------------------------------------------

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
                    else if (showControlsOptionsMenu)
                    {
                        showControlsOptionsMenu = false;
                        showOptionsMenu = true;
                        Logger.Info(
                            "Steuerungseinstellungen verlassen, zurück zu den Einstellungen."
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
                            showControlsOptionsMenu = true;

                            Logger.Info(
                                "Steuerungseinstellungen geöffnet."
                                );
                        }
                        options.Draw();
                    }

                    else if (showGraphicsMenu)
                    {
                        graphicsOptions.Draw();
                       
                        if(mouse.LeftButtonPressed &&
                            mouse.X >= 768 &&
                            mouse.X <= 842 &&
                            mouse.Y >= 201 &&
                            mouse.Y <= 264)
                        {
                            graphicsOptions.pullDownModi = !graphicsOptions.pullDownModi;
                            graphicsOptions.pullDownRes = false;
                        }

                        // Umschalten  zu Windowed
                        if (graphicsOptions.pullDownModi)
                        {
                            if (mouse.LeftButtonPressed &&
                                mouse.X >= 512 &&
                                mouse.X <= 768 &&
                                mouse.Y >= 264 &&
                                mouse.Y <= 327
                                )
                            {
                                window.SetWindowMode(WindowMode.Windowed);
                                graphicsOptions.pullDownModi = false;
                            }

                            // Umschalten zu Borderless
                            else if (mouse.LeftButtonPressed &&
                            mouse.X >= 512 &&
                            mouse.X <= 768 &&
                            mouse.Y >= 328 &&
                            mouse.Y <= 391
                            )
                            {
                                window.SetWindowMode(WindowMode.Borderless);
                                graphicsOptions.pullDownModi = false;
                            }

                            //Umsshalten zu Fullscreen
                            else if (mouse.LeftButtonPressed &&
                                mouse.X >= 512 &&
                                mouse.X <= 768 &&
                                mouse.Y >= 392 &&
                                mouse.Y <= 456)
                            {
                                window.SetWindowMode(WindowMode.Fullscreen);
                                graphicsOptions.pullDownModi = false;
                            }
                        }

                        if (mouse.LeftButtonPressed &&
                            mouse.X >= 768 &&
                            mouse.X <= 842 &&
                            mouse.Y >= 301 &&
                            mouse.Y <= 364)
                        {
                            graphicsOptions.pullDownRes = !graphicsOptions.pullDownRes;
                            graphicsOptions.pullDownModi = false;
                        }

                        // Umschalten  zu 720p
                        if (graphicsOptions.pullDownRes)
                        {
                            if (mouse.LeftButtonPressed &&
                                mouse.X >= 512 &&
                                mouse.X <= 768 &&
                                mouse.Y >= 364 &&
                                mouse.Y <= 427
                                )
                            {
                                window.SetResolution(0);
                                graphicsOptions.pullDownRes = false;
                            }

                            // Umschalten zu 1080i
                            else if (mouse.LeftButtonPressed &&
                            mouse.X >= 512 &&
                            mouse.X <= 768 &&
                            mouse.Y >= 428 &&
                            mouse.Y <= 491
                            )
                            {
                                window.SetResolution(1);
                                graphicsOptions.pullDownRes = false;
                            }

                            //Umsshalten zu 1440
                            else if (mouse.LeftButtonPressed &&
                                mouse.X >= 512 &&
                                mouse.X <= 768 &&
                                mouse.Y >= 492 &&
                                mouse.Y <= 556)
                            {
                                window.SetResolution(2);
                                graphicsOptions.pullDownRes = false;
                            }
                        }

                        if (mouse.LeftButtonPressed &&
                            mouse.X >= 768 &&
                            mouse.X <= 832 &&
                            mouse.Y >= 501 &&
                            mouse.Y <= 564)
                        {
                            graphicsOptions.pullDownFps = true;
                            graphicsOptions.pullDownModi = false;
                            graphicsOptions.pullDownRes = false;
                        }

                        if (graphicsOptions.pullDownFps)
                        {
                            // 30 FPS auswählen
                            if (mouse.LeftButtonPressed &&
                                mouse.X >= 512 &&
                                mouse.X <= 768 &&
                                mouse.Y >= 564 &&
                                mouse.Y <= 627)
                            {
                                frameLimiter.Limit = FpsLimit.Fps30;
                                graphicsOptions.pullDownFps = false;

                                Logger.Info(
                                    "FPS-Limit auf 30 FPS gesetzt."
                                );
                            }

                            // 60 FPS auswählen
                            else if (mouse.LeftButtonPressed &&
                                mouse.X >= 512 &&
                                mouse.X <= 768 &&
                                mouse.Y >= 628 &&
                                mouse.Y <= 691)
                            {
                                frameLimiter.Limit = FpsLimit.Fps60;
                                graphicsOptions.pullDownFps = false;

                                Logger.Info(
                                    "FPS-Limit auf 60 FPS gesetzt."
                                );
                            }

                            // 90 FPS auswählen
                            else if (mouse.LeftButtonPressed &&
                                mouse.X >= 512 &&
                                mouse.X <= 768 &&
                                mouse.Y >= 692 &&
                                mouse.Y <= 755)
                            {
                                frameLimiter.Limit = FpsLimit.Fps90;
                                graphicsOptions.pullDownFps = false;

                                Logger.Info(
                                    "FPS-Limit auf 90 FPS gesetzt."
                                );
                            }
                        }
                    }
                    else if (showSoundOptionsMenu)
                    {
                        // Hier steht der Code für das Zeichnen des Sound-Options-Menüs.
                        soundOptions.Draw();
                    }
                    else if (showControlsOptionsMenu)
                    {
                        // Hier steht der Code für das Zeichnen des ControlsOptions-Menüs.
                         controlsOptions.Draw();
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

                    // Unsere Konsole
                    if (input.IsKeyPressed(ConsoleKey.F1))
                    {
                        debugConsole.Toggle();
                    }

                    if (debugConsole.IsOpen)
                    {
                        debugConsole.Render();
                    }
                }

                string fpsText = $"FPS: {fpsCounter.CurrentFps}";

                renderer.DrawTextRight(
                    fpsText,
                    20,
                    20
                    );

                // ------------------------------------------------
                // 6. FRAME AUF DEN BILDSCHIRM KOPIEREN
                // ------------------------------------------------
                renderer.End();

                // ------------------------------------------------
                // 7. FPS BEGRENZEN
                // ------------------------------------------------
                fpsCounter.Update();

                // ------------------------------------------------
                // 7. FPS BEGRENZEN
                // ------------------------------------------------
                frameLimiter.Wait();
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
            // Databasemanager freigeben.
            databaseManager.Dispose();

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

// ============================================================
// if(input.IsKeyPressed(ConsoleKey.W))
// {
//     player.X -= 5;
// }
// else if(input.IsKeyPressed(ConsoleKey.S))
// {
//     player.X += 5;
// }
//=============================================================