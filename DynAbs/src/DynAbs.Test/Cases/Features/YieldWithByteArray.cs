using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class YieldWithByteArray
    {
        static void Main()
        {
            var data = GetAllRawData();
            Console.WriteLine(data.First().Key);
            return;
        }

        static IEnumerable<KeyValuePair<uint, byte[]>> GetAllRawData()
        {
            var memStream = new uint[] { 1, 2, 3 };
            foreach (var data in memStream)
            {
                yield return new KeyValuePair<uint, byte[]>(data, new byte[] { });
            }
        }
    }
}
