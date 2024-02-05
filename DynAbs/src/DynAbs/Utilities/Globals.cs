using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public static class Globals
    {
        public static InstrumentationResult InstrumentationResult;
        public static Solution UserSolution;
        public static bool reduce_dg = true;
        public static bool loops_optimization_enabled = false;
        public static DateTime start_time = DateTime.Now;
        public static bool print_console = true;

        public static bool properties_as_fields = true;
        public static bool include_receiver_use_on_calls = false;
        public static bool optimize_union_find_set = true;
        public static bool clean_last_def = true;
        public static bool optimize_types = true;
        public static bool skip_trace_enabled = true;
        public static bool? include_all_uses = null;
        public static bool generate_dgs = false;

        // IMPORTANT: this is because we cannot resolve how to wrap structs properties and methods access.
        public static bool wrap_structs_calls = true;

        public const string DefaultKind = "Default";

        // Temp path for debugging purposes
        public const string TempPath = @"C:\Users\alexd\Desktop\Slicer\Varios\temp";

        // ProcessConsumer
        public static int LastProcessConsumerId = 0;
        public static int LastTraceAmount = 0;

        // Aliasing Solver
        public static string TopKind = "ALL";
        public static int NextClusterID = 1;
        public static int NextPtgVertexID = 1;
        public static int ScopesInternalCounter = 0;
        public static int TermInternalCounter = 0;

        // Types
        public static int TypesHitsCache = 0;
        public static int TypesMissCache = 0;
        public static double TypesTimeGetMin = 0;
        public static double TypesTimeGetFieldSymbol = 0;
        public static double TypesTimeCompatibles = 0;

        // Main
        #if DEBUG
            public static bool TimeMeasurement = true;
        #else
            public static bool TimeMeasurement = false;
        #endif
    }
}
