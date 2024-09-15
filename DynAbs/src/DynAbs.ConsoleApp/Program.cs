using DynAbs;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using CommandLine;

namespace SliceConsole;

class Program
{
    #region Properties
    static bool useSameCompilation = false;
    static HashSet<string> foldersToSkip = null;
    static HashSet<string> foldersToAnalyze = null;
    static HashSet<string> traces = null;
    static Orchestrator defaultOrchestrator = null;
    #endregion

    static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<ConsoleOptions>(args)
            .WithParsed(RunOptions)
            .WithNotParsed(HandleParseError);
    }

    static void RunOptions(ConsoleOptions opts)
    {
        #region Settings
        if (opts.Skipped != null && opts.Skipped.Any())
            foldersToSkip = new HashSet<string>(opts.Skipped);
        if (opts.JustTheseFiles != null && opts.JustTheseFiles.Any())
            foldersToAnalyze = new HashSet<string>(opts.JustTheseFiles);
        if (opts.Traces != null && opts.Traces.Any())
            traces = new HashSet<string>(opts.Traces);

        useSameCompilation = traces != null || opts.UseSameCompilation;
        Globals.include_receiver_use_on_calls = opts.IncludeReceiverUseOnCalls;
        Globals.wrap_structs_calls = opts.WrapStructCalls;
        Globals.include_all_uses = opts.IncludeAllUses;

        var configurations = new List<string>();
        foreach (var file in opts.InputFiles)
        {
            if (Directory.Exists(file))
                configurations.AddRange(Directory.GetFiles(file, "*.slc", SearchOption.AllDirectories));
            else
                configurations.Add(file);
        }
        #endregion

        var filesOK = new List<string>();
        var filesWrong = new List<string>();
        foreach (var path in configurations.Where(x => System.IO.Path.GetExtension(x) == ".slc"))
        {
            var stream = System.IO.File.OpenRead(path.Trim());
            var serializer = new XmlSerializer(typeof(UserConfiguration));
            var userConfiguration = (UserConfiguration)serializer.Deserialize(stream);

            if (traces != null)
            {
                var originalOutputFolder = userConfiguration.results.outputFolder;
                foreach (var trace in traces)
                {
                    var currentDirectory = Path.GetFileName(Path.GetDirectoryName(trace));
                    
                    userConfiguration.criteria.fileTraceInputPath = trace;
                    userConfiguration.results.outputFolder = Path.Combine(originalOutputFolder, currentDirectory);
                    userConfiguration.results.name = Path.GetFileNameWithoutExtension(trace);
                    ExecuteConfiguration(trace, userConfiguration, filesOK, filesWrong);
                }
            }
            else
                ExecuteConfiguration(path, userConfiguration, filesOK, filesWrong);
        }

        #region Results
        Console.WriteLine("Files OK:");
        foreach (var f in filesOK)
            Console.WriteLine(f);
        Console.WriteLine("--------------------");
        Console.WriteLine("Files Wrong:");
        foreach (var f in filesWrong)
            Console.WriteLine(f);
        #endregion

        #if DEBUG
        Console.WriteLine("Press a key...");
        Console.ReadLine();
        //DynAbs.BugLogging.Save();
        #endif
    }

    static void ExecuteConfiguration(string pathOrFileName, UserConfiguration userConfiguration, List<string> filesOK, List<string> filesWrong)
    {
        Globals.skip_trace_enabled = foldersToSkip != null || foldersToAnalyze != null || (userConfiguration.customization?.skipTraceEnabled == true);
        Globals.loops_optimization_enabled = userConfiguration.customization.loopsOptimization;

        if (userConfiguration.FoldersToSkip == null && foldersToSkip != null && foldersToSkip.Any())
            userConfiguration.FoldersToSkip = foldersToSkip.Select(x => new UserConfiguration.Folder() { name = x }).ToArray();

        Console.WriteLine("Processing: " + pathOrFileName);
        Console.WriteLine("Trace file: " + (userConfiguration.criteria.fileTraceInputPath ?? "--"));

        var userInteractionCriteria = false;
        var userInteractionOutput = false;
        
        if (foldersToSkip != null)
            foreach (var targetProject in userConfiguration.targetProjects.excluded)
                for (var i = 0; i < targetProject.files?.Length; i++)
                    targetProject.files[i].skip = (foldersToSkip.Any(x => targetProject.files[i].name.Contains(x)));

        if (foldersToAnalyze != null)
            foreach (var targetProject in userConfiguration.targetProjects.excluded)
                for (var i = 0; i < targetProject.files?.Length; i++)
                    targetProject.files[i].skip = !(foldersToAnalyze.Any(x => targetProject.files[i].name.Contains(x)));

        if (Globals.include_all_uses.HasValue)
            userConfiguration.customization.includeAllUses = Globals.include_all_uses.Value;

        if (defaultOrchestrator == null || !useSameCompilation)
            defaultOrchestrator = new Orchestrator(userConfiguration);
        else
            defaultOrchestrator.Reset(userConfiguration);
        defaultOrchestrator.UserInteraction = false;

        #region Line selection interactive mode
        if ((userConfiguration.criteria.mode == UserConfiguration.Criteria.CriteriaMode.Normal) && userInteractionCriteria)
        {
            //SliceBrowser.SliceCriteria.Run(orchestrator.InstrumentationResult.IdToFileDictionary.Select(x => x.Value).ToArray());
            //userConfiguration.criteria.lines.First().file = SliceBrowser.SliceCriteria.SliceCriteriaFile;
            //userConfiguration.criteria.lines.First().line = SliceBrowser.SliceCriteria.SliceCriteriaFileLine;
            throw new NotImplementedException();
        }
        #endregion

        try
        {
            defaultOrchestrator.Orchestrate();
            filesOK.Add(Path.GetFileName(pathOrFileName));

            Console.WriteLine("OK: " + pathOrFileName);
        }
        catch (Exception ex)
        {
            filesWrong.Add(Path.GetFileName(pathOrFileName) + ": " + ex.Message);
            var fullMessage = ex is SlicerException ? ((SlicerException)ex).InternalException ?.ToString() ?? ex.ToString() : ex.ToString();
            Console.WriteLine("WRONG: " + pathOrFileName + ": " + fullMessage);
        }

        if (userInteractionOutput)
        {
            // TODO: check
            var reducedCompleteDG = defaultOrchestrator.GetReducedDependencyGraph();
            var reducedSliceDG = defaultOrchestrator.GetReducedSliceDependencyGraph();
            var completeDG = defaultOrchestrator.CompleteDependencyGraph;

            DynAbs.DesktopApp.Browser.ComplexBrowser.Run(
                defaultOrchestrator.UserSolution,
                reducedSliceDG,
                defaultOrchestrator.InstrumentationResult.fileIdToSyntaxTree,
                defaultOrchestrator.InstrumentationResult.IdToFileDictionary,
                defaultOrchestrator.ExecutedStmts);
        }
    }
    
    class ConsoleOptions
    {
        [Option('f', "files", 
            Required = true,
            HelpText = "Configuration files to be processed.")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('t', "traces",
            Required = false,
            HelpText = "Trace files to be processed.")]
        public IEnumerable<string> Traces { get; set; }

        [Option('s', "skip",
            Required = false,
            HelpText = "Files of the client source code whose trace will be skipped during the analysis.")]
        public IEnumerable<string> Skipped { get; set; }

        [Option('a', "toAnalyze",
            Required = false,
            HelpText = "The only files of the client source code that will be analyzed.")]
        public IEnumerable<string> JustTheseFiles { get; set; }

        [Option('r', "depReceiver",
            Required = false,
            HelpText = "Include the usage of the receiver on calls.",
            Default = true)]
        public bool IncludeReceiverUseOnCalls { get; set; }

        [Option('w', "wrapStructCalls",
            Required = false,
            HelpText = "Prevent callbacks from structs (in development).",
            Default = false)]
        public bool WrapStructCalls { get; set; }

        [Option('u', "includeAllUses",
            Required = false,
            HelpText = "When accessing a property, include all uses (e.g., x.y.z will include the last definitions for x, y, and z).",
            Default = true)]
        public bool IncludeAllUses { get; set; }

        [Option('c', "useSameCompilation",
            Required = false,
            HelpText = "Use the same instrumented version for all configuration files (this is helpful when running multiple inputs).",
            Default = false)]
        public bool UseSameCompilation { get; set; }
    }

    static void HandleParseError(IEnumerable<Error> errs)
    {
        Console.WriteLine("Couldn't parse the arguments.");
        foreach (var err in errs)
            Console.WriteLine(err.ToString());
    }
}
