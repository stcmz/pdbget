using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace pdbget.Tests;

[TestClass]
public class ConflictTests
{
    private string? inputFile;
    private string? outputDir;
    private StringWriter? stringWriter;

    private void AssertOutputFile(params string[] paths)
    {
        Debug.Assert(outputDir != null);

        string filename = Path.Combine(outputDir, Path.Combine(paths));

        Assert.IsTrue(File.Exists(filename), $"Output file {filename} not exists");
        Assert.IsTrue(new FileInfo(filename).Length > 0, $"Output file {filename} is empty");
    }

    private void AssertFileCount(int count, params string[] paths)
    {
        Debug.Assert(outputDir != null);

        string dirname = Path.Combine(outputDir, Path.Combine(paths));
        bool exists = Directory.Exists(dirname);

        if (count == 0 && !exists)
            return;

        Assert.IsTrue(exists);

        int actual = Directory.GetFiles(dirname, "*", SearchOption.AllDirectories).Length;

        Assert.AreEqual(count, actual, $"File count {actual} should be {count} in {dirname}");
    }

    [TestInitialize]
    public void Setup()
    {
        inputFile = Path.GetTempFileName();

        outputDir = Path.GetTempFileName();
        File.Delete(outputDir);
        Directory.CreateDirectory(outputDir);

        Console.WriteLine($"Init {inputFile}, {outputDir}");

        stringWriter = new StringWriter();
        Console.SetError(stringWriter);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.Delete(inputFile);
        Directory.Delete(outputDir, true);

        stringWriter.Dispose();

        Console.WriteLine($"Cleanup {inputFile}, {outputDir}");
    }

    [TestMethod]
    public async Task DuplicateSinglelinePdb()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["4XT1 4XT1 4XT1"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("4XT1.pdb");

        AssertFileCount(1);
    }

    [TestMethod]
    public async Task DuplicateSinglelineUniProt()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["Q9Y5Y4 Q9Y5Y4 Q9Y5Y4"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("Q9Y5Y4", "6D26.pdb");
        AssertOutputFile("Q9Y5Y4", "6D27.pdb");

        AssertFileCount(2);
    }

    [TestMethod]
    public async Task DuplicateSinglelineMixedFlatten()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["Q9Y5Y4 6D26 Q9Y5Y4 6D27 Q9Y5Y4 Q9Y5Y4 6D26 6D27"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-f");

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("6D26.pdb");
        AssertOutputFile("6D27.pdb");

        AssertFileCount(2);
    }

    [TestMethod]
    public async Task DuplicatePdb()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["4XT1", "4XT1", "4XT1"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("4XT1.pdb");

        AssertFileCount(1);
    }

    [TestMethod]
    public async Task DuplicateUniProt()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["Q9Y5Y4", "Q9Y5Y4", "Q9Y5Y4"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("Q9Y5Y4", "6D26.pdb");
        AssertOutputFile("Q9Y5Y4", "6D27.pdb");

        AssertFileCount(2);
    }

    [TestMethod]
    public async Task DuplicatePdbLabeled()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["X:4XT1 4XT1 4XT1", "X:4XT1 4XT1", "X:4XT1"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("X", "4XT1.pdb");

        AssertFileCount(1);
    }

    [TestMethod]
    public async Task DuplicateUniProtLabeled()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["X:Q9Y5Y4 Q9Y5Y4 Q9Y5Y4", "X:Q9Y5Y4 Q9Y5Y4", "X:Q9Y5Y4"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("X", "Q9Y5Y4", "6D26.pdb");
        AssertOutputFile("X", "Q9Y5Y4", "6D27.pdb");

        AssertFileCount(2);
    }

    [TestMethod]
    public async Task DuplicateMixedFlatten()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["Q9Y5Y4 6D26 Q9Y5Y4", "6D27 Q9Y5Y4", "Q9Y5Y4 6D26"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-f");

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("6D26.pdb");
        AssertOutputFile("6D27.pdb");

        AssertFileCount(2);
    }

