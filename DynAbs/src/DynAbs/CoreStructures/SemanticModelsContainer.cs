using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DynAbs
{
    public interface ISemanticModelsContainer
    {
        SemanticModel GetBySyntaxNode(CSharpSyntaxNode node);
    }

    public class SemanticModelsContainer : ISemanticModelsContainer
    {
        IDictionary<string, SemanticModel> _cachedSemanticModelDictionary;
        InstrumentationResult _instrumentationResult;

        public SemanticModelsContainer(InstrumentationResult instrumentationResult)
        {
            _instrumentationResult = instrumentationResult;
            _cachedSemanticModelDictionary = new Dictionary<string, SemanticModel>();
        }

        public SemanticModel GetBySyntaxNode(CSharpSyntaxNode node)
        {
            string lookup = node.SyntaxTree.FilePath;
            if (_cachedSemanticModelDictionary.ContainsKey(lookup)) 
                return _cachedSemanticModelDictionary[lookup];

            var semanticModel = _instrumentationResult.filePathToSemanticModel[lookup];
            _cachedSemanticModelDictionary.Add(lookup, semanticModel);

            return semanticModel;
        }
    }
}
