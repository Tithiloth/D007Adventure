using System;

namespace D007Adventure
{
    /// <summary>
    /// Wurst
    /// </summary>
    // ============================================================
    // PROGRAM
    // ============================================================
    //
    // Diese Klasse ist der Startpunkt unseres Programms.
    //
    // ============================================================

    internal class Program
    {
        static void Main()
        {
            // ----------------------------------------------------
            // Logging initialisieren.
            // ----------------------------------------------------

            Logger.Initialize();

            Logger.Info(
                "D007 Adventure wird gestartet."
            );


            try
            {
                // ------------------------------------------------
                // Game erstellen.
                // ------------------------------------------------

                Game game = new Game();

                try
                {
                    Logger.Info(
                        "Game wurde erfolgreich erstellt."
                    );

                    // Game Loop starten.
                    game.Run();
                }
                finally
                {
                    // Egal ob das Spiel normal oder wegen
                    // eines Fehlers beendet wird:
                    //
                    // Die nativen Ressourcen werden aufgeräumt.
                    game.Dispose();
                }
            }
            catch (Exception exception)
            {
                // ------------------------------------------------
                // Fehler protokollieren.
                // ------------------------------------------------

                Logger.Exception(
                    exception
                );

                Console.WriteLine();
                Console.WriteLine(
                    "Das Programm wurde aufgrund eines Fehlers beendet."
                );
            }


            Logger.Info(
                "Programm wurde beendet."
            );
        }
    }
}