using System.Collections.Generic;
namespace DynAbs.Test.Cases
{
    class AliasingDispatch
    {
        public static void Main(string[] args)
        {
            Persona p = new Persona();
            p.Decir("alguna cosa");
        }

        class Persona
        {
            public void Decir(string queCosa)
            {
                System.Console.WriteLine(queCosa);
            }
            public void Decir(int queCosa)
            {
                System.Console.WriteLine(queCosa);
            }
            public void Decir(Persona queCosa)
            {
                System.Console.WriteLine(queCosa);
            }
            public void Decir(IList<Persona> queCosa)
            {
                System.Console.WriteLine(queCosa);
            }
        }
    }
}
