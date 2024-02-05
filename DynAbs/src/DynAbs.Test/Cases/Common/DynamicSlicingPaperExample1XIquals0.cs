namespace DynAbs.Test.Cases
{
    public class DynamicSlicingPaperExample1XIquals0
    {
        static void Main(string[] args)
        {
            int y;
            int z;
            int x = 0;
            if (x < 0)
            {
                y = x + 2;
                z = x + 3;
            }
            else
            {
                if (x == 0)
                {
                    y = x + 4;
                    z = x + 5;
                }
                else
                {
                    y = x + 6;
                    z = x + 7;
                }
            }
            // y should be 2
            y = y + 1;
            // z should be 3
            z = z + 1;

            return;
        }

        //TestSimpleSlice(typeof(DynamicSlicingPaperExample1XIquals0), new DynAbsResult
        //{
        //    Criteria = new Stmt { FileName = fileName, Line = 29 },
        //    Sliced =
        //        {
        //            new Stmt { Line = 9, FileName = fileName },
        //            new Stmt { Line = 10, FileName = fileName },
        //            new Stmt { Line = 17, FileName = fileName },
        //            new Stmt { Line = 19, FileName = fileName },
        //            new Stmt { Line = 29, FileName = fileName },
        //        },
        //}
        //);

        //public TestResult[] Results
        //{
        //    get {
        //        return new DynAbsResult[]
        //        {
        //            new DynAbsResult {
        //                Criteria = new LineInFile { Line = 35 },
        //                Sliced =
        //                {
        //                    new LineInFile { Line = 15 },
        //                    new LineInFile { Line = 16 },
        //                    new LineInFile { Line = 23 },
        //                    new LineInFile { Line = 25 },
        //                    new LineInFile { Line = 35 },
        //                },
        //            },
        //            new DynAbsResult {
        //                Criteria = new LineInFile { Line = 37 },
        //                Sliced =
        //                {
        //                    new LineInFile { Line = 15 },
        //                    new LineInFile { Line = 16 },
        //                    new LineInFile { Line = 23 },
        //                    new LineInFile { Line = 26 },
        //                    new LineInFile { Line = 37 },
        //                },
        //            },
        //        };
        //    }
        //}
    }
}
