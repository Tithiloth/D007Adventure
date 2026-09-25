namespace D007Adventure.Tools
{
    public class Messer : Tool
    {
        // =======================================================================================================
        // Messer
        // =======================================================================================================
        //
        // Das Messer kann Totems schnitzen, kleine Sachen schneiden(, Gegner/Tiere häuten für Leder und Fleisch?)
        //
        // Beispiel:
        //
        //  InventorySlot1  + InventorySlot2    = InventorySlot0
        //  materialA       + materialB         = totemA
        //
        // =======================================================================================================



        //Stammvariablen
        public new string Name = "Messer";
        public new bool IsWeapon = true;
        public new int Damage = 1;
        public new int Tier = 0;
        public new int MaxTier = 5;


        //Ability überschreiben
        public override void Ability()
        {
            //Schnitzen(/Häuten?)
            return;
        }
    }
}
