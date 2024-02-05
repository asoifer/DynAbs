namespace DynAbs.Test.Cases
{
    class AliasingBankTransferSample 
    {
        static void Main(string[] args)
        {
            Cuenta origen = new Cuenta();
            Cuenta destino = new Cuenta();

            Moneda pesos = new Pesos();

            decimal cantidadATransferir = 35000;

            origen.Disponible = 100000;
            origen.Moneda = pesos;
            destino.Moneda = pesos;
            destino.Disponible = 25000;

            bool pudoTransferir = Transferir(origen, destino, cantidadATransferir);

            decimal saldofinal = origen.Disponible;
        }

        public static bool Transferir(Cuenta cuentaDesde, Cuenta cuentaHasta, decimal cantidad)
        {
            bool desdeEsNacional = cuentaDesde.Moneda.EsMonedaNacional();
            bool hastaEsNacional = cuentaHasta.Moneda.EsMonedaNacional();

            if (desdeEsNacional != hastaEsNacional) return false;

            if (cuentaDesde.Disponible < cantidad) return false;

            cuentaDesde.Disponible = cuentaDesde.Disponible - cantidad;
            cuentaHasta.Disponible = cuentaHasta.Disponible + cantidad;

            return true;
        }

        public class Cuenta
        {
            public string Titular { get; set; }
            public decimal Disponible { get; set; }
            public decimal SobregiroMaximo { get; set; }
            public Moneda Moneda { get; set; }
        }

        public abstract class Moneda
        {
            public abstract bool EsMonedaNacional();
        }

        public class Pesos : Moneda
        {
            public override bool EsMonedaNacional()
            {
                return true;
            }
        }
    }
}
