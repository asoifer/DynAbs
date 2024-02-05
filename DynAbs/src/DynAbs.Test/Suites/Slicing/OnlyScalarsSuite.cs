using System;
using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    [Collection("MainCollection")]
    public class OnlyScalarsSuite : BaseSlicingTest
    {
        [Fact]
        public void DynamicSlicingPaperExample1XIquals0()
        {
            var testedType = typeof (DynamicSlicingPaperExample1XIquals0);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(29),
                Sliced =
                    {
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                        sameFile.WithLine(29),
                    },
            }
            );

            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(31),
                Sliced =
                    {
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(17),
                        sameFile.WithLine(20),
                        sameFile.WithLine(31),
                    },
            }
            );
        }

        [Fact]
        public void DynamicSlicingPaperExample1XLess0()
        {
            var testedType = typeof (DynamicSlicingPaperExample1XLess0);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(35),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(18),
                        sameFile.WithLine(35),
                    },
            }
            );

            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(37),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(19),
                        sameFile.WithLine(37),
                    },
            }
            );
        }

        [Fact]
        public void DynamicSlicingPaperExample2NIquals2()
        {
            var testedType = typeof (DynamicSlicingPaperExample2NIquals2);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(23),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                        sameFile.WithLine(20),
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                    },
            }
            );
        }

        //[Fact]
        //public void LegatingByControlStmt()
        //{
        //    var testedType = typeof (LegatingByControlStmt);
        //    var sameFile = SameFileStmtBuilder(testedType);
        //    TestSimpleSlice(testedType, new DynAbsResult
        //    {
        //        Criteria = sameFile.WithLine(19),
        //        Sliced =
        //            {
        //                sameFile.WithLine(19),
        //            },
        //    }
        //    );

        //    TestSimpleSlice(testedType, new DynAbsResult
        //    {
        //        Criteria = sameFile.WithLine(26),
        //        Sliced =
        //            {
        //                sameFile.WithLine(13),
        //                sameFile.WithLine(15),
        //                sameFile.WithLine(17),
        //                sameFile.WithLine(19),
        //                sameFile.WithLine(20),
        //                sameFile.WithLine(22),
        //                sameFile.WithLine(26),
        //            },
        //    }
        //    );
        //}

        [Fact]
        public void VerySimpleSlicing()
        {
            var testedType = typeof (VerySimpleSlicing);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(29),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(18),
                        sameFile.WithLine(29),
                    },
            }
            );

            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(32),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(21),
                        sameFile.WithLine(22),
                        sameFile.WithLine(24),
                        sameFile.WithLine(25),
                        sameFile.WithLine(32),
                    },
            }
            );

            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(35),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(18),
                        sameFile.WithLine(21),
                        sameFile.WithLine(22),
                        sameFile.WithLine(24),
                        sameFile.WithLine(25),
                        sameFile.WithLine(29),
                        sameFile.WithLine(32),
                        sameFile.WithLine(35),
                    },
            }
            );
        }

        [Fact]
        public void SimpleCallIgnoreParameter()
        {
            Type testedType = typeof(SimpleCallIgnoreParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(10),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(14),
                        sameFile.WithLine(16),
                    },
            }
            );
        }
        
        [Fact]
        public void SlicingSimpleCallFirstStmtIf()
        {
            Type testedType = typeof(SlicingSimpleCallFirstStmtIf);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(26),
                    },
            }
            );
        }

        [Fact]
        public void ScalarDetectCallInCondition()
        {
            Type testedType = typeof(ScalarDetectCallInCondition);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(22),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(16),
                        sameFile.WithLine(22),
                        sameFile.WithLine(25),
                        sameFile.WithLine(27),
                        sameFile.WithLine(29),
                    },
            }
            );
        }

        [Fact]
        public void SimpleCallNestedCall()
        {
            Type testedType = typeof(SimpleCallNestedCall);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(8),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(20),
                        sameFile.WithLine(22),
                        sameFile.WithLine(25),
                        sameFile.WithLine(27),
                        sameFile.WithLine(30),
                        sameFile.WithLine(32),
                    },
            }
            );
        }

        //[Fact]
        public void SimpleCallNestedConstructor()
        {
            Type testedType = typeof(SimpleCallNestedConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(20),
                        sameFile.WithLine(21),
                        sameFile.WithLine(30),
                    },
            }
            );
        }


        [Fact]
        public void SimpleCallNestedExpression()
        {
            Type testedType = typeof(SimpleCallNestedExpression);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(8),
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(20),
                        sameFile.WithLine(22),
                        sameFile.WithLine(25),
                        sameFile.WithLine(27),
                        sameFile.WithLine(30),
                        sameFile.WithLine(32),
                    },
            }
            );
        }

        [Fact]
        public void SimpleCallNestedExpressionOutside()
        {
            Type testedType = typeof(SimpleCallNestedExpressionOutside);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(8),
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(20),
                        sameFile.WithLine(22),
                        sameFile.WithLine(25),
                        sameFile.WithLine(27),
                        sameFile.WithLine(30),
                        sameFile.WithLine(32),
                    },
            }
            );
        }

        [Fact]
        public void CallWithinAnotherCall()
        {
            Type testedType = typeof(CallWithinAnotherCall);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(8),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(8),
                        sameFile.WithLine(11),
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                    },
            }
            );
        }


        /// <summary>
        /// FIXME: works but `j` shouldn't be connected to `(inicial == 1)` by control. Related to early return (bug #10)
        /// </summary>
        [Fact]
        public void CallRecursive()
        {
            Type testedType = typeof(CallRecursive);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(9),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(8),
                        sameFile.WithLine(9),
                        sameFile.WithLine(12),
                        sameFile.WithLine(14),
                        sameFile.WithLine(16),
                    },
            }
            );
        }

        [Fact]
        public void NonInstrumentedCall()
        {
            Type testedType = typeof(NonInstrumentedCall);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(8),
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                    },
            }
            );
        }

        [Fact]
        public void NonInstrumentedCallAsParameter()
        {
            Type testedType = typeof(NonInstrumentedCallAsParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(8),
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                        sameFile.WithLine(25),
                    },
            }
            );
        }


        [Fact]
        public void ImplicitConstructor()
        {
            Type testedType = typeof(ImplicitConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(8),
                        sameFile.WithLine(12),
                    },
            }
            );
        }

        [Fact]
        public void ExplicitConstructorPrefixedThis()
        {
            Type testedType = typeof(ExplicitConstructorPrefixedThis);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(23),
                        sameFile.WithLine(24),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void ExplicitConstructorUnprefixedThis()
        {
            Type testedType = typeof(ExplicitConstructorUnprefixedThis);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(23),
                        sameFile.WithLine(24),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void ScalarSimpleFor()
        {
            Type testedType = typeof(ScalarSimpleFor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact]
        public void ScalarForWithDeclaration()
        {
            Type testedType = typeof(ScalarForWithDeclaration);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(16),
                        sameFile.WithLine(18),
                    },
            }
            );
        }

        [Fact]
        public void BreakingGlobal()
        {
            Type testedType = typeof(BreakingGlobal);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(23),
                Sliced =
                    {
                        sameFile.WithLine(11),
                        sameFile.WithLine(18),
                        sameFile.WithLine(19),
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                    },
            }
            );
        }

        [Fact]
        public void ScalarDoubleUse()
        {
            Type testedType = typeof(ScalarDoubleUse);
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

        [Fact]
        public void ScalarNonSettedField()
        {
            Type testedType = typeof(ScalarNonSettedField);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(19),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void ClassConstrWithoutInstructions()
        {
            Type testedType = typeof(ClassConstrWithoutInstructions);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void ConstField()
        {
            Type testedType = typeof(ConstField);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(15),
                        sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact]
        public void ClassicForWithExternalDependency()
        {
            Type testedType = typeof(ClassicForWithExternalDependency);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(18),
                        sameFile.WithLine(14),
                        sameFile.WithLine(12),
                    },
            }
            );
        }
    }
}