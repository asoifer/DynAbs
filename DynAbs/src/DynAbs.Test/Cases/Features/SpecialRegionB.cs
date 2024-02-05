using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class SpecialRegionB
    {
        static void Main()
        {
            var t = M();
            Console.WriteLine(t);
            var t2 = CentralAccessPolicy;
            return;
        }

#if PEPITO
        static bool M()
        {
            return true;
        }
#else
        static bool M()
        {
            return false;
        }
#endif

#if !PEPITO
        /// <summary>
        /// Parameter '-CentralAccessPolicy' is not supported in OneCore powershell,
        /// because function 'LsaQueryCAPs' is not available in OneCoreUAP and NanoServer.
        /// </summary>
        private static string CentralAccessPolicy
        {
            get; set;
        }
#else
        private static string centralAccessPolicy;

        /// <summary>
        /// Gets or sets the central access policy to be
        /// set on the target item(s).
        /// </summary>
        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPath")]
        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByLiteralPath")]
        public static string CentralAccessPolicy
        {
            get
            {
                return centralAccessPolicy;
            }

            set
            {
                centralAccessPolicy = value;
            }
        }
#endif

        private static bool AnotherMethod()
        {
            return true;
        }
    }
}
