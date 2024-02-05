using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class StructWithPrivateFields
    {
        public static void Main()
        {
            var a = new DnsNameRepresentation();
            var b = new DnsNameRepresentation();
            var c = a.Equals(b);
            return;
        }

        public struct DnsNameRepresentation
        {
            private string _punycodeName; // { get; set; }
            private string _unicodeName; // { get; set; }

            public DnsNameRepresentation(string inputDnsName)
            {
                _punycodeName = inputDnsName;
                _unicodeName = inputDnsName;
            }

            public DnsNameRepresentation(string inputPunycodeName,string inputUnicodeName)
            {
                _punycodeName = inputPunycodeName;
                _unicodeName = inputUnicodeName;
            }

            public bool Equals(DnsNameRepresentation dnsName)
            {
                bool match = false;
                if (_unicodeName != null && dnsName._unicodeName != null)
                {
                    if (String.Equals(_unicodeName,dnsName._unicodeName,StringComparison.OrdinalIgnoreCase))
                        match = true;
                }
                else if (_unicodeName == null && dnsName._unicodeName == null)
                    match = true;
                return match;
            }

            public string Punycode { get { return _punycodeName; } }

            public string Unicode { get { return _unicodeName; } }

            public override string ToString()
            {
                return String.Equals(_punycodeName, _unicodeName) ? _punycodeName : _unicodeName + " (" + _punycodeName + ")";
            }
        }
    }
}
