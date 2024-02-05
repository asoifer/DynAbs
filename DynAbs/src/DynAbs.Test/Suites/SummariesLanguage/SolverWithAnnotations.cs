using Xunit;
using DynAbs.Test.Cases.Solver;
using DynAbs.Test.Util;
using static DynAbs.UserConfiguration;

namespace DynAbs.Test.SummariesLanguage
{
    public class SolverWithAnnotations : BaseSlicingTest
    {
        [Fact(Skip = "Check annotations for the old PTG mode")]
        public void UntilTypeA()
        {
            var testedType = typeof(UntilTypeA);
            var sameFile = SameFileStmtBuilder(testedType);
            var excluded = new ExcludedProject[1];
            excluded[0] = new ExcludedProject() {
                mode = ExcludedMode.Custom,
                classes = new ExcludedClass[] { new ExcludedClass() {
                    mode = ExcludedMode.All,
                    name = "UntilTypeA_ExternalClass",
                    methods = null
                } },
                files = null,
                name = "TestProject"
            };

            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(22),
                Sliced =
                    {
                        sameFile.WithLine(22),
                        sameFile.WithLine(17)
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(13),
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                    sameFile.WithLine(18),
                    sameFile.WithLine(19),
                    sameFile.WithLine(21)
                }
            }, 
            excluded
            );

            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(24),
                Sliced =
                    {
                        sameFile.WithLine(24),
                        sameFile.WithLine(19)
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(13),
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                    sameFile.WithLine(17),
                    sameFile.WithLine(18),
                    sameFile.WithLine(21)
                }
            },
            excluded
            );

            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(23),
                Sliced =
                    {
                        sameFile.WithLine(23),
                        sameFile.WithLine(21),
                        sameFile.WithLine(18)
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(13),
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                    sameFile.WithLine(17),
                    sameFile.WithLine(19)
                }
            },
            excluded
            );
        }
    }
}
