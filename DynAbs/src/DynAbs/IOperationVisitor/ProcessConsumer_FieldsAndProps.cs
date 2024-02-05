using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using DynAbs.Tracing;

namespace DynAbs
{
    public partial class ProcessConsumer
    {
        #region Properties
        // For identifying each ProcessConsumer
        public int processId = ++Globals.LastProcessConsumerId;

        UserSliceConfiguration _configuration;
        ITraceConsumer _traceConsumer;
        ISemanticModelsContainer _semanticModelsContainer;
        IBroker _broker;
        InstrumentationResult _instrumentationResult;
        TermFactory _termFactory;
        ExecutedStatementsContainer _executedStatements;
        public Term _thisObject { get; set; }
        public Term _returnObject { get; set; }
        public Term _returnExceptionTerm { get; set; }
        public Term ExceptionTerm { get; set; }
        public Term PatternOperationReceiver { get; set; }
        public bool _setReturn { get; set; }
        public bool _sliceCriteriaReached { get; set; }
        public bool _returnComplexType { get; set; }
        public bool _returnPostponed { get; set; }
        public bool _rv_assigned { get; set; }
        public bool _throwingException { get; set; }
        Stack<Term> _usingStack;
        Stmt _originalStatement;
        SyntaxNode _methodNode;
        int _currentFileId;
        IDictionary<CSharpSyntaxNode, Term> _foreachHubsDictionary = new Dictionary<CSharpSyntaxNode, Term>();
        IDictionary<Stmt, bool> _enterLoopSet = new Dictionary<Stmt, bool>();
        Term yieldReturnValuesContainer = null;
        Term temporaryConditionalAccessTerm = null;
        Term temporarySwitchTerm = null;
        IDictionary<ITypeSymbol, ITypeSymbol> _typeArguments { get; set; }
        #endregion

        #region Aux
        public static int lineNumber = 662;
        public static int fileId = 959;

        public static int spanStart = 2704; 
        public static int spanEnd = 2713;
        #region Exceptions
        static string[] ClosingMethods = new string[] { "Dispose", "Close", "RemoveTemporaryDirectory" };

        static List<Tuple<int, int, int>> methodsExceptions = new List<Tuple<int, int, int>>() {
            new Tuple<int, int, int>(1560, 30029, 30046),
            new Tuple<int, int, int>(10213, 3780, 3800),
            new Tuple<int, int, int>(661, 14086, 14106),
            new Tuple<int, int, int>(98, 1643, 1672),
            new Tuple<int, int, int>(105, 21473, 21490),
            new Tuple<int, int, int>(98, 2129, 2170),
            new Tuple<int, int, int>(98, 2309, 2344),
            new Tuple<int, int, int>(98, 1812, 1847),
            new Tuple<int, int, int>(420, 16194, 16256),
        };

