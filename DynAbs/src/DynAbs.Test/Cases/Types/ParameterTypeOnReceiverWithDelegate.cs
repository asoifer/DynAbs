using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Types
{
    class ParameterTypeOnReceiverWithDelegate
    {
        static void Main()
        {
            var a = new A();
            var l = new List<int> { 1, 2 };
            a.AddEventHandlerDataAdded<int>(l);
            a.ExecDataAdded<int>(l);
            var new_counter = l.Count;
            return;
        }

        class A
        {
            public event EventHandler<DataAddedEventArgs> DataAdded;

            public void AddEventHandlerDataAdded<T>(List<T> collection)
            {
                DataAdded += delegate (object sender, DataAddedEventArgs e)
                        { 
                            var localDataCollection = (List<T>)sender;
                            localDataCollection.Clear();
                        };
            }

            public void ExecDataAdded<T>(List<T> collection)
            {
                DataAdded.Invoke(collection, null);
            }
        }

        public sealed class DataAddedEventArgs : EventArgs { }
    }
}
