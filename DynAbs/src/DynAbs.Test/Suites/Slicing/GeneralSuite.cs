using System;
using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Util;
using DynAbs.Test.Cases.Common;

namespace DynAbs.Test
{
    [Collection("MainCollection")]
    public class GeneralSuite : BaseSlicingTest
    {
        [Fact]
        public void ListAddingElements()
        {
            var testedType = typeof(ListAddingElements);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        //estas 3 lineas no se slicean porque list.Add no modifica el puntero sino que solamente el objeto interno (en el aPtG)
                        //sameFile.WithLine(16),
                        //sameFile.WithLine(15),
                        //sameFile.WithLine(14),
                        sameFile.WithLine(13),
                    },
            }
            );
        }
        [Fact]
        public void ListAddingElementsCount()
        {
            var testedType = typeof(ListAddingElementsCount);
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
        public void NewArray()
        {
            var testedType = typeof(NewArray);
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

        [Fact]
        public void NewUsingThisWithArguments()
        {
            var testedType = typeof(NewUsingThisWithArguments);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(12),
                        sameFile.WithLine(22),
                        sameFile.WithLine(20),
                        sameFile.WithLine(25),
                        sameFile.WithLine(35),
                        sameFile.WithLine(11)
                    },
            }
            );
        }

        [Fact]
        public void MethodFromTwoReceptors()
        {
            var testedType = typeof(MethodFromTwoReceptors);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(22),
                Sliced =
                    {
                        sameFile.WithLine(22),
                    },
            }
            );
        }
        [Fact]
        public void MethodFromTwoReceptorsWithDependencies()
        {
            var testedType = typeof(MethodFromTwoReceptorsWithDependencies);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(20),
                        sameFile.WithLine(36),
                        sameFile.WithLine(35),
                        sameFile.WithLine(33),
                        sameFile.WithLine(28),
                        sameFile.WithLine(26),
                        sameFile.WithLine(15),
                    },
            }
            );
        }
        [Fact]
        public void MethodFromTwoReceptorsWithDoubleDependencies()
        {
            var testedType = typeof(MethodFromTwoReceptorsWithDoubleDependencies);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(20),
                        sameFile.WithLine(36),
                        sameFile.WithLine(35),
                        sameFile.WithLine(33),
                        sameFile.WithLine(28),
                        sameFile.WithLine(26),
                        sameFile.WithLine(15),
                        sameFile.WithLine(18),
                        sameFile.WithLine(17),
                        sameFile.WithLine(14),
                    },
            }
            );
        }
        [Fact]
        public void AliasingTestCopyRef()
        {
            var testedType = typeof(AliasingTestCopyRef);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                        sameFile.WithLine(12),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void AliasingTestCopyScalar()
        {
            var testedType = typeof(AliasingTestCopyScalar);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                    },
            }
            );
        }

        [Fact]
        public void CallWithReferencesParameters()
        {
            var testedType = typeof(CallWithReferencesParameters);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                        sameFile.WithLine(20),
                        sameFile.WithLine(21),
                        sameFile.WithLine(24),
                        sameFile.WithLine(26),
                        sameFile.WithLine(27),
                    },
            }
            );
        }

        [Fact]
        public void AliasingReturningReference()
        {
            var testedType = typeof(AliasingReturningReference);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(9),
                Sliced =
                    {
                        sameFile.WithLine(8),
                        sameFile.WithLine(9),
                        sameFile.WithLine(14),
                        sameFile.WithLine(17),
                        // Non use of the receiver
                        //sameFile.WithLine(7),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void AliasingUsingSingleThis()
        {
            var testedType = typeof(AliasingUsingSingleThis);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(10),
                        sameFile.WithLine(12),
                        sameFile.WithLine(13),
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void AliasingUsingNestedThis()
        {
            var testedType = typeof(AliasingUsingNestedThis);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(8),
                        sameFile.WithLine(11),
                        sameFile.WithLine(12),
                        sameFile.WithLine(19),
                        sameFile.WithLine(21),
                        sameFile.WithLine(22),
                        sameFile.WithLine(23),
                        sameFile.WithLine(24),
                        sameFile.WithLine(29),
                        sameFile.WithLine(38),
                        sameFile.WithLine(40),
                        sameFile.WithLine(41),
                        sameFile.WithLine(46),
                        sameFile.WithLine(10),
                        sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void AliasingUsingNestedThis2nd()
        {
            var testedType = typeof(AliasingUsingNestedThis);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(40),
                Sliced =
                    {
                        sameFile.WithLine(40),
                        sameFile.WithLine(46),
                        // Control dependencies
                        sameFile.WithLine(22),
                        sameFile.WithLine(20),
                        sameFile.WithLine(11),
                        // Receiver
                        sameFile.WithLine(7),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void AliasingPassingReference()
        {
            var testedType = typeof(AliasingPassingReference);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(10),
                Sliced =
                    {
                        sameFile.WithLine(10),
                        sameFile.WithLine(20),
                        sameFile.WithLine(9),
                        sameFile.WithLine(7),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void AliasingAssigningReference()
        {
            var testedType = typeof(AliasingAssigningReference);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(12),
                        sameFile.WithLine(21),
                        sameFile.WithLine(33),
                        sameFile.WithLine(8),
                        sameFile.WithLine(7),
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                    },
            }
            );
        }

        [Fact]
        public void AliasingGlobal()
        {
            var testedType = typeof(AliasingGlobal);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                    },
            }
                );
        }
        [Fact]
        public void AliasingGlobalPrefix()
        {
            var testedType = typeof(AliasingGlobalPrefix);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                    },
            }
                );
        }

        [Fact]
        public void AliasingGlobalWithConstructor()
        {
            var testedType = typeof(AliasingGlobalWithConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
            }
                );
        }


        //[Fact]
        //public void AliasingSwitchCaseWithoutBreak()
        //{
        //    var testedType = typeof(AliasingSwitchCaseWithoutBreak);
        //    var sameFile = SameFileStmtBuilder(testedType);
        //    TestSimpleSlice(testedType, new DynAbsResult
        //    {
        //        Criteria = sameFile.WithLine(16),
        //        Sliced =
        //            {
        //                sameFile.WithLine(7),
        //                sameFile.WithLine(8),
        //                sameFile.WithLine(9),
        //                sameFile.WithLine(16),
        //            },
        //    }
        //        );
        //}

        [Fact]
        public void NonInstrumentedCallParentInMetadata()
        {
            var testedType = typeof(NonInstrumentedCallParentInMetadata);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                    },
            }
                );
        }

        [Fact]
        public void NonInstrumentedStatic()
        {
            var testedType = typeof(NonInstrumentedStatic);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(9),
                        sameFile.WithLine(10),
                        sameFile.WithLine(11),
                    },
            }
                );
        }

        [Fact]
        public void SwitchCase()
        {
            var testedType = typeof(SwitchCase);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(22),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(9),
                        sameFile.WithLine(16),
                        sameFile.WithLine(22),
                    },
            }
                );
        }

        [Fact]
        public void SwitchWithVariableDeclaration()
        {
            var testedType = typeof(SwitchWithVariableDeclaration);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(25),
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                    },
            }
                );
        }

        [Fact(Skip = "TODO")]
        public void SwitchCaseWhen()
        {
            var testedType = typeof(SwitchCaseWhen);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(25),
                Sliced =
                    {
                        sameFile.WithLine(25),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(31),
                        sameFile.WithLine(13),
                        sameFile.WithLine(11),
                        sameFile.WithLine(7),
                    },
            });
        }

        [Fact]
        public void SwitchWithLambda()
        {
            var testedType = typeof(SwitchWithLambda);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(10),
                        sameFile.WithLine(28),
                        sameFile.WithLine(38),
                        sameFile.WithLine(41),
                        sameFile.WithLine(44),
                        sameFile.WithLine(47),
                        sameFile.WithLine(52),
                        sameFile.WithLine(54),
                        sameFile.WithLine(55),
                        sameFile.WithLine(57),
                        sameFile.WithLine(60),
                    },
            });
        }

        [Fact(Skip = "TODO")]
        public void SwitchWithDeconstructor()
        {
            var testedType = typeof(SwitchWithDeconstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(18),
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(13),
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(31),
                        sameFile.WithLine(29),
                        sameFile.WithLine(27),
                        sameFile.WithLine(24),
                        sameFile.WithLine(22)
                    },
            });
        }

        [Fact]
        public void AliasingDispatch()
        {
            var testedType = typeof(AliasingDispatch);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(9),
                Sliced =
                    {
                        sameFile.WithLine(9),
                        //Alejandro cree que estas 3 líneas también debieran formar parte del Slice
                        //seguramente quedarán automáticamente cuando solucionemos el tema de
                        //las dependencias de control por llamada
                        //sameFile.WithLine(8),
                        //sameFile.WithLine(14),
                        //sameFile.WithLine(16),
                    },
            }
                );
        }

        [Fact]
        public void BreakingWhileInsideIf()
        {
            var testedType = typeof(BreakingWhileInsideIf);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(9),
                        sameFile.WithLine(11),
                        sameFile.WithLine(15),
                    },
            }
                );
        }

        [Fact]
        public void SlicingDispatchLookupNonInstrumented()
        {
            var testedType = typeof(SlicingDispatchLookupNonInstrumented);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(8),
                Sliced =
                    {
                        sameFile.WithLine(7),
                        sameFile.WithLine(8),
                    },
            }
                );
        }

        [Fact]
        public void SlicingDispatchLookupInstrumented()
        {
            var testedType = typeof(SlicingDispatchLookupInstrumented);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(8),
                Sliced =
                    {
                        sameFile.WithLine(8),
                        sameFile.WithLine(15),
                        // Non use of the receiver
                        //sameFile.WithLine(7),
                    },
            }
                );
        }

        [Fact]
        public void AliasingAssigningSimpleRef()
        {
            var testedType = typeof(AliasingAssigningSimpleRef);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(16),
                    },
            }
            );
        }

        [Fact]
        public void AliasingAllocOnNull()
        {
            var testedType = typeof(AliasingAllocOnNull);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void AliasingRecoveringReference()
        {
            var testedType = typeof(AliasingRecoveringReference);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(25),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void GlobalTwiceEntry()
        {
            var testedType = typeof(GlobalTwiceEntry);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(19),
                        sameFile.WithLine(17),
                    },
            }
            );
        }

        [Fact]
        public void AliasingFollowReferences()
        {
            var testedType = typeof(AliasingFollowReferences);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(22),
                Sliced =
                    {
                        //Alejandro cree que este slice tiene muchos falsos negativos
                        //habría que hacer una segunda iteración de análisis, con
                        //Christian/Maxi, y con Victor/Diego
                        sameFile.WithLine(16),
                        sameFile.WithLine(22),
                    },
            }
            );
        }

        [Fact]
        public void AliasingReturningRefExpression()
        {
            var testedType = typeof(AliasingReturningRefExpression);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(20),
                        sameFile.WithLine(31),
                        sameFile.WithLine(14),
                        // Use of the receiver
                        //sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void BindingInInvocation()
        {
            var testedType = typeof(ThisBindingInInvocation);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(30),
                        sameFile.WithLine(25),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void ActionOnFieldNeedingThisPrefix()
        {
            var testedType = typeof(ActionOnFieldNeedingThisPrefix);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(23),
                        sameFile.WithLine(30),
                        sameFile.WithLine(31),
                        sameFile.WithLine(39),
                        // Call control dependency
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void ActionOnFieldThroughMemberAccess()
        {
            var testedType = typeof(ActionOnFieldThroughMemberAccess);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(23),
                        sameFile.WithLine(22),
                        sameFile.WithLine(28),
                        sameFile.WithLine(36),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void ActionInArgument()
        {
            var testedType = typeof(ActionInArgument);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(11),
                        sameFile.WithLine(10),
                        sameFile.WithLine(36),
                        sameFile.WithLine(34),
                        sameFile.WithLine(31),
                        sameFile.WithLine(19),
                        sameFile.WithLine(17),
                        sameFile.WithLine(9),
                        sameFile.WithLine(23),
                        sameFile.WithLine(21),
                    },
            }
            );
        }

        [Fact]
        public void ArrayMultipleLastDef()
        {
            var testedType = typeof(ArrayMultipleLastDef);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(18),
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                    },
            }
            );
        }


        [Fact(Skip = "Compilation")]
        public void HavocValueType()
        {
            var testedType = typeof(HavocValueType);
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
                    },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void NonInstrumentedNestedOutside()
        {
            var testedType = typeof(NonInstrumentedNestedOutside);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void NonInstrumentedNestedInside()
        {
            var testedType = typeof(NonInstrumentedNestedInside);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void NonInstrumentedCallWithBinaryExpr()
        {
            Type testedType = typeof(NonInstrumentedCallWithBinaryExpr);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                    },
            }
            );
        }

        [Fact]
        public void NonInstrumentedCallWithStringParam()
        {
            Type testedType = typeof(NonInstrumentedCallWithStringParam);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(22),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                        sameFile.WithLine(20),
                        sameFile.WithLine(22),
                    },
            }
            );
        }

        [Fact]
        public void TypeOf()
        {
            var testedType = typeof(TypeOf);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(20),
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                        sameFile.WithLine(27),
                        sameFile.WithLine(25),
                        sameFile.WithLine(23),
                        sameFile.WithLine(13),
                    },
            }
            );
        }




        [Fact]
        public void GetterWithReturn()
        {
            var testedType = typeof(GetterWithReturn);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(25),
                        sameFile.WithLine(19),
                    },
            }
            );
        }
        [Fact]
        public void MemberAccessChain()
        {
            var testedType = typeof(MemberAccessChainCase);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(18),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void MemberAccessChainCombined()
        {
            var testedType = typeof(MemberAccessCombined);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(8),
                Sliced =
                    {
                        sameFile.WithLine(8),
                        sameFile.WithLine(33),
                        sameFile.WithLine(16),
                        sameFile.WithLine(7),
                    },
            }
            );
        }

        [Fact]
        public void InstrumentedPropertyDetect()
        {
            var testedType = typeof(InstrumentedPropertyDetect);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(23),
                        sameFile.WithLine(24),
                        // Non use of this
                        //sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void InlineNewInstrumented()
        {
            var testedType = typeof(InlineNewInstrumented);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(21),
                        sameFile.WithLine(28),
                        sameFile.WithLine(30),
                    },
            }
            );
        }

        [Fact]
        public void InlineNewNonInstrumented()
        {
            var testedType = typeof(InlineNewNonInstrumented);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(18),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact]
        public void FieldInitializationAtDeclaration()
        {
            var testedType = typeof(FieldInitializationAtDeclaration);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(41),
                        sameFile.WithLine(26),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void LinqQueryToListWithCallBack()
        {
            var testedType = typeof(LinqQueryToListWithCallBack);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(18),
                        sameFile.WithLine(22),
                        sameFile.WithLine(23),
                        sameFile.WithLine(25),
                    },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void NonInstrumentedProperty()
        {
            var testedType = typeof(NonInstrumentedProperty);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                    },
            }
            );
        }

        [Fact]
        public void ArrayInitialization()
        {
            var testedType = typeof(ArrayInitialization);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(19),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void NotSupportedForeach()
        {
            var testedType = typeof(NotSupportedForeach);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(25),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(18),
                        sameFile.WithLine(19),
                        sameFile.WithLine(20),
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                        sameFile.WithLine(25),
                        sameFile.WithLine(29),
                        sameFile.WithLine(31),
                    },
            }
            );
        }

        [Fact]
        public void NotSupportedLambda()
        {
            var testedType = typeof(NotSupportedLambda);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void ListWithForeach()
        {
            var testedType = typeof(ListWithForeach);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(15),
                }
            }
            );
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

        [Fact(Skip = "TODO")]
        public void ForeachWithAssignment()
        {
            var testedType = typeof(ForeachWithAssignment);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                //este criteria trae lineas que asignan la VARIABLE
                //aunque no se modifiquen los campos
                Criteria = sameFile.WithLine(21),
                Sliced =
                    {
                        sameFile.WithLine(21),
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                        sameFile.WithLine(13),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(27),
                    sameFile.WithLine(25),
                }
            }
            );
            TestSimpleSlice(testedType, new DynAbsResult
            {
                //este criteria trae lineas que asignan los campos
                //aunque no se modifique las variable
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(20)
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(16),
                    sameFile.WithLine(25),
                    sameFile.WithLine(27),
                }
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void LockPassthrough()
        {
            var testedType = typeof(LockPassthrough);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(23),
                        sameFile.WithLine(28),
                    },
            }
            );
        }

        [Fact]
        public void AliasingThisLikeParameter()
        {
            var testedType = typeof(AliasingThisLikeParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(9),
                Sliced =
                    {
                        sameFile.WithLine(9),
                        sameFile.WithLine(16),
                        sameFile.WithLine(18),
                        sameFile.WithLine(7),
                        sameFile.WithLine(14),
                        sameFile.WithLine(8),
                    },
            }
            );
        }

        [Fact]
        public void OrExpressionWithTrue()
        {
            var testedType = typeof(OrExpressionWithTrue);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void NestedClasses()
        {
            var testedType = typeof(NestedClasses);
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
        public void LiteralToObjectBinding()
        {
            var testedType = typeof(LiteralToObjectBinding);
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
        public void RefParamInsideInvocation()
        {
            var testedType = typeof(RefParamInsideInvocation);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(26),
                    },
            }
            );
        }

        [Fact]
        public void RefSimpleInvocation()
        {
            var testedType = typeof(RefSimpleInvocation);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(32),
                        sameFile.WithLine(30),
                        sameFile.WithLine(38),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void RefStructs()
        {
            var testedType = typeof(RefStructs);
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

        [Fact]
        public void StructWithoutInitializer()
        {
            var testedType = typeof(StructWithoutInitializer);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14)
                    },
            }
            );
        }

        [Fact]
        public void StructExtension()
        {
            var testedType = typeof(StructExtension);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(12),
                        sameFile.WithLine(27),
                    },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void StructsWithArrayAccess()
        {
            var testedType = typeof(StructsWithArrayAccess);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                    {
                        sameFile.WithLine(21),
                        sameFile.WithLine(20),
                        sameFile.WithLine(87),
                        sameFile.WithLine(85),
                        sameFile.WithLine(84),
                        sameFile.WithLine(54),
                        sameFile.WithLine(42),
                        sameFile.WithLine(41),
                        sameFile.WithLine(19),
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(13),
                        sameFile.WithLine(12),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void GoToStatement()
        {
            var testedType = typeof(GoToStatement);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(21),
                        sameFile.WithLine(22),
                    },
            }
            );
        }

        [Fact]
        public void InheritanceCallBase()
        {
            var testedType = typeof(InheritanceCallBase);
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
        public void InheritanceCallBaseWithFunction()
        {
            var testedType = typeof(InheritanceCallBaseWithFunction);
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
        public void InheritanceImplicitCallBase()
        {
            var testedType = typeof(InheritanceImplicitCallBase);
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
        public void InheritanceCallBaseMultipleCtors()
        {
            var testedType = typeof(InheritanceCallBaseMultipleCtors);
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
        public void ComplexBaseConstructor()
        {
            var testedType = typeof(ComplexBaseConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                    },
            }
            );
        }

        [Fact]
        public void ComplexBaseCalls()
        {
            var testedType = typeof(ComplexBaseCalls);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                    },
            }
            );
        }

        [Fact]
        public void InvocationBinaryArgWithRef()
        {
            var testedType = typeof(InvocationBinaryArgWithRef);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                        sameFile.WithLine(20),
                        sameFile.WithLine(22),
                        sameFile.WithLine(24),
                    },
            }
            );
        }

        [Fact]
        public void FormRepresentation()
        {
            var testedType = typeof(FormRepresentation);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                    {
                        sameFile.WithLine(21),
                    },
            }
            );
        }

        [Fact]
        public void PostfixAndPrefixExpression()
        {
            var testedType = typeof(PostfixAndPrefixExpression);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(20),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void StaticAccessStringCons()
        {
            var testedType = typeof(StaticAccessStringCons);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                        sameFile.WithLine(36),
                        sameFile.WithLine(41),
                    },
            }
            );
        }

        [Fact]
        public void PropertyInInterface()
        {
            var testedType = typeof(PropertyInInterface);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(22),
                        sameFile.WithLine(14),
                        // Non use of the receiver
                        //sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void StaticPropertyWithEnum()
        {
            var testedType = typeof(StaticPropertyWithEnum);
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
        public void StructsModifyInsideInvocation()
        {
            var testedType = typeof(StructsModifytInsideInvocation);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact]
        public void StructsExplicitConstructor()
        {
            var testedType = typeof(StructsExplicitConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(20),
                        sameFile.WithLine(22),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void StructsExplicitStaticConstructor()
        {
            var testedType = typeof(StructsExplicitStaticConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void StructsImplicitStaticConstructor()
        {
            var testedType = typeof(StructsImplicitStaticConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(23),
                        sameFile.WithLine(17),
                    },
            }
            );
        }

        [Fact]
        public void SimpleDecimalConstant()
        {
            var testedType = typeof(SimpleDecimalConstant);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(19)
                    },
            }
            );
        }

        [Fact]
        public void DictionariesInicialization()
        {
            var testedType = typeof(DictionariesInicialization);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(38),
                Sliced =
                    {
                        sameFile.WithLine(38)
                    },
            }
            );
        }

        [Fact]
        public void NonInstrumentedConst()
        {
            var testedType = typeof(NonInstrumentedConst);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14)
                    }
            });
        }

        [Fact(Skip = "TODO: revisar")]
        public void ContinueInLoopInsideIf()
        {
            var testedType = typeof(ContinueInLoopInsideIf);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(27),
                Sliced =
                    {
                        sameFile.WithLine(26),
                        sameFile.WithLine(27)
                    },
            }
            );
        }

        [Fact]
        public void BreakInsideForeach()
        {
            var testedType = typeof(BreakInsideForeach);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(28),
                Sliced =
                    {
                        sameFile.WithLine(28)
                    },
            }
            );
        }

        [Fact]
        public void CallDestructorFromGarbage()
        {
            var testedType = typeof(CallDestructorFromGarbage);
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

        [Fact(Skip = "Compilation")]
        public void CallbackFromNonInstrumentedDestructor()
        {
            var testedType = typeof(CallbackFromNonInstrumentedDestructor);
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

        [Fact]
        public void UsingUninitializedScalarFields()
        {
            var testedType = typeof(UsingUninitializedScalarFields);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(24),
                        sameFile.WithLine(12),
                    },
            }
            );
        }

        [Fact]
        public void ComplexForWithFunction()
        {
            var testedType = typeof(ComplexForWithFunction);
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
                        sameFile.WithLine(26),
                        sameFile.WithLine(21),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void SimpleDo()
        {
            var testedType = typeof(SimpleDo);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(18),
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13)
                    },
            }
            );
        }

        [Fact]
        public void ComplexDo()
        {
            var testedType = typeof(ComplexDo);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(27),
                Sliced =
                    {
                        sameFile.WithLine(27)
                    },
            }
            );
        }

        [Fact]
        public void ComplexDo_B()
        {
            var testedType = typeof(ComplexDo_B);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(46),
                Sliced =
                    {
                        sameFile.WithLine(46),
                        sameFile.WithLine(57),
                        sameFile.WithLine(54),
                        sameFile.WithLine(52),
                        sameFile.WithLine(28),
                        sameFile.WithLine(26),
                        sameFile.WithLine(24),
                        //sameFile.WithLine(44), // It's the line 21
                        sameFile.WithLine(21),
                        sameFile.WithLine(39),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(18),
                        sameFile.WithLine(19),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),

                    },
            }
            );
        }

        [Fact]
        public void SimpleBaseWithoutConstructors()
        {
            var testedType = typeof(SimpleBaseWithoutConstructors);
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

        [Fact]
        public void ComplexBaseConstructor2()
        {
            var testedType = typeof(ComplexBaseConstructor2);
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

        [Fact]
        public void MemberAccessArrayAsParameter()
        {
            var testedType = typeof(ArrayAsParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void OperatorOverride()
        {
            var testedType = typeof(OperatorOverride);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(21),
                        sameFile.WithLine(22)
                    },
            }
            );
        }

        [Fact]
        public void TypeInGenericClasses()
        {
            var testedType = typeof(GenericClasses);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                        sameFile.WithLine(27),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void ImplicitCastOperator()
        {
            var testedType = typeof(ImplicitCastOperator);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                        sameFile.WithLine(18),
                        sameFile.WithLine(22),
                        sameFile.WithLine(26),
                        sameFile.WithLine(30),
                        sameFile.WithLine(32),
                        sameFile.WithLine(34),
                    },
            }
            );
        }

        [Fact]
        public void ArrayMethodExtension()
        {
            var testedType = typeof(ArrayMethodExtension);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
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

        //[Fact]
        public void ForWithContinue()
        {
            var testedType = typeof(ForWithContinue);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(18),
                    },
            }
            );
        }

        [Fact]
        public void NewTypeT()
        {
            var testedType = typeof(NewTypeT);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(18),
                    },
            }
            );
        }

        [Fact]
        public void RegionsTrivia()
        {
            var testedType = typeof(RegionsTrivia);
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

        [Fact(Skip = "TODO")]
        public void PropertiesAssignment()
        {
            var testedType = typeof(PropertiesAssignment);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(28),
                        sameFile.WithLine(32),
                        sameFile.WithLine(38),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(36),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void PropertiesCompoundAssignment()
        {
            var testedType = typeof(PropertiesCompoundAssignment);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(28),
                        sameFile.WithLine(32),
                        sameFile.WithLine(38),
                        sameFile.WithLine(15),
                        sameFile.WithLine(23),
                        sameFile.WithLine(16),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(36),
                    },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void ForWithComplexDependencies()
        {
            var testedType = typeof(ForWithComplexDependencies);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(24),
                Sliced =
                    {
                        sameFile.WithLine(24),
                        sameFile.WithLine(19),
                        sameFile.WithLine(34),
                        sameFile.WithLine(29),
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(54),
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(21),
                }
            });
        }

        [Fact]
        public void StructWithPrivateFields()
        {
            var testedType = typeof(StructWithPrivateFields);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(46),
                        sameFile.WithLine(45),
                        sameFile.WithLine(44),
                        sameFile.WithLine(39),
                    },
            });
        }

        [Fact(Skip = "External Library")]
        public void StructPropertyKey()
        {
            var testedType = typeof(StructPropertyKey);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(13),
                        sameFile.WithLine(65),
                        sameFile.WithLine(66),
                        sameFile.WithLine(67),
                        sameFile.WithLine(28),
                        sameFile.WithLine(27),
                        sameFile.WithLine(25),
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(38),
                    sameFile.WithLine(14),
                }
            });
        }

        [Fact(Skip = "External Library")]
        public void StructPropertyAccessInsideForeach()
        {
            var testedType = typeof(StructPropertyAccessInsideForeach);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(22),
                Sliced =
                {
                    sameFile.WithLine(22),
                    sameFile.WithLine(20),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(12),
                    sameFile.WithLine(11),
                    sameFile.WithLine(10),
                },
                WithoutSummariesExtraLines =
                {
                    // Esto se tendría que mejorar con información de tipos no hacen falta anotaciones
                    sameFile.WithLine(19),
                    // Al copiarse en teoría no tiene que estar. Sin summaries ni tipos, esta línea no debería estar
                    sameFile.WithLine(17),
                }
            }
            );
        }

        [Fact]
        public void SubstractTimeSpan()
        {
            var testedType = typeof(SubstractTimeSpan);
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

        [Fact(Skip = "Compilation")]
        public void SpecialDelegateAccess()
        {
            var testedType = typeof(SpecialDelegateAccess);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                {
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(44),
                    sameFile.WithLine(42),
                    sameFile.WithLine(41),
                    sameFile.WithLine(40),
                    sameFile.WithLine(60),
                    sameFile.WithLine(36),
                    sameFile.WithLine(37),
                    sameFile.WithLine(38), //(por control)
                    sameFile.WithLine(31),
                    sameFile.WithLine(30),
                    sameFile.WithLine(29),
                    sameFile.WithLine(53),
                    sameFile.WithLine(55),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void SpecialParametricType()
        {
            var testedType = typeof(SpecialParametricType);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                // XXX: TODO: Está pinchando por compatibilidad de tipos no se puede asignar <T> a <A> (si al revés)
                // Eso sucede porque T es A en el where... hay que cambiar algo, es un bug que hay que ver como resolver
                Criteria = sameFile.WithLine(18),
                Sliced =
                {
                    sameFile.WithLine(18),
                    sameFile.WithLine(43),
                    sameFile.WithLine(42),
                    sameFile.WithLine(69),
                    sameFile.WithLine(25),
                    sameFile.WithLine(22),
                    sameFile.WithLine(17),
                    sameFile.WithLine(14),
                    sameFile.WithLine(49),
                    sameFile.WithLine(47),
                    sameFile.WithLine(16),
                    sameFile.WithLine(29),
                    sameFile.WithLine(27),
                    sameFile.WithLine(15),
                    sameFile.WithLine(24),
                },
            }
            );
        }

        [Fact]
        public void NestedCallBaseConstructor()
        {
            var testedType = typeof(NestedCallBaseConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(10),
                Sliced =
                {
                    sameFile.WithLine(10),
                    sameFile.WithLine(25),
                    sameFile.WithLine(21),
                    sameFile.WithLine(31),
                    sameFile.WithLine(42),
                    sameFile.WithLine(9),
                    // Dep. control
                    sameFile.WithLine(34),
                    sameFile.WithLine(47),
                    // Son los callers del this
                    sameFile.WithLine(39),
                    sameFile.WithLine(28),
                },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void BaseNestedCallsExample()
        {
            var testedType = typeof(BaseNestedCallsExample);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                {
                    sameFile.WithLine(11),
                    sameFile.WithLine(29),
                    sameFile.WithLine(27),
                    sameFile.WithLine(22),
                    sameFile.WithLine(18)
                },
            }
            );
        }

        [Fact]
        public void SimplePrintWithCallControlDependency()
        {
            var testedType = typeof(SimplePrintWithCallControlDependency);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                {
                    sameFile.WithLine(20),
                    // Ahora debería slicear la dependencia de control
                    sameFile.WithLine(13)
                },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void ComplexCaseA()
        {
            var testedType = typeof(ComplexCaseA);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                {
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(44),
                    sameFile.WithLine(43),
                    sameFile.WithLine(42),
                    sameFile.WithLine(41),
                    sameFile.WithLine(20),
                    sameFile.WithLine(18),
                    sameFile.WithLine(27),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void EmptyStruct()
        {
            var testedType = typeof(EmptyStruct);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    sameFile.WithLine(17),
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(52),
                    sameFile.WithLine(41),
                    sameFile.WithLine(27),
                    sameFile.WithLine(48),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact]
        public void StructInInitializer()
        {
            var testedType = typeof(StructInInitializer);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                {
                    sameFile.WithLine(11),
                    sameFile.WithLine(9),
                },
            }
            );
        }

        [Fact]
        public void ComplexThisConstructorInitializer()
        {
            var testedType = typeof(ComplexThisConstructorInitializer);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                {
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(13),
                    sameFile.WithLine(25),
                    sameFile.WithLine(23),
                    sameFile.WithLine(29),
                    sameFile.WithLine(28),
                    sameFile.WithLine(34),
                    sameFile.WithLine(48),
                    sameFile.WithLine(12),
                },
            }
            );
        }

        [Fact]
        public void StaticClassOnlyExtensions()
        {
            var testedType = typeof(StaticClassOnlyExtensions);
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
                    sameFile.WithLine(32),
                    sameFile.WithLine(31),
                    sameFile.WithLine(29),
                },
            }
            );
        }

        [Fact]
        public void NullableEnum()
        {
            var testedType = typeof(NullableEnum);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                {
                    sameFile.WithLine(11),
                    sameFile.WithLine(10),
                    sameFile.WithLine(18),
                    sameFile.WithLine(17),
                    sameFile.WithLine(15),
                    sameFile.WithLine(9),
                }
            });
        }

        [Fact]
        public void ControlDependenciesAndRecursiveStructures()
        {
            var testedType = typeof(ControlDependenciesAndRecursiveStructures);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                {
                    sameFile.WithLine(13),
                    sameFile.WithLine(12),
                    sameFile.WithLine(11),
                    sameFile.WithLine(41),
                    sameFile.WithLine(40),
                    sameFile.WithLine(39),
                    sameFile.WithLine(36),
                    sameFile.WithLine(33),
                    sameFile.WithLine(32),
                    sameFile.WithLine(31),
                    sameFile.WithLine(30),
                    sameFile.WithLine(25),
                    sameFile.WithLine(24),
                    sameFile.WithLine(22),
                },
            });
        }
    }
}