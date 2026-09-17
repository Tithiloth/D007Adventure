using D007Adventure.Graphics;
using D007Adventure.Input;
using System;
using System.Diagnostics;
using System.Threading;

namespace D007Adventure
{
    // ============================================================
    // GAME LOOP
    // ============================================================
    //
    // Die Game Loop ist das "Herz" unseres Spiels.
    //
    // Sie wiederholt immer wieder:
    //
    //     1. Eingaben verarbeiten
    //     2. Spiel aktualisieren
    //     3. Grafik zeichnen
    //
    // Ohne Game Loop hätten wir kein laufendes Spiel.
    // ============================================================

    public class GameLoop
    {
        private Window window;
        private Renderer2D renderer;
        private D007Adventure.Input.Input input;


        // --------------------------------------------------------
        // Hier speichern wir, wie schnell unser Spiel laufen soll.
        //
        // 60 FPS bedeutet:
        //
        //     60 Bilder pro Sekunde
        // --------------------------------------------------------

        private const int TargetFPS = 60;


        // ========================================================
        // KONSTRUKTOR
        // ========================================================

        public GameLoop(
            Window window,
            Renderer2D renderer,
            D007Adventure.Input.Input input)
        {
            this.window = window;
            this.renderer = renderer;
            this.input = input;
        }


        // ========================================================
        // RUN
        // ========================================================

        public void Run()
        {
            // Wir merken uns die Zeit, zu der unsere Game Loop
            // gestartet wurde.

            Stopwatch stopwatch = Stopwatch.StartNew();


            // Solange das Fenster geöffnet ist, läuft unser Spiel.

            while (!window.ShouldClose)
            {
                // ------------------------------------------------
                // 1. WINDOWS-NACHRICHTEN VERARBEITEN
                // ------------------------------------------------
                //
                // Windows schickt uns beispielsweise Nachrichten
                // wenn:
                //
                // - eine Taste gedrückt wird
                // - die Maus bewegt wird
                // - das Fenster geschlossen wird
                //
                // Diese müssen wir regelmäßig abholen.
                // ------------------------------------------------

                window.ProcessMessages();


                // ------------------------------------------------
                // 2. INPUT
                // ------------------------------------------------

                input.Update();


                // ------------------------------------------------
                // 3. SPIEL AKTUALISIEREN
                // ------------------------------------------------
                //
                // Hier würde später beispielsweise stehen:
                //
                //     Spieler bewegen
                //     Gegner bewegen
                //     Kollisionen prüfen
                //     Punkte berechnen
                //
                // Im Moment machen wir noch nichts.
                // ------------------------------------------------

                Update();


                // ------------------------------------------------
                // 4. GRAFIK ZEICHNEN
                // ------------------------------------------------

                renderer.Begin();

                Draw();

                renderer.End();


                // ------------------------------------------------
                // 5. FPS BEGRENZEN
                // ------------------------------------------------
                //
                // Wir wollen nicht, dass unsere Schleife einfach
                // mit mehreren tausend Durchläufen pro Sekunde
                // läuft.
                //
                // Deshalb warten wir ein kleines bisschen.
                // ------------------------------------------------

                Thread.Sleep(1000 / TargetFPS);
            }
        }


        // ========================================================
        // UPDATE
        // ========================================================
        //
        // Hier wird die Spiellogik aktualisiert.
        // ========================================================

        private void Update()
        {
            // ----------------------------------------------------
            // TEST:
            //
            // Wenn ESC gedrückt wurde, beenden wir das Spiel.
            // ----------------------------------------------------

            if (input.IsKeyDown(ConsoleKey.Escape))
            {
                window.Close();
            }
        }


        // ========================================================
        // DRAW
        // ========================================================
        //
        // Hier wird festgelegt, was auf dem Bildschirm gezeichnet
        // werden soll.
        // ========================================================

        private void Draw()
        {
            // Wir zeichnen momentan ein einfaches Rechteck.
            //
            // x = 100
            // y = 100
            // Breite = 200
            // Höhe = 100

            renderer.DrawRectangle(
                100,
                100,
                200,
                100
            );
        }
    }
}