using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class AutoProperties
    {
        static void Main(string[] args)
        {
            Client cust1 = new Client("Northwind");
            cust1.Name = "Christian";
            return;
        }

    }


    class Client
    {
        public string Name { get; set; }

        public Client(string name)
        {
            Name = name;
        }
    }
}