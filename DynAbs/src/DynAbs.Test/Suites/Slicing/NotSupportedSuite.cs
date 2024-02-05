using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Util;

namespace DynAbs.Test.Suites.Slicing
{
    [Collection("MainCollection")]
    public class NotSupportedSuite : BaseSlicingTest
    {
        

        [Fact]
        public void NotSupportedForeachWithCall()
        {
            var testedType = typeof(NotSupportedForeachWithCall);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                    {
                        sameFile.WithLine(21),
                        sameFile.WithLine(19),
                        sameFile.WithLine(16),
                        sameFile.WithLine(27),
                        // Use of the receiver
                        //sameFile.WithLine(25),
                        sameFile.WithLine(32),
                        sameFile.WithLine(30),
                        sameFile.WithLine(17),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                    },
            }
            );
        }       
    }
}
