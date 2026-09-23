namespace D007Adventure.Tools
{
    public class Kessel : Tool
    {
        // ===========================================================================
        // Kessel
        // ===========================================================================
        //
        // Der Kessel kann Tränke o.Ä. craften
        //
        // Beispiel:
        //
        //  InventorySlot1  + InventorySlot2    +   InventorySlot3  = InventorySlot0
        //  fuelA           + materialA         +   materialB       = materialC
        //
        // ===========================================================================



        //Stammvariablen
        public new string Name = "Kessel";
        public new bool IsWeapon = false;
        public new int Damage = 0;
        public new int Tier = 0;
        public new int MaxTier = 5;



        public int[] Inventory;
        //private Dictionary<string, int> RecipeDatabase;
        private Array[] RecipeDatabase;

        //Ability überschreiben; Kessel hat keine Ability
        public override void Ability()
        {
            return;
        }

        public void Craft()
        {
            // =========
            // Crafting
            // =========

            //Das Rezept vom Inventar filtern

                List<int> recipe = new List<int>();

                //Schleife von Index 1 bis Letztes Index; kein Index 0
                for (int i  = 1; i < Inventory.Length; i++)
                {
                    recipe.Add(Inventory[i]);
                }

            //Das Rezept craften
                //Rezept testen
                //if (RecipeDatabase.Contains(recipe))
                //{
                  //Craft  
                //} else {
                    //Don't craft
                //}
        }
    }
}
