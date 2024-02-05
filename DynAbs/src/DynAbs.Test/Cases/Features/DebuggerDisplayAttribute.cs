using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class DebuggerDisplayAttribute
    {
        static void Main(string[] args)
        {
            InternClass intern = new InternClass();
            int x = intern.getCount();
            return;
        }

    }
}

[DebuggerDisplay("Count = {count}")]
internal class InternClass
{
    public int count = 4;
    public int getCount(){
        return count;
    }
}