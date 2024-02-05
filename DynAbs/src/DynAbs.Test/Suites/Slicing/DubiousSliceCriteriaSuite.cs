using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Util;

namespace DynAbs.Test.Suites.Slicing
{
    [Collection("MainCollection")]
    public class DubiousSliceCriteriaSuite : BaseSlicingTest
    {
        [Fact]
        public void InvocationChainWithCall()
        {
            var testedType = typeof(InvocationChainWithCall);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(25),
                        sameFile.WithLine(35),
                        sameFile.WithLine(43),
                        sameFile.WithLine(45),
                        sameFile.WithLine(54),
                        // Use of receiver
                        //sameFile.WithLine(13),
                        //sameFile.WithLine(26),
                        //sameFile.WithLine(24),
                    },
            }
            );
        }

        [Fact]
        public void InvocationChainWithProperty()
        {
            var testedType = typeof(InvocationChainWithProperty);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        // Use of the receiver
                        //sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(26),
                        sameFile.WithLine(41),
                        sameFile.WithLine(43),
                        sameFile.WithLine(52),
                    },
            }
            );
        }

        [Fact]
        public void PreProcessorDirectiveInstrumentation()
        {
            var testedType = typeof(PreprocessorDirectiveInstrumentation);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(25),
                Sliced =
                    {
                        sameFile.WithLine(22),
                        sameFile.WithLine(25),
                    },
            }
            );
        }

        [Fact(Skip = "External library")]
        public void SimpleNonInstrumentedString()
        {
            var testedType = typeof (SimpleNonInstrumentedString);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        //aca agregué la 23, no entiendo cómo valor no va a depender
                        //del get de Obj si está accediendo a Obj???
                        //sameFile.WithLine(23),
                        sameFile.WithLine(19),
                        sameFile.WithLine(18),
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact(Skip = "External library")]
        public void SimpleNonInstrumentedObject()
        {
            var testedType = typeof (SimpleNonInstrumentedObject);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        //aca agregué la 23, no entiendo cómo valor no va a depender
                        //del get de Obj si está accediendo a Obj???
                        //sameFile.WithLine(23),
                        sameFile.WithLine(19),
                        sameFile.WithLine(18),
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
            }
            );
        }
    }
}