namespace DynAbs.Test.Cases
{
    class MemberAccessCombined
    {
        public static void Main(string[] args)
        {
            Alice alice = new Alice();
            string almacenado = alice.GiveBob().claire.david.DavDaUnValor();
        }
        class Alice
        {
            public Bob GiveBob()
            {
                Bob bob = new Bob();
                bob.claire = new Claire();
                bob.claire.david = new David();
                return bob;
            }
        }

        class Bob
        {
            public Claire claire;
        }
        class Claire
        {
            public David david;
        }
        class David
        {
            public string DavDaUnValor()
            {
                return "un valor";
            }
        }
    }
}
