using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs
{
    public class DynAbsSlicer
    {
        string ProgramName { get; set; }
        string Arguments { get; set; }
        string TraceFile { get; set; }

        UserConfiguration UserConfiguration { get; set; } = null;
        Orchestrator Orchestrator { get; set; } = null;
        MainTraceConsumer MainTraceConsumer { get; set; } = null;

        public DynAbsSlicer(string InstanceName)
        {
            ProgramName = InstanceName;
        }

        public void Instrument(string solutionPath, string executableProject, string outputPath)
        {
            UserConfiguration = new UserConfiguration();
            UserConfiguration.solutionFiles = new UserConfiguration.SolutionFiles()
            {
                solutionPath = solutionPath,
                compilationOutputFolder = outputPath,
                instrumentedSolutionPath = null,
                executableProject = executableProject
            };
            UserConfiguration.criteria = new UserConfiguration.Criteria()
            {
                fileTraceInput = true,
                mode = UserConfiguration.Criteria.CriteriaMode.Normal,
                notCompile = false,
                onlyOverrideCodeFiles = false,
                runAutomatically = false
            };
            UserConfiguration.customization = new UserConfiguration.Customization()
            {
                includeAllUses = true,
                includeControlDependencies = true
            };
            UserConfiguration.results = new UserConfiguration.Results()
            {
                resultFile = "Results.txt",
                summaryResultFile = "Summary.txt",
                executedLinesFileForUser = "Executed.txt"
            };
            Orchestrator = new Orchestrator(UserConfiguration);
        }

        public void Run(string args, string traceFile)
        {
            Arguments = args;
            TraceFile = traceFile;
            Orchestrator.Run(args, traceFile);
        }

        public void Slice(string traceFile, string fileToSlice, int lineToSlice)
        {
            var linesToSlice = new UserConfiguration.Criteria.FileLine[] 
            { 
                new UserConfiguration.Criteria.FileLine()
                {
                    file = fileToSlice,
                    line = lineToSlice
                }
            };

            MainTraceConsumer = Orchestrator.Slice(traceFile, linesToSlice);
        }

        public void SaveResults(string outputFolder)
        {
            Orchestrator.InitializeOutputFolder(outputFolder, ProgramName, Arguments);
            Orchestrator.SaveResults(MainTraceConsumer, 1, ProgramName, Arguments, TraceFile, false);
        }
    }
}
