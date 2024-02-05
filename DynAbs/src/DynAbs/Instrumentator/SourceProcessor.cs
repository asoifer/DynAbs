using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public interface ISourceProcessor
    {
        CSharpCompilation Process(CSharpCompilation compilation, string projectName);
        Dictionary<int, string> FilesIds();
        Dictionary<int, string> LastInstrumentedFileIds();

        ISet<string> InstrumentedFiles { get; } 
    }
}
