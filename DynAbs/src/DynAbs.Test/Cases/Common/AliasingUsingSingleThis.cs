namespace DynAbs.Test.Cases
{
    class AliasingUsingSingleThis
    {
        public static void Main(string[] args)
        {
            B b = new B();
            int i = 25;
            int j = 42;
            b.Init("pepe");
            int k = i + j;
            string str = b.Campo;
            string hola = str;
        }


        class B
        {
            public string Campo { get; set; }

            public void Init(string valor)
            {
                this.Campo = valor;
                return;
            }
        }
    }
}
