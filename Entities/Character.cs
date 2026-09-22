using System;
using System.Collections.Generic;
using System.Text;

namespace D007Adventure.Entities
{
    public class Character
    {

        public string Name { get; set; } = "";

        public int Health { get; set; } = 0; // Current Health
        public int BaseHealth { get; set; } = 100;
        public int MaxHealth { get; set; } = 0;
        public int Mana { get; set; } = 0; // Current Mana
        public int BaseMana { get; set; } = 100;
        public int MaxMana { get; set; } = 0;


        // Is a combined stat of all the character's armor and any armor from equipment ! 
        public int Armor { get; set; } = 0;
        public double DamageReduction => Math.Min(Armor * 0.75);


        // Damage Calculation goes here ! 
        public int BaseDamage { get; set; } = 0;
        public int WeaponDamage { get; set; } = 0;
        public double RawDamage = BaseDamage + WeaponDamage * 0.75;



        int SkillPoints { get; set; } = 0;


        // methods for using comes here later on



        // List Abilities { get; set; } = new List<Ability>();
        // private overrride void ExecuteAbility(Ability ability);


    }
   
}
