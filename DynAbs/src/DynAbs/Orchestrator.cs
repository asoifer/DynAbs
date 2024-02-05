using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using QuikGraph;
using Microsoft.CodeAnalysis;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace DynAbs
{
    public class Orchestrator
    {
        public UserSliceConfiguration Configuration { get; set; }
        public Solution UserSolution { get; set; }
        public Solution InstrumentedSolution { get; set; }
        public InstrumentationResult InstrumentationResult { get; private set; }
        SlicerResultsManager ResultsManager { get; set; }

        public ISet<Stmt> ExecutedStmts { get; private set; }
        public ExecutedStatementsContainer ExecutedStmtsContainer { get; private set; }
        public List<ISet<Stmt>> SlicedStmts { get; private set; }
        public AdjacencyGraph<string, Edge<string>> CompleteDependencyGraph { get; private set; }
        public List<AdjacencyGraph<string, Edge<string>>> SlicedDependencyGraphs { get; private set; }
        public bool UserInteraction { get; set; }
        public double totalSecondsInstrumentation { get; set; }
        public double? generationTraceTime { get; set; }

        public Orchestrator(UserConfiguration userConfiguration, params Type[] additionalTypes) : this (userConfiguration, null, additionalTypes) { }

        public Orchestrator(UserConfiguration userConfiguration, Solution userSolution, params Type[] additionalTypes)
        {
            Configuration = new UserSliceConfiguration(userConfiguration);
            if (userSolution == null)
            {
                if (MSBuildLocator.CanRegister)
                    MSBuildLocator.RegisterDefaults();
                var msBuildWorkspace = MSBuildWorkspace.Create();
                UserSolution = msBuildWorkspace.OpenSolutionAsync(Configuration.User.solutionFiles.solutionPath).Result;
            }
            else
                UserSolution = userSolution;
            Globals.UserSolution = UserSolution;
            LoadSources(additionalTypes);            
        }

        void LoadSources(params Type[] additionalTypes)
        {
            var start = System.DateTime.Now;
            if (!string.IsNullOrWhiteSpace(Configuration.User.solutionFiles.instrumentedSolutionPath) && File.Exists(Configuration.User.solutionFiles.instrumentedSolutionPath))
            {
                var msBuildWorkspace = MSBuildWorkspace.Create();
                InstrumentedSolution = msBuildWorkspace.OpenSolutionAsync(Configuration.User.solutionFiles.instrumentedSolutionPath).Result;
            }
            else if (string.IsNullOrWhiteSpace(Configuration.User.solutionFiles.compilationOutputFolder))
                throw new SlicerException("Ingrese una carpeta para guardar la compilación (tag:compilationOutputFolder)");

            var compiler = new SourceCompiler(Configuration, UserSolution, InstrumentedSolution, Configuration.User.criteria.mode != UserConfiguration.Criteria.CriteriaMode.LoadResults);
            InstrumentationResult = compiler.GetInstrumentationResult(additionalTypes);
            totalSecondsInstrumentation = DateTime.Now.Subtract(start).TotalSeconds;
            ResultsManager = new SlicerResultsManager(InstrumentationResult);
        }

        public void Reset(UserConfiguration userConfiguration, bool refreshSkipInfo = false)
        {
            Configuration = new UserSliceConfiguration(userConfiguration);
            ResultsManager = new SlicerResultsManager(InstrumentationResult);

            foreach (var project in UserSolution.Projects)
            {
                if (Configuration?.User.targetProjects?.excluded != null &&
                    ((Configuration.User.targetProjects.excluded.Any(x => x.name == project.Name && x.mode == UserConfiguration.ExcludedMode.All) ||
                    (Configuration.User.targetProjects.defaultMode == UserConfiguration.ExcludedMode.All && !Configuration.User.targetProjects.excluded.Any(x => x.name == project.Name && x.mode != UserConfiguration.ExcludedMode.All)))))
                    continue;

                var configProject = Configuration.User.targetProjects?.excluded?.FirstOrDefault(x => x.name == project.Name);
                if (configProject != null && configProject.files != null)
                {
                    Configuration.FilesToSkip ??= new HashSet<int>();
                    foreach (var file in configProject.files)
                    {
                        if (InstrumentationResult.FileToIdDictionary.ContainsKey(file.name))
                        { 
                            var id = InstrumentationResult.FileToIdDictionary[file.name];
                            if (file.skip == true)
                                Configuration.FilesToSkip.Add(id);
                            else
                                Configuration.FilesToSkip.Remove(id);
                        }
                    }
                }
            }
        }

        public void Orchestrate()
        {
            if (InstrumentationResult == null)
                return;

            if (Configuration.User.criteria.mode == UserConfiguration.Criteria.CriteriaMode.OnlyInstrument)
                return;

            if (Configuration.User.criteria.mode != UserConfiguration.Criteria.CriteriaMode.LoadResults)
            {
                var instancesList = (Configuration.User.instances ?? new UserConfiguration.Instance[]{ new UserConfiguration.Instance() }).ToList();
                for (var i = 0; i < instancesList.Count; i++)
                {
                    generationTraceTime = null;
                    string name = Configuration.User.results.name ?? "NN";
                    string arguments = instancesList[i].parameters != null ? string.Join(" ", instancesList[i].parameters) : "";
                    
                    #region Result folder and files
                    InitializeOutputFolder(Configuration.User.results.outputFolder, name, arguments);
                    #endregion

                    #region Execution and trace generation
                    if (Configuration.User.criteria.runAutomatically && string.IsNullOrWhiteSpace(Configuration.User.criteria.fileTraceInputPath))
                    {
                        Run(arguments, GetDefaultPath(InstrumentationResult.Executable));
                    }

                    string fileTraceInputPath = null;
                    if (Configuration.User.criteria.fileTraceInput)
                    {
                        if (string.IsNullOrWhiteSpace(Configuration.User.criteria.fileTraceInputPath))
                            fileTraceInputPath = GetDefaultPath(InstrumentationResult.Executable);
                        else
                            fileTraceInputPath = Configuration.User.criteria.fileTraceInputPath;
                    }
                    #endregion

                    #region Trace consumption
                    var mainTraceConsumer = Slice(fileTraceInputPath, Configuration.User.criteria.lines);
                    #endregion

                    #region Saving results
                    SaveResults(mainTraceConsumer, i, name, arguments, fileTraceInputPath);
                    #endregion
                }
            }
            else
            {
                ExecutedStmts = ResultsManager.LoadExecutedStatements(Configuration.User.results.executedLinesFile);
                CompleteDependencyGraph = ResultsManager.LoadDependencyGraph(Configuration.User.results.dependencyGraphFile);
                if (!string.IsNullOrWhiteSpace(Configuration.User.results.sliceDependenciesGraphFolder))
                    SlicedDependencyGraphs = new List<AdjacencyGraph<string, Edge<string>>>() { ResultsManager.LoadDependencyGraph(
                        System.IO.Directory.GetFiles(Configuration.User.results.sliceDependenciesGraphFolder).First(x => x.EndsWith(".dg"))) };
            }
        }

        public void Run(string arguments, string outputTraceFile)
        {
            var executableProject = InstrumentationResult.Executable;
            Process pr = null;
            // NET CORE
            if (executableProject.EndsWith(".csproj"))
            {
                if (Configuration.User.criteria.fileTraceInput)
                {
                    var execResult = Utils.RunProject(executableProject, arguments, 60000);
                    generationTraceTime = execResult.Timeout ? int.MaxValue : execResult.ElapsedTime.TotalSeconds;
                }
                else
                {
                    pr = Utils.GetRunProcess(executableProject, arguments);
                    pr.Start();
                }
            }
            // NET Framework
            else
            {
                var windowsStyle = UserInteraction ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
                var process = new Process { StartInfo = { FileName = executableProject, WindowStyle = windowsStyle, Arguments = arguments } };
                process.Start();

                // Si usamos archivo, hay que esperar a que el proceso termine y escriba toda la traza.
                if (Configuration.User.criteria.fileTraceInput)
                {
                    process.WaitForExit();
                    generationTraceTime = process.ExitTime.Subtract(process.StartTime).TotalSeconds;
                }
            }
            
            if (Configuration.User.criteria.fileTraceInput && !string.IsNullOrWhiteSpace(outputTraceFile))
            {
                var originalTracePath = GetDefaultPath(executableProject);
                if (File.Exists(originalTracePath) && !string.Equals(outputTraceFile, originalTracePath, StringComparison.InvariantCultureIgnoreCase))
                    File.Copy(originalTracePath, outputTraceFile, true);
            }
        }

        public MainTraceConsumer Slice(string traceFilePath, UserConfiguration.Criteria.FileLine[] linesToSlice)
        {
            var filesLines = new List<Tuple<int, int>>();
            if (linesToSlice != null)
                foreach (var fileLine in linesToSlice)
                    filesLines.Add(new Tuple<int, int>(InstrumentationResult.IdToFileDictionary.Single(x =>
                        System.IO.Path.GetFileName(fileLine.file) == fileLine.file ?
                        System.IO.Path.GetFileName(x.Value).Equals(System.IO.Path.GetFileName(fileLine.file)) :
                        x.Value == System.IO.Path.GetFullPath(fileLine.file)).Key, fileLine.line.Value));

            var atEndModes = new UserConfiguration.Criteria.CriteriaMode[] {
                        UserConfiguration.Criteria.CriteriaMode.AtEnd, UserConfiguration.Criteria.CriteriaMode.AtEndWithCriteria };
            var mainTraceConsumer = new MainTraceConsumer(Configuration, InstrumentationResult,
                Configuration.User.criteria.mode == UserConfiguration.Criteria.CriteriaMode.AtEnd ? (List<Tuple<int, int>>)null : filesLines, traceFilePath);
            var hasError = false;
            var errMsg = "";
            try
            {
                mainTraceConsumer.Launch(Configuration.User.criteria.mode == UserConfiguration.Criteria.CriteriaMode.TraceAnalysis);
            }
            catch (Exception ex)
            {
                hasError = true;
                errMsg = ex.Message;
            }

            if (hasError)
            {
                if (Configuration.User.criteria.fileTraceInput)
                {
                    var processed = mainTraceConsumer._traceConsumer.Stack.Count;
                    var total = System.IO.File.ReadAllLines(traceFilePath).Length - 1;
                    var percentage = Math.Round((decimal)processed * (decimal)100 / (decimal)total, 0);

                    errMsg += $" - {processed}/{total} ({percentage}%)";
                }

                throw new SlicerException(errMsg);
            }

            return mainTraceConsumer;
        }

        public void SaveResults(MainTraceConsumer mainTraceConsumer, int instanceNumber, string programName, string arguments, string traceFilePath, bool printResult = true)
        {
            #region Basic data
            var slicesSummaryData = mainTraceConsumer.GetDataForeachSlice();
            foreach (var data in slicesSummaryData)
            {
                var executionSeconds = data.ElapsedTime.TotalSeconds;
                var totalTraceLines = data.TotalTraceLines;
                var totalSkippedTrace = data.TotalSkippedTrace;
                var totalReceivedTrace = data.TotalReceivedTrace;
                var totalStatementsLines = data.TotalStatements;
                var distinctStatementLines = data.DistinctStatements;
                var slicedStatements = data.SlicedStatements.Count;
                var localGenerationTraceTime = (generationTraceTime.HasValue ? generationTraceTime.Value.ToString() : "");
                var skippedTrace = totalSkippedTrace.HasValue ? totalSkippedTrace.ToString() : "";
                var skippedLinesPercent = totalSkippedTrace.HasValue ? Math.Round(totalSkippedTrace.Value * (double)100 / totalReceivedTrace.Value, 0).ToString() : "";
                var traceSize = traceFilePath != null ? Math.Round(new FileInfo(traceFilePath).Length / (double)1024).ToString() : "";

                if (printResult)
                {     
                    Console.WriteLine("Instrumentation: " + totalSecondsInstrumentation);
                    Console.WriteLine("Generation trace time: " + localGenerationTraceTime);
                    Console.WriteLine("Analysis time: " + executionSeconds);
                    Console.WriteLine("#Stmt: " + totalStatementsLines);
                    Console.WriteLine("#Unique Stmt: " + distinctStatementLines);
                    Console.WriteLine("#Stmt Slice: " + slicedStatements);
                    Console.WriteLine("#Trace lines: " + totalTraceLines);
                    Console.WriteLine("#Skipped trace lines: " + (!totalSkippedTrace.HasValue ? "-" : string.Format("{0}/{1} ({2}%)", totalSkippedTrace, totalReceivedTrace, skippedLinesPercent)));
                    Console.WriteLine("----------------------------");
                }

                // Nombre|Inputs|#Stmt|#UniqueStmt|#Slice|T. Instr|T. traza|T. Ejecución|Size Traza|Traza Total|Traza Salteada|%Salteado
                if (!string.IsNullOrWhiteSpace(Configuration.User.results.summaryResultFile))
                    System.IO.File.AppendAllLines(string.Format(Configuration.User.results.summaryResultFile, instanceNumber),
                        new string[] { $"{programName}|{arguments}|{totalStatementsLines}|{distinctStatementLines}|{slicedStatements}|{totalSecondsInstrumentation}|" +
                                            $"{localGenerationTraceTime}|{executionSeconds}|{traceSize}|{totalTraceLines}|{skippedTrace}|{skippedLinesPercent}" });
            }
            #endregion

            #region Profiling (only in DEBUG mode)
            if (Globals.TimeMeasurement)
            {
                // Esta información la guardamos siempre en modo debug (por ahora)
                var debugProfileDataFile = string.IsNullOrWhiteSpace(Configuration.User.results.debugProfileDataFile) ? @"C:\temp\tiempos\totalTimes.txt" : Configuration.User.results.debugProfileDataFile;
                GlobalPerformanceValues.Save(debugProfileDataFile, mainTraceConsumer.entryPointClassName, mainTraceConsumer.elapsedTime, mainTraceConsumer.totalStatementLines, traceFilePath);

                if (Configuration.User.criteria.mode != UserConfiguration.Criteria.CriteriaMode.TraceAnalysis)
                {
                    if (!string.IsNullOrWhiteSpace(Configuration.User.results.debugMemoryConsumptionFile))
                        GlobalPerformanceValues.MemoryConsumptionValues.Save(string.Format(Configuration.User.results.debugMemoryConsumptionFile, instanceNumber));

                    mainTraceConsumer.SaveBrokerAndAliasingSolverData(Configuration.User.results.debugMethodsCounterFile, Configuration.User.results.debugPTGEvolutionFile, Configuration.User.results.debugInternalSolverProfileFile);
                }
            }
            else if (Configuration.User.criteria.mode != UserConfiguration.Criteria.CriteriaMode.TraceAnalysis)
                mainTraceConsumer.SaveBrokerAndAliasingSolverData(string.Empty, Configuration.User.results.debugPTGEvolutionFile, Configuration.User.results.debugInternalSolverProfileFile);
            #endregion

            ExecutedStmtsContainer = mainTraceConsumer.GetExecutedStatements();
            ExecutedStmts = ExecutedStmtsContainer.ExecutedStatements;
            if (!string.IsNullOrWhiteSpace(Configuration.User.results.executedLinesFile))
                ResultsManager.SaveExecutedStatements(ExecutedStmts, string.Format(Configuration.User.results.executedLinesFile, instanceNumber));

            if (!string.IsNullOrWhiteSpace(Configuration.User.results.executedLinesFileForUser))
                ResultsManager.SaveExecutedStatementsForUser(ExecutedStmtsContainer, string.Format(Configuration.User.results.executedLinesFileForUser, instanceNumber));

            if (!string.IsNullOrWhiteSpace(Configuration.User.results.executedLinesFileWithoutHeadersForUser))
                ResultsManager.SaveExecutedStatementsWithoutHeadersForUser(ExecutedStmtsContainer, string.Format(Configuration.User.results.executedLinesFileWithoutHeadersForUser, instanceNumber));

            if (!string.IsNullOrWhiteSpace(Configuration.User.results.executionCounters))
                ResultsManager.SaveExecutionCounters(ExecutedStmtsContainer, string.Format(Configuration.User.results.executionCounters, instanceNumber));

            if (Configuration.User.criteria.mode != UserConfiguration.Criteria.CriteriaMode.TraceAnalysis)
            {
                CompleteDependencyGraph = mainTraceConsumer.GetSliceDependencyGraph();
                if (!string.IsNullOrWhiteSpace(Configuration.User.results.dependencyGraphFile))
                    ResultsManager.SaveDependencyGraph(CompleteDependencyGraph, Configuration.User.results.dependencyGraphFile);

                if (!string.IsNullOrWhiteSpace(Configuration.User.results.debugLOTraceConsumer))
                    mainTraceConsumer.SaveLOTraceData(Configuration.User.results.debugLOTraceConsumer);

                if (!string.IsNullOrWhiteSpace(Configuration.User.results.callGraphPath))
                    mainTraceConsumer.PrintCallGraph(Configuration.User.results.callGraphPath);

                mainTraceConsumer.SaveExecutedMethodsAndCallbacks(Configuration.User.results.executedMethodsInfo, Configuration.User.results.executedCallbacksInfo);

                Utils.SaveSkippedFilesInfo(Configuration, Configuration?.User.results?.skippedFilesInfo, traceFilePath, InstrumentationResult, UserSolution);

                if (Configuration.User.criteria.mode != UserConfiguration.Criteria.CriteriaMode.AtEnd)
                {
                    SlicedStmts = new List<ISet<Stmt>>(mainTraceConsumer.resultSummarydata.Select(x => x.SlicedStatements));
                    if (!string.IsNullOrWhiteSpace(Configuration.User.results.resultFile))
                        Utils.SaveSliceResult(mainTraceConsumer.resultSummarydata, string.Format(Configuration.User.results.resultFile, instanceNumber));

                    if (!string.IsNullOrWhiteSpace(Configuration.User.results.filteredResultFile))
                        Utils.SaveFilteredSliceResult(mainTraceConsumer.resultSummarydata, ExecutedStmtsContainer, string.Format(Configuration.User.results.filteredResultFile, instanceNumber));

                    SlicedDependencyGraphs = mainTraceConsumer.GetSliceDependenciesGraph();
                    if (!string.IsNullOrWhiteSpace(Configuration.User.results.sliceDependenciesGraphFolder))
                    {
                        Utils.SaveSliceDependenciesGraphResult(SlicedDependencyGraphs, mainTraceConsumer.GetDependencyGraphVertexLabels(), Configuration.User.results.sliceDependenciesGraphFolder);
                        ResultsManager.SaveDependencyGraph(SlicedDependencyGraphs, Configuration.User.results.sliceDependenciesGraphFolder);
                    }
                }

                #region DOT
                try
                {
                    if (!string.IsNullOrWhiteSpace(Configuration.User.results.dependencyGraphDot))
                        mainTraceConsumer.PrintGraph(string.Format(Configuration.User.results.dependencyGraphDot, instanceNumber));
                    if (!string.IsNullOrWhiteSpace(Configuration.User.results.pointsToGraphDot))
                        mainTraceConsumer.PrintPTG(string.Format(Configuration.User.results.pointsToGraphDot, instanceNumber));
                }
                catch (Exception)
                {

                }
                #endregion
            }
        }

        public void InitializeOutputFolder(string outputFolder, string programName, string arguments)
        {
            if (!string.IsNullOrEmpty(outputFolder))
            {
                var outputFolderForInstance = Path.Combine(outputFolder, $"{programName} {arguments}".Trim());
                if (!Directory.Exists(outputFolderForInstance))
                    Directory.CreateDirectory(outputFolderForInstance);
                // Setear los paths de la configuración completos (TODO: Completar)
                if (!string.IsNullOrEmpty(Configuration.User.results.summaryResultFile))
                    Configuration.User.results.summaryResultFile = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.summaryResultFile));
                if (!string.IsNullOrEmpty(Configuration.User.results.executedLinesFile))
                    Configuration.User.results.executedLinesFile = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.executedLinesFile));
                if (!string.IsNullOrEmpty(Configuration.User.results.executedLinesFileForUser))
                    Configuration.User.results.executedLinesFileForUser = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.executedLinesFileForUser));
                if (!string.IsNullOrEmpty(Configuration.User.results.executedLinesFileWithoutHeadersForUser))
                    Configuration.User.results.executedLinesFileWithoutHeadersForUser = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.executedLinesFileWithoutHeadersForUser));
                if (!string.IsNullOrEmpty(Configuration.User.results.executionCounters))
                    Configuration.User.results.executionCounters = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.executionCounters));
                if (!string.IsNullOrEmpty(Configuration.User.results.resultFile))
                    Configuration.User.results.resultFile = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.resultFile));
                if (!string.IsNullOrEmpty(Configuration.User.results.filteredResultFile))
                    Configuration.User.results.filteredResultFile = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.filteredResultFile));
                if (!string.IsNullOrEmpty(Configuration.User.results.sliceDependenciesGraphFolder))
                    Configuration.User.results.sliceDependenciesGraphFolder = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.sliceDependenciesGraphFolder));
                if (!string.IsNullOrEmpty(Configuration.User.results.dependencyGraphFile))
                    Configuration.User.results.dependencyGraphFile = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.dependencyGraphFile));
                if (!string.IsNullOrEmpty(Configuration.User.results.dependencyGraphDot))
                    Configuration.User.results.dependencyGraphDot = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.dependencyGraphDot));
                if (!string.IsNullOrEmpty(Configuration.User.results.pointsToGraphDot))
                    Configuration.User.results.pointsToGraphDot = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.pointsToGraphDot));
                if (!string.IsNullOrEmpty(Configuration.User.results.callGraphPath))
                    Configuration.User.results.callGraphPath = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.callGraphPath));
                if (!string.IsNullOrEmpty(Configuration.User.results.executedMethodsInfo))
                    Configuration.User.results.executedMethodsInfo = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.executedMethodsInfo));
                if (!string.IsNullOrEmpty(Configuration.User.results.executedCallbacksInfo))
                    Configuration.User.results.executedCallbacksInfo = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.executedCallbacksInfo));
                if (!string.IsNullOrEmpty(Configuration.User.results.skippedFilesInfo))
                    Configuration.User.results.skippedFilesInfo = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.skippedFilesInfo));
                if (!string.IsNullOrEmpty(Configuration.User.results.debugProfileDataFile))
                    Configuration.User.results.debugProfileDataFile = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.debugProfileDataFile));
                if (!string.IsNullOrEmpty(Configuration.User.results.debugInternalSolverProfileFile))
                    Configuration.User.results.debugInternalSolverProfileFile = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.debugInternalSolverProfileFile));
                if (!string.IsNullOrEmpty(Configuration.User.results.debugPTGEvolutionFile))
                    Configuration.User.results.debugPTGEvolutionFile = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.debugPTGEvolutionFile));
                if (!string.IsNullOrEmpty(Configuration.User.results.mainResultsFolder))
                {
                    Configuration.User.results.mainResultsFolder = Path.Combine(outputFolderForInstance, Path.GetFileName(Configuration.User.results.mainResultsFolder));
                    if (!Directory.Exists(Configuration.User.results.mainResultsFolder))
                        Directory.CreateDirectory(Configuration.User.results.mainResultsFolder);
                }
            }
        }

        public BrowsingData GetReducedDependencyGraph()
        {
            return new BrowsingData(ResultsManager, CompleteDependencyGraph);
        }

        public BrowsingData GetReducedSliceDependencyGraph()
        {
            if (SlicedDependencyGraphs.Count > 0)
                return new BrowsingData(ResultsManager, SlicedDependencyGraphs.First());
            return null;
        }

        string GetDefaultPath(string executablePath)
        {
            return Path.Combine(Path.GetDirectoryName(executablePath), Tracing.TracerGlobals.FileTraceInput);
        }
    }
}
