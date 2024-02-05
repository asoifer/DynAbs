using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SingletonB
    {
        public static void Main()
        {
            AddTab<PrincipalControl>();
            return;
        }

        private static void AddTab<T>() where T : class
        {
            var pepe = Generador<T>.Instance;
        }

        public interface IPrincipalControlView { }

        public partial class PrincipalControl : IPrincipalControlView
        {
            public PrincipalControl()
            {
                PrincipalControlPresenter.GetInstance(this);
            }
        }

        public interface IPrincipalControlPresenter
        {
            IPrincipalControlView View { get; set; }
        }

        public class PrincipalControlPresenter : IPrincipalControlPresenter
        {
            private static PrincipalControlPresenter _principalControlPresenter;
            public IPrincipalControlView View { get; set; }

            private PrincipalControlPresenter() { }

            public static PrincipalControlPresenter GetInstance(IPrincipalControlView view)
            {
                GetInstance().View = view;
                return _principalControlPresenter;
            }

            public static PrincipalControlPresenter GetInstance()
            {
                return _principalControlPresenter ?? (_principalControlPresenter = new PrincipalControlPresenter());
            }
        }

        public static class Generador<T> where T : class
        {
            private static volatile T _instance;
            private static readonly object _lock = new object();
            public static T Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        lock (_lock)
                        {
                            ConstructorInfo constructor = null;
                            constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[0], null);

                            _instance = (T)constructor.Invoke(null);
                        }
                    }

                    return _instance;
                }
            }
        }
    }
}
