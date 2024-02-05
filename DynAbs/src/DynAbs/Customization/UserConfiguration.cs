using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class UserConfiguration
    {
        public SolutionFiles solutionFiles { get; set; }
        public Criteria criteria { get; set; }
        public Results results { get; set; }
        public Customization customization { get; set; }
        
        public bool IsLoadedResult { get { return criteria.mode == Criteria.CriteriaMode.LoadResults; } }

        public class SolutionFiles
        {
            public string solutionPath { get; set; }
            public string instrumentedSolutionPath { get; set; }
            public string compilationOutputFolder { get; set; }
            public string executableProject { get; set; }
        }

        public class Criteria
        {
            public FileLine[] lines { get; set; }
            public CriteriaMode mode { get; set; }
            public string fileTraceInputPath { get; set; }
            public bool runAutomatically { get; set; }
            public bool notCompile { get; set; }
            public bool onlyOverrideCodeFiles { get; set; }
            public bool fileTraceInput { get; set; }

            public enum CriteriaMode
            {
                Normal,
                AtEnd,
                AtEndWithCriteria,
                LoadResults,
                OnlyInstrument,
                TraceAnalysis
            }

            public class FileLine
            {
                public string file { get; set; }
                public int? line { get; set; }

                public override bool Equals(object obj)
                {
                    if (obj == null || !(obj is FileLine))
                        return false;
                    var other = (FileLine)obj;
                    return other.file.Equals(this.file) && other.line.Equals(this.line);
                }

                public override int GetHashCode()
                {
                    // This will never have a collision
                    return file.GetHashCode() + line.GetHashCode();
                }
            }
        }

        public class Results
        {
            public string name { get; set; }
            public string outputFolder { get; set; }
            public string executedLinesFile { get; set; }
            public string executedLinesFileForUser { get; set; }
            public string executedLinesFileWithoutHeadersForUser { get; set; }
            public string executionCounters { get; set; }
            public string resultFile { get; set; }
            public string filteredResultFile { get; set; }
            public string sliceDependenciesGraphFolder { get; set; }
            public string dependencyGraphFile { get; set; }
            public string dependencyGraphDot { get; set; }
            public string pointsToGraphDot { get; set; }
            public bool printGraphForEachStatement { get; set; }
            public string mainResultsFolder { get; set; }
            public string summaryResultFile { get; set; }
            public bool logMethodsCalls { get; set; }
            public string executedMethodsInfo { get; set;  }
            public string executedCallbacksInfo { get; set; }
            public string callGraphPath { get; set; }
            public string skippedFilesInfo { get; set; }
            // Solo para modo debug
            public string debugProfileDataFile { get; set; }
            public string debugMemoryConsumptionFile { get; set; }
            public string debugMethodsCounterFile { get; set; }
            public string debugPTGEvolutionFile { get; set; }
            public string debugInternalSolverProfileFile { get; set; }
            public string debugLOTraceConsumer { get; set; }
        }
        
        public class Customization
        {
            public string summaries { get; set; }
            public bool includeControlDependencies { get; set; }
            public bool includeAllUses { get; set; }
            public bool staticMode { get; set; }
            public bool loopsOptimization { get; set; }
            public MemoryModelKind memoryModel { get; set; }
        }

        public enum MemoryModelKind 
        {
            Clusters,
            Default,
            Annotations,
            Speed,
            Mixed,
            SingleObject
        }

        public Instance[] instances { get; set; }
        public class Instance
        {
            public string[] parameters { get; set; }
        }

        public TargetProjects targetProjects { get; set; }

        public class TargetProjects
        {
            public ExcludedMode defaultMode { get; set; }
            public ExcludedProject[] excluded { get; set; }
        }

        public class ExcludedProject
        {
            public ExcludedMode mode { get; set; }
            public string name { get; set; }
            public int initialFileID { get; set; }

            public bool skipDefault { get; set; }

            public ExcludedClass[] classes { get; set; }

            public ExcludedFile[] files { get; set; }
        }

        public class ExcludedFile
        {
            public string name { get; set; }
            public bool? skip { get; set; }
            public int id { get; set; }
        }

        public class ExcludedClass
        {
            public ExcludedMode mode { get; set; }
            public string name { get; set; }

            public ExcludedMethod[] methods { get; set; }
        }

        public class ExcludedMethod
        {
            public string name { get; set; }
        }

        public enum ExcludedMode
        {
            None,
            All,
            AllExcept,
            Custom
        }

        public Dictionary<string, ISet<string>> GetExcludedClasses(string projectName)
        {
            var result = new Dictionary<string, ISet<string>>();
            var excludedProject = targetProjects?.excluded?.FirstOrDefault(x => x.name == projectName);
            if (excludedProject != null && excludedProject.classes != null)
            {
                foreach(var c in excludedProject.classes)
                {
                    if (c.mode == ExcludedMode.None)
                        continue;
                    if (c.mode == ExcludedMode.All)
                        result.Add(c.name, null);
                    if (c.mode == ExcludedMode.AllExcept)
                        result.Add(c.name, null);
                    if (c.mode == ExcludedMode.Custom && c.methods != null)
                        result.Add(c.name, new HashSet<string>(c.methods.Select(x => x.name)));
                }
            }
            return result;
        }
        public ISet<string> GetAllowedClasses(string projectName)
        {
            var result = new HashSet<string>();
            var excludedProject = targetProjects?.excluded?.FirstOrDefault(x => x.name == projectName);
            if (excludedProject != null && excludedProject.classes != null)
            {
                
                foreach (var c in excludedProject.classes)
                {
                    if (c.mode == ExcludedMode.None)
                        continue;
                    if (c.mode == ExcludedMode.AllExcept)
                        result.Add(c.name);
                }
            }
            return result;
        }

        public Folder[] FoldersToSkip { get; set; }

        public class Folder
        {
            public string name { get; set; }
        }
    }
}
