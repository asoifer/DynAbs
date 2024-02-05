using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PropertyInInterface
    {
        public static void Main(string[] args)
        {
            PrincipalControlPresenter presenter = new PrincipalControlPresenter();
            presenter.Func();
            var a = presenter.View.WellTypeList;
        }
        public class PrincipalControlPresenter : IPrincipalControlPresenter
        {
            public void Func()
            {
                View = new ControlView();
                View.WellTypeList = new List<string>();
            }
            public IPrincipalControlView View { get; set; }
        }
        public class ControlView : IPrincipalControlView
        {
            public List<string> WellTypeList { get; set; }
        }
        public interface IPrincipalControlView
        {
            List<string> WellTypeList { set; get; }
        }
        public interface IPrincipalControlPresenter
        {
            IPrincipalControlView View { get; set; }
        }
    }
}
