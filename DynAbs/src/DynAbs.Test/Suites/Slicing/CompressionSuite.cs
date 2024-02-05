using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Util;

namespace DynAbs.Test.Suites.Slicing
{
    [Collection("MainCollection")]
    public class CompressionSuite : BaseSlicingTest
    {
        [Fact]
        public void Figure7()
        {
            var testedType = typeof (Figure7);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(32),
                Sliced =
                    {
                        sameFile.WithLine(32),
                        sameFile.WithLine(30),
                        sameFile.WithLine(29),
                        sameFile.WithLine(23),
                        sameFile.WithLine(21),
                        sameFile.WithLine(20),
                        sameFile.WithLine(18),
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                    },
            }
            );
        }

        [Fact]
        public void WhileMultiple()
        {
            var testedType = typeof(WhileMultiple);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(20),
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void ListAddFor()
        {
            var testedType = typeof(ListAddFor);
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
    }
}
