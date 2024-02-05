using System;
namespace ExternalLibraryExample
{
    public class Binary : IDisposable
    {
        ~Binary()
        {
            Dispose(true);
        }

        public Binary()
        {

        }

        public Binary(IFramework0 callbackReceiver)
        {
            receiver0 = callbackReceiver;
        }

        public Binary(IFramework1 callbackReceiver)
        {
            receiver1 = callbackReceiver;
        }

        public Binary(IFramework1R callbackReceiver)
        {
            receiver2 = callbackReceiver;
        }

        public Binary(IFramework2 callbackReceiver)
        {
            receiver3 = callbackReceiver;
        }

        public SimpleBinary SimpleExternalMethod(object o1)
        {
            return new SimpleBinary() { };
        }

        public SimpleBinary SimpleExternalMethod(object o1, object o2) 
        {
            return new SimpleBinary() { };
        }

        public void performCallback0_Once()
        {
            receiver0.Callback();
        }

        public void performCallback0_Twice()
        {
            receiver0.Callback();
            receiver0.Callback();
        }

        public void performCallback1_Once()
        {
            object arg = new object();
            receiver1.Callback(arg);
        }

        public object performCallback1_Once_Returning(object arg0)
        {
            object val = receiver2.Callback(arg0);
            return val;
        }
        
        public object performCallback1_Once_Returning()
        {
            object arg = "hola";
            object val = receiver2.Callback(arg);
            return val;
        }

        public object performCallback2(object arg0)
        {
            var a = receiver3.Callback1(arg0);
            var b = receiver3.Callback2(a);
            return b;
        }

        public static object Test(object a)
        {
            return new object();
        }

        public static object Test(object a, object b)
        {
            return new object();
        }

        public static object Test(object a, object b, object c)
        {
            return new object();
        }

        public static int TestScalarRet(object a, object b)
        {
            return 0;
        }

        public IFramework0 receiver0 { get; set; }
        public IFramework1 receiver1 { get; set; }
        public IFramework1R receiver2 { get; set; }
        public IFramework2 receiver3 { get; set; }

        public string PropertyWithThrow
        {
            get
            {
                throw new Exception("Binary => PropertyWithThrow");
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public int this[int key]
        {
            get
            {
                return (int)receiver2.Callback((object)5);
            }
            set
            {
                receiver2.Callback((object)5);
            }
        }
        int thisProp;
        public int this[string key]
        {
            get
            {
                return (int)receiver2.Callback((object)5);
            }
            set
            {
                thisProp = value;
            }
        }

        public void Test(ref object a)
        {
            a = new object();
        }
    }
}