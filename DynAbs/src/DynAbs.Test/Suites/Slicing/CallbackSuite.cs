using Xunit;
using DynAbs.Test.Cases;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    [Collection("MainCollection")]
    public class CallbackSuite : BaseSlicingTest
    {
        const string Skip = "TODO";

        [Fact(Skip = Skip)]
        public void Callback2different()
        {
            var testedType = typeof(Callback2different);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(42),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(22),
                        sameFile.WithLine(23),
                        sameFile.WithLine(27),
                        sameFile.WithLine(30),
                        sameFile.WithLine(33),
                        sameFile.WithLine(39),
                        sameFile.WithLine(40),
                        sameFile.WithLine(41),
                        sameFile.WithLine(42),
                        sameFile.WithLine(47),
                        sameFile.WithLine(48),
                        sameFile.WithLine(65),
                        sameFile.WithLine(68),
                        sameFile.WithLine(72),
                        sameFile.WithLine(73),

                        // TODO: USO DEL THIS
                        //sameFile.WithLine(24),
                        sameFile.WithLine(36),
                        sameFile.WithLine(45),

                        // DEP DE CONTROL X CALLBACK
                        sameFile.WithLine(49),
                        sameFile.WithLine(50),
                        sameFile.WithLine(52),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void Callback0()
        {
            var testedType = typeof (Callback0);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        // En el callback externo se puede usar Val aunque no esté inicializada
                        sameFile.WithLine(25),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void Callback0FollowLdOutside()
        {
            var testedType = typeof (Callback0FollowLdOutside);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(18),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(18),
                        //sameFile.WithLine(25), TODO: Preguntar uso del this
                        sameFile.WithLine(27),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void Callback1()
        {
            var testedType = typeof (Callback1);
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
                        //sameFile.WithLine(23), //TODO: Uso del this
                        sameFile.WithLine(22), // Potencial uso del Val aunque no esté declarado explísicamente
                        sameFile.WithLine(25),
                        sameFile.WithLine(26),
                    },
            }
            );
        }
        [Fact(Skip = Skip)]
        public void Callback1R()
        {
            var testedType = typeof (Callback1R);
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
                        sameFile.WithLine(24),
                        sameFile.WithLine(23), // Potencial uso del val
                        //aca agregué la 26, no entiendo cómo resultado no va a depender
                        //del valor de retorno si se está asignando???
                        sameFile.WithLine(26),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void Callback1RInstrumentedOutside()
        {
            var testedType = typeof (Callback1RInstrumentedOutside);
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
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                        sameFile.WithLine(28), // Val
                        sameFile.WithLine(29),
                        //aca agregué la 31, no entiendo cómo resultado no va a depender
                        //del valor de retorno si se está asignando???
                        sameFile.WithLine(31),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void Callback1RNonInstrumentedOutside()
        {
            var testedType = typeof (Callback1RNonInstrumentedOutside);
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
                        sameFile.WithLine(23),
                        sameFile.WithLine(24),
                        //aca agregué la 26, no entiendo cómo resultado no va a depender
                        //del valor de retorno si se está asignando???
                        sameFile.WithLine(26),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void Callback0Twice()
        {
            var testedType = typeof (Callback0Twice);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(17),
                Sliced =
                    {
                        sameFile.WithLine(17),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void ForeachCallback()
        {
            var testedType = typeof(ForeachCallback);
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
                        sameFile.WithLine(28),
                        sameFile.WithLine(26)
                    },
                StaticModeExtraLines =
                {
                    sameFile.WithLine(24)
                }
            }
            );
        }

        [Fact(Skip = "TODO: revisar")]
        public void LazyInitialization()
        {
            var testedType = typeof(LazyInitialization);
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

        [Fact(Skip = Skip)]
        public void FrameworkInitProperties()
        {
            var testedType = typeof(FrameworkInitProperties);
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

        [Fact(Skip = Skip)]
        public void IndexedPropertyCallback()
        {
            var testedType = typeof(IndexedPropertyCallback);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(16),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(21),
                        sameFile.WithLine(23),
                        sameFile.WithLine(24),
                        sameFile.WithLine(25),
                        sameFile.WithLine(28),
                        sameFile.WithLine(30),
                    },
            }
            );
        }

        [Fact(Skip = Skip)]
        public void IndexedPropertySetCallback()
        {
            var testedType = typeof(IndexedPropertySetCallback);
            var sameFile = SameFileStmtBuilder(testedType);
            TestSimpleSlice(testedType, new DynAbsResult
            {
                Criteria = sameFile.WithLine(20),
                Sliced =
                    {
                        sameFile.WithLine(14),
                        sameFile.WithLine(15),
                        sameFile.WithLine(16),
                        sameFile.WithLine(17),
                        sameFile.WithLine(18),
                        sameFile.WithLine(19),
                        sameFile.WithLine(20),
                        sameFile.WithLine(25),
                        sameFile.WithLine(27),
                        sameFile.WithLine(34),
                        sameFile.WithLine(38),
                        sameFile.WithLine(42),
                    },
            }
            );
        }
    }
}
