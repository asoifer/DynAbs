using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class LockPassthrough
    {
        public static void Main(string[] args)
        {
            Account a = new Account();
            a.balance = 20;
            a.Withdraw(10);
            decimal y = a.balance;
        }
        public class Account
        {
            public decimal balance;
            private Object thisLock;

            public void Withdraw(decimal amount)
            {
                thisLock = new Object();
                lock (thisLock)
                {
                    balance = balance - amount;
                }
            }
        }
    }
}
