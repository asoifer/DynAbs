using Xunit;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    [Collection("MainCollection")]
    public class LoopsOptimization : BaseSlicingTest
    {
        const string Skip = "TODO";

        [Fact(Skip = Skip)]
        public void LO_SimpleFor_Scalars()
        {
            var testedType = typeof(LO_SimpleFor_Scalars);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void LO_NestedFor_Scalars()
        {
            var testedType = typeof(LO_NestedFor_Scalars);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void LO_NestedFor_AddingList()
        {
            var testedType = typeof(LO_NestedFor_AddingList);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(26),
                        sameFile.WithLine(24),
                        sameFile.WithLine(13)
                    },
                WithoutSummariesExtraLines = { },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(23),
                }
            }
            );
        }
        
        [Fact(Skip = Skip)]
        public void LO_NesterFor_Benchmark_1()
        {
            var testedType = typeof(LO_NesterFor_Benchmark_1);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                    {
                        sameFile.WithLine(21)
                    },
            }
            );
        }
    }
}
