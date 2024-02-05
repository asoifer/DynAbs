using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class LambdaExpressions
    {
        delegate int del(int i);
        static void Main(string[] args)
        {
            del myDelegate = x => x * x;
            int j = myDelegate(5); //j = 25
            j = j + 1;
            return;
        }
    }
}