using DynAbs;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.CodeAnalysis.Operations;

namespace SliceConsole
{
    class Program
    {
        enum ConsoleMode
        {
            Default=1,
            MultipleFiles=2,
            MultipleConfigurations=3,
            MultipleInputs=4
        }

        #region Properties
        static string[] defaultConfigPaths;
        static bool useJustTheseFiles;
        static string[] justTheseFiles;
        static bool excludedFilesEnabled;
        static string[] excludedFolders;
        static bool useSameCompilation;
        static bool useTraces;
        static string[] traces;
        static bool useAnnotations;
        static bool addSkippedFilesResult;
        static bool addExecutedStmtWithoutHeaders;
        static bool addExecutionCounters;
        static bool justThisFolders;
        static bool staticModeEnabled;
        static HashSet<string> foldersToSkip;
        static Orchestrator defaultOrchestrator = null;
        #endregion

        static void Main(string[] args)
        {
            var appSettings = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            #region Settings
            defaultConfigPaths = appSettings.GetSection("defaultConfigPaths").Get<string[]>();
            useJustTheseFiles = bool.Parse(appSettings["useJustTheseFiles"]);
            justTheseFiles = appSettings.GetSection("justTheseFiles").Get<string[]>();
            excludedFilesEnabled = bool.Parse(appSettings["excludedFilesEnabled"]);
            excludedFolders = appSettings.GetSection("excludedFolders").Get<string[]>() ?? new string[] { };
            useAnnotations = bool.Parse(appSettings["useAnnotations"]);
            addSkippedFilesResult = bool.Parse(appSettings["addSkippedFilesResult"]);
            addExecutedStmtWithoutHeaders = bool.Parse(appSettings["addExecutedStmtWithoutHeaders"]);
            addExecutionCounters = bool.Parse(appSettings["addExecutionCounters"]);
            useSameCompilation = bool.Parse(appSettings["useSameCompilation"]);            
            useTraces = bool.Parse(appSettings["useTraces"]);
            justThisFolders = bool.Parse(appSettings["justThisFolders"]);
            staticModeEnabled = bool.Parse(appSettings["staticModeEnabled"]);
            traces = appSettings.GetSection("traces").Get<string[]>();
            foldersToSkip = new HashSet<string>(excludedFolders);
            Globals.skip_trace_enabled = excludedFilesEnabled || justThisFolders;
            Globals.include_receiver_use_on_calls = bool.Parse(appSettings["includeReceiverUseOnCalls"]);
            Globals.wrap_structs_calls = bool.Parse(appSettings["wrapStructCalls"]);
            var settingIncludeAllUses = appSettings.AsEnumerable().FirstOrDefault(x => x.Key == "includeAllUses");
            if (!string.IsNullOrWhiteSpace(settingIncludeAllUses.Key))
                Globals.include_all_uses = bool.Parse(settingIncludeAllUses.Value);
            #endregion

            var configurations = new List<string>();
            if (args != null && args.Count() > 0)
                configurations = args.ToList();
            else
            {
                foreach (var fileOrFolder in defaultConfigPaths)
                {
                    if (Directory.Exists(fileOrFolder))
                        configurations.AddRange(Directory.GetFiles(fileOrFolder, "*.slc", SearchOption.AllDirectories).OrderByDescending(x => x).ToList());
                    else
                        configurations.Add(fileOrFolder);
                }
            }

            //LoadOldenInputs();

            var filesOK = new List<string>();
            var filesWrong = new List<string>();
            foreach (var path in configurations.Where(x => System.IO.Path.GetExtension(x) == ".slc"))
            {
                if (useJustTheseFiles && !justTheseFiles.Any(x => path.Contains(x)))
                    continue;

                var stream = System.IO.File.OpenRead(path.Trim());
                var serializer = new XmlSerializer(typeof(UserConfiguration));
                var userConfiguration = (UserConfiguration)serializer.Deserialize(stream);
                if (!useAnnotations && userConfiguration.customization != null)
                    userConfiguration.customization.summaries = null;

                if (useTraces)
                {
                    var originalOutputFolder = userConfiguration.results.outputFolder;

                    var localTraces = new List<string>();
                    foreach (var trace in traces)
                        // TODO XXX: Hardcode
                        localTraces.AddRange(Directory.GetFiles(trace, "*_ORIG.txt", SearchOption.AllDirectories));

                    foreach (var localTrace in localTraces)
                    {
                        var currentDirectory = Path.GetFileName(Path.GetDirectoryName(localTrace));

                        userConfiguration.criteria.fileTraceInputPath = localTrace;
                        userConfiguration.results.outputFolder = Path.Combine(originalOutputFolder, currentDirectory);
                        userConfiguration.results.name = Path.GetFileNameWithoutExtension(localTrace);
                        ExecuteConfiguration(localTrace, userConfiguration, filesOK, filesWrong);
                    }
                }
                else if (OldenExecutions.Any())
                {
                    var progName = Path.GetFileNameWithoutExtension(path).ToLower().Replace("jolden", "");
                    var executions = OldenExecutions[progName];
                    var i = 0;
                    foreach (var execution in executions)
                    {
                        userConfiguration.instances = new UserConfiguration.Instance[]
                        {
                            new UserConfiguration.Instance()
                            {
                                parameters = execution.Inputs.Select(x => "-" + x.Name + " " + x.Value).ToArray()
                            }
                        };
                        ExecuteConfiguration(path, userConfiguration, filesOK, filesWrong);
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

        static Dictionary<string, List<OldenExecution>> OldenExecutions = new();
        static void LoadOldenInputs()
        {
            var path = @"C:\Users\alexd\Desktop\Slicer\Olden\oldenExecutions.csv";
            var allLines = System.IO.File.ReadAllLines(path);
            foreach (var line in allLines)
            {
                var s = line.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                List<OldenExecution> l = null;
                if (!OldenExecutions.TryGetValue(s[0].ToLower(), out l))
                {
                    l = new List<OldenExecution>();
                    OldenExecutions[s[0].ToLower()] = l;
                }
                l.Add(GetFromLine(s));
            }

            static OldenExecution GetFromLine(string[] s)
            {
                var l = new OldenExecution();
                l.Name = s[0];
                for (var i = 1; i < s.Length; i++)
                {
                    var x = s[i].Substring(1);
                    var y = x.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var input = new OldenInput();
                    input.Name = y[0];
                    input.Value = Convert.ToInt32(y[1]);
                    l.Inputs.Add(input);
                }
                return l;
            }
        }

        static void ExecuteConfiguration(string pathOrFileName, UserConfiguration userConfiguration, List<string> filesOK, List<string> filesWrong)
        {
            if (addSkippedFilesResult)
                userConfiguration.results.skippedFilesInfo = "SkippedFilesInfo.txt";

            if (addExecutedStmtWithoutHeaders)
                userConfiguration.results.executedLinesFileWithoutHeadersForUser = "ExecutedLinesWithoutHeadersForUser.txt";

            if (addExecutionCounters)
                userConfiguration.results.executionCounters = "ExecutionCounters.txt";

            // If this is false, keep the previous value (even if it's true)
            if (staticModeEnabled)
                userConfiguration.customization.staticMode = true;

            if (userConfiguration.FoldersToSkip == null && foldersToSkip != null && excludedFilesEnabled)
                userConfiguration.FoldersToSkip = foldersToSkip.Select(x => new UserConfiguration.Folder() { name = x }).ToArray();

            Console.WriteLine("Processing: " + pathOrFileName);
            Console.WriteLine("Trace file: " + (userConfiguration.criteria.fileTraceInputPath ?? "--"));

            var userInteractionCriteria = false;
            var userInteractionOutput = false;
            var local_mode = ConsoleMode.Default;
            
            if (excludedFilesEnabled)
                foreach (var targetProject in userConfiguration.targetProjects.excluded)
                    for (var i = 0; i < targetProject.files?.Length; i++)
                        targetProject.files[i].skip = (excludedFolders.Any(x => targetProject.files[i].name.Contains(x)));

            if (justThisFolders)
            {
                var folders = LayerOneDic[Path.GetDirectoryName(pathOrFileName)];
                foreach (var targetProject in userConfiguration.targetProjects.excluded)
                    for (var i = 0; i < targetProject.files?.Length; i++)
                    {
                        if (targetProject.files[i].name == @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Compilation\CSharpCompilation.cs")
                            ;
                        targetProject.files[i].skip = !(folders.Any(x => targetProject.files[i].name.Contains(x)));
                    }
            }

            if (Globals.include_all_uses.HasValue && userConfiguration.customization != null)
                userConfiguration.customization.includeAllUses = Globals.include_all_uses.Value;

            if (defaultOrchestrator == null || !useSameCompilation)
                defaultOrchestrator = new Orchestrator(userConfiguration);
            else
                defaultOrchestrator.Reset(userConfiguration, justThisFolders);

            defaultOrchestrator.UserInteraction = false;

            // Si el modo es normal solicitamos criterio. Caso contrario debemos seleccionar en el browser.
            #region Selección Line Modo interactivo
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
                Console.WriteLine("WRONG: " + pathOrFileName + ": " + ex.Message);
            }

            if (userInteractionOutput || userConfiguration.IsLoadedResult)
            {
                //throw new NotImplementedException();
                //if (userConfiguration.criteria.mode != UserConfiguration.Criteria.CriteriaMode.Normal)
                //{
                var reducedCompleteDG = defaultOrchestrator.GetReducedDependencyGraph();
                var reducedSliceDG = defaultOrchestrator.GetReducedSliceDependencyGraph();


                var completeDG = defaultOrchestrator.CompleteDependencyGraph;

                //TODONET6
                //DynAbs.DesktopApp.Browser.ComplexBrowser.Run(
                //    defaultOrchestrator.UserSolution,
                //    reducedSliceDG,
                //    defaultOrchestrator.InstrumentationResult.fileIdToSyntaxTree,
                //    defaultOrchestrator.InstrumentationResult.IdToFileDictionary,
                //    defaultOrchestrator.ExecutedStmts);

                //}
                //else
                //    DynAbs.DesktopApp.Browser.ComplexBrowser.SliceBrowser.Run(
                //        orchestrator.ExecutedStmts, 
                //        orchestrator.InstrumentationResult.IdToFileDictionary, 
                //        orchestrator.InstrumentationResult.fileIdToSyntaxTree, 
                //        orchestrator.SlicedStmts.First(), 
                //        orchestrator.CompleteDependencyGraph, 
                //        null);
            }
        }

        static Dictionary<string, List<string>> LayerOneDic = new Dictionary<string, List<string>>()
        {
            // ROSLYN
            {
                @"C:\Users\alexd\Desktop\Slicer\Roslyn\config\aut\BindingTests", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\eng\config\test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Binder"
                }
            },
            {
                @"C:\Users\alexd\Desktop\Slicer\Roslyn\config\aut\CompilationEmitTests", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\eng\config\test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Compilation",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Compiler",

                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\EditAndContinue",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\NoPia",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\TypeMemberReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SpecializedNestedTypeReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SpecializedMethodReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SpecializedGenericNestedTypeInstanceReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SpecializedGenericMethodInstanceReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SpecializedFieldReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\PENetModuleBuilder.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\PEModuleBuilder.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\PEAssemblyBuilder.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\ParameterTypeInformation.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\NamedTypeReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\ModuleReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\MethodReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\GenericTypeInstanceReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\GenericNestedTypeInstanceReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\GenericNamespaceTypeInstanceReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\GenericMethodInstanceReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\ExpandedVarargsMethodReference.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\AssemblyReference.cs",
                }
            },
            {
                @"C:\Users\alexd\Desktop\Slicer\Roslyn\config\aut\FlowTests", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\eng\config\test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\FlowAnalysis"
                }
            },
            {
                @"C:\Users\alexd\Desktop\Slicer\Roslyn\config\aut\PDBTests", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\eng\config\test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Binder",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Symbols",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\SymbolDisplay",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Compilation\CSharpCompilation.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\TypeParameterSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SynthesizedPrivateImplementationDetailsStaticConstructor.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SourceAssemblySymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\PropertySymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\PointerTypeSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\ParameterSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\NamespaceSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\NamedTypeSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\MethodSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\FunctionPointerTypeSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\FieldSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\EventSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\CustomModifierAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\AttributeDataAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\ArrayTypeSymbolAdapter.cs",
                }
            },
            {
                @"C:\Users\alexd\Desktop\Slicer\Roslyn\config\aut\StatementParsingTests", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\eng\config\test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Parser",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Syntax"
                }
            },
            {
                @"C:\Users\alexd\Desktop\Slicer\Roslyn\config\aut\TypeTests", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\eng\config\test",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Binder",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Symbols",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\SymbolDisplay",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Compilation\CSharpCompilation.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\TypeParameterSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SynthesizedPrivateImplementationDetailsStaticConstructor.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SourceAssemblySymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\PropertySymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\PointerTypeSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\ParameterSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\NamespaceSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\NamedTypeSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\SymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\MethodSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\FunctionPointerTypeSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\FieldSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\EventSymbolAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\CustomModifierAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\AttributeDataAdapter.cs",
                    @"C:\Users\alexd\Desktop\Slicer\Roslyn\src\src\Compilers\CSharp\Portable\Emitter\Model\ArrayTypeSymbolAdapter.cs",
                }
            },