    [TestMethod]
    public async Task MultipleTargets()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["X:Q9Y5Y4 6D26 Q9Y5Y4 6D27", "6D27 Q9Y5Y4 6D26", "X:Q9Y5Y4 6D26"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("X", "Q9Y5Y4", "6D26.pdb");
        AssertOutputFile("X", "Q9Y5Y4", "6D27.pdb");
        AssertOutputFile("X", "6D26.pdb");
        AssertOutputFile("X", "6D27.pdb");
        AssertOutputFile("Q9Y5Y4", "6D26.pdb");
        AssertOutputFile("Q9Y5Y4", "6D27.pdb");
        AssertOutputFile("6D26.pdb");
        AssertOutputFile("6D27.pdb");

        AssertFileCount(8);
    }

    [TestMethod]
    public async Task MultipleTargetsFlatten()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["X:Q9Y5Y4 6D26 Q9Y5Y4 6D27", "6D27 Q9Y5Y4 6D26", "X:Q9Y5Y4 6D26"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-f");

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("X", "6D26.pdb");
        AssertOutputFile("X", "6D27.pdb");
        AssertOutputFile("6D26.pdb");
        AssertOutputFile("6D27.pdb");

        AssertFileCount(4);
    }

    [TestMethod]
    public async Task MultipleTargetsInplace()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["X:Q9Y5Y4 6D26 Q9Y5Y4 6D27", "6D27 Q9Y5Y4 6D26", "X:Q9Y5Y4 6D26"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-s");

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("X", "Q9Y5Y4", "6D26", "6D26.pdb");
        AssertOutputFile("X", "Q9Y5Y4", "6D27", "6D27.pdb");
        AssertOutputFile("X", "6D26", "6D26.pdb");
        AssertOutputFile("X", "6D27", "6D27.pdb");
        AssertOutputFile("Q9Y5Y4", "6D26", "6D26.pdb");
        AssertOutputFile("Q9Y5Y4", "6D27", "6D27.pdb");
        AssertOutputFile("6D26", "6D26.pdb");
        AssertOutputFile("6D27", "6D27.pdb");

        AssertFileCount(21 * 8);
    }

    [TestMethod]
    public async Task MultipleTargetsSeparate()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["X:Q9Y5Y4 6D26 Q9Y5Y4 6D27", "6D27 Q9Y5Y4 6D26", "X:Q9Y5Y4 6D26"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-s", "-O", "separate");

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("original", "X", "Q9Y5Y4", "6D26.pdb");
        AssertOutputFile("original", "X", "Q9Y5Y4", "6D27.pdb");
        AssertOutputFile("original", "X", "6D26.pdb");
        AssertOutputFile("original", "X", "6D27.pdb");
        AssertOutputFile("original", "Q9Y5Y4", "6D26.pdb");
        AssertOutputFile("original", "Q9Y5Y4", "6D27.pdb");
        AssertOutputFile("original", "6D26.pdb");
        AssertOutputFile("original", "6D27.pdb");

        AssertFileCount(20 * 8 + 8);
        AssertFileCount(8, "original");
    }

    [TestMethod]
    public async Task MultipleTargetsNoLabel()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["X:Q9Y5Y4 6D26 Q9Y5Y4 6D27", "6D27 Q9Y5Y4 6D26", "X:Q9Y5Y4 6D26"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-s", "-O", "nolabel");

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertOutputFile("original", "Q9Y5Y4", "6D26.pdb");
        AssertOutputFile("original", "Q9Y5Y4", "6D27.pdb");
        AssertOutputFile("original", "6D26.pdb");
        AssertOutputFile("original", "6D27.pdb");

        AssertFileCount(20 * 8 + 4);
        AssertFileCount(4, "original");
    }

    [TestMethod]
    public async Task MultipleTargetsDelete()
    {
        Debug.Assert(inputFile != null);
        Debug.Assert(outputDir != null);
        Debug.Assert(stringWriter != null);

        File.WriteAllLines(inputFile, ["X:Q9Y5Y4 6D26 Q9Y5Y4 6D27", "6D27 Q9Y5Y4 6D26", "X:Q9Y5Y4 6D26"]);
        int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-s", "-O", "delete");

        Assert.AreEqual(retCode, 0);
        Assert.AreEqual(string.Empty, stringWriter.ToString());

        AssertFileCount(20 * 8);
        AssertFileCount(0, "original");
    }
}
