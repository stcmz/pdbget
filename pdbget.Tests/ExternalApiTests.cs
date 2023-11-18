using Microsoft.VisualStudio.TestTools.UnitTesting;
using pdbget.Services;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pdbget.Tests;

[TestClass]
public class ExternalApiTests
{
    [TestMethod]
    public async Task GetPdb()
    {
        Rcsb rcsb = new("4XT1");
        Assert.AreEqual("4XT1", rcsb.Entry);
        Assert.AreEqual("https://files.rcsb.org/download/4XT1", rcsb.Uri);

        using HttpClient client = new();
        string pdb = await client.GetStringAsync($"{rcsb.Uri}.pdb");
        string sha256sum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pdb)));
        Assert.AreEqual("9AEC8FED0DA81D692C2D2B1A4E4CD29AB83976E494154C2C1CAC4DE942622F3F", sha256sum);
    }

    [TestMethod]
    public void GetStructuresByUniProtId()
    {
        UniProt uniprot = new("Q9Y5Y4");
        Assert.AreEqual("Q9Y5Y4", uniprot.Entry);
        Assert.AreEqual("https://rest.uniprot.org/uniprotkb/search?query=reviewed:true+AND+Q9Y5Y4", uniprot.Uri);

        UniProt.StructRecord[]? structures = uniprot.GetStructures();
        Assert.IsNotNull(structures);
        Assert.AreEqual(3, structures.Length);
        Assert.AreEqual("Q9Y5Y4", structures[0].UniProtId);
        Assert.AreEqual("6D26", structures[0].PdbEntry);
        Assert.AreEqual("X-ray", structures[0].Method);
        Assert.AreEqual("2.80 A", structures[0].Resolution);
        Assert.AreEqual("A", structures[0].Chain);
        Assert.AreEqual("1-339", structures[0].Positions);

        Assert.AreEqual("Q9Y5Y4", structures[1].UniProtId);
        Assert.AreEqual("6D27", structures[1].PdbEntry);
        Assert.AreEqual("X-ray", structures[1].Method);
        Assert.AreEqual("2.74 A", structures[1].Resolution);
        Assert.AreEqual("A", structures[1].Chain);
        Assert.AreEqual("1-339", structures[1].Positions);

        Assert.AreEqual("Q9Y5Y4", structures[2].UniProtId);
        Assert.AreEqual("7M8W", structures[2].PdbEntry);
        Assert.AreEqual("X-ray", structures[2].Method);
        Assert.AreEqual("2.61 A", structures[2].Resolution);
        Assert.AreEqual("A", structures[2].Chain);
        Assert.AreEqual("1-339", structures[2].Positions);
    }
}
