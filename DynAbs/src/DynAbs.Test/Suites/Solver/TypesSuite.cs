using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Cases.Types;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    [Collection("MainCollection")]
    public class TypesSuite : BaseSlicingTest
    {
        const string Skip = "TODO";

        [Fact(Skip = Skip)]
        public void SingletonA()
        {
            var testedType = typeof(SingletonA);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(19),
                        sameFile.WithLine(18),
                        sameFile.WithLine(15),
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(24),
                    sameFile.WithLine(40),
                    sameFile.WithLine(45),
                    sameFile.WithLine(47),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void SingletonB()
        {
            var testedType = typeof(SingletonB);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15)
                    },
            }
            );
        }
        [Fact(Skip = Skip)]
        public void CastingElementsForeach()
        {
            var testedType = typeof(CastingElementsForeach);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(23),
                Sliced =
                    {
                        sameFile.WithLine(23)
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void ParameterTypeOnReceiverWithDelegate()
        {
            var testedType = typeof(ParameterTypeOnReceiverWithDelegate);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(17),
                        sameFile.WithLine(27),
                        sameFile.WithLine(16),
                        sameFile.WithLine(36),
                        sameFile.WithLine(34),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void ForeachWithGenericCollection()
        {
            var testedType = typeof(ForeachWithGenericCollection);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(21),
                        sameFile.WithLine(20),
                        sameFile.WithLine(19),
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                        sameFile.WithLine(10),
                    },
            }
            );
        }

        [Fact(Skip = "TODO: Revisar")]
        public void Pointers_A()
        {
            var testedType = typeof(Pointers_A);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(22),
                        sameFile.WithLine(27),
                        sameFile.WithLine(33),
                        sameFile.WithLine(30),
                        sameFile.WithLine(21),
                        sameFile.WithLine(19),
                        sameFile.WithLine(13),
                    },
            }
            );
        }
    }
}
