using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class StaticAccessStringCons
    {
        public static void Main(string[] args)
        {
	        var _instance = NHibernateConfiguration.BuildSessionFactory(@"Data Source=" + Common.DataDirectory.GetDataDirectory() + ConnectionStrings.DATA_BASE_PATH, typeof (StaticAccessStringCons).Assembly);
        }
        public static class NHibernateConfiguration
        {
            internal static object BuildSessionFactory(string p, System.Reflection.Assembly assembly)
            {
                return p;
            }
        }
        public static class Common
        {
            public static DataDir DataDirectory {
                get
                {
                    return new DataDir();
                }

            }
        }
        public class DataDir
        {
            public string GetDataDirectory()
            {
                return "algo";
            }
        }
        public static class ConnectionStrings
        {
            public static string DATA_BASE_PATH = "//";
        }
    }
}