        static List<Tuple<int, int, int>> structsExceptions = new List<Tuple<int, int, int>>() {
            new Tuple<int, int, int>(10071, 7149, 7223),
            new Tuple<int, int, int>(10077, 34345, 34421),
            new Tuple<int, int, int>(10030, 6397, 6422),
            new Tuple<int, int, int>(10040, 26366, 27083),
            new Tuple<int, int, int>(10073, 53010, 53078),
            new Tuple<int, int, int>(10073, 105776, 105890),
            new Tuple<int, int, int>(10319, 32472, 32517),
            new Tuple<int, int, int>(10064, 1017, 1102),
            new Tuple<int, int, int>(10030, 10447, 10459),
            new Tuple<int, int, int>(10030, 10472, 10484),
            new Tuple<int, int, int>(10052, 44037, 44091),
            new Tuple<int, int, int>(10073, 105920, 106034),
            new Tuple<int, int, int>(10052, 43043, 43108),
            new Tuple<int, int, int>(10286, 10292, 10329),
            new Tuple<int, int, int>(10319, 25009, 25038),
            new Tuple<int, int, int>(10002, 5805, 5831),
            new Tuple<int, int, int>(10270, 1727, 1749),
            new Tuple<int, int, int>(10049, 28367, 28426),
            new Tuple<int, int, int>(10287, 1860, 1890),
            new Tuple<int, int, int>(10345, 7237, 7259),
            new Tuple<int, int, int>(10135, 5308, 5355),
            new Tuple<int, int, int>(10053, 7033, 7107),
            new Tuple<int, int, int>(10273, 799, 824),
            new Tuple<int, int, int>(10073, 180496, 180586),
            new Tuple<int, int, int>(10003, 225778, 225807),
            new Tuple<int, int, int>(10071, 5415, 5469),
            new Tuple<int, int, int>(10073, 66493, 66515),
            new Tuple<int, int, int>(10073, 65800, 65857),
            new Tuple<int, int, int>(10056, 10750, 10805),
            new Tuple<int, int, int>(10395, 1534, 1587),
            new Tuple<int, int, int>(10064, 1600, 1638),
            new Tuple<int, int, int>(10268, 25407, 25433),
            new Tuple<int, int, int>(10268, 25461, 25483),
            new Tuple<int, int, int>(10039, 36167, 36190),
            new Tuple<int, int, int>(10226, 9711, 9821),
            new Tuple<int, int, int>(10225, 4791, 4899),
            new Tuple<int, int, int>(10269, 24666, 24713),
            new Tuple<int, int, int>(10194, 7416, 7529),
            new Tuple<int, int, int>(10218, 81555, 81624),
            new Tuple<int, int, int>(10401, 1406, 1463),
            new Tuple<int, int, int>(10255, 11945, 11973),
            new Tuple<int, int, int>(10318, 84425, 84448),
            new Tuple<int, int, int>(10119, 35089, 35159),
            new Tuple<int, int, int>(10232, 2336, 2356),
            new Tuple<int, int, int>(10073, 105998, 106112),
            new Tuple<int, int, int>(10319, 34367, 34396),
            new Tuple<int, int, int>(10176, 32069, 32081),
            new Tuple<int, int, int>(10250, 2095, 2139),
            new Tuple<int, int, int>(10176, 14969, 14984),
            new Tuple<int, int, int>(10002, 8373, 8445),
            new Tuple<int, int, int>(10261, 15825, 15876),
            new Tuple<int, int, int>(10073, 180574, 180664),
            new Tuple<int, int, int>(10010, 10345, 10366),
            new Tuple<int, int, int>(10218, 82961, 83028),
            new Tuple<int, int, int>(10203, 56959, 57054),
            new Tuple<int, int, int>(10225, 5084, 5166),
            new Tuple<int, int, int>(10073, 53101, 53155),
            new Tuple<int, int, int>(10318, 137557, 137580),
            new Tuple<int, int, int>(10318, 137583, 137603),
            new Tuple<int, int, int>(10032, 19638, 19659),
            new Tuple<int, int, int>(10272, 36403, 36435),
            new Tuple<int, int, int>(10272, 36438, 36471),
            new Tuple<int, int, int>(10073, 48481, 48503),
            new Tuple<int, int, int>(10579, 15417, 15437),
            new Tuple<int, int, int>(10073, 48481, 48555),
            new Tuple<int, int, int>(10707, 35191, 35218),
            new Tuple<int, int, int>(10698, 7170, 7179),
            new Tuple<int, int, int>(10715, 4533, 4568),
            new Tuple<int, int, int>(10300, 9341, 9408),
            new Tuple<int, int, int>(25019, 15126, 15167),
            new Tuple<int, int, int>(10440, 67802, 67861),
            new Tuple<int, int, int>(10712, 35075, 35102),
            new Tuple<int, int, int>(10310, 68728, 68834),
            new Tuple<int, int, int>(10709, 66482, 66618),
            new Tuple<int, int, int>(26001, 9811, 9823),
            new Tuple<int, int, int>(26001, 9913, 9925),
            new Tuple<int, int, int>(10781, 4589, 4614),
            new Tuple<int, int, int>(10781, 3960, 4039),
            new Tuple<int, int, int>(10713, 27564, 27581),
            new Tuple<int, int, int>(10073, 66571, 66593),
            new Tuple<int, int, int>(10073, 65878, 65935),
            new Tuple<int, int, int>(10047, 955, 999),
            new Tuple<int, int, int>(10159, 7698, 7739),
            new Tuple<int, int, int>(10286, 20078, 20100),
            new Tuple<int, int, int>(10874, 183141, 183160),
            new Tuple<int, int, int>(10899, 83436, 83450),
            new Tuple<int, int, int>(10159, 7698, 7739),
            new Tuple<int, int, int>(10159, 7698, 7790),
            new Tuple<int, int, int>(10128, 11653, 11735),
            new Tuple<int, int, int>(10286, 20381, 20403),
            new Tuple<int, int, int>(10065, 1352, 1390),
            new Tuple<int, int, int>(10957, 16894, 17003),
            new Tuple<int, int, int>(10899, 235789, 235814),
            new Tuple<int, int, int>(10849, 10749, 10772),
            new Tuple<int, int, int>(10849, 11336, 11357),
            new Tuple<int, int, int>(10874, 121095, 121126),
            new Tuple<int, int, int>(10874, 121220, 121251),
            new Tuple<int, int, int>(10899, 242659, 242691),
            new Tuple<int, int, int>(10899, 242694, 242727),
            new Tuple<int, int, int>(10318, 65992, 66019),
            new Tuple<int, int, int>(10314, 111604, 111633),
            new Tuple<int, int, int>(10899, 298030, 298098),
            new Tuple<int, int, int>(10899, 235816, 235878),
            new Tuple<int, int, int>(10899, 240197, 240228),
            new Tuple<int, int, int>(10967, 25027, 25056),
            new Tuple<int, int, int>(10319, 15782, 15866),
            new Tuple<int, int, int>(10319, 26411, 26453),
            new Tuple<int, int, int>(10889, 49027, 49051),
            new Tuple<int, int, int>(10899, 420505, 420528),
            new Tuple<int, int, int>(10889, 49027, 49051),
            new Tuple<int, int, int>(10899, 138225, 138277),
            new Tuple<int, int, int>(10462, 7620, 7674),
            new Tuple<int, int, int>(10035, 31260, 31266),
            new Tuple<int, int, int>(10899, 210245, 210259),
            new Tuple<int, int, int>(10462, 3943, 3983),
            new Tuple<int, int, int>(10462, 4597, 4619),
            new Tuple<int, int, int>(10751, 2779, 2796),
            new Tuple<int, int, int>(10035, 31260, 31266),
            new Tuple<int, int, int>(10591, 15533, 15551),
            new Tuple<int, int, int>(10591, 58469, 58496),
            new Tuple<int, int, int>(10314, 10344, 10369),
            new Tuple<int, int, int>(10473, 41610, 41663),
            new Tuple<int, int, int>(10452, 33297, 33313),
            new Tuple<int, int, int>(10330, 66195, 66214),
            new Tuple<int, int, int>(10259, 34214, 34260),
            new Tuple<int, int, int>(10073, 203262, 203319),
            new Tuple<int, int, int>(10073, 203428, 203487),
            new Tuple<int, int, int>(10040, 63249, 63313),
            new Tuple<int, int, int>(10040, 62463, 62509),
            new Tuple<int, int, int>(10040, 62991, 63054),
            new Tuple<int, int, int>(561, 2520, 2550),
            new Tuple<int, int, int>(145, 3237, 3259),
            new Tuple<int, int, int>(409, 5253, 5279),
            new Tuple<int, int, int>(426, 1139, 1165),
            new Tuple<int, int, int>(420, 20932, 20950),
            new Tuple<int, int, int>(98, 2140, 2169),
            new Tuple<int, int, int>(409, 8352, 8406),
            new Tuple<int, int, int>(105, 21473, 21490),
            new Tuple<int, int, int>(23, 5984, 6031),
            new Tuple<int, int, int>(23, 6141, 6188),
            new Tuple<int, int, int>(23, 4588, 4626),
            new Tuple<int, int, int>(23, 8353, 8382),
            new Tuple<int, int, int>(23, 8607, 8636),
            new Tuple<int, int, int>(10593, 280740, 280766),
            new Tuple<int, int, int>(10593, 280567, 280599),
            new Tuple<int, int, int>(10218, 93682, 93718),
            new Tuple<int, int, int>(706, 6063, 6085),
            new Tuple<int, int, int>(10003, 404701, 404727),
            new Tuple<int, int, int>(415, 111612, 111627),
            new Tuple<int, int, int>(415, 112176, 112191),
            new Tuple<int, int, int>(563, 7958, 7977),
            new Tuple<int, int, int>(506, 10887, 10918),
            new Tuple<int, int, int>(501, 1537, 1597),
            new Tuple<int, int, int>(10593, 274222, 274243),
            new Tuple<int, int, int>(10593, 211127, 211149),
            new Tuple<int, int, int>(10593, 133508, 133526),
            new Tuple<int, int, int>(10593, 133508, 133526),
            new Tuple<int, int, int>(10593, 133814, 133836),
            new Tuple<int, int, int>(55, 2243, 2253),
        };
        #endregion
        #endregion
    }
}
