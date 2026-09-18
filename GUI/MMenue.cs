using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using D007Adventure;
using D007Adventure.Input;

namespace D007Adventure.GUI
{
    public class MMenue
    {
        public MMenue(D007Adventure.Input.Input input) 
        {
            int MainMenue;

            if(input.IsKeyDown(ConsoleKey.M))
            {
                MainMenue = 1;

                if (MainMenue == 1)
                    {
                    Logger.Info("MainMenü initialisiert");
                    }
            } 
        }
    }
}