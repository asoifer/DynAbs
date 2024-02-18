using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class UserSliceConfiguration
    {
        public UserConfiguration User { get; internal set; }
        public bool IncludeControlDependencies { get; internal set; }
        public bool UseAnnotations { get; internal set; }
        public bool MixedModes { get; internal set; }
        public bool LogCalls { get; internal set; }
        public bool IncludeAllUses => User?.customization?.includeAllUses ?? true;

        public bool StaticModeEnabled { get; internal set; }
        public bool LoopsOptimizationEnabled { get; internal set; }
        public bool ForeachAnnotation { get; internal set; }
        public bool TypesOptimization { get; internal set; }
        public HashSet<string> FoldersToSkip { get; set; }
        public HashSet<int> FilesToSkip { get; set; } = null;

        public UserSliceConfiguration(UserConfiguration userConfiguration)
        {
            User = userConfiguration;
            IncludeControlDependencies = userConfiguration.customization?.includeControlDependencies ?? true;
            UseAnnotations = userConfiguration.customization != null && (User.customization.memoryModel == UserConfiguration.MemoryModelKind.Annotations || userConfiguration.customization.memoryModel == UserConfiguration.MemoryModelKind.Mixed || userConfiguration.customization.memoryModel == UserConfiguration.MemoryModelKind.Clusters);
            MixedModes = userConfiguration.customization != null ? userConfiguration.customization.memoryModel == UserConfiguration.MemoryModelKind.Mixed || userConfiguration.customization.memoryModel == UserConfiguration.MemoryModelKind.Clusters : false;
            LogCalls = userConfiguration.results?.logMethodsCalls ?? false;
            StaticModeEnabled = userConfiguration.customization?.staticMode ?? false;
            LoopsOptimizationEnabled = userConfiguration.customization?.loopsOptimization ?? false;
            ForeachAnnotation = UseAnnotations;
            TypesOptimization = UseAnnotations;
            if (userConfiguration.FoldersToSkip != null)
                FoldersToSkip = new HashSet<string>(userConfiguration.FoldersToSkip.Select(x => x.name));

            Globals.generate_dgs = !string.IsNullOrWhiteSpace(userConfiguration?.results?.sliceDependenciesGraphFolder ?? "");
        }
    }
}
