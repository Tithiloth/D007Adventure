namespace D007Adventure.Tools
{
    public class Schaufel : Tool
    {
        // ===========================================================================
        // Schaufel
        // ===========================================================================
        //
        // Die Schaufel kann boden umgraben, under Samen o.Ä. ausgraben
        //
        // ===========================================================================



        //Stammvariablen
        public new string Name = "Schaufel";
        public new bool IsWeapon = true;
        public new int Damage = 1;
        public new int Tier = 0;
        public new int MaxTier = 5;


        //Ability überschreiben; Kessel hat keine Ability
        public override void Ability()
        {
            //ausgraben/umgraben
            return;
        }
    }
}
