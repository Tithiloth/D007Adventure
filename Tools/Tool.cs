namespace D007Adventure.Tools
{
    public class Tool
    {
        public string Name { get; set; } = "default";
        public bool IsWeapon { get; set; } = false;
        public int Damage { get; set; } = 0;
        public int Tier { get; set; } = 0;
        public int MaxTier { get; set; } = 5;
        public virtual void Ability()
        {
            Logger.Error($"Werkzeug [{Name}] wurde keine Ability Funktion zugewiesen.");
        }

        //Upgrade Funktion:
        //erhöht Tier um += 1 oder mit Angabe von setTier setzt Tier auf bestimmten Integer Wert 
        public void Upgrade(int setTier = 0)
        {
            if (setTier != 0 && Tier < MaxTier)
            {
                //Wenn setTier nicht angegeben ist und MaxTier nicht erreicht ist
                Tier += 1;
            }else if (Tier >= MaxTier) 
            {
                //Wenn MaxTier schon erreicht ist: Error
                Logger.Error($"Werkzeug {Name} konnte Upgrade nicht durchführen: {Name} hat höchstes Upgrade erreicht");
            }
            else
            {
                //Wenn SetTier angegeben ist
                Tier = setTier;
            }
        }
    }
}

