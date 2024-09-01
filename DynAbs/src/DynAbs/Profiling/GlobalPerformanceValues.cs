using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public static class GlobalPerformanceValues
    {
        public static class AliasingSolverValues
        {
            public static class Times
            {
                public static TimeSpan LastDefGet = new TimeSpan(0);
                public static TimeSpan LastDefSet = new TimeSpan(0);
                public static TimeSpan Alloc = new TimeSpan(0);
                public static TimeSpan Assign = new TimeSpan(0);
                public static TimeSpan AssignRV = new TimeSpan(0);
                public static TimeSpan EnterMethodAndBind = new TimeSpan(0);
                public static TimeSpan ExitMethodAndUnbind = new TimeSpan(0);
                public static TimeSpan Reachable = new TimeSpan(0);
                public static TimeSpan Havoc = new TimeSpan(0);
                public static TimeSpan AddToRegion = new TimeSpan(0);
                public static TimeSpan RedefineType = new TimeSpan(0);
                public static TimeSpan CleanTemporaryEntries = new TimeSpan(0);
                public static TimeSpan EnterStaticMode = new TimeSpan(0);
                public static TimeSpan ExitStaticMode = new TimeSpan(0);

                public static TimeSpan LastDefGet_Max = new TimeSpan(0);
                public static TimeSpan LastDefSet_Max = new TimeSpan(0);
                public static TimeSpan Reachable_Max = new TimeSpan(0);
                public static TimeSpan Havoc_Max = new TimeSpan(0);

                public static TimeSpan Total
                {
                    get
                    {
                        return GlobalPerformanceValues.AliasingSolverValues.Times.LastDefGet
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.LastDefSet)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.Alloc)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.Assign)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.AssignRV)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.EnterMethodAndBind)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.ExitMethodAndUnbind)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.Reachable)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.Havoc)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.AddToRegion)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.RedefineType)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.CleanTemporaryEntries)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.EnterStaticMode)
                    .Add(GlobalPerformanceValues.AliasingSolverValues.Times.ExitStaticMode);
                    }
                }

                public static void Clean()
                {
                    LastDefGet = new TimeSpan(0);
                    LastDefSet = new TimeSpan(0);
                    Alloc = new TimeSpan(0);
                    Assign = new TimeSpan(0);
                    AssignRV = new TimeSpan(0);
                    EnterMethodAndBind = new TimeSpan(0);
                    ExitMethodAndUnbind = new TimeSpan(0);
                    Reachable = new TimeSpan(0);
                    Havoc = new TimeSpan(0);
                    AddToRegion = new TimeSpan(0);
                    RedefineType = new TimeSpan(0);
                    CleanTemporaryEntries = new TimeSpan(0);
                    EnterStaticMode = new TimeSpan(0);
                    ExitStaticMode = new TimeSpan(0);

                    LastDefGet_Max = new TimeSpan(0);
                    LastDefSet_Max = new TimeSpan(0);
                    Reachable_Max = new TimeSpan(0);
                    Havoc_Max = new TimeSpan(0);
                }
            }

            public static class Counters
            {
                public static int LastDefGet = 0;
                public static int LastDefSet = 0;
                public static int Alloc = 0;
                public static int Assign = 0;
                public static int AssignRV = 0;
                public static int EnterMethodAndBind = 0;
                public static int ExitMethodAndUnbind = 0;
                public static int Reachable = 0;
                public static int Havoc = 0;
                public static int AddToRegion = 0;
                public static int RedefineType = 0;
                public static int CleanTemporaryEntries = 0;
                public static int EnterStaticMode = 0;
                public static int ExitStaticMode = 0;

                public static int Total
                {
                    get
                    {
                        return GlobalPerformanceValues.AliasingSolverValues.Counters.LastDefGet +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.LastDefSet +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Alloc +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Assign +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.AssignRV +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.EnterMethodAndBind +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.ExitMethodAndUnbind +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Reachable +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Havoc +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.AddToRegion +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.RedefineType +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.CleanTemporaryEntries +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.EnterStaticMode +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.ExitStaticMode;
                    }
                }

                public static void Clean()
                {
                    LastDefGet = 0;
                    LastDefSet = 0;
                    Alloc = 0;
                    Assign = 0;
                    AssignRV = 0;
                    EnterMethodAndBind = 0;
                    ExitMethodAndUnbind = 0;
                    Reachable = 0;
                    Havoc = 0;
                    AddToRegion = 0;
                    RedefineType = 0;
                    CleanTemporaryEntries = 0;
                    EnterStaticMode = 0;
                    ExitStaticMode = 0;
                }
            }

            public static void Print()
            {
                Console.WriteLine("AliasingSolver times:");
                Console.WriteLine("lastDefGet: " + GlobalPerformanceValues.AliasingSolverValues.Times.LastDefGet.TotalSeconds);
                Console.WriteLine("lastDefSet: " + GlobalPerformanceValues.AliasingSolverValues.Times.LastDefSet.TotalSeconds);
                Console.WriteLine("Alloc: " + GlobalPerformanceValues.AliasingSolverValues.Times.Alloc.TotalSeconds);
                Console.WriteLine("Assign: " + GlobalPerformanceValues.AliasingSolverValues.Times.Assign.TotalSeconds);
                Console.WriteLine("AssignRV: " + GlobalPerformanceValues.AliasingSolverValues.Times.AssignRV.TotalSeconds);
                Console.WriteLine("EnterMethodAndBind: " + GlobalPerformanceValues.AliasingSolverValues.Times.EnterMethodAndBind.TotalSeconds);
                Console.WriteLine("ExitMethodAndUnbind: " + GlobalPerformanceValues.AliasingSolverValues.Times.ExitMethodAndUnbind.TotalSeconds);
                Console.WriteLine("Havoc: " + GlobalPerformanceValues.AliasingSolverValues.Times.Havoc.TotalSeconds);
                Console.WriteLine("Total: " + GlobalPerformanceValues.AliasingSolverValues.Times.Total.TotalSeconds);

                Console.WriteLine("------ Max values ------");
                Console.WriteLine("lastDefGetMax: " + GlobalPerformanceValues.AliasingSolverValues.Times.LastDefGet_Max.TotalSeconds);
                Console.WriteLine("lastDefSetMax: " + GlobalPerformanceValues.AliasingSolverValues.Times.LastDefSet_Max.TotalSeconds);
                Console.WriteLine("HavocMax: " + GlobalPerformanceValues.AliasingSolverValues.Times.Havoc_Max.TotalSeconds);

                Console.WriteLine("------ Operations counters ------");
                Console.WriteLine("lastDefGet: " + GlobalPerformanceValues.AliasingSolverValues.Counters.LastDefGet);
                Console.WriteLine("lastDefSet: " + GlobalPerformanceValues.AliasingSolverValues.Counters.LastDefSet);
                Console.WriteLine("Alloc: " + GlobalPerformanceValues.AliasingSolverValues.Counters.Alloc);
                Console.WriteLine("Assign: " + GlobalPerformanceValues.AliasingSolverValues.Counters.Assign);
                Console.WriteLine("AssignRV: " + GlobalPerformanceValues.AliasingSolverValues.Counters.AssignRV);
                Console.WriteLine("EnterMethodAndBind: " + GlobalPerformanceValues.AliasingSolverValues.Counters.EnterMethodAndBind);
                Console.WriteLine("ExitMethodAndUnbind: " + GlobalPerformanceValues.AliasingSolverValues.Counters.ExitMethodAndUnbind);
                Console.WriteLine("Havoc: " + GlobalPerformanceValues.AliasingSolverValues.Counters.Havoc);

                Console.WriteLine("------- Cantidad de operaciones TOTAL de AliasingSolver: " +
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Total + " ----------");

                // Havoc history
                //System.IO.File.WriteAllLines(@"C:\temp\compilation\HavocEvolution.txt",
                //    HavocEvolution.Select(x => x.TotalMilliseconds.ToString()));
            }

            public static void Clean()
            {
                Times.Clean();
                Counters.Clean();
            }
        }

        public static class DependencyGraphValues
        {
            public static class Times
            {
                public static TimeSpan AddVertex = new TimeSpan(0);

                public static TimeSpan Total
                {
                    get
                    {
                        return GlobalPerformanceValues.DependencyGraphValues.Times.AddVertex;
                    }
                }

                public static void Clean()
                {
                    AddVertex = new TimeSpan(0);
                }
            }

            public static class Counters
            {
                public static int AddVertex = 0;

                public static int Total
                {
                    get
                    {
                        return GlobalPerformanceValues.DependencyGraphValues.Counters.AddVertex;
                    }
                }

                public static void Clean()
                {
                    AddVertex = 0;
                }
            }

            public static void Print()
            {
                Console.WriteLine("DependencyGraph times:");
                Console.WriteLine("AddVertex: " + GlobalPerformanceValues.DependencyGraphValues.Times.AddVertex.TotalSeconds);
                Console.WriteLine("Total: " + GlobalPerformanceValues.DependencyGraphValues.Times.Total.TotalSeconds);

                Console.WriteLine("AddVertex: " + GlobalPerformanceValues.DependencyGraphValues.Counters.AddVertex);
                Console.WriteLine("------- Operations counter - DependencyGraph (all): " +
                    GlobalPerformanceValues.DependencyGraphValues.Counters.Total + " ----------");
            }

            public static void Clean()
            {
                Times.Clean();
                Counters.Clean();
            }
        }

        public static class BrokerValues
        {
            public static class Times
            {
                public static TimeSpan Break = new TimeSpan(0);
                public static TimeSpan Continue = new TimeSpan(0);
                public static TimeSpan EnterCondition = new TimeSpan(0);
                public static TimeSpan ExitCondition = new TimeSpan(0);
                public static TimeSpan EnterMethod = new TimeSpan(0);
                public static TimeSpan ExitMethod = new TimeSpan(0);
                public static TimeSpan Alloc = new TimeSpan(0);
                public static TimeSpan DefUseOperation = new TimeSpan(0);
                public static TimeSpan Assign = new TimeSpan(0);
                public static TimeSpan RedefineType = new TimeSpan(0);
                public static TimeSpan EnterStaticMode = new TimeSpan(0);
                public static TimeSpan ExitStaticMode = new TimeSpan(0);
                public static TimeSpan AssignRV = new TimeSpan(0);
                public static TimeSpan HandleNonInstrumentedMethod = new TimeSpan(0);
                public static TimeSpan HandleArrayInitialization = new TimeSpan(0);
                public static TimeSpan HandleArrayElementReference = new TimeSpan(0);
                public static TimeSpan CreateNonInstrumentedRegion = new TimeSpan(0);
                public static TimeSpan CatchReturnedValueIntoRegion = new TimeSpan(0);

                public static TimeSpan Total
                {
                    get
                    {
                        return GlobalPerformanceValues.BrokerValues.Times.Break
                            .Add(GlobalPerformanceValues.BrokerValues.Times.Continue)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.EnterCondition)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.EnterMethod)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.ExitMethod)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.Alloc)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.DefUseOperation)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.Assign)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.RedefineType)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.EnterStaticMode)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.ExitStaticMode)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.AssignRV)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.HandleNonInstrumentedMethod)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.HandleArrayInitialization)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.HandleArrayElementReference)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.CreateNonInstrumentedRegion)
                            .Add(GlobalPerformanceValues.BrokerValues.Times.CatchReturnedValueIntoRegion);
                    }
                }

                public static void Clean()
                {
                    Break = new TimeSpan(0);
                    Continue = new TimeSpan(0);
                    EnterCondition = new TimeSpan(0);
                    ExitCondition = new TimeSpan(0);
                    EnterMethod = new TimeSpan(0);
                    ExitMethod = new TimeSpan(0);
                    Alloc = new TimeSpan(0);
                    DefUseOperation = new TimeSpan(0);
                    Assign = new TimeSpan(0);
                    RedefineType = new TimeSpan(0);
                    EnterStaticMode = new TimeSpan(0);
                    ExitStaticMode = new TimeSpan(0);
                    AssignRV = new TimeSpan(0);
                    HandleNonInstrumentedMethod = new TimeSpan(0);
                    HandleArrayInitialization = new TimeSpan(0);
                    HandleArrayElementReference = new TimeSpan(0);
                    CreateNonInstrumentedRegion = new TimeSpan(0);
                    CatchReturnedValueIntoRegion = new TimeSpan(0);
                }
            }

            public static class Counters
            {
                public static int Break = 0;
                public static int Continue = 0;
                public static int EnterCondition = 0;
                public static int ExitCondition = 0;
                public static int EnterMethod = 0;
                public static int ExitMethod = 0;
                public static int Alloc = 0;
                public static int DefUseOperation = 0;
                public static int Assign = 0;
                public static int RedefineType = 0;
                public static int EnterStaticMode = 0;
                public static int ExitStaticMode = 0;
                public static int AssignRV = 0;
                public static int HandleNonInstrumentedMethod = 0;
                public static int HandleArrayInitialization = 0;
                public static int HandleArrayElementReference = 0;
                public static int CreateNonInstrumentedRegion = 0;
                public static int CatchReturnedValueIntoRegion = 0;

                public static int Total
                {
                    get
                    {
                        return GlobalPerformanceValues.BrokerValues.Counters.Break +
                            GlobalPerformanceValues.BrokerValues.Counters.Continue +
                            GlobalPerformanceValues.BrokerValues.Counters.EnterCondition +
                            GlobalPerformanceValues.BrokerValues.Counters.ExitCondition +
                            GlobalPerformanceValues.BrokerValues.Counters.EnterMethod +
                            GlobalPerformanceValues.BrokerValues.Counters.ExitMethod +
                            GlobalPerformanceValues.BrokerValues.Counters.Alloc +
                            GlobalPerformanceValues.BrokerValues.Counters.DefUseOperation +
                            GlobalPerformanceValues.BrokerValues.Counters.Assign +
                            GlobalPerformanceValues.BrokerValues.Counters.RedefineType +
                            GlobalPerformanceValues.BrokerValues.Counters.EnterStaticMode +
                            GlobalPerformanceValues.BrokerValues.Counters.ExitStaticMode +
                            GlobalPerformanceValues.BrokerValues.Counters.AssignRV +
                            GlobalPerformanceValues.BrokerValues.Counters.HandleNonInstrumentedMethod +
                            GlobalPerformanceValues.BrokerValues.Counters.HandleArrayInitialization +
                            GlobalPerformanceValues.BrokerValues.Counters.HandleArrayElementReference +
                            GlobalPerformanceValues.BrokerValues.Counters.CreateNonInstrumentedRegion +
                            GlobalPerformanceValues.BrokerValues.Counters.CatchReturnedValueIntoRegion;
                    }
                }

                public static void Clean()
                {
                    Break = 0;
                    Continue = 0;
                    EnterCondition = 0;
                    ExitCondition = 0;
                    EnterMethod = 0;
                    ExitMethod = 0;
                    Alloc = 0;
                    DefUseOperation = 0;
                    Assign = 0;
                    RedefineType = 0;
                    EnterStaticMode = 0;
                    ExitStaticMode = 0;
                    AssignRV = 0;
                    HandleNonInstrumentedMethod = 0;
                    HandleArrayInitialization = 0;
                    HandleArrayElementReference = 0;
                    CreateNonInstrumentedRegion = 0;
                    CatchReturnedValueIntoRegion = 0;
                }
            }

            public static void Print()
            {
                Console.WriteLine("Tiempos del Broker:");
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.Break: " + GlobalPerformanceValues.BrokerValues.Times.Break.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.Continue: " + GlobalPerformanceValues.BrokerValues.Times.Continue.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.EnterCondition: " + GlobalPerformanceValues.BrokerValues.Times.EnterCondition.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.ExitCondition: " + GlobalPerformanceValues.BrokerValues.Times.ExitCondition.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.EnterMethod: " + GlobalPerformanceValues.BrokerValues.Times.EnterMethod.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.ExitMethod: " + GlobalPerformanceValues.BrokerValues.Times.ExitMethod.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.Alloc: " + GlobalPerformanceValues.BrokerValues.Times.Alloc.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.DefUseOperation: " + GlobalPerformanceValues.BrokerValues.Times.DefUseOperation.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.Assign: " + GlobalPerformanceValues.BrokerValues.Times.Assign.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.AssignRV: " + GlobalPerformanceValues.BrokerValues.Times.AssignRV.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.HandleNonInstrumentedMethod: " + GlobalPerformanceValues.BrokerValues.Times.HandleNonInstrumentedMethod.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.HandleArrayInitialization: " + GlobalPerformanceValues.BrokerValues.Times.HandleArrayInitialization.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.HandleArrayElementReference: " + GlobalPerformanceValues.BrokerValues.Times.HandleArrayElementReference.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.RedefineType: " + GlobalPerformanceValues.BrokerValues.Times.RedefineType.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.CreateNonInstrumentedRegion: " + GlobalPerformanceValues.BrokerValues.Times.CreateNonInstrumentedRegion.TotalSeconds);
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Times.CatchReturnedValueIntoRegion: " + GlobalPerformanceValues.BrokerValues.Times.CatchReturnedValueIntoRegion.TotalSeconds);
                Console.WriteLine("Total: " + GlobalPerformanceValues.BrokerValues.Times.Total.TotalSeconds);

                Console.WriteLine("------ Operations Counter ------");
                Console.WriteLine("GlobalPerformanceValues.BrokerValues.Counters.HandleNonInstrumentedMethod " + GlobalPerformanceValues.BrokerValues.Counters.HandleNonInstrumentedMethod);

                Console.WriteLine(" ----- Operations Counter - Broker (all) " +
                    GlobalPerformanceValues.BrokerValues.Counters.Total + " ----------");
            }

            public static void Clean()
            {
                Times.Clean();
                Counters.Clean();
            }
        }

        public static class TraceConsumerValues
        {
            public static class Times
            {
                public static TimeSpan GetNextStatement = new TimeSpan(0);
                public static TimeSpan ObserveNextStatement = new TimeSpan(0);
                public static TimeSpan HasNext = new TimeSpan(0);
                public static TimeSpan SliceCriteriaReached = new TimeSpan(0);

                public static TimeSpan Total
                {
                    get
                    {
                        return GetNextStatement.Add(ObserveNextStatement).Add(HasNext).Add(SliceCriteriaReached);
                    }
                }

                public static void Clean()
                {
                    GetNextStatement = new TimeSpan(0);
                    ObserveNextStatement = new TimeSpan(0);
                    HasNext = new TimeSpan(0);
                    SliceCriteriaReached = new TimeSpan(0);
                }
            }

            public static class Counters
            {
                public static int GetNextStatement = 0;
                public static int ObserveNextStatement = 0;
                public static int HasNext = 0;
                public static int SliceCriteriaReached = 0;

                public static int Total
                {
                    get
                    {
                        return GetNextStatement + ObserveNextStatement + HasNext + SliceCriteriaReached;
                    }
                }

                public static void Clean()
                {
                    GetNextStatement = 0;
                    ObserveNextStatement = 0;
                    HasNext = 0;
                    SliceCriteriaReached = 0;
                }
            }

            public static void Clean()
            {
                Times.Clean();
                Counters.Clean();
            }
        }

        public static class MemoryConsumptionValues
        {
            public static double maxMilliseconds = 60000;
            public static int currentCallsCount = 0;
            public static DateTime startDate = DateTime.Now;
            public static List<Tuple<long, int>> totalBytes = new List<Tuple<long, int>>();

            public static void Clean()
            {
                currentCallsCount = 0;
                startDate = DateTime.Now;
                totalBytes = new List<Tuple<long, int>>();
            }

            public static void Eval()
            {
                currentCallsCount++;
                if (DateTime.Now.Subtract(startDate).TotalMilliseconds >= maxMilliseconds)
                {
                    totalBytes.Add(new Tuple<long, int>(GC.GetTotalMemory(true) / 1024, currentCallsCount));
                    currentCallsCount = 0;
                    startDate = DateTime.Now;
                }
            }

            public static void Save(string file)
            {
                System.IO.File.WriteAllLines(file, totalBytes.Select(x => x.Item1.ToString() + " " + x.Item2.ToString()));
            }
        }

        public static void Print()
        {
            AliasingSolverValues.Print();
            DependencyGraphValues.Print();
        }

        public static void Clean()
        {
            DependencyGraphValues.Clean();
            AliasingSolverValues.Clean();
            BrokerValues.Clean();
            TraceConsumerValues.Clean();
            MemoryConsumptionValues.Clean();
        }

        public static void Save(string file, string name, TimeSpan diff, double executedStatements, string fileTraceInput)
        {
            var directory = Path.GetDirectoryName(file);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // GENERAL (3): Name Qty._STMT. TOTAL
            // BROKER (16x2): TOTAL | Break | Continue | EnterCondition | ExitCondition | EnterMethod | ExitMethod | Alloc | DefUseOperation | Assign | RedefineType | EnterStaticMode | ExitStaticMode | AssignRV | HandleNonInstrumentedMethod | HandleArrayInitialization | HandleArrayElementReference | CreateNonInstrumentedRegion | CatchReturnedValueIntoRegion
            // DG (1x2): TOTAL | 
            // SOLVER (12x2): TOTAL | LastDef_GET | LastDef_SET | Alloc | Assign | AssignRV | EnterMethodAndBind | ExitMethodAndUnbind | Reachable | Havoc | AddToRegion | RedefineType | CleanTemporaryEntries | EnterStaticMode | ExitStaticMode
            // TRACE (4x2): TOTAL | GetNextStmt | ObserveNextStmt | HasNext
            // EXTRA: Trace size (if it exists)

            var line = new StringBuilder();
            for (var si = 0; si < 82; si++)
                line.Append("{" + si + "}|");

            System.IO.File.AppendAllLines(file,
                new string[] { string.Format(line.ToString(), 
                    name, 
                    executedStatements, 
                    diff.TotalMilliseconds.ToString("N0"),

                    GlobalPerformanceValues.BrokerValues.Times.Total.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.Total,
                        
                    GlobalPerformanceValues.BrokerValues.Times.Break.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.Break,
                    GlobalPerformanceValues.BrokerValues.Times.Continue.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.Continue,
                    GlobalPerformanceValues.BrokerValues.Times.EnterCondition.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.EnterCondition,
                    GlobalPerformanceValues.BrokerValues.Times.ExitCondition.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.ExitCondition,
                    GlobalPerformanceValues.BrokerValues.Times.EnterMethod.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.EnterMethod,
                    GlobalPerformanceValues.BrokerValues.Times.ExitMethod.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.ExitMethod,
                    GlobalPerformanceValues.BrokerValues.Times.Alloc.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.Alloc,
                    GlobalPerformanceValues.BrokerValues.Times.DefUseOperation.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.DefUseOperation,
                    GlobalPerformanceValues.BrokerValues.Times.Assign.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.Assign,
                    GlobalPerformanceValues.BrokerValues.Times.RedefineType.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.RedefineType,
                    GlobalPerformanceValues.BrokerValues.Times.EnterStaticMode.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.EnterStaticMode,
                    GlobalPerformanceValues.BrokerValues.Times.ExitStaticMode.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.ExitStaticMode,
                    GlobalPerformanceValues.BrokerValues.Times.AssignRV.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.AssignRV,
                    GlobalPerformanceValues.BrokerValues.Times.HandleNonInstrumentedMethod.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.HandleNonInstrumentedMethod,
                    GlobalPerformanceValues.BrokerValues.Times.HandleArrayInitialization.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.HandleArrayInitialization,
                    GlobalPerformanceValues.BrokerValues.Times.HandleArrayElementReference.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.HandleArrayElementReference,
                    GlobalPerformanceValues.BrokerValues.Times.CreateNonInstrumentedRegion.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.CreateNonInstrumentedRegion,
                    GlobalPerformanceValues.BrokerValues.Times.CatchReturnedValueIntoRegion.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.BrokerValues.Counters.CatchReturnedValueIntoRegion,

                    GlobalPerformanceValues.DependencyGraphValues.Times.Total.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.DependencyGraphValues.Counters.Total,

                    GlobalPerformanceValues.AliasingSolverValues.Times.Total.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Total,
                    GlobalPerformanceValues.AliasingSolverValues.Times.LastDefGet.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.LastDefGet,
                    GlobalPerformanceValues.AliasingSolverValues.Times.LastDefSet.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.LastDefSet,
                    GlobalPerformanceValues.AliasingSolverValues.Times.Alloc.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Alloc,        
                    GlobalPerformanceValues.AliasingSolverValues.Times.Assign.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Assign,
                    GlobalPerformanceValues.AliasingSolverValues.Times.AssignRV.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.AssignRV,
                    GlobalPerformanceValues.AliasingSolverValues.Times.EnterMethodAndBind.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.EnterMethodAndBind,
                    GlobalPerformanceValues.AliasingSolverValues.Times.ExitMethodAndUnbind.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.ExitMethodAndUnbind,
                    GlobalPerformanceValues.AliasingSolverValues.Times.Reachable.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Reachable,
                    GlobalPerformanceValues.AliasingSolverValues.Times.Havoc.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.Havoc,
                    GlobalPerformanceValues.AliasingSolverValues.Times.AddToRegion.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.AddToRegion,
                    GlobalPerformanceValues.AliasingSolverValues.Times.RedefineType.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.RedefineType,
                    GlobalPerformanceValues.AliasingSolverValues.Times.CleanTemporaryEntries.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.CleanTemporaryEntries,
                    GlobalPerformanceValues.AliasingSolverValues.Times.EnterStaticMode.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.EnterStaticMode,
                    GlobalPerformanceValues.AliasingSolverValues.Times.ExitStaticMode.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.AliasingSolverValues.Counters.ExitStaticMode,

                    GlobalPerformanceValues.TraceConsumerValues.Times.Total.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.TraceConsumerValues.Counters.Total,
                    GlobalPerformanceValues.TraceConsumerValues.Times.GetNextStatement.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.TraceConsumerValues.Counters.GetNextStatement,
                    GlobalPerformanceValues.TraceConsumerValues.Times.ObserveNextStatement.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.TraceConsumerValues.Counters.ObserveNextStatement,
                    GlobalPerformanceValues.TraceConsumerValues.Times.HasNext.TotalMilliseconds.ToString("N0"),
                    GlobalPerformanceValues.TraceConsumerValues.Counters.HasNext,
                    
                    !string.IsNullOrWhiteSpace(fileTraceInput) ? Convert.ToInt32(new FileInfo(fileTraceInput).Length / 1024).ToString() : "") });

        }
    }
}
