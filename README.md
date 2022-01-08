pdbget
=====
![build workflow](https://github.com/stcmz/pdbget/actions/workflows/build.yml/badge.svg)
![release workflow](https://github.com/stcmz/pdbget/actions/workflows/release.yml/badge.svg)

A command line tool for fetching PDB protein structure files and splitting protein chains.


Features
--------

pdbget downloads, layouts and splits protein structure files (*.pdb) from [RCSB Protein Data Bank](https://www.rcsb.org/) for the given list of structure entries.

pdbget enhances the experience with:
* multi-thread downloading to speed up
* duplicate entry detection to avoid unnecessary downloading
* controllable neat layout with labels, flattening and original location options
* fully support of various input ways (see below)
* post-validation of split fragments
* incremental processing with existing file detection

pdbget recognizes the following types of entry:
* PDB entry, e.g. [6PT0](https://www.rcsb.org/structure/6PT0)
* UniProt ID, e.g. [P34972](https://www.uniprot.org/uniprot/P34972) or [CNR2_HUMAN](https://www.uniprot.org/uniprot/CNR2_HUMAN)

pdbget accepts the following ways of input:
* standard input: `pdbget`
* file input: `pdbget -l entrylist`
* input redirect: `pdbget -l <(cat entrylist)`
* pipe input: `cat entrylist | pdbget`
* as script file with header: `#!/usr/local/bin/pdbget -l` (Linux/Unix only)

Supported operating systems and compilers
-----------------------------------------

All systems with [.NET SDK v5.0] or higher supported, e.g.
* Windows 10 version 1809 or higher
* macOS 10.13 "High Sierra" or higher
* most of current Linux distros: Ubuntu, CentOS, openSUSE, RHEL, Fedora, Debian, Alpine, SLES

Compilation from source code
----------------------------

### Compiler and SDK

pdbget compiles with [.NET SDK v5.0]. Follow the official guide to download and install the SDK before the build. The SDK also comes with the Visual Studio 2019 installer version 16.8 or higher.

The Visual Studio solution and project files, as well as the vscode settings are provided. One may open `pdbget.sln` in Visual Studio 2019 and do a rebuild (predefined profiles for Windows/Linux/macOS are also provided), or open the cloned repository in vscode and run the build task.

Inside each project, the generated objects are placed in the `obj` folder, and the generated executable are placed in the `bin` folder.

Thanks to the complete cross-platform nature of the SDK, one can target any supported system on any supported system. For example, one can build and publish a macOS or Linux binary of `pdbget` on Windows or vice versa. Build for "Portable" on "Any CPU" is also possible. However, to enable single-file publish and ready-to-run compilation, a platform-specific build is suggested.


### Build for Linux

To compile for Linux on any supported system, simply run either of
```Powershell
# Dynamic build, will require installation of .NET Runtime/SDK
dotnet publish -c release --no-self-contained -r linux-x64 -p:UseAppHost=true

# Static build, no .NET Runtime/SDK is required
dotnet publish -c release --self-contained -r linux-x64 -p:UseAppHost=true -p:PublishTrimmed=true
```

Or with MSBuild, simply run either of
```Powershell
# Dynamic build, will require installation of .NET Runtime/SDK
msbuild /t:"Restore;Clean;Build;Publish" /p:Configuration=Release /p:PublishProfile=LinuxFolderProfile

# Static build, no .NET Runtime/SDK is required
msbuild /t:"Restore;Clean;Build;Publish" /p:Configuration=Release /p:PublishProfile=LinuxFolderProfile_static
```

### Build for Windows

To compile for Windows on any supported system, simply run either of
```Powershell
# Dynamic build, will require installation of .NET Runtime/SDK
dotnet publish -c release --no-self-contained -r win-x64 -p:UseAppHost=true -p:PublishReadyToRun=true

# Static build, no .NET Runtime/SDK is required
dotnet publish -c release --self-contained -r win-x64 -p:UseAppHost=true -p:PublishTrimmed=true -p:PublishReadyToRun=true
```

Or with MSBuild, simply run either of
```Powershell
# Dynamic build, will require installation of .NET Runtime/SDK
msbuild /t:"Restore;Clean;Build;Publish" /p:Configuration=Release /p:PublishProfile=WinFolderProfile

# Static build, no .NET Runtime/SDK is required
msbuild /t:"Restore;Clean;Build;Publish" /p:Configuration=Release /p:PublishProfile=WinFolderProfile_static
```

Kindly note that, the `PublishReadyToRun` option is only available while building on Windows.

### Build for macOS

To compile for macOS on any supported system, simply run either of
```PowerShell
# Dynamic build, will require installation of .NET Runtime/SDK
dotnet publish -c release --no-self-contained -r osx-x64 -p:UseAppHost=true

# Static build, no .NET Runtime/SDK is required
dotnet publish -c release --self-contained -r osx-x64 -p:UseAppHost=true -p:PublishTrimmed=true
```

Or with MSBuild, simply run either of
```PowerShell
# Dynamic build, will require installation of .NET Runtime/SDK
msbuild /t:"Restore;Clean;Build;Publish" /p:Configuration=Release /p:PublishProfile=MacFolderProfile

# Static build, no .NET Runtime/SDK is required
msbuild /t:"Restore;Clean;Build;Publish" /p:Configuration=Release /p:PublishProfile=MacFolderProfile_static
```

Usage
-----

First add pdbget to the PATH environment variable or place pdbget in a PATH location (e.g. C:\Windows\System32\ or /usr/local/bin).

To display a full list of available options, simply run the program with the `--help` argument
```
pdbget --help
```

### Default layout

Simply run pdbget without any arguments from a command line:
```
pdbget
```
and input entries one by one or line by line, for example:
```
6PT0 4XT1
Q9Y5Y4
```

This will result in output layout:
```
./6PT0.pdb
./4XT1.pdb
./Q9Y5Y4/6D26.pdb
./Q9Y5Y4/6D27.pdb
```

### Flattened layout

Adding the `-f` or `--flatten` option
```
pdbget -f
```
will force pdbget to eliminate the indirection level in the layout hierarchy. Taking the above example, the output will become:
```
./6PT0.pdb
./4XT1.pdb
./6D26.pdb
./6D27.pdb
```

### Splitting

Adding the `-s` or `--split` option
```
pdbget -s
```
will ask pdbget to split every requested PDB into separate fragment PDB files of protein chains and small molecules. Taking the above example, the output will become:
```
./4XT1/4XT1.pdb
./4XT1/A_AminoAcids.pdb
./4XT1/...
./4XT1/C_AminoAcids.pdb
./6PT0/6PT0.pdb
./6PT0/A_AminoAcids.pdb
./6PT0/...
./6PT0/R_WI5_401.pdb
./Q9Y5Y4/6D26/6D26.pdb
./Q9Y5Y4/6D26/A_AminoAcids.pdb
./Q9Y5Y4/6D26/...
./Q9Y5Y4/6D26/A_YCM_2308.pdb
./Q9Y5Y4/6D27/6D27.pdb
./Q9Y5Y4/6D27/A_AminoAcids.pdb
./Q9Y5Y4/6D27...
./Q9Y5Y4/6D27/A_YCM_2308.pdb
```
Water molecules are excluded automatically.

In case the split option is on, the `-O` or `--original` option can be used to control where to place the original PDB files. Available values for the option includes:
* `inplace`, this is the default value and can be omitted
* `separate`, place the original PDB files separately in a folder named original
* `nolabel`, same as `separate` but without the label grouping folder
* `delete`, keep only the fragments and delete the original PDB files

### Labelled layout

Every input line of entries can have a label indicated by a colon separator. By using a label, pdbget creates a grouping folder named after the label and layouts the hierarchy for the corresponding entries in that folder. For example:
```
3CL pro: 5R7Y 5R80
GPCR: 6PT0 Q9Y5Y4
```
will resulting
```
./3CL pro/5R7Y.pdb
./3CL pro/5R80.pdb
./GPCR/6PT0.pdb
./GPCR/Q9Y5Y4/6D26.pdb
./GPCR/Q9Y5Y4/6D27.pdb
```
One can combine the split and flatten options with labels.


Author
--------------

[Maozi Chen]


[Maozi Chen]: https://www.linkedin.com/in/maozichen/
[.NET SDK v5.0]: https://dotnet.microsoft.com/download/dotnet/5.0
