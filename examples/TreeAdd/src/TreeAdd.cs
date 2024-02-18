
using System;

//
// A Java implementation of the <tt>treeadd</tt> Olden benchmark.
// <p>
// Treeadd is a very simple program that performs a recursive depth
// first traversal of a binary tree and sums the value of each element
// in the tree.  We initialize the elements in the tree to contain
// '1'.
//
public class TreeAdd
{
	//
  // The number of levels in the tree.
  //
	private static int levels = 0;
   //
  // Set to true to print the final result.
  //
	private static bool printResult = false;
   //
  // Set to true to print informative messages
  //
	private static bool printMsgs = false;

  //
  // The main routine which creates a tree and traverses it.
  // @param args the arguments to the program
  //
	public static void Main(string[] args)
	{
		parseCmdLine(args);
		
		var start0 = System.DateTime.Now;
		TreeNode root = new TreeNode(levels);
		var end0 = System.DateTime.Now;
		
		var start1 = System.DateTime.Now;
		int result = root.addTree();
		var end1 = System.DateTime.Now;
		
		if (printResult || printMsgs)
			System.Console.WriteLine("Received results of " + result);
		
		if (printMsgs)
		{
			System.Console.WriteLine("Treeadd alloc time " + (end0.Subtract(start0).TotalMilliseconds) / 1000.0);
			System.Console.WriteLine("Treeadd add time " + (end1.Subtract(start1).TotalMilliseconds) / 1000.0);
			System.Console.WriteLine("Treeadd total time " + (end1.Subtract(start0).TotalMilliseconds) / 1000.0);
		}
        var slicingVariable1 = result;
		System.Console.WriteLine("Done!");
	}

	//
  // Parse the command line options.
  // @param args the command line options.
  //
	private static void parseCmdLine(string[] args)
	{
		int i = 0;
		string arg;
		
		while (i < args.Length && args[i].StartsWith("-"))
		{
			arg = args[i++];
			if (arg.Equals("-l"))
			{
				if (i < args.Length)
				{
					levels = System.Convert.ToInt32(args[i++]);
				}
				else
					throw new System.Exception("-l requires the number of levels");
			}
			else if (arg.Equals("-p"))
			{
				printResult = true;
			}
			else if (arg.Equals("-m"))
			{
				printMsgs = true;
			}
			else if (arg.Equals("-h"))
			{
				usage();
			}
		}
		if (levels == 0)
			usage();
	}

	//
   // The usage routine which describes the program options.
   //
	private static void usage()
	{
		System.Console.Error.WriteLine("usage: java TreeAdd -l <levels> [-p] [-m] [-h]");
		System.Console.Error.WriteLine("    -l the number of levels in the tree");
		System.Console.Error.WriteLine("    -m (print informative messages)");
		System.Console.Error.WriteLine("    -p (print the result>)");
		System.Console.Error.WriteLine("    -h (this message)");
		System.Environment.Exit(0);
	}
}
