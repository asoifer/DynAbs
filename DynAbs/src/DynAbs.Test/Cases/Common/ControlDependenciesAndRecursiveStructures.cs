using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class ControlDependenciesAndRecursiveStructures
    {
        static void Main()
        {
            var mainNode = Node.Make(2);
            var total = mainNode.getValues();
            var x = total;
            return;
        }

        internal class Node
        {
            Node child;
            int value;

            public Node(Node child, int val)
            {
                this.child = child;
                value = val;
            }

            public int getValues()
            {
                var total = value;
                if (child != null)
                    total += child.getValues();
                return total;
            }

            public static Node Make(int levels)
            {
                Node child = null;
                if (levels > 1)
                    child = Make(levels - 1);
                return new Node(child, 1);
            }
        }
    }
}
