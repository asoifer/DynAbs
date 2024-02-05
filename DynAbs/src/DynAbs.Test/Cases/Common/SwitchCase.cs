namespace DynAbs.Test.Cases
{
    class SwitchCase
    {
        public static void Main(string[] args)
        {
            int caseSwitch = 2;
            int x = 0;
            switch (caseSwitch)
            {
                case 1: { 
                    x = 1;
                    break;
                }
                case 2:
                    x = 2;
                    break;
                default:
                    x = 3;
                    break;
            }
            int y = x;
        }
    }
}
