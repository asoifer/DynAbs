namespace DynAbs.Test.Cases
{
    class AliasingAssigningReference
    {
        static void Main(string[] args)
        {
            ClaseB y2 = new ClaseB();
            y2.Init();
            ClaseA _inst = new ClaseA();
            _inst.Init();
            y2.punteroBA = _inst;
            int k = y2.punteroBA.campoenteroA + y2.campoenteroB;
        }

        public class ClaseA
        {
            public int campoenteroA;

            public void Init()
            {
                this.campoenteroA = 3;
                return;
            }
        }

        public class ClaseB
        {
            public int campoenteroB;
            public ClaseA punteroBA;

            public void Init()
            {
                this.campoenteroB = 3;
                this.punteroBA = null;
                return;
            }
        }
    }
}
