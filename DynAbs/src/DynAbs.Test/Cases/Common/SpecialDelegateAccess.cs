using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class SpecialDelegateAccess
    {
        static void Main()
        {
            var a = new A<Aux>();
            var j = a.Test();
            Console.WriteLine(j);
        }

        public class Aux
        {

        }

        public class A<T> where T : Aux
        {
            List<CollectionEntry<T>> Collections { get; }

            public A()
            {
                Collections = new List<CollectionEntry<T>>();
                Collections.Add(new CollectionEntry<T>(CollectionEntry<T>.TypeTableGetMembersDelegate));
                Collections.Add(new CollectionEntry<T>(CollectionEntry<T>.TypeTableGetMembersDelegate));
            }

            public int Test()
            {
                var name = "Test";
                var i = 0;
                foreach (CollectionEntry<T> collection in Collections)
                {
                    var memberAsT = collection.GetMember(name);
                    if (memberAsT == null)
                        i++;
                }
                return i;
            }
        }

        public class CollectionEntry<T> where T : Aux
        {
            public delegate CollectionEntry<T> GetMemberDelegate(string name);
            public GetMemberDelegate GetMember { get; }

            public CollectionEntry(GetMemberDelegate getMember)
            {
                GetMember = getMember;
            }

            public static CollectionEntry<T> TypeTableGetMembersDelegate(string name)
            {
                return null;
            }
        }
    }
}