            #region Powershell by functionality
            // POWERSHELL (by functionality)
            //{ 
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\Binders", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\runtime"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\CorePsPlatform", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\CoreCLR"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\ExtensionMethods", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\utils",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\Utils.cs",
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\FileSystemProvider", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\namespaces",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\SessionStateNavigation.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\MshSnapinInfo", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\singleshell"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\NamedPipe", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\remoting"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\PowerShellAPI", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\hostifaces",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\MshObject.cs",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\AutomationNull.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\PSConfiguration", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\PSConfiguration.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\PSObject", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\MshObject.cs",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\MshMemberInfo.cs",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\AutomationNull.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\PSVersionInfo", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\Runspace", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\hostifaces",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\runtime",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\lang\scriptblock.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\SecuritySupport", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\security"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\SessionState", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\Utils", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\Utils.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\WildcardPattern", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\regex.cs"
            //    }
            //}
            #endregion

            #region Powershell one file
            // POWERSHELL (by first file)
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\Binders", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\runtime\Binding\Binders.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\CorePsPlatform", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\CoreCLR\CorePsPlatform.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\ExtensionMethods", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\utils\ExtensionMethods.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\FileSystemProvider", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\namespaces\FileSystemProvider.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\MshSnapinInfo", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\singleshell\config\MshSnapinInfo.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\NamedPipe", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\remoting\common\RemoteSessionNamedPipe.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\PowerShellAPI", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\hostifaces\PowerShell.cs",
            //    }
            //},
            {
                @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\PSConfiguration", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
                    @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\PSConfiguration.cs",
                    //@"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\Utils.cs"
                }
            },
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\PSObject", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\AutomationNull.cs",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\MshObject.cs",
            //        //@"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\MshMemberInfo.cs",
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\PSVersionInfo", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\PSVersionInfo.cs"
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\Runspace", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\hostifaces\LocalConnection.cs",
            //    }
            //},
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\SecuritySupport", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\security\SecuritySupport.cs"
            //    }
            //},
            {
                @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\SessionState", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
                    @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\SessionState.cs"
                    //@"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\SessionStateDriveAPIs.cs",
                    //@"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\InitialSessionState.cs",
                }
            },
            {
                @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\Utils", new List<string>()
                {
                    @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
                    @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\Utils.cs"
                }
            },
            //{
            //    @"C:\Users\alexd\Desktop\Slicer\Powershell\config\tests\WildcardPattern", new List<string>()
            //    {
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\test\xUnit",
            //        @"C:\Users\alexd\Desktop\Slicer\Powershell\src\src\System.Management.Automation\engine\regex.cs"
            //    }
            //}
        #endregion
        };

        #region AuxOlden
        public class OldenExecution
        {
            public string Name { get; set; }
            public List<OldenInput> Inputs { get; set; } = new List<OldenInput>();

            public override bool Equals(object x) =>
                ((OldenExecution)x).Name == this.Name &&
                ((OldenExecution)x).Inputs.TrueForAll(y => this.Inputs.Any(z => y.Equals(z)));

            public override int GetHashCode() => this.Inputs.Sum(x => x.GetHashCode() / 1000);
        }
        public class OldenInput
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public int MinValue { get; set; }
            public int MaxValue { get; set; }

            public override bool Equals(object x) => ((OldenInput)x).Name == this.Name && ((OldenInput)x).Value == this.Value;

            public override int GetHashCode() => this.Name.GetHashCode() + this.Value.GetHashCode();
        }
        #endregion
    }
}
