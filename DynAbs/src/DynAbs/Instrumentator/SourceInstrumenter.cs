using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System;
using System.Linq;
using System.Text;

namespace DynAbs
{
    public class SourceInstrumenter : ISourceProcessor
    {
        UserSliceConfiguration _configuration;
        Dictionary<int, string> fileIdAssoc = new Dictionary<int, string>();
        Dictionary<int, string> lastInstrumentedFileIdAssoc;
        Dictionary<string, int> pathToIdAssoc = new Dictionary<string, int>();
        ISet<string> files = new HashSet<string>();
        Dictionary<string, bool> filesSkipInfo = new Dictionary<string, bool>();
        Dictionary<string, int> predefinedIds = new Dictionary<string, int>();
        UserConfiguration.ExcludedMode mode = UserConfiguration.ExcludedMode.None;
        int fileId = 0;
        ISet<string> referencesToAdd;
        public ISet<string> InstrumentedFiles { get; } = new HashSet<string>();

        public SourceInstrumenter(UserSliceConfiguration userSliceConfiguration, ISet<string> referencesToAdd)
        {
            _configuration = userSliceConfiguration;
            this.referencesToAdd = referencesToAdd;
        }

        /*
         * Input: Original Compilation.
         * Output: Instrumented Compilation.
         * Idea: I call rewriter with the tree for that instrument. Replacement compilation with the new instrumented modified tree. Write in C:\temp\Instrumentado.cs the instrumented code
         */
        public CSharpCompilation Process(CSharpCompilation compilation, string projectName)
        {
            lastInstrumentedFileIdAssoc = new Dictionary<int, string>();
            var assemblyReferences = new List<MetadataReference>();
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var reference in referencesToAdd)
                using (var stream = assembly.GetManifestResourceStream(reference))
                    assemblyReferences.Add(MetadataReference.CreateFromStream(stream));
            var modifiedCompilation = compilation.AddReferences(assemblyReferences);

            var project = _configuration.User.targetProjects?.excluded?.FirstOrDefault(x => x.name == projectName);
            var defaultSkip = project?.skipDefault ?? false;
            if (project != null && project.files != null)
            {
                mode = project.mode;
                files.UnionWith(project.files.Select(x => x.name));
                foreach (var file in project.files)
                {
                    if (file.skip.HasValue)
                        filesSkipInfo[file.name] = file.skip.Value;
                    if (file.id > 0 && !predefinedIds.ContainsKey(file.name))
                        predefinedIds[file.name] = file.id;
                }
            }

            if (project != null && project.initialFileID > 0)
                fileId = project.initialFileID-1;

            foreach (var tree in compilation.SyntaxTrees)
            {
                //Evita instrumentar de mas
                if (IgnoreSourceFile(tree.FilePath))
                    continue;          
                AssocIdToFile(tree.FilePath);
            }

            BugLogging.Clean();
            var excludedClassesAndMethods = _configuration.User.GetExcludedClasses(projectName);
            var allowedClasses = _configuration.User.GetAllowedClasses(projectName);
            foreach (var tree in compilation.SyntaxTrees)
            {
                //Evita instrumentar de mas
                if (IgnoreSourceFile(tree.FilePath) || SkipFile(tree.FilePath, defaultSkip))
                    continue;          
                SemanticModel model = compilation.GetSemanticModel(tree);
                int idBelongingToThisPath = pathToIdAssoc[tree.FilePath];

                var rewriter = new InstrumenterRewriter(idBelongingToThisPath, compilation, tree, 
                    pathToIdAssoc, excludedClassesAndMethods, allowedClasses, this.IsBeingAnalized);
                try
                {
                    var newRoot = (CSharpSyntaxNode)rewriter.Visit(tree.GetRoot());
                    var st = CSharpSyntaxTree.ParseText(newRoot.ToFullString());
                    var instrumentedTree = (CSharpSyntaxTree)tree.WithRootAndOptions(st.GetRoot(), tree.Options);
                    #if DEBUG
                    File.WriteAllText(Path.Combine(Globals.TempPath, "Instrumented.cs"), instrumentedTree.ToString());
                    #endif
                    modifiedCompilation = modifiedCompilation.ReplaceSyntaxTree(tree, instrumentedTree);
                    InstrumentedFiles.Add(tree.FilePath);
                }
                catch (Exception e)
                {
                    Logger.Error("Excepcion en instrumentacion: " + e.ToString());
                    //throw e;
                }
            }

            //var dict = InstrumenterRewriter.notSupportedExpressions;
            //foreach (var kv in dict)
            //    foreach (var l in kv.Value)
            //        Console.WriteLine(kv.Key + " - " + l);

