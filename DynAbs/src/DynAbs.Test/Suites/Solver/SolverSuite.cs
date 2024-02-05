using Xunit;
using DynAbs.Test.Cases.Forms;
using DynAbs.Test.Cases.Solver;
using DynAbs.Test.Util;

namespace DynAbs.Test.Suites.Solver
{
    
    public class SolverSuite : BaseSlicingTest
    {
        [Fact]
        public void SolverScalars()
        {
            var testedType = typeof(Scalars);
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

        [Fact]
        public void SolverInstrumentedNew()
        {
            var testedType = typeof(InstrumentedNew);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13)
                    },
            }
            );
        }

        [Fact]
        public void SolverNonInstrumentedNew()
        {
            var testedType = typeof(NonInstrumentedNew);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13)
                    },
            }
            );
        }

        [Fact]
        public void SolverNonInstrumentedNewWithArguments()
        {
            var testedType = typeof(NonInstrumentedNewWithArguments);
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

        [Fact]
        public void SolverNonInstrumentedChainObjects()
        {
            var testedType = typeof(NonInstrumentedChainObjects);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                    },
            }
            );
        }

        [Fact]
        public void SolverNonInstrumentedInvocation()
        {
            var testedType = typeof(NonInstrumentedInvocation);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                    },
            }
            );
        }

        [Fact(Skip = "External Library")]
        public void SolverCallback0()
        {
            var testedType = typeof(SolverCallback0);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                    },
            }
            );
        }

        [Fact]
        public void PtgVertexDisposing()
        {
            var testedType = typeof(PtgVertexDisposing);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void SolverHubCompression()
        {
            var testedType = typeof(HubCompression);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(24),
                Sliced =
                    {
                        sameFile.WithLine(24),
                    },
            }
            );
        }

        [Fact]
        public void ListAddWithReadOnlyLambda()
        {
            var testedType = typeof(ListAddWithReadOnlyLambda);
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

        [Fact]
        public void InitializeComponent()
        {
            var testedType = typeof(InitializeComponent);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14)
                    },
            }
            );
        }

        [Fact(Skip = "External Library")]
        public void InitializeComponentV2()
        {
            var testedType = typeof(InitializeComponentV2);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14)
                    },
            }
            );
        }

        [Fact(Skip = "External Library")]
        public void FormBasico()
        {
            var testedType = typeof(FormBasico);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14)
                    },
            }
            );
        }

        [Fact(Skip = "External Library")]
        public void FormWithForeachOfControls()
        {
            var testedType = typeof(FormWithForeachOfControls);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16)
                    },
            }
            );
        }

        [Fact(Skip = "External Library")]
        public void ColorBehaviour()
        {
            var testedType = typeof(ColorBehaviour);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(23),
                Sliced =
                    {
                        sameFile.WithLine(23),
                        sameFile.WithLine(22),
                        sameFile.WithLine(21),
                        sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void NewAnnotations_OfType()
        {
            var testedType = typeof(NewAnnotations_OfType);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(20),
                        sameFile.WithLine(13),
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                    sameFile.WithLine(17),
                    sameFile.WithLine(18),
                    sameFile.WithLine(19),
                }
            }
            );
        }
    }
}
