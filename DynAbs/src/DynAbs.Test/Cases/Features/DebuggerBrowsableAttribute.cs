using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class DebuggerBrowsableAttribute
    {
        public static void Main(string[] args)
        {   
            Customer c = new Customer { CustomerName = "HarryCustomer"};
            return;
        }
    }
}

public class Customer
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string CustomerName
    {
        set;
        get;
    }
}