namespace D007Adventure.Farming
{
    public class PlantData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public int GrowthDays { get; set; }
        public int MaxGrowthStages { get; set; }

        // public ItemData HarvestItem { get; set; } = new ItemData();

        public int HarvestAmountMin { get; set; }
        public int HarvestAmountMax { get; set; }

        public int RegrowDays { get; set; }

        public int GetHarvestAmount()
        {
            return Random.Shared.Next
            (
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
        public static void PlantDiscription()
        {
            var sorrowGrapes = new PlantData
            {
                Name = "Trauertrauben",
                Description = "Bringen selbst den stärksten Mann zum Weinen",
                GrowthDays = 4,
                MaxGrowthStages = 4,
                HarvestAmountMin = 2,
                HarvestAmountMax = 4,
                RegrowDays = 3,

            };

            var someCrops = new PlantData
            {
                Name = "Knochenspornbusch",
                Description = "Knochensporn-Teeblätter zersetzen Wildfleisch und Knochen, wenn sie in Wasser eingelegt werden",
                GrowthDays = 4,
                MaxGrowthStages = 4,
                HarvestAmountMin = 2,
                HarvestAmountMax = 4,
                RegrowDays = 3,
            };

            Console.WriteLine(sorrowGrapes.Name);
            Console.WriteLine(someCrops.Name);
        }
    }
}