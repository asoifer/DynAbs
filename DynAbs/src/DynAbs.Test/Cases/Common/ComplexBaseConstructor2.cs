using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class ComplexBaseConstructor2
    {
        public static void Main()
        {
            var node = new GreyNode(null, null);
            return;
        }

        public class GreyNode : QuadTreeNode
        {
            public GreyNode(Quadrant quadrant, QuadTreeNode parent)
                : base(quadrant, parent)
            { 
                ;
            }
        }

        public abstract class QuadTreeNode
        {
            public Quadrant quadrant;
            public QuadTreeNode nw;
            public QuadTreeNode ne;
            public QuadTreeNode sw;
            public QuadTreeNode se;
            public QuadTreeNode parent;

            public QuadTreeNode(Quadrant quad, QuadTreeNode parent)
                : this(quad, null, null, null, null, parent)
            { 
                ;
            }

            private QuadTreeNode(Quadrant quad, QuadTreeNode nw, QuadTreeNode ne, QuadTreeNode sw, QuadTreeNode se, QuadTreeNode parent)
            {
                this.quadrant = quad;
                this.nw = nw;
                this.ne = ne;
                this.sw = sw;
                this.se = se;
                this.parent = parent;
            }
        }

        public abstract class Quadrant
        {

        }
    }
}
