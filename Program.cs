using D007Adventure;
using System;

namespace D007Adventure
{
    // ============================================================
    // PROGRAM
    // ============================================================
    //
    // Diese Klasse ist der Startpunkt unseres Programms.
    //
    // Wenn wir unser Programm starten, wird zuerst die Methode
    //
    //     Main()
    //
    // ausgeführt.
    //
    // Von hier aus starten wir später unser komplettes Spiel.
    // ============================================================

    internal class Program
    {
        static void Main()
        {
            // ----------------------------------------------------
            // Wir erstellen unser Game-Objekt.
            //
            // "new Game()" bedeutet:
            //
            //     Erstelle ein neues Objekt der Klasse Game.
            //
            // Dieses Objekt enthält später die gesamte
            // Spiellogik Und initialisiert das Logginsystem.
            // Über das Loggingsystem können wir später Informationen,
            // Warnungen und Fehler in eine Log-Datei und in die
            // Konsole, die wir NUR zum debuggen verwenden und temporär
            // eingeschalten lassen, schreiben.
            // ----------------------------------------------------

            Logger.Initialize();

            Logger.Info("D007 Adventure wird gestartet.");

            try
            {
                Game game = new Game();

                Logger.Info("Game wurde erfolgreich erstellt.");

                game.Run();

                Logger.Info("Game wurde beendet.");
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);

                Console.WriteLine();
                Console.WriteLine("Das Programm wurde aufgrund eines Fehlers beendet.");
            }

            Logger.Info("Programm wurde beendet.");

            Console.WriteLine();
            Console.WriteLine("Drücke eine Taste zum Beenden...");
            Console.ReadKey();


            // ----------------------------------------------------
            // Jetzt starten wir das Spiel.
            //
            // Die Methode Run() startet unsere Game Loop.
            // ----------------------------------------------------
        }
    }
}
