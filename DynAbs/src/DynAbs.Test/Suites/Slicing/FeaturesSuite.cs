using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Util;
using DynAbs.Test.Cases.Roslyn;
using DynAbs.Test.Cases.Features;
using DynAbs.Test.Cases.List;

namespace DynAbs.Test
{
    [Collection("MainCollection")]
    public class FeaturesSuite : BaseSlicingTest
    {
        //Generics
        [Fact]
        public void WithoutGenerics()
        {
            var testedType = typeof(WithoutGenerics);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(16),
                    sameFile.WithLine(17),
                },
            }
            );
        }

        [Fact]
        public void WithGenerics()
        {
            var testedType = typeof(WithGenerics);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                },
            }
            );
        }

        [Fact]
        public void KeywordVarSimple()
        {
            var testedType = typeof(KeywordVar);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                {
                    sameFile.WithLine(15),
                    sameFile.WithLine(16),
                    sameFile.WithLine(21),
                },
            });
        }

        [Fact]
        public void NullableTypes()
        {
            var testedType = typeof(NullableTypes);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced = {
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                    sameFile.WithLine(17),
                },
            });
        }

        [Fact]
        public void ObsoleteAttribute()
        {
            var testedType = typeof(ObsoleteAttributeTest);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced = {
                    sameFile.WithLine(16),
                    sameFile.WithLine(20),
                },
            });
        }

        [Fact]
        public void DebuggerDisplayAttribute()
        {
            var testedType = typeof(DebuggerDisplayAttribute);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                {
                    sameFile.WithLine(16),
                    sameFile.WithLine(28),
                    sameFile.WithLine(26),
                    sameFile.WithLine(15),
                },
            }
            );
        }

        [Fact]
        public void DebuggerBrowsableAttribute()
        {
            var testedType = typeof(DebuggerBrowsableAttribute);
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
        public void FlagAttribute()
        {
            var testedType = typeof(FlagsAttributeTest);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(38),
                Sliced =
                {
                    sameFile.WithLine(36),
                    sameFile.WithLine(38),
                },
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void CondAttribute()
        {
            var testedType = typeof(CondAttribute);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                {
                    sameFile.WithLine(16),
                    sameFile.WithLine(18),
                },
            }
            );
        }

        [Fact]
        public void AutoProperties()
        {
            var testedType = typeof(AutoProperties);
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
        public void FormatString()
        {
            var testedType = typeof(FormatString);
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
        public void LiteralStrings()
        {
            var testedType = typeof(LiteralString);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(31),
                Sliced =
                {
                    sameFile.WithLine(31),
                    sameFile.WithLine(30),
                    sameFile.WithLine(29),
                    sameFile.WithLine(21),
                    sameFile.WithLine(28),
                    sameFile.WithLine(20),
                    sameFile.WithLine(27),
                    sameFile.WithLine(19),
                    sameFile.WithLine(26),
                    sameFile.WithLine(18),
                    sameFile.WithLine(25),
                    sameFile.WithLine(17),
                    sameFile.WithLine(24),
                    sameFile.WithLine(16),
                    sameFile.WithLine(23),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact]
        public void LambaExpressions()
        {
            var testedType = typeof(LambdaExpressionsWithoutDelegate);
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
        public void VariableNames()
        {
            var testedType = typeof(VariableNames);
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
        public void RoslynEmptyString()
        {
            var testedType = typeof(EmptyString);
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
        public void RoslynParenthesizedSimpleExpression()
        {
            var testedType = typeof(ParenthesizedSimpleExpression);
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
        public void RoslynPrimaryExpressionsBool()
        {
            var testedType = typeof(PrimaryExpressionsTrue);
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
        public void RoslynStringLiteral()
        {
            var testedType = typeof(StringLiteral);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(31),
                Sliced =
                    {
                    sameFile.WithLine(31),
                    sameFile.WithLine(30),
                    sameFile.WithLine(29),
                    sameFile.WithLine(21),
                    sameFile.WithLine(28),
                    sameFile.WithLine(20),
                    sameFile.WithLine(27),
                    sameFile.WithLine(19),
                    sameFile.WithLine(26),
                    sameFile.WithLine(18),
                    sameFile.WithLine(25),
                    sameFile.WithLine(17),
                    sameFile.WithLine(24),
                    sameFile.WithLine(16),
                    sameFile.WithLine(23),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void RoslynCharacterLiteralExpression()
        {
            var testedType = typeof(CharacterLiteralExpression);
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
        public void RoslynNumericLiteralExpressionn()
        {
            var testedType = typeof(NumericLiteralExpression);
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
        public void RoslynAssignmentOperators()
        {
            var testedType = typeof(AssignmentOperators);
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
        public void RoslynCall()
        {
            var testedType = typeof(Call);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(14),
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact]
        public void RoslynCast()
        {
            var testedType = typeof(Cast);
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
        public void RoslynCallWithNamedArgument()
        {
            var testedType = typeof(CallWithNamedArgument);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(23),
                Sliced =
                    {
                        sameFile.WithLine(13),
                        sameFile.WithLine(22),
                        sameFile.WithLine(15),
                        sameFile.WithLine(20),
                        sameFile.WithLine(23),
                    },
            }
            );
        }

        [Fact]
        public void RoslynNew()
        {
            var testedType = typeof(TestNew);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(24),
                    },
            }
            );
        }

        [Fact]
        public void RoslynNewWithArgument()
        {
            var testedType = typeof(NewWithArgument);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(22),
                        sameFile.WithLine(13),
                        sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact]
        public void RoslynNewWithNamedArgument()
        {
            var testedType = typeof(NewWithNamedArgument);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(22),
                        sameFile.WithLine(13),
                        sameFile.WithLine(20),
                    },
            }
            );
        }

        [Fact]
        public void RoslynNewWithEmptyInitializer()
        {
            var testedType = typeof(NewWithEmptyInitializer);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void RoslynNewWithNoArgumentsAndEmptyInitializer()
        {
            var testedType = typeof(NewWithNoArgumentsAndEmptyInitializer);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                        sameFile.WithLine(19),
                    },
            }
            );
        }

        [Fact]
        public void RoslynNewWithNoArgumentsAndInitializer()
        {
            var testedType = typeof(NewWithNoArgumentsAndInitializer);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void RoslynNewWithNoArgumentsAndInitializers()
        {
            var testedType = typeof(NewWithNoArgumentsAndInitializers);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact]
        public void AssignOrder()
        {
            var testedType = typeof(AssignOrder);
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

        [Fact]
        public void ParamsArray()
        {
            var testedType = typeof(ParamsArray);
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
        public void ParamsArrayWithArray()
        {
            var testedType = typeof(ParamsArrayWithArray);
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
        public void PlusEqualsOperator()
        {
            var testedType = typeof(PlusEqualsOperator);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                {
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(23)
                },
            }
            );
        }

        [Fact]
        public void PlusEqualsOperatorWithProperties()
        {
            var testedType = typeof(PlusEqualsOperatorWithProperties);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    sameFile.WithLine(17),
                    sameFile.WithLine(16),
                    sameFile.WithLine(24)
                },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void ComplexPlusEqualsOperator()
        {
            var testedType = typeof(ComplexPlusEqualsOperator);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    sameFile.WithLine(17),
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(23),
                    sameFile.WithLine(40),
                    sameFile.WithLine(45),
                    sameFile.WithLine(11),
                    sameFile.WithLine(29),
                },
            }
            );
        }

        [Fact]
        public void BasicQueries()
        {
            var testedType = typeof(BasicQueries);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                {
                    sameFile.WithLine(20),
                    sameFile.WithLine(19),
                    sameFile.WithLine(17),
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(11),
                    sameFile.WithLine(30),
                    sameFile.WithLine(31),
                    sameFile.WithLine(40),
                    sameFile.WithLine(38)
                },
            }
            );
        }

        [Fact]
        public void ComplexQueries()
        {
            var testedType = typeof(ComplexQueries);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(31),
                Sliced =
                {
                    sameFile.WithLine(31),
                    //sameFile.WithLine(29),
                    //sameFile.WithLine(28),
                    //sameFile.WithLine(27),
                    //sameFile.WithLine(26), ==> Salen del 25
                    sameFile.WithLine(25),
                    sameFile.WithLine(23),
                    sameFile.WithLine(22),
                    sameFile.WithLine(21),
                    sameFile.WithLine(20),
                    sameFile.WithLine(19),
                    sameFile.WithLine(18),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    //sameFile.WithLine(12), // No la lee posta
                    sameFile.WithLine(11),
                    sameFile.WithLine(40),
                    sameFile.WithLine(41),
                    sameFile.WithLine(48),
                    sameFile.WithLine(50),
                },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(36),
                    sameFile.WithLine(37),
                    sameFile.WithLine(47),
                }
            }
            );
        }

        [Fact(Skip = "Compilation")]
        public void ComplexQuerieWithFromInside()
        {
            var testedType = typeof(ComplexQuerieWithFromInside);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(30),
                Sliced =
                {
                    sameFile.WithLine(30),
                    sameFile.WithLine(18),
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(40),
                    sameFile.WithLine(41),
                    sameFile.WithLine(42),
                    sameFile.WithLine(43),
                    sameFile.WithLine(51),
                    sameFile.WithLine(49),
                    sameFile.WithLine(11),
                },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(29),
                    sameFile.WithLine(17),
                },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(34),
                    sameFile.WithLine(35),
                    sameFile.WithLine(36),
                    sameFile.WithLine(47),
                }
            }
            );
        }

        [Fact]
        public void StructsAssign()
        {
            var testedType = typeof(StructsAssign);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                {
                    sameFile.WithLine(19),
                    sameFile.WithLine(14)
                },
            }
            );
        }

        [Fact]
        public void NullParameter()
        {
            var testedType = typeof(NullParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                {
                    sameFile.WithLine(15),
                    sameFile.WithLine(21),
                    sameFile.WithLine(20),
                    sameFile.WithLine(18),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact]
        public void BasicSlicerList_1()
        {
            var testedType = typeof(BasicSlicerList);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                {
                    sameFile.WithLine(18),
                    // Dep. de control
                    sameFile.WithLine(17),
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                },
            }
            );
        }

        [Fact]
        public void BasicSlicerList_2()
        {
            var testedType = typeof(BasicSlicerList);
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
        public void GetElementSlicerList()
        {
            var testedType = typeof(GetElementSlicerList);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    // No diferencia el 1 del 2 porque atrás tiene un array
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                    sameFile.WithLine(16),
                    sameFile.WithLine(17)
                },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void ListSpecialInitialization()
        {
            var testedType = typeof(ListSpecialInitialization);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    // Como es una lista y es no instrumentada creo que al acceder a un elemento también se queda en el HUB devolviendo todas las líneas
                    sameFile.WithLine(13),
                    sameFile.WithLine(14),
                    sameFile.WithLine(15),
                    sameFile.WithLine(17),
                    sameFile.WithLine(22),
                    sameFile.WithLine(23),
                    sameFile.WithLine(32),
                },
            }
            );
        }

        [Fact]
        public void ActionWithLambda()
        {
            var testedType = typeof(ActionWithLambda);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    sameFile.WithLine(17),
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                },
            }
            );
        }

        [Fact]
        public void ActionWithMethod()
        {
            var testedType = typeof(ActionWithMethod);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
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
        public void SpecialEventHandler()
        {
            var testedType = typeof(SpecialEventHandler);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
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
        public void ExpressionBodyFunctionA()
        {
            var testedType = typeof(ExpressionBodyFunctionA);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    sameFile.WithLine(17),
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(11),
                },
            }
            );
        }

        [Fact]
        public void OutInline()
        {
            var testedType = typeof(OutInline);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(20),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact(Skip = "External Library")]
        public void NonInstrumentedNonScalarOutParameter()
        {
            var testedType = typeof(NonInstrumentedNonScalarOutParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                {
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    // Esta línea está porque la variable se define aunque no tenga inicializador
                    // Para hacerlo más preciso habría que modificar el Assign del Broker (creo)
                    sameFile.WithLine(14),
                },
            }
            );
        }

        [Fact]
        public void NameOf()
        {
            var testedType = typeof(NameOf);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                {
                    sameFile.WithLine(15),
                    sameFile.WithLine(16),
                },
            }
            );
        }

        [Fact]
        public void SpecialRegionA()
        {
            var testedType = typeof(SpecialRegionA);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                {
                    sameFile.WithLine(21)
                },
            }
            );
        }

        [Fact]
        public void SpecialRegionB()
        {
            var testedType = typeof(SpecialRegionB);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(27),
                },
            }
            );
        }

        [Fact]
        public void PropertyInitializationInline()
        {
            var testedType = typeof(PropertyInitializationInline);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(32),
                    sameFile.WithLine(13),
                    sameFile.WithLine(26),
                    sameFile.WithLine(22),
                },
            }
            );
        }

        [Fact]
        public void PropertyInitializationInline_B()
        {
            var testedType = typeof(PropertyInitializationInline_B);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(21),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact]
        public void ReservedKeywords()
        {
            var testedType = typeof(ReservedKeywords);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
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
        public void DefaultKeywordWithTrivia()
        {
            var testedType = typeof(DefaultKeywordWithTrivia);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                {
                    sameFile.WithLine(20),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact]
        public void InlineGetter()
        {
            var testedType = typeof(InlineGetter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(15),
                Sliced =
                {
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(21),
                    sameFile.WithLine(25),
                    // Use of the receiver (it was not used directly)
                    //sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact]
        public void SpecialRegionAndDefaultKeyword()
        {
            var testedType = typeof(SpecialRegionAndDefaultKeyword);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
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
        public void ReservedKeywordsOnParameters()
        {
            var testedType = typeof(ReservedKeywordsOnParameters);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
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
        public void StaticWithUsing()
        {
            var testedType = typeof(StaticWithUsing);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                {
                    sameFile.WithLine(17),
                    sameFile.WithLine(15),
                },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void NestedInitializations()
        {
            var testedType = typeof(NestedInitializations);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(23),
                Sliced =
                {
                    sameFile.WithLine(23),
                    sameFile.WithLine(20),
                    sameFile.WithLine(13),
                },
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void NestedInitializationsWithList()
        {
            var testedType = typeof(NestedInitializationsWithList);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                {
                    sameFile.WithLine(21),
                    sameFile.WithLine(27),
                    sameFile.WithLine(25),
                    sameFile.WithLine(14),
                },
            }
            );
        }

        [Fact]
        public void ConstructorWithArrowExpression()
        {
            var testedType = typeof(ConstructorWithArrowExpression);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(76),
                    sameFile.WithLine(74),
                    sameFile.WithLine(31),
                    sameFile.WithLine(26),
                    sameFile.WithLine(75),
                    sameFile.WithLine(42),
                    sameFile.WithLine(13),
                    sameFile.WithLine(12),
                    sameFile.WithLine(11),
                    // Header
                    //sameFile.WithLine(34),
                },
            }
            );
        }

        [Fact]
        public void FuncWithTypeParameter()
        {
            var testedType = typeof(FuncWithTypeParameter);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(12),
                    sameFile.WithLine(11),
                    sameFile.WithLine(20),
                    sameFile.WithLine(27),
                    sameFile.WithLine(25),
                },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void ConditionalFuncResult()
        {
            var testedType = typeof(ConditionalFuncResult);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(12),
                    sameFile.WithLine(11),
                    sameFile.WithLine(56),
                    sameFile.WithLine(54),
                    sameFile.WithLine(53),
                    sameFile.WithLine(52),
                    sameFile.WithLine(50),
                    sameFile.WithLine(47),
                    sameFile.WithLine(35),
                    sameFile.WithLine(34),
                    sameFile.WithLine(32),
                    sameFile.WithLine(49),
                    sameFile.WithLine(29),
                    sameFile.WithLine(27),
                    sameFile.WithLine(20),
                },
            }
            );
        }

        [Fact]
        public void ConditionalFuncResultWithNull()
        {
            var testedType = typeof(ConditionalFuncResultWithNull);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                {
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(56),
                    sameFile.WithLine(49),
                },
            }
            );
        }

        [Fact]
        public void ConditionalOperatorOverAsKeyword()
        {
            var testedType = typeof(ConditionalOperatorOverAsKeyword);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                {
                    sameFile.WithLine(11),
                    sameFile.WithLine(10),
                    sameFile.WithLine(9),
                    sameFile.WithLine(25),
                    sameFile.WithLine(22),
                    sameFile.WithLine(15),
                },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void LocalFunctions()
        {
            var testedType = typeof(LocalFunctions);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                {
                    sameFile.WithLine(11),
                    sameFile.WithLine(10),
                    sameFile.WithLine(36),
                    sameFile.WithLine(25),
                    sameFile.WithLine(21),
                    sameFile.WithLine(9),
                    sameFile.WithLine(23),
                    sameFile.WithLine(20),
                },
            }
            );
        }

        [Fact]
        public void LocalFunctions_RepeatedNames()
        {
            var testedType = typeof(LocalFunctions_RepeatedNames);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                {
                    sameFile.WithLine(11),
                    sameFile.WithLine(10),
                    sameFile.WithLine(9),
                    sameFile.WithLine(33),
                    sameFile.WithLine(29),
                    sameFile.WithLine(28),
                    sameFile.WithLine(22),
                    sameFile.WithLine(18),
                    sameFile.WithLine(17),
                },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void LocalFunctions_LocalScopeVariable()
        {
            var testedType = typeof(LocalFunctions_LocalScopeVariable);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(11),
                Sliced =
                {
                    sameFile.WithLine(11),
                    sameFile.WithLine(10),
                    sameFile.WithLine(9),
                    sameFile.WithLine(25),
                    sameFile.WithLine(21),
                    sameFile.WithLine(20),
                    sameFile.WithLine(19),
                },
            }
            );
        }

        [Fact]
        public void ComplexTypeParameters()
        {
            var testedType = typeof(ComplexTypeParameters);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(12),
                Sliced =
                {
                    sameFile.WithLine(12),
                },
            }
            );
        }

        [Fact]
        public void AsWithNullableExpression()
        {
            var testedType = typeof(AsWithNullableExpression);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                {
                    sameFile.WithLine(21),
                    sameFile.WithLine(18),
                    sameFile.WithLine(16),
                    sameFile.WithLine(15),
                    sameFile.WithLine(9),
                    sameFile.WithLine(11),
                    sameFile.WithLine(13),
                    sameFile.WithLine(27),
                    sameFile.WithLine(32),
                    sameFile.WithLine(43),
                    sameFile.WithLine(49),
                },
            }
            );
        }

        [Fact(Skip = "TODO")]
        public void RecursivePattern()
        {
            var testedType = typeof(RecursivePattern);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                {
                    sameFile.WithLine(20),
                    sameFile.WithLine(19),
                    sameFile.WithLine(18),
                    sameFile.WithLine(17),
                    sameFile.WithLine(15),
                    sameFile.WithLine(14),
                    sameFile.WithLine(13),
                    sameFile.WithLine(12),
                    sameFile.WithLine(25),
                    sameFile.WithLine(27),
                    sameFile.WithLine(37),
                    sameFile.WithLine(34),
                    sameFile.WithLine(30),
                    sameFile.WithLine(32),
                    sameFile.WithLine(33),
                    sameFile.WithLine(48),
                    sameFile.WithLine(53),
                    sameFile.WithLine(54),
                    sameFile.WithLine(59),
                },
            }
            );
        }
    }
}