            var sb = new StringBuilder();
            //var dictSkippedMethods = InstrumenterRewriter.skippedMethods;
            //foreach (var kv in dictSkippedMethods)
            //    foreach (var l in kv.Value)
            //        sb.AppendLine(kv.Key + " - " + l);

            //if (sb.Length > 0)
            //{
            //    Console.WriteLine("Skipped methods: ");
            //    Console.WriteLine(sb.ToString());
            //}

            //sb.Clear();
            //var methodsToComplete = InstrumenterRewriter.methodsToComplete;
            //foreach (var kv in methodsToComplete)
            //{
            //    var fileName = kv.Key;
            //    sb.AppendLine(fileName);
            //    foreach (var kv2 in kv.Value)
            //    {
            //        sb.AppendLine();
            //        sb.AppendLine(kv2.Key);
            //        sb.AppendLine(kv2.Value);
            //    }
            //}

            //if (sb.Length > 0)
            //{
            //    Console.WriteLine("ToComplete: ");
            //    Console.WriteLine(sb.ToString());
            //}

            //ReplaceMethods(methodsToComplete);

            //BugLogging.Save();
            return modifiedCompilation;
        }

        void ReplaceMethods(Dictionary<string, Dictionary<string, string>> filesToReplace)
        {
            var sb = new StringBuilder();
            foreach (var kv in filesToReplace)
            {
                var currentFilePath = kv.Key.Replace(@"src\src", @"instrumented\src");
                sb.AppendLine(currentFilePath);

                var allLines = File.ReadAllLines(currentFilePath).ToList();
                foreach (var replace in kv.Value)
                {
                    var indexes = new List<int>();
                    for (var i = 0; i < allLines.Count; i++)
                    {
                        if (allLines[i].Trim() == replace.Key.Trim())
                            indexes.Add(i);
                    }

                    if (indexes.Count == 0)
                        continue;

                    var whereToInsert = indexes.First();
                    if (indexes.Count > 1)
                        ;

                    var methodToAdd = replace.Value.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None).ToList();
                    methodToAdd.Add("");

                    allLines.InsertRange(whereToInsert, methodToAdd);

                    sb.AppendLine("    " + replace.Key.Trim());
                }

                File.WriteAllLines(currentFilePath, allLines);
            }
            Console.WriteLine(sb.ToString());
        }

        /*
         *  Input: FilePath
         *  Output: false
         *  Idea: If exits AssemblyInfo.cs or AssemblyAtributes.cs files return true.
         */
        private bool IgnoreSourceFile(string filePath)
        {
            return  System.IO.Path.GetFileName(filePath) == "AssemblyInfo.cs" ||
                    System.IO.Path.GetFileName(filePath) == "AssemblyAttributes.cs" ||
                    (mode == UserConfiguration.ExcludedMode.Custom && files.Contains(filePath)) ||
                    (mode == UserConfiguration.ExcludedMode.AllExcept && !files.Contains(filePath));
        }

        private bool SkipFile(string filePath, bool defaultSkip)
        {
            if (filesSkipInfo.ContainsKey(filePath))
                return filesSkipInfo[filePath];
            return defaultSkip;
        }

        /*
         *  Input: FilePath
         *  Output: false
         *  Idea: Set dicctionaries with actual file id.
         */
        private void AssocIdToFile(string filePath)
        {
            if (!pathToIdAssoc.ContainsKey(filePath))
            {
                int id = 0;
                if (predefinedIds.ContainsKey(filePath))
                    id = predefinedIds[filePath];
                else
                {
                    id = ++fileId;
                    while (predefinedIds.Values.Contains(id))
                        id = ++fileId;
                }
                fileIdAssoc.Add(id, filePath);
                lastInstrumentedFileIdAssoc.Add(id, filePath);
                pathToIdAssoc.Add(filePath, id);
            }
        }

        public bool IsBeingAnalized(string filePath)
        {
            var ignored = IgnoreSourceFile(filePath);
            var inSkipList = filesSkipInfo.ContainsKey(filePath);
            return !ignored && inSkipList;
        }

        /*
         *  Input: 
         *  Output: fileIdAssoc
         *  Idea: return fileIdAssoc
         */
        public Dictionary<int, string> FilesIds()
        {
            return fileIdAssoc;
        }

        /*
         *  Input: 
         *  Output: lastInstrumentedFileIdAssoc
         *  Idea: returna lastInstrumentedFileIdAssoc
         */
        public Dictionary<int, string> LastInstrumentedFileIds()
        {
            return lastInstrumentedFileIdAssoc;
        }
    }
}