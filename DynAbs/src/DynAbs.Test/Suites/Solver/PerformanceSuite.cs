using Xunit;
using DynAbs.Test.Cases.Solver;
using DynAbs.Test.Util;

namespace DynAbs.Test.Suites.Solver
{
    
    public class PerformanceSuite : BaseSlicingTest
    {
        const string Skip = "TODO";

        [Fact(Skip = Skip)]
        public void ChainNews()
        {
            var testedType = typeof(ChainNews);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                         sameFile.WithLine(20)
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void ReachableSimple()
        {
            var testedType = typeof(ReachableSimple);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(26),
                Sliced =
                    {
                         sameFile.WithLine(26)
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void ReachabilityComplexity()
        {
            var testedType = typeof(ReachabilityComplexity);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                         sameFile.WithLine(17)
                    },
            }
            );
        }
    }
}
