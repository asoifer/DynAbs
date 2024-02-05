namespace DynAbs.Tracing
{
    public enum TraceType
    {
        SimpleStatement = 0,
        EnterCondition = 1,
        EnterExpression = 2,
        ExitCondition = 3,
        EnterMethod = 4,
        ExitMethod = 5,
        EnterStaticMethod = 6,
        ExitStaticMethod = 7,
        EnterConstructor = 8,
        ExitConstructor = 9,
        EnterStaticConstructor = 10,
        ExitStaticConstructor = 11,
        EndInvocation = 12,
        EndMemberAccess = 13,
        Break = 14,
        ExitUsing = 15,
        BeforeConstructor = 16,
        EnterCatch = 17,
        ExitCatch = 18,
        EnterFinally = 19,
        ExitFinally = 20,
        EnterFinalCatch = 21,
        EnterFinalFinally = 22,
        ExitLoopByException = 23,
        ExitLoop = 24,
        BaseCall = 25,
        ConditionalAccessIsNull = 26
    }
}