# DynAbs

Welcome to DynAbs, a tool that implements the Abstract Dynamic Slicing approach tailored for C# programs.

DynAbs is designed to receive C# programs and efficiently generate backward slices from a user-selected executed statement. 
One of its standout features is its ability to skip arbitrary code at the method level in a conservative manner.

When encountering non-instrumented calls, DynAbs adopts a cautious approach, assuming the worst-case scenario: every reachable object from the external code might be read or written, with the potential for an unbounded number of new object allocations.

This unique feature is realized by replacing real memory addresses with abstract equivalences that evolve throughout the execution. As a result, DynAbs excels in handling the largest .NET applications by filtering out non-relevant code for users, such as Roslyn or Powershell.
It's worth noting that DynAbs does not currently instrument external libraries, prioritizing its focus on the core functionality of .NET applications.

<b>Citations:</b>

Alexis Soifer, Diego Garbervetsky, Victor Braberman, and Sebastian Uchitel. [Focused Dynamic Slicing for Large Applications using an Abstract Memory-Model](https://arxiv.org/pdf/2211.04560.pdf). arXiv, 2022. [\<bibtex>](misc/dynabs.bib)

## Table of Contents
1. [Tooling](#Tooling)
2. [Requirements](#Requirements)
3. [Building the Tool](#Building-the-Tool)
4. [Configuring the Tool](#Configuring-the-Tool)
5. [Using the Tool](#Using-the-Tool)

---
---

## Tooling

We provide a console (command line) and a desktop applications.
Both provide the same capabilities, but the latter also provides a way to visualize and navigate across the dependencies.

---
---

## Requirements

* Runs only on Windows due to some libraries limitations

* Install NET CORE 6 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/6.0

* Clone the repository: https://github.com/asoifer/DynAbs

---
---

## Building the Tool

Just run the build command from your shell.

```bash
cd DynAbs/src/DynAbs.DesktopApp
dotnet build --configuration release
```

And for the command line app:

```bash
cd DynAbs/src/DynAbs.ConsoleApp
dotnet build --configuration release
```

---
---

## Configuring the Tool

The main input for the slicer is a *.slc file with the following information.

Required inputs:
* .NET solution path (i.e., your-file.sln)
* Executable project name
* Inputs (if there are...)
* Criteria (a collection of File Name + Line)

Other configurations (for development - optional):
* Use MSBuild (default=false)
* File trace input (default) vs. pipeline input (the trace is consumed on the fly by the slicer)
* Run automatically (default=true) - for running the instrumented project manually
* File trace input path => for providing the trace file after running the instrumented app. by yourself
* Summaries file path (in development - for providing specifications in order to avoid the worst case scenario)
* IncludeControlDependencies (default=true)
* IncludeAllUses (default=true) - for adding all uses when accessing to the properties (a.b.c adding uses for { a, a.b, a.b.c }
* MemoryModel (default=Clusters) (there are other available memory models including the naivest one that do not compress anything)

Outputs:
* Output folder - for storing the instrumented project, generating the trace, and the slicing results
* [still in development]

There are some examples in this repository (ongoing).

---
---

## Using the Tool

For running the console app:
```bash
cd DynAbs/src/DynAbs.ConsoleApp
dotnet run your-slc-file.slc --configuration release
```

---

### Desktop App

```bash
cd DynAbs/src/DynAbs.DesktopApp
dotnet run --configuration release
```

This will display a textbox for pasting your slc path.
Then press Run, and at the end, show Slice.

---
---
# Contact

For questions, issues, or contributions contact us at asoifer@dc.uba.ar