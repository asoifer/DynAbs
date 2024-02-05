using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class StaticPropertyWithEnum
    {
        public static void Main(string[] args)
        {
	        CommonVariables.ActualUnitId = Units.Imperial.ToString();
        }
    }
	public enum Units
	{
		Meter = 1,
		Imperial = 2
	}
	public static class CommonVariables
	{
		public static string ActualUnitId { get; set; }
	}
}
