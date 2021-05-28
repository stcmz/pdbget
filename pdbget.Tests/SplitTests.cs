using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pdbget.Tests
{
    [TestClass]
    public class SplitTests
    {
        private string? inputFile;
        private string? outputDir;
        private StringWriter? stringWriter;

        private readonly Dictionary<string, string[]> _fragments = new()
        {
            {
                "5R7Y",
                new[]
                {
                    "A_AminoAcids.pdb",
                    "A_JFM_1001.pdb",
                    "A_DMS_1002.pdb",
                    "A_DMS_1003.pdb",
                    "A_DMS_1004.pdb",
                    "A_DMS_1005.pdb",
                    "A_CL_1006.pdb"
                }
            },
            {
                "5R80",
                new[]
                {
                    "A_AminoAcids.pdb",
                    "A_DMS_401.pdb",
                    "A_DMS_402.pdb",
                    "A_DMS_403.pdb",
                    "A_RZG_404.pdb",
                }
            },
            {
                "6D26",
                new[]
                {
                    "A_AminoAcids.pdb",
                    "A_YCM_2308.pdb",
                    "A_FSY_2401.pdb",
                    "A_SO4_2402.pdb",
                    "A_SO4_2403.pdb",
                    "A_SO4_2404.pdb",
                    "A_SO4_2405.pdb",
                    "A_SO4_2406.pdb",
                    "A_SO4_2407.pdb",
                    "A_SO4_2408.pdb",
                    "A_SO4_2409.pdb",
                    "A_SO4_2410.pdb",
                    "A_SIN_2411.pdb",
                    "A_SIN_2412.pdb",
                    "A_SIN_2413.pdb",
                    "A_SIN_2414.pdb",
                    "A_SIN_2415.pdb",
                    "A_PGE_2416.pdb",
                    "A_OLA_2417.pdb",
                    "A_PGO_2418.pdb",
                }
            },
            {
                "6D27",
                new[]
                {
                    "A_AminoAcids.pdb",
                    "A_YCM_2308.pdb",
                    "A_SO4_2401.pdb",
                    "A_SO4_2402.pdb",
                    "A_SO4_2403.pdb",
                    "A_SO4_2404.pdb",
                    "A_SO4_2405.pdb",
                    "A_SO4_2406.pdb",
                    "A_SO4_2407.pdb",
                    "A_FT4_2408.pdb",
                    "A_MES_2409.pdb",
                    "A_OLA_2410.pdb",
                    "A_OLA_2411.pdb",
                    "A_PGE_2412.pdb",
                    "A_PGE_2413.pdb",
                    "A_PGO_2414.pdb",
                    "A_PGO_2415.pdb",
                    "A_PGO_2416.pdb",
                    "A_PGO_2417.pdb",
                    "A_PEG_2418.pdb",
                }
            },
            {
                "4XT1",
                new[]
                {
                    "A_AminoAcids.pdb",
                    "B_AminoAcids.pdb",
                    "C_AminoAcids.pdb",
                    "A_CLR_401.pdb",
                    "A_CLR_402.pdb",
                    "A_OLC_403.pdb",
                    "A_OLC_404.pdb",
                    "A_OLC_405.pdb",
                    "A_OLC_406.pdb",
                    "A_OLC_407.pdb",
                    "A_OLC_408.pdb",
                    "A_UNL_409.pdb",
                    "A_UNL_410.pdb",
                    "A_UNL_411.pdb",
                    "A_UNL_412.pdb",
                    "A_UNL_413.pdb",
                    "A_UNL_414.pdb",
                    "A_UNL_415.pdb",
                    "A_SIN_416.pdb",
                    "B_PCA_1.pdb",
                }
            },
            {
                "4XT3",
                new[]
                {
                    "A_AminoAcids.pdb",
                    "B_AminoAcids.pdb",
                    "A_UNL_401.pdb",
                    "A_PO4_402.pdb",
                    "B_PCA_1.pdb",
                    "C_NAG_1.pdb",
                    "C_NAG_2.pdb",
                    "C_NAG_3.pdb",
                }
            },
        };

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

        private void AssertFragments(int inc, bool flatten)
        {
            foreach (string name in _fragments["5R7Y"])
                AssertOutputFile("5R7Y", name);
            AssertFileCount(_fragments["5R7Y"].Length + inc, "5R7Y");

            foreach (string name in _fragments["5R80"])
                AssertOutputFile("3CL pro", "5R80", name);
            AssertFileCount(_fragments["5R80"].Length + inc, "3CL pro", "5R80");

            if (flatten)
            {
                foreach (string name in _fragments["6D26"])
                    AssertOutputFile("GPCR", "6D26", name);
                AssertFileCount(_fragments["6D26"].Length + inc, "GPCR", "6D26");

                foreach (string name in _fragments["6D27"])
                    AssertOutputFile("GPCR", "6D27", name);
                AssertFileCount(_fragments["6D27"].Length + inc, "GPCR", "6D27");

                foreach (string name in _fragments["4XT1"])
                    AssertOutputFile("4XT1", name);
                AssertFileCount(_fragments["4XT1"].Length + inc, "4XT1");

                foreach (string name in _fragments["4XT3"])
                    AssertOutputFile("4XT3", name);
                AssertFileCount(_fragments["4XT3"].Length + inc, "4XT3");
            }
            else
            {
                foreach (string name in _fragments["6D26"])
                    AssertOutputFile("GPCR", "Q9Y5Y4", "6D26", name);
                AssertFileCount(_fragments["6D26"].Length + inc, "GPCR", "Q9Y5Y4", "6D26");

                foreach (string name in _fragments["6D27"])
                    AssertOutputFile("GPCR", "Q9Y5Y4", "6D27", name);
                AssertFileCount(_fragments["6D27"].Length + inc, "GPCR", "Q9Y5Y4", "6D27");

                foreach (string name in _fragments["4XT1"])
                    AssertOutputFile("P69332", "4XT1", name);
                AssertFileCount(_fragments["4XT1"].Length + inc, "P69332", "4XT1");

                foreach (string name in _fragments["4XT3"])
                    AssertOutputFile("P69332", "4XT3", name);
                AssertFileCount(_fragments["4XT3"].Length + inc, "P69332", "4XT3");
            }
        }

        [TestInitialize]
        public void Setup()
        {
            inputFile = Path.GetTempFileName();
            File.WriteAllLines(inputFile, new[] {
                "5R7Y",
                "GPCR:Q9Y5Y4",
                "3CL pro:5R80",
                "P69332",
            });

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
        public async Task SplitInplace()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-s");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            // original
            AssertOutputFile("5R7Y", "5R7Y.pdb");
            AssertOutputFile("3CL pro", "5R80", "5R80.pdb");
            AssertOutputFile("GPCR", "Q9Y5Y4", "6D26", "6D26.pdb");
            AssertOutputFile("GPCR", "Q9Y5Y4", "6D27", "6D27.pdb");
            AssertOutputFile("P69332", "4XT1", "4XT1.pdb");
            AssertOutputFile("P69332", "4XT3", "4XT3.pdb");

            // fragments
            AssertFragments(1, false);

            AssertFileCount(6 + _fragments.Values.Sum(o => o.Length));
        }

        [TestMethod]
        public async Task SplitSeparate()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-s", "-O", "separate");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            // original
            AssertOutputFile("original", "5R7Y.pdb");
            AssertOutputFile("original", "3CL pro", "5R80.pdb");
            AssertOutputFile("original", "GPCR", "Q9Y5Y4", "6D26.pdb");
            AssertOutputFile("original", "GPCR", "Q9Y5Y4", "6D27.pdb");
            AssertOutputFile("original", "P69332", "4XT1.pdb");
            AssertOutputFile("original", "P69332", "4XT3.pdb");

            // fragments
            AssertFragments(0, false);

            AssertFileCount(6 + _fragments.Values.Sum(o => o.Length));
        }

        [TestMethod]
        public async Task SplitNoLabel()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-s", "-O", "nolabel");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            // original
            AssertOutputFile("original", "5R7Y.pdb");
            AssertOutputFile("original", "5R80.pdb");
            AssertOutputFile("original", "Q9Y5Y4", "6D26.pdb");
            AssertOutputFile("original", "Q9Y5Y4", "6D27.pdb");
            AssertOutputFile("original", "P69332", "4XT1.pdb");
            AssertOutputFile("original", "P69332", "4XT3.pdb");

            // fragments
            AssertFragments(0, false);

            AssertFileCount(6 + _fragments.Values.Sum(o => o.Length));
        }

        [TestMethod]
        public async Task SplitDelete()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-s", "-O", "delete");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            // fragments
            AssertFragments(0, false);

            AssertFileCount(_fragments.Values.Sum(o => o.Length));
        }

        [TestMethod]
        public async Task SplitInplaceFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-sf");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            // original
            AssertOutputFile("5R7Y", "5R7Y.pdb");
            AssertOutputFile("3CL pro", "5R80", "5R80.pdb");
            AssertOutputFile("GPCR", "6D26", "6D26.pdb");
            AssertOutputFile("GPCR", "6D27", "6D27.pdb");
            AssertOutputFile("4XT1", "4XT1.pdb");
            AssertOutputFile("4XT3", "4XT3.pdb");

            // fragments
            AssertFragments(1, true);

            AssertFileCount(6 + _fragments.Values.Sum(o => o.Length));
        }

        [TestMethod]
        public async Task SplitSeparateFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-sf", "-O", "separate");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            // original
            AssertOutputFile("original", "5R7Y.pdb");
            AssertOutputFile("original", "3CL pro", "5R80.pdb");
            AssertOutputFile("original", "GPCR", "6D26.pdb");
            AssertOutputFile("original", "GPCR", "6D27.pdb");
            AssertOutputFile("original", "4XT1.pdb");
            AssertOutputFile("original", "4XT3.pdb");

            // fragments
            AssertFragments(0, true);

            AssertFileCount(6 + _fragments.Values.Sum(o => o.Length));
        }

        [TestMethod]
        public async Task SplitNoLabelFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-sf", "-O", "nolabel");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            // original
            AssertOutputFile("original", "5R7Y.pdb");
            AssertOutputFile("original", "5R80.pdb");
            AssertOutputFile("original", "6D26.pdb");
            AssertOutputFile("original", "6D27.pdb");
            AssertOutputFile("original", "4XT1.pdb");
            AssertOutputFile("original", "4XT3.pdb");

            // fragments
            AssertFragments(0, true);

            AssertFileCount(6 + _fragments.Values.Sum(o => o.Length));
        }

        [TestMethod]
        public async Task SplitDeleteFlatten()
        {
            Debug.Assert(inputFile != null);
            Debug.Assert(outputDir != null);
            Debug.Assert(stringWriter != null);

            int retCode = await Program.Main("-l", inputFile, "-o", outputDir, "-sf", "-O", "delete");

            Assert.AreEqual(retCode, 0);
            Assert.AreEqual(string.Empty, stringWriter.ToString());

            // fragments
            AssertFragments(0, true);

            AssertFileCount(_fragments.Values.Sum(o => o.Length));
        }
    }
}
