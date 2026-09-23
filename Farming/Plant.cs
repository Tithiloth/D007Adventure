namespace D007Adventure.Farming
{
    public class PlantData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public int GrowthDays { get; set; }
        public int MaxGrowthStages { get; set; }

        public string HarvestName { get; set; } = "";
        public int HarvestAmountMin { get; set; }
        public int HarvestAmountMax { get; set; }

        public int RegrowDays { get; set; }

        public string CropUse { get; set; } = "";

        public int GetHarvestAmount()
        {
            return Random.Shared.Next(
                HarvestAmountMin,
                HarvestAmountMax + 1
            );
        }
    }


    public class PlantInstance
    {
        public PlantData Data { get; set; } = new PlantData();

        public double GrowthProgress { get; set; } = 0.1;

        public int GetGrowthStage()
        {
            return (int)GrowthProgress;
        }
    }


    public class Plant
    {
        public static void PlantDescription()
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

//Settings settings =
//        //     JsonParser.Load<Settings>(
//        //         "Assets/Config/settings.json"
//        //     );