using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace pdbget.Tests
{
    [TestClass]
    public class NonSplitTests
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
        public async Task SinglePdb()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "4XT1" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("4XT1.pdb");

            AssertFileCount(1);
        }

        [TestMethod]
        public async Task SingleUniProt()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "Q9Y5Y4" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("Q9Y5Y4", "6D26.pdb");
            AssertOutputFile("Q9Y5Y4", "6D27.pdb");

            AssertFileCount(2);
        }

        [TestMethod]
        public async Task SinglePdbLabeled()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "X:4XT1" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("X", "4XT1.pdb");

            AssertFileCount(1);
        }

        [TestMethod]
        public async Task SingleUniProtLabeled()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "X:Q9Y5Y4" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("X", "Q9Y5Y4", "6D26.pdb");
            AssertOutputFile("X", "Q9Y5Y4", "6D27.pdb");

            AssertFileCount(2);
        }

        [TestMethod]
        public async Task SinglePdbFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "4XT1" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-f");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("4XT1.pdb");

            AssertFileCount(1);
        }

        [TestMethod]
        public async Task SingleUniProtFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "Q9Y5Y4" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-f");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("6D26.pdb");
            AssertOutputFile("6D27.pdb");

            AssertFileCount(2);
        }

        [TestMethod]
        public async Task SinglePdbLabeledFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "X:4XT1" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-f");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("X", "4XT1.pdb");

            AssertFileCount(1);
        }

        [TestMethod]
        public async Task SingleUniProtLabeledFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "X:Q9Y5Y4" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-f");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("X", "6D26.pdb");
            AssertOutputFile("X", "6D27.pdb");

            AssertFileCount(2);
        }

        [TestMethod]
        public async Task SinglelineTolerance()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] { "  \t \v5R7Y\f \x20\xA0\x85  Q9Y5Y4    \t\t\t\x20\xA0\x00855R80\v\f" });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("5R7Y.pdb");
            AssertOutputFile("5R80.pdb");
            AssertOutputFile("Q9Y5Y4", "6D26.pdb");
            AssertOutputFile("Q9Y5Y4", "6D27.pdb");

            AssertFileCount(4);
        }

        [TestMethod]
        public async Task MultilineTolerance()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] {
                "\x20\xA0\x85",
                "# full line comment",
                "",
                " \t\x20\xA0\x85 5R7Y \t ",
                "",
                "\t  Q9Y5Y4",
                "",
                "    #  `1234567890\x20\xA0\x85\t\v\f~!@#$%^&*()_+-=[]\\{}|;':\",./<>? abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ  ",
                "",
                "\v \t\x20\xA0\x85\v3cl pro\v\f\x20\xA0\x85\t:   \t5R80\t    \v\t\f  ",
                "",
            });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir);

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("5R7Y.pdb");
            AssertOutputFile("3cl pro", "5R80.pdb");
            AssertOutputFile("Q9Y5Y4", "6D26.pdb");
            AssertOutputFile("Q9Y5Y4", "6D27.pdb");

            AssertFileCount(4);
        }

        [TestMethod]
        public async Task MixedFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            File.WriteAllLines(inputFile, new[] {
                "5R7Y",
                "GPCR:Q9Y5Y4",
                "3CL pro:5R80",
                "P69332",
            });
            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-f");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            AssertOutputFile("5R7Y.pdb");
            AssertOutputFile("3CL pro", "5R80.pdb");
            AssertOutputFile("GPCR", "6D26.pdb");
            AssertOutputFile("GPCR", "6D27.pdb");
            AssertOutputFile("4XT1.pdb");
            AssertOutputFile("4XT3.pdb");

            AssertFileCount(6);
        }
    }
}
