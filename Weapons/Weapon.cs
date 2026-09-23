namespace D007Adventure.Weapons
{
    public class Weapon
    {
        //Stammvariablen
        public string Name { get; set; } = "";
        public int Damage { get; set; } = 0;



        //Fähigkeit die jede Waffe überschreiben soll
        public void Ability()
        {
            Logger.Error($"Waffe [{Name}] wurde keine Ability Funktion zugewiesen.");
        }
    }
}
