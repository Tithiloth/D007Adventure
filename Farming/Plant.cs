namespace D007Adventure.Farming
    {
        public class PlantData
        {
            public string Name {get;set;}
            public string Description {get;set;}
            public int GrowthDays {get;set;}
            public string HarvestName {get;set;}
            public string CropUse {get;set;}
        }

        public class Plant
        {
            public static void PlantDiscription()
            {
                var sorrowGrapes = new PlantData
                {
                    Name = "Trauertrauben",
                    Description = "Bringen selbst den stärksten Mann zum Weinen",
                    GrowthDays = 4,
                    HarvestName = "Trauertraubensaft",
                    CropUse = ""
                };

                var someCrops = new PlantData
                {
                    Name = "Knochenspornbusch",
                    Description = "Knochensporn-Teeblätter zersetzen Wildfleisch und Knochen, wenn sie in Wasser eingelegt werden",
                    GrowthDays = 4,
                    HarvestName = "Knochenspornblatt-Umschlag",
                    CropUse = ""
                };

                Console.WriteLine(sorrowGrapes.Name);
                Console.WriteLine(someCrops.Name);
            }
        }
     }