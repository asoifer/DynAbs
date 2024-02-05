grammar Summaries;

/*
 * Parser Rules
 */
s					: rv SEPARATION ro SEPARATION r SEPARATION w SEPARATION cn EOF? ;
rv					: null | fresh | isIn ;
//ro				: null | single | many ;
ro					: '{}' | '{' singleMany '}' ;
r					: '{}' | '{' metf '}' ;
w					: '{}' | '{' metf '}' ;
cn					: '{}' | '{' mc '}' ;
// RV/RO parts
null				: NULL ;
fresh				: FRESH | FRESH '{' types '}' | FRESH '{|' kind '}' | FRESH '{' types '|' kind '}' ;
isIn				: ISIN '{' met '}' ;
singleMany			: single | many | single ';' singleMany | many ';' singleMany ;
single				: SINGLE | SINGLE '{' types '}' | SINGLE '{|' kind '}' | SINGLE '{' types '|' kind '}' ;
many				: MANY | MANY '{' types '}' | MANY '{|' kind '}' | MANY '{' types '|' kind '}' ;

// Types
types				: type | type ', ' types ;
type				: words | elementType | parametricType ;
kinds				: kind | kind ', ' kinds ;
kind				: WORD ;
words				: WORD | words '.' WORD ;
elementType			: '@' RECEIVER | '@' PARAMS | '@' PARAMS '[' number ']' | '@' RETURNVALUE ;
parametricType		: '*' WORD ;
// Field
field				: WORD ;
anyField			: ANY ;
// Multiple connection
mc					: c | c ', ' mc ;
// Connection
c					: '[' met ', ' met ', ' fa ']' ;
// Multiple extended term with field
metf				: etf | etf ';' metf ;
// Extended term and field, or root variable
etf					: b | et '.' fa ;
fa					: field | ANY | '@' WORD ;
// Multiple extended terms
met					: et | et ';' met ;
// Extended term
et				    : bf filter? ;
bf					: b | b '.' f ;
b				    : RECEIVER | PARAMS | PARAMS '[' number ']'  | GLOBALS | RETURNVALUE | REACHABLEOBJ '[' number ']' | REACHABLEOBJ ;
f					: '*' | anyField | anyField '.' f | field | field '.' f ;
filter				: filterT | filterOK | filterT filterOK ;
filterT				: filterUT | filterOT | filterUT filterOT ;
filterUT				: '.' UNTILTYPE '{' types '}' ;
filterOT				: '.' OFTYPE '{' types '}' ;
filterOK				: '.' OFKIND '{' kinds '}' ;
number				: NUMBER;

/*
 * Lexer Rules
 */
fragment LOWERCASE  : [a-z] ;
fragment UPPERCASE  : [A-Z] ;

SEPARATION			: '.' ;
NULL				: 'Null' ;
FRESH				: 'Fresh' ;
ISIN				: 'IsIn' ;
SINGLE				: 'Single' ;
MANY				: 'Many' ;
ANY					: '?' ;
RECEIVER			: 'R' ;
PARAMS				: 'P' ;
GLOBALS				: 'G' ;
RETURNVALUE			: 'RV' ;
REACHABLEOBJ		: 'RO' ;
UNTILTYPE			: 'UntilType' ;
OFTYPE				: 'OfType' ;
OFKIND				: 'OfKind' ;
NUMBER				: [0-9] ;
NEWLINE             : ('\r'? '\n' | '\r')+ ;
WORD                : (LOWERCASE | UPPERCASE | '_' | NUMBER)+ ;
