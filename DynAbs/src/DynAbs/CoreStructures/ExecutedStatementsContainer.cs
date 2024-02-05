using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public class ExecutedStatementsContainer
    {
        public ISet<Stmt> ExecutedStatements { get; internal set; }
        public ISet<Stmt> PropertiesOrFieldsInitialization { get; internal set; }
        public ISet<Stmt> ExecutedMethods { get; internal set; }
        public ISet<Stmt> ExecutedClasses { get; internal set; }
        public int ExecutedStatmentsCounter { get; internal set; }
        public int PropertiesOrFieldsInitializationCounter { get; internal set; }
        public int MethodsCounter { get; internal set; }
        public int ClassesCounter { get; internal set; }

        public ExecutedStatementsContainer()
        {
            ExecutedStatements = new HashSet<Stmt>();
            PropertiesOrFieldsInitialization = new HashSet<Stmt>();
            ExecutedMethods = new HashSet<Stmt>();
            ExecutedClasses = new HashSet<Stmt>();
            ExecutedStatmentsCounter = 0;
            PropertiesOrFieldsInitializationCounter = 0;
            MethodsCounter = 0;
            ClassesCounter = 0;
        }

        public void Add(Stmt stmt)
        {
            ExecutedStatmentsCounter++;
            ExecutedStatements.Add(stmt);
        }

        public void AddPropertyOrFieldInitialization(Stmt stmt)
        {
            PropertiesOrFieldsInitializationCounter++;
            PropertiesOrFieldsInitialization.Add(stmt);
        }

        public void AddMethod(Stmt stmt)
        {
            MethodsCounter++;
            ExecutedMethods.Add(stmt);
        }

        public void AddClass(Stmt stmt)
        {
            ClassesCounter++;
            ExecutedClasses.Add(stmt);
        }

        /// <summary>
        /// Incluye líneas internas a los métodos y properties/fields initialization
        /// </summary>
        public int DistinctExecutedStatements
        {
            get
            {
                return ExecutedStatements.Select(x => new { FileId = x.FileId, Line = x.Line }).Distinct().Count();
            }
        }

        public int DistinctPropertiesOrFieldsInitialization
        {
            get
            {
                return PropertiesOrFieldsInitialization.Select(x => new { FileId = x.FileId, Line = x.Line }).Distinct().Count();
            }
        }

        public int DistinctExecutedMethods
        {
            get
            {
                return ExecutedMethods.Select(x => new { FileId = x.FileId, Line = x.Line }).Distinct().Count();
            }
        }

        public int DistinctExecutedClasses
        {
            get
            {
                return ExecutedClasses.Select(x => new { FileId = x.FileId, Line = x.Line }).Distinct().Count();
            }
        }

        public int DistinctExecutedLines
        {
            get
            {
                return DistinctExecutedStatements + DistinctExecutedMethods + DistinctExecutedClasses;
            }
        }
    }
}
