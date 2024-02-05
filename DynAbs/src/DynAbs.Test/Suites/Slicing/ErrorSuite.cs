using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Util;
using DynAbs.Test.Suites.Slicing;
using DynAbs.Test.Cases.Roslyn;
using DynAbs.Test.Cases.Features;
using DynAbs.Test.Cases.Common;

namespace DynAbs.Test
{
    [Collection("MainCollection")]
    public class ErrorSuite : BaseSlicingTest
    {
        //Motivo: Los resultados del slicer no son correctos.
        [Fact]
        public void GenericTypeConstraint()
        {
            var testedType = typeof(GenericTypeConstraint);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                {
                    sameFile.WithLine(13),
                    //sameFile.WithLine(15),
                    //sameFile.WithLine(23),
                    sameFile.WithLine(16),
                },
            }
            );
        }


        //No se esta teniendo en cuenta sobreescribir el ToString
        [Fact(Skip = "TODO")]
        public void OverrideToStringTest()
        {
            var testedType = typeof(OverrideToStringTest);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                    sameFile.WithLine(24),
                    sameFile.WithLine(25),
                },
            }
            );
        }

        [Fact]
        public void OverrideToStringInParameter()
        {
            var testedType = typeof(OverrideToStringInParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(29),
                    sameFile.WithLine(23),
                    sameFile.WithLine(21),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        //Motivo: Esta buscando this, cuando no deberia porque esta dentro de metodo estatico.
        [Fact]
        public void LambdaExpression()
        {
            var testedType = typeof(LambdaExpressions);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                {
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(16),
                },
            }
            );
        }

        //Motivo: No estamos instrumentando new[]
        [Fact]
        public void KeywordNewArray()
        {
            var testedType = typeof(KeywordVar);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(25),
                Sliced =
                {
                    sameFile.WithLine(18),
                    sameFile.WithLine(19),
                    sameFile.WithLine(23),
                    sameFile.WithLine(25),
                },
            });
        }

        //Motivo: No estamos instrumentado la keyword yield
        [Fact]
        public void KeywordYield()
        {
            var testedType = typeof(KeywordYield);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                //los assert estan puesto de ejemplo.
                //el problema esta cuando se consume la traza.
                //entones el slicer no soporta yield
                Criteria = sameFile.WithLine(19),
                Sliced =
                {
                    sameFile.WithLine(19),
                    sameFile.WithLine(17),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(29),
                    sameFile.WithLine(28),
                    sameFile.WithLine(27),
                    sameFile.WithLine(26),
                    sameFile.WithLine(25),
                    sameFile.WithLine(24),
                },
            }
            );
        }

        [Fact]
        public void YieldWithoutElements()
        {
            var testedType = typeof(YieldWithoutElements);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                {
                    sameFile.WithLine(16),
                    sameFile.WithLine(13),
                    sameFile.WithLine(22),
                    sameFile.WithLine(25)
                },
            }
            );
        }

        [Fact]
        public void KeywordYieldBreak()
        {
            var testedType = typeof(KeywordYieldBreak);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                {
                    sameFile.WithLine(19),
                    sameFile.WithLine(17),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(28),
                    sameFile.WithLine(27),
                    sameFile.WithLine(26),
                    sameFile.WithLine(25),
                    sameFile.WithLine(24),
                },
            }
            );
        }

        [Fact]
        public void YieldReturnInAccessor()
        {
            var testedType = typeof(YieldReturnInAccessor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                {
                    sameFile.WithLine(13),
                    sameFile.WithLine(12),
                    sameFile.WithLine(11),
                    sameFile.WithLine(23),
                    sameFile.WithLine(21)
                },
            }
            );
        }

        //Motivo: Los tipos nulos tiene metodos, esos no los estamos teniendo en cuenta.
        [Fact]
        public void NullableTypesWithMethods()
        {
            var testedType = typeof(NullableTypesWithMethods);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced = {
                    sameFile.WithLine(13),
                    sameFile.WithLine(14),
                    sameFile.WithLine(16),
                },
            });
        }


        [Fact]
        public void GetConstructor()
        {
            var testedType = typeof(GetConstructor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(26),
                        sameFile.WithLine(36),
                        sameFile.WithLine(34),
                        sameFile.WithLine(32),
                        sameFile.WithLine(29),
                        // TODO
                        sameFile.WithLine(21),
                        //sameFile.WithLine(33),
                    },
                WithoutSummariesExtraLines =
                {
                    // La siguiente línea no está por el summary que le da el special behavior de new object al invoke del getconstructor
                    sameFile.WithLine(21)
                }
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void NewArrayInArgument()
        {
            var testedType = typeof(NewArrayInArgumenter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                        sameFile.WithLine(28),
                        //sameFile.WithLine(24), // ==> El havoc benévolo evita que entre en el slice
                        sameFile.WithLine(23),
                        //sameFile.WithLine(21), // ==> No se lee el parámetro sino sus elementos :)
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        // No utiliza "a" directamente
                        //sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void NotSupportedOutInCondition()
        {
            var testedType = typeof(NotSupportedOutInCondition);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(20),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void NotSupportedUsing()
        {
            var testedType = typeof(NotSupportedUsing);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(19),
                        sameFile.WithLine(30),
                        // Dep de control:
                        sameFile.WithLine(15),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void NotSupportedUsingVarDeclaration()
        {
            var testedType = typeof(NotSupportedUsingVarDeclaration);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(19),
                        sameFile.WithLine(32),
                        sameFile.WithLine(17),
                        sameFile.WithLine(25),
                        sameFile.WithLine(23),
                        sameFile.WithLine(15),
                    },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void UsingWithIDisposableStruct()
        {
            var testedType = typeof(UsingWithIDisposableStruct);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(30),
                        sameFile.WithLine(37),
                        sameFile.WithLine(36),
                        sameFile.WithLine(34),
                        sameFile.WithLine(29),
                        sameFile.WithLine(22),
                        sameFile.WithLine(20),
                        sameFile.WithLine(27)
                    },
            }
            );
        }

        //Motivo: Esta devolviendo mal el slice, la linea 14 no deberia devolverla. Esto es porque no se da cuenta
        // que ?? es un if
        [Fact]
        public void NullCoalescingOperator()
        {
            var testedType = typeof(NullCoalescingOperator);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    sameFile.WithLine(13),
                    sameFile.WithLine(16),
                    sameFile.WithLine(17),
                },
            }
            );
        }

        //Motivo: Devuelve mal el slice, esto es porque no entiende el condicional (?:)
        [Fact]
        public void ConditionalOperator()
        {
            var testedType = typeof(ConditionalOperator);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(17),
                    },
            }
            );
        }

        [Fact]
        public void ConditionalAccessA()
        {
            var testedType = typeof(ConditionalAccessA);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(10)
                    },
            }
            );
        }

        [Fact]
        public void ConditionalAccessB()
        {
            var testedType = typeof(ConditionalAccessB);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(11),
                        sameFile.WithLine(10),
                        sameFile.WithLine(9)
                    },
            }
            );
        }

        [Fact]
        public void ConditionalAccessC()
        {
            var testedType = typeof(ConditionalAccessC);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(11),
                        sameFile.WithLine(10),
                        sameFile.WithLine(19)
                        // Non use of the receiver
                        //sameFile.WithLine(9),
                    },
            }
            );
        }

        [Fact]
        public void ConditionalAccessD()
        {
            var testedType = typeof(ConditionalAccessD);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(11),
                        sameFile.WithLine(10),
                        sameFile.WithLine(9)
                    },
            }
            );
        }

        [Fact]
        public void ConditionalAccessE()
        {
            var testedType = typeof(ConditionalAccessE);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(10)
                    },
            }
            );
        }

        [Fact]
        public void ConditionalAccessF()
        {
            var testedType = typeof(ConditionalAccessF);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                    {
                        sameFile.WithLine(11),
                        sameFile.WithLine(10),
                        sameFile.WithLine(9)
                    },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void ConditionalAccessG()
        {
            var testedType = typeof(ConditionalAccessG);
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

        [Fact(Skip = "Compilation")]
        public void ConditionalAccessH()
        {
            var testedType = typeof(ConditionalAccessH);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(10),
                Sliced =
                    {
                        sameFile.WithLine(10),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void ConditionalAccessI()
        {
            var testedType = typeof(ConditionalAccessI);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(18),
                    },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void InlineOperator()
        {
            var testedType = typeof(InlineOperator);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(27),
                        sameFile.WithLine(23),
                        sameFile.WithLine(21),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void SimpleImplicitOperator()
        {
            var testedType = typeof(SimpleImplicitOperator);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(18),
                    },
            }
            );
        }

        [Fact]
        public void LocalEventHandler()
        {
            var testedType = typeof(LocalEventHandler);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void YieldWithImplicitTuples()
        {
            var testedType = typeof(YieldWithImplicitTuples);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(27),
                        sameFile.WithLine(25),
                        sameFile.WithLine(23),
                        sameFile.WithLine(18)
                    },
            }
            );
        }

        [Fact]
        public void YieldWithByteArray()
        {
            var testedType = typeof(YieldWithByteArray);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(23),
                        sameFile.WithLine(21),
                        sameFile.WithLine(20)
                    },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void InternalMethods()
        {
            var testedType = typeof(InternalMethods);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(37),
                        sameFile.WithLine(13),
                        sameFile.WithLine(14)
                    },
            }
            );
        }

        [Fact]
        public void OutInlineInAccessors()
        {
            var testedType = typeof(OutInlineInAccessors);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(25),
                        sameFile.WithLine(27),
                        sameFile.WithLine(31),
                        sameFile.WithLine(11),
                        sameFile.WithLine(12),
                        sameFile.WithLine(13)
                        // Not used receiver dependency
                        //sameFile.WithLine(10),
                    },
            }
            );
        }

        [Fact]
        public void FuncsAsParameters()
        {
            var testedType = typeof(FuncsAsParameters);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                    {
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(10),
                        sameFile.WithLine(9),
                        sameFile.WithLine(24),
                        sameFile.WithLine(23),
                        sameFile.WithLine(17),
                        sameFile.WithLine(22),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact]
        public void InlineMethodsWithNativeMethods()
        {
            var testedType = typeof(InlineMethodsWithNativeMethods);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(20),
                        sameFile.WithLine(29),
                        sameFile.WithLine(19)
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void InlineAccessor()
        {
            var testedType = typeof(InlineAccessor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(29),
                        sameFile.WithLine(26)
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void InlineAccessorSet()
        {
            var testedType = typeof(InlineAccessorSet);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(27),
                        sameFile.WithLine(26)
                    },
            }
            );
        }

        [Fact]
        public void InlineVoidMethod()
        {
            var testedType = typeof(InlineVoidMethod);
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
        public void DeconstructionAssignment()
        {
            var testedType = typeof(DeconstructionAssignment);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(24),
                        sameFile.WithLine(22),
                        sameFile.WithLine(20),
                        sameFile.WithLine(13)
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void ForeachWithDeconstructionAssignment()
        {
            var testedType = typeof(ForeachWithDeconstructionAssignment);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(24),
                        sameFile.WithLine(29),
                    },
            }
            );
        }

        [Fact]
        public void ForeachWithDeconstructionAssignmentB()
        {
            var testedType = typeof(ForeachWithDeconstructionAssignmentB);
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
        public void ImplicitConvert()
        {
            var testedType = typeof(ImplicitConvert);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(29),
                        sameFile.WithLine(26),
                        sameFile.WithLine(13),
                        sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void ExternalImplicitConvert()
        {
            var testedType = typeof(ExternalImplicitConvert);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(20),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        //Motivo: No se esta tratando el AnonymousObjectMemberDeclaratorSyntax
        [Fact]
        public void AnonymousTypes()
        {
            var testedType = typeof(AnonymousTypes);
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

        //Motivo: No se da cuenta que el c1 dentro del metodo es el “casa” pasado por parametro.
        [Fact(Skip = "TODO")]
        public void Overloading()
        {
            var testedType = typeof(Overloading);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(25),
                        // Dep. de control
                        sameFile.WithLine(15),
                    },
            }
            );
        }

        [Fact]
        public void RoslynParenthesizedNullExpression()
        {
            var testedType = typeof(ParenthesizedNulleExpression);
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
        public void RoslynTestName()
        {
            var testedType = typeof(TestName);
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
        public void RoslynTernaryConditional()
        {
            var testedType = typeof(TernaryConditional);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(16)
                    },
            }
            );
        }

        [Fact]
        public void RoslynTernaryConditionalWithNull()
        {
            var testedType = typeof(TernaryConditionalWithNull);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(15)
                    },
            }
            );
        }

        [Fact]
        public void RoslynCallWithObjectRef()
        {
            var testedType = typeof(CallWithObjectRef);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(21),
                        sameFile.WithLine(26),
                        sameFile.WithLine(28),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void RoslynCallWithRef()
        {
            var testedType = typeof(CallWithRef);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(22),
                        sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact]
        public void MemberAccessOfRefExpr()
        {
            var testedType = typeof(MemberAccessOfRefExpr);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(28),
                        sameFile.WithLine(30),
                        sameFile.WithLine(32),
                        sameFile.WithLine(21),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void MemberAccessOfRefExprAsParameter()
        {
            var testedType = typeof(MemberAccessOfRefExprAsParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(21),
                        sameFile.WithLine(26),
                        sameFile.WithLine(28),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void RoslynCallWithOut()
        {
            var testedType = typeof(CallWithOut);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(21),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void RoslynCallWithOutNotInitialized()
        {
            var testedType = typeof(CallWithOutNotInitialized);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(21),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void RoslynCallWithOutAsParameter()
        {
            var testedType = typeof(CallWithOutAsParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(26),
                        sameFile.WithLine(21),
                        sameFile.WithLine(14),
                    },
            }
            );
        }

        [Fact]
        public void RoslynIndex()
        {
            var testedType = typeof(Index);
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
                    },
            }
            );
        }

        [Fact]
        public void ParamsBinaryExpression()
        {
            var testedType = typeof(ParamsBinaryExpression);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        //sameFile.WithLine(19),
                        //sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact]
        public void NewWithNoArgumentsAndInitializerCollection()
        {
            var testedType = typeof(NewWithNoArgumentsAndInitializersCollection);
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

        [Fact(Skip = "TODO")]
        public void EmptyMain()
        {
            var testedType = typeof(EmptyMain);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(22),
                Sliced = { },
            }
            );
        }

        [Fact]
        public void StringInObjectParameter()
        {
            var testedType = typeof(StringInObjectParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced = {
                    sameFile.WithLine(14),
                },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void SetWithNonImplementedBase()
        {
            var testedType = typeof(SetWithNonImplementedBase);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced = {
                    sameFile.WithLine(17),
                    sameFile.WithLine(16),
                    sameFile.WithLine(14),
                    sameFile.WithLine(30),
                    sameFile.WithLine(26),
                    sameFile.WithLine(15),
                    sameFile.WithLine(20),
                    // TODO: XXX: BUG: AGREGO ESTA LÍNEA PARA QUE PINCHE
                    // El motivo: la asignación al base es lo no instrumentado, pero se toma como "this". Esto no está bien tratado hoy en día.
                    sameFile.WithLine(-1)
                },
            }
            );
        }

        [Fact]
        public void FieldsInitializationWithMoreConstructors()
        {
            var testedType = typeof(FieldsInitializationWithMoreConstructors);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced = {
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(29),
                    sameFile.WithLine(27),
                    sameFile.WithLine(34),
                    sameFile.WithLine(21),
                },
            }
            );
        }

        [Fact]
        public void ComplexFieldsInitializationWithMoreConstructors()
        {
            var testedType = typeof(ComplexFieldsInitializacionWithMoreConstructors);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced = {
                    sameFile.WithLine(16),
                    sameFile.WithLine(34),
                    sameFile.WithLine(29),
                    sameFile.WithLine(21),
                    sameFile.WithLine(27),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(44),
                    sameFile.WithLine(41),
                    sameFile.WithLine(38),
                },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void DynamicVarWithElementAccessor()
        {
            var testedType = typeof(DynamicVarWithElementAccessor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced = {
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(21),
                    sameFile.WithLine(20)
                },
            }
            );
        }

        [Fact(Skip = "External Library")]
        public void NonInstrumentedRefParameter()
        {
            var testedType = typeof(NonInstrumentedRefParameter);
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

        [Fact(Skip = "Compilation")]
        public void DelegatesAndPointers()
        {
            var testedType = typeof(DelegatesAndPointers);
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
        public void ComplexLazyList()
        {
            var testedType = typeof(ComplexLazyList);
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

        [Fact(Skip = "Compilation")]
        public void ExternalCalls()
        {
            var testedType = typeof(ExternalCalls);
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
        public void NullForgivingOperator()
        {
            var testedType = typeof(NullForgivingOperator);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                    },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void SwitchCaseWithPropertiesCallback()
        {
            var testedType = typeof(SwitchCaseWithPropertiesCallback);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(13),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(12),
                        sameFile.WithLine(11),
                        sameFile.WithLine(26),
                        sameFile.WithLine(19),
                        sameFile.WithLine(5),
                        // Non used receiver
                        //sameFile.WithLine(10),
                        //sameFile.WithLine(9),
                        //sameFile.WithLine(17),
                    },
            }
            );
        }
    }
}
