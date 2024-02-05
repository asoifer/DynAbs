namespace DynAbs.Test.Cases
{
    class TestFieldInit
    {
        public static void Main(string[] args)
        {
            Casa casa = new Casa();
            casa.Init("Jose");
            string ret = casa.duenho;
        }
        
        class Casa
        {
            public string duenho = "valor";
            public void Init(string paramDuenho)
            {
                var pepe = duenho;
                duenho = paramDuenho;
            }
        }
    }
}
