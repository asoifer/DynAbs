using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace DynAbs.Test.Cases
{
    public class LambdaTreeExpressions
    {
        delegate int del(int i);
        static void Main(string[] args)
        {
            Expression<del> myET = x => x * x;
            Console.WriteLine(myET.ToString()); 
            return;
        }
    }
}