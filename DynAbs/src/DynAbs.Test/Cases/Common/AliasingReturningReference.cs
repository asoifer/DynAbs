namespace DynAbs.Test.Cases
{
    class AliasingReturningReference
    {
        public static void Main(string[] args)
        {
            Factory factory = new Factory();
            House house2 = factory.CreateWithColor("white");
            string color = house2.Color;
        }

        class Factory
        {
            public House CreateWithColor(string color_param)
            {
                House house = new House();
                house.Color = color_param;
                return house;
            }
        }

        class House
        {
            public string Color;
        }
    }
}
