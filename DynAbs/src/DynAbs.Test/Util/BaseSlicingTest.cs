using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CSharp.RuntimeBinder;
using Xunit;
using static DynAbs.UserConfiguration;

namespace DynAbs.Test.Util
{
    public abstract class BaseSlicingTest
    {
        public void TestSimpleSlice(Type type, DynAbsResult expectedResult, ExcludedProject[] excluded = null)
        {
            var rndNumber = new Random().Next(10000);
            var outputDir = Path.GetTempPath() + string.Format("slicer_{0}_output", rndNumber);
            var projectDir = Path.GetTempPath() + string.Format("slicer_{0}_project", rndNumber);
            try
            {
                string baseCaseDir = Path.GetFullPath(Directory.GetCurrentDirectory() + @"\..\..\..\Cases\");
                string classFile = Directory.GetFiles(baseCaseDir, type.Name + ".cs", SearchOption.AllDirectories).First();

                var extraLine = GetExtraLine(classFile, type.Name);
                if (extraLine != null)
                    expectedResult.Sliced.Add(SameFileStmtBuilder(type).WithLine(extraLine.Value));

                // Nuestra implementación de listas
                string baseCaseDirList = Path.GetFullPath(Directory.GetCurrentDirectory() + @"\..\..\..\Util\");
                string classFileList = Directory.GetFiles(baseCaseDirList, "SlicerList" + ".cs", SearchOption.AllDirectories).First();

                //Directory.CreateDirectory(outputDir);

                var dotnet = true;
                Solution solution = null;
                string solutionFilePath = null;
                if (dotnet)
                {
                    solutionFilePath = CreateSolution(projectDir, "TestProject", 
                        new HashSet<string>() { classFile, classFileList });
                    //var msBuildWorkspace = MSBuildWorkspace.Create();
                    //solution = msBuildWorkspace.OpenSolutionAsync(solutionFilePath).Result;
                }
                else
                {
                    var msCorLibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                    var linqReference = MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location);
                    // Para por ejemplo, el test de dynamic
                    var CSharpReference = MetadataReference.CreateFromFile(typeof(RuntimeBinderException).Assembly.Location);
                    // FORMULARIOS
                    var systemComponentModel = MetadataReference.CreateFromFile(typeof(System.ComponentModel.BrowsableAttribute).Assembly.Location);
                    var drawingReference = MetadataReference.CreateFromFile(typeof(System.Drawing.Size).Assembly.Location);
                    //var formsReference = MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Form).Assembly.Location);
                    // LIBRERÍA EXTERNA
                    var ExternalLibraryExampleReference = MetadataReference.CreateFromFile(typeof(ExternalLibraryExample.Binary).Assembly.Location);
                    var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TestProject", "TestProject",
                        LanguageNames.CSharp,
                        filePath: Path.Combine(projectDir, "TestProject.csproj"),
                        metadataReferences: new List<MetadataReference> {
                        msCorLibReference,
                        linqReference,
                        CSharpReference,
                        systemComponentModel,
                        drawingReference,
                        //formsReference, TODONET6
                        ExternalLibraryExampleReference });

                    var workspace = new AdhocWorkspace();
                    var newProject = workspace.AddProject(projectInfo);
                    workspace.AddDocument(newProject.Id, type.Name + ".cs", SourceText.From(File.ReadAllText(classFile)));
                    workspace.AddDocument(newProject.Id, "SlicerList.cs", SourceText.From(File.ReadAllText(classFileList)));

                    solution = workspace.CurrentSolution;
                }
                

                var test_static_mode = false;
                var solver_mode = MemoryModelKind.Clusters;
                var use_summaries = solver_mode == MemoryModelKind.Annotations || solver_mode == MemoryModelKind.Mixed || solver_mode == MemoryModelKind.Clusters;

                Orchestrator orchestrator = new Orchestrator(
                    new UserConfiguration
                    {
                        solutionFiles = new UserConfiguration.SolutionFiles()
                        {
                            solutionPath = solutionFilePath,
                            executableProject = "TestProject",
                            compilationOutputFolder = outputDir,
                            instrumentedSolutionPath = null
                        },
                        criteria = new UserConfiguration.Criteria()
                        {
                            mode = UserConfiguration.Criteria.CriteriaMode.Normal,
                            lines = new UserConfiguration.Criteria.FileLine[] { new DynAbs.UserConfiguration.Criteria.FileLine() { file = expectedResult.Criteria.FileName, line = expectedResult.Criteria.Line } },
                            runAutomatically = true,
                            fileTraceInput = true
                            // Actualmente el programa guarda acá @"C:\temp\trace.txt"
                            //fileTraceInputPath = @"C:\temp\trace.txt"
                        },
                        customization = new UserConfiguration.Customization()
                        {
                            summaries = @"C:\temp\configurations\Annotations.xml",
                            includeControlDependencies = true,
                            includeAllUses = false,
                            staticMode = test_static_mode,
                            loopsOptimization = test_static_mode,
                            memoryModel = solver_mode
                        },
                        results = new UserConfiguration.Results()
                        {
                            pointsToGraphDot = @"C:\temp\PointsTo.dot"
                            ,dependencyGraphDot = @"C:\temp\DependencyGraph.dot"
                            ,callGraphPath = @"C:\temp\callGraph.dgml"
                            /*mainResultsFolder = @"C:\temp\SlicerTests\Results",
                            ,sliceDependenciesGraphFolder = @"C:\temp\SlicerTests\Results\"*/
                        },
                        targetProjects = new TargetProjects() { defaultMode = ExcludedMode.None, excluded = excluded }
                    },
                    solution
                    //,typeof(ExternalLibraryExample.Binary)
                ) { UserInteraction = false };

                orchestrator.Orchestrate();

                var slicedStmts = new HashSet<Stmt>(orchestrator.SlicedStmts.Any() ? orchestrator.SlicedStmts.First().Where(x => expectedResult is DynAbsResultWithSkipFile ? Path.GetFileName(x.FileName) != ((DynAbsResultWithSkipFile)expectedResult).SkipFile : true) : new Stmt[] { });

                if (test_static_mode)
                    foreach (var s in expectedResult.StaticModeExtraLines)
                        expectedResult.Sliced.Add(s);
                if (!use_summaries)
                    foreach (var s in expectedResult.WithoutSummariesExtraLines)
                        expectedResult.Sliced.Add(s);

                Assert.NotNull(orchestrator.SlicedStmts);
                Assert.NotNull(expectedResult.Sliced);

                var sliceResult = slicedStmts.Select(x => x.FileId + "." + x.Line);
                var expectedResultSet = new HashSet<string>();
                foreach (var expectedFile in expectedResult.Sliced)
                {
                    var expFileId = orchestrator.InstrumentationResult.FileToIdDictionary.Where(x => x.Key.Contains(expectedFile.FileName)).First().Value;
                    expectedResultSet.Add(expFileId + "." + expectedFile.Line);
                }
                try
                {
                    Assert.Equal(expectedResult.Sliced.Count, slicedStmts.Count);
                    Assert.True(AreSetsEquivalent(expectedResultSet, sliceResult));
                }
                catch (Exception)
                {
                    // Lo que buscamos acá es ver las diferencias
                    // 1. Lo que esperabamos y no está
                    var expectedNotFound = expectedResultSet.Except(sliceResult).Select(x =>
                        orchestrator.InstrumentationResult.IdToFileDictionary[Convert.ToInt32(x.Substring(0, x.IndexOf('.')))] + '.' + x.Substring(x.IndexOf('.') + 1, x.Length - x.IndexOf('.') - 1));
                    var resultNotExpected = sliceResult.Except(expectedResultSet).Select(x =>
                        orchestrator.InstrumentationResult.IdToFileDictionary[Convert.ToInt32(x.Substring(0, x.IndexOf('.')))] + '.' + x.Substring(x.IndexOf('.') + 1, x.Length - x.IndexOf('.') - 1));

                    var sb = new System.Text.StringBuilder();
                    if (expectedNotFound.Any())
                        sb.AppendLine("Expected not found:");
                    foreach (var enf in expectedNotFound)
                        sb.AppendLine(enf);
                    if (resultNotExpected.Any())
                        sb.AppendLine("In slice but not expected:");
                    foreach (var rnf in resultNotExpected)
                        sb.AppendLine(rnf);
                    throw new Exception(sb.ToString());
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                // HACK-- Cleanup.May throw errors so will need to retry several times
                DeleteDirectory(outputDir);
                DeleteDirectory(projectDir);
            }
        }

        void DeleteDirectory(string directoryName)
        {
            bool stillExists;
            do
            {
                stillExists = Directory.Exists(directoryName);
                try
                {
                    Directory.Delete(directoryName, true);
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
            while (stillExists);
        }

        bool AreStmtSetsEquivalent(IEnumerable<Stmt> a, IEnumerable<Stmt> b)
        {
            return new HashSet<Stmt>(a.Union(b), new StmtFileNameAndLineEqualityComparer()).Count == a.Count();
        }

        bool AreSetsEquivalent(IEnumerable<string> a, IEnumerable<string> b)
        {
            var hash = new HashSet<string>(a);
            hash.SymmetricExceptWith(b);

            return hash.Count == 0;
        }

        public SameFileStmtBuilder SameFileStmtBuilder(string filename)
        {
            return new SameFileStmtBuilder(filename);
        }

        public SameFileStmtBuilder SameFileStmtBuilder(Type type)
        {
            return new SameFileStmtBuilder(type.Name + ".cs");
        }

        string CreateSolution(string workspaceDirectory, string projectName, ISet<string> filesToCompile)
        {
            if (Directory.Exists(workspaceDirectory))
                Directory.Delete(workspaceDirectory, true);
            Directory.CreateDirectory(workspaceDirectory);

            //var commands = new List<string>()
            //{
            //    "new sln -n " + projectName,
            //    "new console -n " + projectName + " -f netcoreapp3.1",
            //    "sln " + projectName + ".sln add " + projectName + "\\" + projectName + ".csproj"
            //};

            //foreach (var command in commands)
            //    DynAbs.Utils.RunDotnet(workspaceDirectory, command);

            //var projectDirectory = Path.Combine(workspaceDirectory, projectName);

            //File.Delete(Path.Combine(projectDirectory, "Program.cs"));

            //foreach (var file in filesToCompile)
            //    File.Copy(file, Path.Combine(projectDirectory, Path.GetFileName(file)));

            //commands = new List<string>() { "build", "run" };
            //foreach (var command in commands)
            //{
            //    var temp = DynAbs.Utils.RunDotnet(workspaceDirectory, command);
            //    Console.WriteLine(temp);
            //}

            var projectFilePath = Path.Combine(workspaceDirectory, projectName + ".csproj");
            var solutionFilePath = Path.Combine(workspaceDirectory, projectName + ".sln");

            foreach (var file in filesToCompile)
                File.Copy(file, Path.Combine(workspaceDirectory, Path.GetFileName(file)));

            var extLib = typeof(ExternalLibraryExample.Binary).Assembly.Location;
            var extLibName = Path.GetFileName(extLib);
            File.Copy(extLib, Path.Combine(workspaceDirectory, extLibName));

            CreateSolutionFile(solutionFilePath, projectName);
            CreateProjectFile(projectFilePath, extLibName);

            return Path.Combine(workspaceDirectory, projectName + ".sln");
        }

        void CreateProjectFile(string projectFilePath, string externalLib)
        {
            var projectContent = string.Format(@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|AnyCPU'"">
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""ExternalLibraryExample"">
      <HintPath>{0}</HintPath>
    </Reference>
  </ItemGroup>
</Project>", externalLib);
            File.WriteAllText(projectFilePath, projectContent);
        }

        void CreateSolutionFile(string solutionFilePath, string projectName)
        {
            var projectFileName = projectName + ".csproj";
            var solutionContent = string.Format(
            @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 15
VisualStudioVersion = 15.0.26124.0
MinimumVisualStudioVersion = 15.0.26124.0
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{0}"", ""{1}"", ""{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Debug|x64 = Debug|x64
		Debug|x86 = Debug|x86
		Release|Any CPU = Release|Any CPU
		Release|x64 = Release|x64
		Release|x86 = Release|x86
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Debug|x64.ActiveCfg = Debug|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Debug|x64.Build.0 = Debug|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Debug|x86.ActiveCfg = Debug|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Debug|x86.Build.0 = Debug|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Release|x64.ActiveCfg = Release|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Release|x64.Build.0 = Release|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Release|x86.ActiveCfg = Release|Any CPU
		{{2563B0C3-1A9A-4163-B7F2-6B6BF773B759}}.Release|x86.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
", projectName, projectFileName);
            File.WriteAllText(solutionFilePath, solutionContent);
        }

        int? GetExtraLine (string classFile, string typeName)
        {
            var allLines = File.ReadAllLines(classFile).ToList();
            for (var i = 0; i < allLines.Count; i++)
                if (allLines[i].Contains("class " + typeName))
                    return i + 1;
            return null;
        }
    }

    public class SameFileStmtBuilder
    {
        private readonly string _filename;

        public SameFileStmtBuilder(string filename)
        {
            _filename = filename;
        }

        public Stmt WithLine(int line)
        {
            return new Stmt {FileName = _filename, Line = line};
        }
    }
}
