using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExternalLibraryExample
{
    public interface IFramework0
    {
        void Callback();
    }
    public interface IFramework1
    {
        void Callback(object arg);
    }
    public interface IFramework1R
    {
        object Callback(object arg);
    }
    public interface IFramework2
    {
        object Callback1(object arg);
        BasicClass Callback2(object arg);
    }
    public interface TestInterface
    {

    }
    public abstract class InitializePropertiesFramework
    {
        public InitializePropertiesFramework(int valor)
        {
            MyProperty = valor;
        }

        public abstract int MyProperty { get; set; }
    }
}
