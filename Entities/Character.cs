using System;
using System.Collections.Generic;
using System.Text;

namespace D007Adventure.Entities
{
    public class Character
    {

        public string Name { get; set; } = "";

        public int Health { get; set; } = 0; // Current Health

        public int BaseHealth { get; set; } = 100; // brauchen wir vielleicht nicht
        public int MaxHealth { get; set; } = 0;
        public int Mana { get; set; } = 0; // Current Mana
        public int BaseMana { get; set; } = 100; // brauchen wir vielleicht nicht

        public int MaxMana { get; set; } = 0;


        // Is a combined stat of all the character's armor and any armor from equipment ! 

        public int Armor { get; set; } = 0;
        public double DamageReduction => Math.Min(Armor, Armor * 0.75);


        // Damage Calculation goes here ! 
        public int BaseDamage { get; set; } = 0;
        public int WeaponDamage { get; set; } = 0;
        //public double RawDamage = BaseDamage + WeaponDamage * 0.75;

        // Platzhalter Berechnung
        // oder meinstest du den DamageReduction als eine Methode, die die maximale Schadensreduktion berechnet? Dann könnte es so aussehen:
        // public double DamageReduction => Math.Min(Armor, Armor * 0.75);

        // methods for using comes here later on

        // methods for using/calculation comes here later on

        public void ActualDamage(double damage)
        {
            Health -= (int)damage;

            //Später können wir hier noch eine Methode einbauen, die prüft, ob der Charakter noch lebt oder nicht.
            //und:
            //double damage = DamageCalc();
            //ActualDamage(damage);
        }

        // List Abilities { get; set; } = new List<Ability>();
        // private overrride void ExecuteAbility(Ability ability);


    }
   
}
