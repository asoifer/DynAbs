using System;
using System.Globalization;

namespace DynAbs.Test.Cases
{
    class OutInlineInAccessors
    {
        static void Main()
        {
            var a = new A();
            a.Time = DateTime.Now;
            var b = a.Time.ToString(" hh:mm");
            Console.WriteLine(b);
            return;
        }

        class A
        {
            string Text;

            public DateTime Time
            {
                get
                {
                    if (!DateTime.TryParseExact(Text.ToString(), " hh:mm", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
                        return new DateTime();
                    return result;
                }
                set
                {
                    this.Text = value.ToString(" hh:mm");
                }
            }
        }
    }
}
