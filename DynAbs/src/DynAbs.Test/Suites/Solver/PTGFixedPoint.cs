using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Cases.PTGFixedPoint;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    [Collection("MainCollection")]
    public class PTGFixedPoint : BaseSlicingTest
    {
        const string Skip = "TODO";

        [Fact(Skip = Skip)]
        public void PTGFP_Basic()
        {
            var testedType = typeof(PTGFP_Basic);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(23),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_Basic_ExternalList()
        {
            var testedType = typeof(PTGFP_Basic_ExternalList);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(22),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
                WithoutSummariesExtraLines =
                {
                    sameFile.WithLine(13),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_NestedFor()
        {
            var testedType = typeof(PTGFP_NestedFor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(25),
                Sliced =
                    {
                        sameFile.WithLine(25),
                        sameFile.WithLine(31),
                        sameFile.WithLine(22),
                        sameFile.WithLine(19),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(18),
                    },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(36),
                    sameFile.WithLine(21),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_MultiplePath()
        {
            var testedType = typeof(PTGFP_MultiplePath);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(31),
                Sliced =
                    {
                        sameFile.WithLine(31),
                        sameFile.WithLine(37),
                        sameFile.WithLine(27),
                        sameFile.WithLine(24),
                        sameFile.WithLine(21),
                        sameFile.WithLine(19),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(18),
                    },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(42),
                    sameFile.WithLine(23),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_Functions()
        {
            var testedType = typeof(PTGFP_Functions);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                    {
                        sameFile.WithLine(21),
                        sameFile.WithLine(12),
                        sameFile.WithLine(44),
                        sameFile.WithLine(43),
                        sameFile.WithLine(39),
                        sameFile.WithLine(36),
                        sameFile.WithLine(49),
                        sameFile.WithLine(33),
                        sameFile.WithLine(31),
                        sameFile.WithLine(28),
                        sameFile.WithLine(27),
                        sameFile.WithLine(30),
                        sameFile.WithLine(19),
                        sameFile.WithLine(18),
                        sameFile.WithLine(16),
                    },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(35),
                    sameFile.WithLine(54),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_NI_Basic()
        {
            var testedType = typeof(PTGFP_NI_Basic);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(18),
                        sameFile.WithLine(17),
                        sameFile.WithLine(16),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_NI_NestedFor()
        {
            var testedType = typeof(PTGFP_NI_NestedFor);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(25),
                Sliced =
                    {
                        sameFile.WithLine(25),
                        sameFile.WithLine(31),
                        sameFile.WithLine(22),
                        sameFile.WithLine(19),
                        sameFile.WithLine(16),
                        sameFile.WithLine(15),
                        sameFile.WithLine(18),
                        // Está porque B hereda de BasicClass (no instrumentada) y le crea un LAMBDA IDA/VUELTA instanciado en el constructor, o sea, en la línea 21... luego cuando hacés temp.a te movés por ambos
                        // Aunque esté el feature sigma menos sigue estando porque al entrar por LAMBDA al base ya son 2 nodos y no puede utilizar el feature
                        // Para que esto no esté hay que tener anotaciones para object creation! ahí si
                        sameFile.WithLine(21),
                        sameFile.WithLine(34),
                        // Hereda de una externa, está bien... no podés pisar un múltiple
                        sameFile.WithLine(36),
                    },
                WithoutSummariesExtraLines =
                {
                    
                },
                StaticModeExtraLines =
                {
                    // Dep. de control, hubo inicializaciones
                    sameFile.WithLine(14),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_Callback1()
        {
            var testedType = typeof(PTGFP_Callback1);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(18),
                        sameFile.WithLine(19),
                        sameFile.WithLine(25),
                        sameFile.WithLine(28),
                        sameFile.WithLine(29),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_Callback2()
        {
            var testedType = typeof(PTGFP_Callback2);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(19),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(18),
                        sameFile.WithLine(19),
                        sameFile.WithLine(25),
                        sameFile.WithLine(28),
                        sameFile.WithLine(29),
                        sameFile.WithLine(33),
                        sameFile.WithLine(37),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_Clear2ndLevel()
        {
            var testedType = typeof(PTGFP_Clear2ndLevel);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(21),
                Sliced =
                    {
                        sameFile.WithLine(21)
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_SimpleTest()
        {
            var testedType = typeof(PTGFP_SimpleTest);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(20)
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_MatrixMultiplication()
        {
            var testedType = typeof(PTGFP_MatrixMultiplication);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(48),
                Sliced =
                    {
                        sameFile.WithLine(48),
                        sameFile.WithLine(46),
                        sameFile.WithLine(45),
                        sameFile.WithLine(44),
                        sameFile.WithLine(43),
                        sameFile.WithLine(42),
                        sameFile.WithLine(39),
                        sameFile.WithLine(37),
                        //sameFile.WithLine(36),
                        sameFile.WithLine(34),
                        sameFile.WithLine(33),
                        sameFile.WithLine(32),
                        sameFile.WithLine(30),
                        sameFile.WithLine(28),
                        //sameFile.WithLine(27), No van porque inicializan el array no sus elementos.
                        sameFile.WithLine(25),
                        sameFile.WithLine(24),
                        sameFile.WithLine(23),
                        sameFile.WithLine(21),
                        sameFile.WithLine(19),
                        sameFile.WithLine(18),
                        sameFile.WithLine(15),
                        sameFile.WithLine(13),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_REC_Basic()
        {
            var testedType = typeof(PTGFP_REC_Basic);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        sameFile.WithLine(23),
                        sameFile.WithLine(20),
                        sameFile.WithLine(25),
                        sameFile.WithLine(24),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                    },
                StaticModeExtraLines =
                {
                    // Por compresión weak update (23, 31)
                    sameFile.WithLine(31),
                    // Por control de la inicialización de la 23
                    sameFile.WithLine(22),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_REC_TwoFunctions()
        {
            var testedType = typeof(PTGFP_REC_TwoFunctions);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResultWithSkipFile("SlicerList.cs")
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16),
                        //sameFile.WithLine(23), --> No tiene porque estar
                        sameFile.WithLine(20),
                        sameFile.WithLine(25),
                        sameFile.WithLine(24),
                        sameFile.WithLine(15),
                        sameFile.WithLine(14),
                        sameFile.WithLine(28),
                        sameFile.WithLine(31),
                        sameFile.WithLine(32),
                        sameFile.WithLine(33),
                        
                    },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(39),
                    // Dep. control (new, field i)
                    sameFile.WithLine(30),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_BasicExternalLoop()
        {
            var testedType = typeof(PTGFP_BasicExternalLoop);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(14),
                Sliced =
                    {
                        sameFile.WithLine(14)
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_ExternalLoopInInternalLoop()
        {
            var testedType = typeof(PTGFP_ExternalLoopInInternalLoop);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(16)
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_REC_WithoutEscapeLevel_A()
        {
            var testedType = typeof(PTGFP_REC_WithoutEscapeLevel_A);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(32),
                        sameFile.WithLine(31),
                        sameFile.WithLine(11),
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                        sameFile.WithLine(25),
                        sameFile.WithLine(15),
                    },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(34),
                    // Comportamiento conocido, se marcan las líneas de declaración de las variables como valor inicial (weak update)
                    sameFile.WithLine(44),
                    // Dep. control de la 44
                    sameFile.WithLine(27),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_REC_WithoutEscapeLevel_B()
        {
            var testedType = typeof(PTGFP_REC_WithoutEscapeLevel_B);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(22),
                Sliced =
                    {
                        sameFile.WithLine(22),
                        sameFile.WithLine(40),
                        sameFile.WithLine(37),
                        sameFile.WithLine(6),
                        sameFile.WithLine(28),
                        sameFile.WithLine(26),
                        sameFile.WithLine(31),
                        sameFile.WithLine(20),
                    },
                StaticModeExtraLines = 
                {
                    sameFile.WithLine(38),
                        // Comportamiento conocido, se marcan las líneas de declaración de las variables como valor inicial (weak update)
                    sameFile.WithLine(50),
                    // Dep. control de la 50
                    sameFile.WithLine(33),
                }
            }
            );
        }

        [Fact(Skip = Skip)]
        public void PTGFP_REC_WithoutEscapeLevel_C()
        {
            var testedType = typeof(PTGFP_REC_WithoutEscapeLevel_C);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult()
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                        sameFile.WithLine(36),
                        sameFile.WithLine(35),
                        sameFile.WithLine(21),

                        sameFile.WithLine(11),
                        sameFile.WithLine(29),
                        sameFile.WithLine(28),
                        sameFile.WithLine(15),
                    },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(38),
                    sameFile.WithLine(47),
                    // Dep. control de la 47
                    sameFile.WithLine(24),
                }
            }
            );
        }
    }
}
