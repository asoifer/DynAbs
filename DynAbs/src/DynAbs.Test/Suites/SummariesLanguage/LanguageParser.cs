using Xunit;
using System.Linq;
using DynAbs.Summaries;

namespace DynAbs.Test.SummariesLanguage
{
    
    public class LanguageParser
    {
        [Fact]
        public void Empty()
        {
            var r = InterpretedAnnotation.Parse("Null.{}.{}.{}.{}");
            Assert.True(r.RV == RVType.Null);
            Assert.True(r.RO.Count == 0);
            Assert.True(r.R.Count == 0);
            Assert.True(r.W.Count == 0);
            Assert.True(r.CN.Count == 0);
        }

        [Fact]
        public void Basic()
        {
            var r = InterpretedAnnotation.Parse("Null.{}.{R}.{G}.{[R, R, f]}");
            Assert.True(r.RV == RVType.Null);
            Assert.True(r.RO.Count == 0);
            Assert.True(r.R.Count == 1 && r.R.First().et.@base == BaseET.R);
            Assert.True(r.W.Count == 1 && r.W.First().et.@base == BaseET.G);
            Assert.True(r.CN.Count == 1 && r.CN.First().met1.First().@base == BaseET.R && r.CN.First().field == "f");
        }

        [Fact]
        public void Complex()
        {
            var r = InterpretedAnnotation.Parse("Null.{}.{R.*.OfType{myType}.f}.{G.f;P}.{[R.f;G, R.f, f]}");
            Assert.True(r.RV == RVType.Null);
            Assert.True(r.RO.Count == 0);
            Assert.True(r.R.Count == 1 && r.R.First().et.ofType.First().Name == "myType");
            Assert.True(r.W.Count == 2);
            Assert.True(r.CN.Count == 1);
        }

        [Fact]
        public void OtherKeywords()
        {
            var r = InterpretedAnnotation.Parse("IsIn{R;G.*}.{Many{A, B}}.{}.{}.{[R.f;G, R.f, f], [P, R, ?]}");
            Assert.True(r.RV == RVType.IsIn);
            Assert.True(r.RVMET.Count == 2 && r.RVMET.Last().chainOfFields.Last() == "*");
            Assert.True(r.RO.Count == 1);
            Assert.True(r.RO.Single().Type == ROType.Many);
            Assert.True(r.RO.Single().ROTypes.Count == 2 && r.RO.Single().ROTypes.First().Name == "A");
            Assert.True(r.R.Count == 0);
            Assert.True(r.W.Count == 0);
            Assert.True(r.CN.Count == 2 && r.CN.Last().field == "?");
        }

        [Fact]
        public void BeginTransaction()
        {
            var r = InterpretedAnnotation.Parse("Fresh.{Many}.{R.?;R.?.?}.{R.?;R.?.?;RV.?;RO.?}.{[RV;RO, RO, ?], [R, RV, ?]}");
            Assert.True(r.RV == RVType.Fresh);
            Assert.True(r.RO.Count == 1);
            Assert.True(r.RO.Single().Type == ROType.Many);
            Assert.True(r.R.Count == 2);
            Assert.True(r.W.Count == 4);
            Assert.True(r.CN.Count == 2);
        }

        [Fact]
        public void KindInFresh()
        {
            var r = InterpretedAnnotation.Parse("Fresh{|A}.{}.{}.{}.{}");
            Assert.True(r.RV == RVType.Fresh);
            Assert.True(r.RVKind == "A");
        }

        [Fact]
        public void MultipleOtherObjects()
        {
            var r = InterpretedAnnotation.Parse("Null.{Single{T1|K1};Many{T2|K2}}.{}.{}.{[RO[0], RO, ?]}");
            Assert.True(r.RO.Count == 2);
            Assert.True(r.RO[0].ROTypes.Count == 1 && r.RO[0].ROTypes.Single().Name == "T1");
            Assert.True(r.RO[0].Kind == "K1");
            Assert.True(r.RO[1].ROTypes.Count == 1 && r.RO[1].ROTypes.Single().Name == "T2");
            Assert.True(r.RO[1].Kind == "K2");
            Assert.True(r.CN.Count == 1);
            Assert.True(r.CN.Single().met1.Single().@base == BaseET.RO && r.CN.Single().met1.Single().ParamIndex == 0);
            Assert.True(r.CN.Single().met2.Single().@base == BaseET.RO && !r.CN.Single().met2.Single().ParamIndex.HasValue);
        }

        [Fact]
        public void ParametrizedField()
        {
            var r = InterpretedAnnotation.Parse("Null.{}.{R.@p1}.{}.{}");
            Assert.True(r.R.Count == 1);
            Assert.True(r.R.Single().field == "@p1");
            Assert.True(r.Parameters.Single() == "p1");
        }
    }
}
