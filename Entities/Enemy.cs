using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Schema;

namespace D007Adventure.Entities
{
   
    public class Enemy : Character
    {
        public virtual void  Attributes()  // Set the states of each charackter here (should be in Awake())
        {
         
        
        }
           
       public virtual void Verhalten() // Calculate the distance between enemy and player and act according to it
       {
          
       }
         // Enemy possible Actions
         public virtual void Action1() { }
         public virtual void Action2() { }
         public virtual void Action3() { }
    }

    
    public class NormalEnemy : Character // first simple enemy
    {
        //Change numbers later
        public override void Attributes() 
        {
          Health = 0;
          MaxHealth = 0;
          BaseDamage = 0;
        }




    }





}