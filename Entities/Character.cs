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
        public double Armor { get; set; } = 0;
        public double DamageReduction =>  Armor * 0.75; // Platzhalter Berechnung
        // oder meinstest du den DamageReduction als eine Methode, die die maximale Schadensreduktion berechnet? Dann könnte es so aussehen:
        // public double DamageReduction => Math.Min(Armor, Armor * 0.75);


        // Damage Calculation goes here ! 
        public double BaseDamage { get; set; } = 0;
        public double WeaponDamage { get; set; } = 0;
        public double RawDamage => BaseDamage + WeaponDamage; // Platzhalter Berechnung


        int SkillPoints { get; set; } = 0;


        // methods for using/calculation comes here later on
        public double DamageCalc()
        {
            double result = RawDamage - DamageReduction; // Platzhalter Berechnung
            return result; 
        }

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
