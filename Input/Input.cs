using System;
using System.Runtime.InteropServices;

namespace D007Adventure.Input
{
    // ============================================================
    // INPUT
    // ============================================================
    //
    // Diese Klasse kümmert sich um Tastatureingaben.
    //
    // Zum Beispiel:
    //
    //     Ist W gedrückt?
    //     Ist A gedrückt?
    //     Ist SPACE gedrückt?
    //
    // ============================================================

    public class Input
    {
        // --------------------------------------------------------
        // GetAsyncKeyState kommt direkt aus Windows.
        //
        // Das bedeutet:
        //
        // Wir brauchen keine zusätzliche Bibliothek.
        //
        // user32.dll gehört zu Windows.
        // --------------------------------------------------------

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);


        // ========================================================
        // UPDATE
        // ========================================================
        //
        // Diese Methode wird jeden Frame aufgerufen.
        //
        // Später können wir hier komplexere Input-Logik einbauen.
        // ========================================================

        public void Update()
        {
            // Momentan müssen wir hier noch nichts machen.
        }


        // ========================================================
        // IS KEY DOWN
        // ========================================================
        //
        // Diese Methode fragt:
        //
        // "Ist diese Taste gerade gedrückt?"
        //
        // Beispiel:
        //
        //     IsKeyDown(ConsoleKey.Escape)
        //
        // ========================================================

        public bool IsKeyDown(ConsoleKey key)
        {
            // ConsoleKey ist für unsere einfache Version praktisch.
            //
            // Wir wandeln die Taste in den entsprechenden
            // Windows-Key-Code um.

            int virtualKey = ConvertToVirtualKey(key);

            // GetAsyncKeyState liefert Informationen über den
            // aktuellen Zustand der Taste.
            //
            // Wenn das höchste Bit gesetzt ist, ist die Taste
            // gerade gedrückt.

            return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
        }


        // ========================================================
        // CONVERT TO VIRTUAL KEY
        // ========================================================
        //
        // Windows benutzt sogenannte Virtual-Key-Codes.
        //
        // Wir wandeln unsere ConsoleKey-Werte in diese Codes um.
        // ========================================================

        private int ConvertToVirtualKey(ConsoleKey key)
        {
            switch (key)
            {
                // ========================================================
                // Sondertasten (ESC, Enter, Shift und Co.)
                // ========================================================
                //
                // Hier sind die System-/ Sondertasten definiert
                //
                // ========================================================
                case ConsoleKey.Escape:
                    return 0x1B;

                // ========================================================
                // Buchstaben
                // ========================================================
                //
                // Hier sind die Buchstaben definiert
                //
                // INFO: ConsoleKey kennt keine unterschiede zwischen 
                // Groß- und Kleinschreibung!
                //
                // ========================================================

                case ConsoleKey.A:
                    return 0x41;

                case ConsoleKey.B:
                    return 0x42;

                case ConsoleKey.D:
                    return 0x44;

                case ConsoleKey.C:
                    return 0x43;

                case ConsoleKey.W:
                    return 0x57;

                case ConsoleKey.S:
                    return 0x53;

                case ConsoleKey.Spacebar:
                    return 0x20;

                case ConsoleKey.Tab:
                    return 0x09;

                

                default:
                    return 0;
            }
        }
    }
}