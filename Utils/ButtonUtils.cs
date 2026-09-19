using D007Adventure.Input;

namespace D007Adventure.Utils
{
    // ============================================================
    // BUTTON UTILS
    // ============================================================
    //
    // Diese Klasse enthält Hilfsfunktionen für Buttons.
    //
    // Sie kümmert sich hauptsächlich darum festzustellen,
    // ob sich die Maus über einem bestimmten Bereich befindet
    // oder ob dieser Bereich angeklickt wurde.
    //
    // Die eigentliche Darstellung des Buttons übernimmt später
    // weiterhin der Renderer.
    // ============================================================

    public static class ButtonUtils
    {
        // ========================================================
        // MOUSE OVER
        // ========================================================
        //
        // Prüft, ob sich die Maus innerhalb eines Rechtecks
        // befindet.
        //
        // x/y:
        // Position des Buttons.
        //
        // width/height:
        // Größe des Buttons.
        // ========================================================

        public static bool IsMouseOver(
            Mouse mouse,
            int x,
            int y,
            int width,
            int height)
        {
            // ----------------------------------------------------
            // Prüfen, ob die Maus horizontal innerhalb des
            // Buttons liegt.
            // ----------------------------------------------------

            bool insideX =
                mouse.X >= x &&
                mouse.X <= x + width;


            // ----------------------------------------------------
            // Prüfen, ob die Maus vertikal innerhalb des
            // Buttons liegt.
            // ----------------------------------------------------

            bool insideY =
                mouse.Y >= y &&
                mouse.Y <= y + height;


            // ----------------------------------------------------
            // Nur wenn beide Bedingungen erfüllt sind,
            // befindet sich die Maus über dem Button.
            // ----------------------------------------------------

            return insideX && insideY;
        }


        // ========================================================
        // CLICKED
        // ========================================================
        //
        // Prüft, ob ein Button gerade angeklickt wurde.
        //
        // Wichtig:
        //
        // Wir verwenden LeftButtonPressed und nicht
        // LeftButtonDown.
        //
        // Dadurch wird ein Klick nur einmal ausgelöst,
        // auch wenn die Maustaste mehrere Frames gehalten wird.
        // ========================================================

        public static bool IsClicked(
            Mouse mouse,
            int x,
            int y,
            int width,
            int height)
        {
            // Zuerst muss sich die Maus über dem Button befinden.
            if (!IsMouseOver(
                mouse,
                x,
                y,
                width,
                height))
            {
                return false;
            }


            // Danach prüfen wir, ob die linke Maustaste
            // gerade neu gedrückt wurde.
            return mouse.LeftButtonPressed;
        }


        // ========================================================
        // RELEASED
        // ========================================================
        //
        // Prüft, ob die linke Maustaste innerhalb des Buttons
        // losgelassen wurde.
        //
        // Das kann später für komplexere Buttons nützlich sein,
        // zum Beispiel wenn ein Button erst beim Loslassen
        // aktiviert werden soll.
        // ========================================================

        public static bool IsReleased(
            Mouse mouse,
            int x,
            int y,
            int width,
            int height)
        {
            if (!IsMouseOver(
                mouse,
                x,
                y,
                width,
                height))
            {
                return false;
            }


            return mouse.LeftButtonReleased;
        }


        // ========================================================
        // RECTANGLE
        // ========================================================
        //
        // Diese Methode kann verwendet werden, wenn nur geprüft
        // werden soll, ob zwei rechteckige Bereiche sich
        // überschneiden.
        //
        // Das wird später beispielsweise für UI-Elemente,
        // Hitboxen oder kollidierende Buttons interessant.
        // ========================================================

        public static bool Overlaps(
            int x1,
            int y1,
            int width1,
            int height1,
            int x2,
            int y2,
            int width2,
            int height2)
        {
            return
                x1 < x2 + width2 &&
                x1 + width1 > x2 &&
                y1 < y2 + height2 &&
                y1 + height1 > y2;
        }
    }
}