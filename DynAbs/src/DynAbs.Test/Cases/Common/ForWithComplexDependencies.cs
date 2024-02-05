using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalLibraryExample;

namespace DynAbs.Test.Cases
{
    class ForWithComplexDependencies
    {
        public static void Main()
        {
            var customer = new Customer();
            customer.Orders.Add(new Order() { Name = "1" });
            customer.Orders.Add(new Order() { Name = "2" });
            Binary.Test(customer);
            int i = 0;
            for (i = 0; i < customer.Orders.Count; i++)
            {
                var t = customer.Orders[i];
                Console.WriteLine("{0}", t.Name);
            }
            var sliceVariable = i;
        }

        class Customer
        {
            List<Order> p_orders = new List<Order>();
            public List<Order> Orders
            {
                get
                {
                    return p_orders;
                }
                set
                {
                    p_orders = value;
                }
            }
        }

        class Order
        {
            string p_Name = "";
            public string Name
            {
                get
                {
                    return p_Name;
                }
                set
                {
                    p_Name = value;
                }
            }
        }
    }
}
