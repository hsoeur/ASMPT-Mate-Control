using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LO_ASM_MATE_CONTROL.MVVM.Models;
using LO_ASM_MATE_CONTROL.Services;
class InventoryCsvTests
{
    static void Main() { Run().GetAwaiter().GetResult(); }
    static async Task Run()
    {
        foreach (var location in new[] { "G00101", "C6001", "C7001", "C8001", " c7001 " })
            Assert(InventoryCsvGenerator.IsEligiblePackagingUnit("BOB1", "  ", location), "Allowed store: " + location);
        foreach (var location in new[] { "C5001", "C9001", "AG001", "", null })
            Assert(!InventoryCsvGenerator.IsEligiblePackagingUnit("BOB1", null, location), "Disallowed store: " + location);
        Assert(!InventoryCsvGenerator.IsEligiblePackagingUnit(" dmy123 ", null, "G00101"), "DMY excluded even in allowed store");
        Assert(!InventoryCsvGenerator.IsEligiblePackagingUnit("BOB1", "CLIENT", "C6001"), "Client stock excluded even in allowed store");
        Assert(InventoryCsvGenerator.IsEligiblePackagingUnit(null, null, "G00101"), "Empty UID is not DMY");
        var quantities = new Dictionary<string, int> { { "NEW", 12 }, { "DMYONLY", 0 }, { "OLD", 4 } };
        var articles = new Dictionary<string, Article> {
            { "NEW", new Article { Designation = "Nouveau; \"composant\" µH", Store = "G00201", GpaoQuantity = 10 } },
            { "DMYONLY", new Article { Designation = "Dummy", Store = "G00102", GpaoQuantity = 20 } },
            { "OLD", new Article { Designation = "Existant", Store = "G00101", GpaoQuantity = 30 } }
        };
        articles.Add("GPAOONLY", new Article { Designation = "Absent ASMPT", GpaoQuantity = 50 });
        int calls = 0;
        var rows = await InventoryCsvGenerator.BuildLinesAsync(quantities, code => { calls++; return Task.FromResult(articles[code]); });
        Assert(rows.Count == 3 && calls == 3, "All ASMPT codes requested once");
        Assert(rows[0] == "MAG1HT;OLD;Existant;0;G00101;30;4;UN", "Eight columns, no header, physical minimum and location sort");
        Assert(rows[1] == "MAG1HT;DMYONLY;Dummy;0;G00102;20;0;UN", "Zero physical stock retained");
        Assert(rows[2] == "MAG1HT;NEW;\"Nouveau; \"\"composant\"\" µH\";0;G00201;10;10;UN", "New article, GPAO cap and CSV escaping");
        var zeroGpaoRows = await InventoryCsvGenerator.BuildLinesAsync(
            new Dictionary<string, int> { { "ZERO", 12 }, { "BOTHZERO", 0 }, { "DMYONLY", 0 }, { "OLD", 4 } },
            code => Task.FromResult(code == "ZERO" || code == "BOTHZERO"
                ? new Article { GpaoQuantity = 0 } : articles[code]));
        Assert(zeroGpaoRows.Count == 2, "Zero GPAO stock excluded for positive and zero ASMPT stock");
        Assert(zeroGpaoRows[0] == rows[0] && zeroGpaoRows[1] == rows[1],
            "Zero ASMPT stock retained and other comparison and sorting rules unchanged");
        var missingRows = await InventoryCsvGenerator.BuildLinesAsync(quantities,
            code => Task.FromResult(code == "NEW" ? null : articles[code]));
        Assert(missingRows.Count == 2 && !missingRows.Exists(row => row.StartsWith("MAG1HT;NEW;")),
            "Missing GPAO article excluded without error");
        Assert(missingRows.Contains("MAG1HT;OLD;Existant;0;G00101;30;4;UN"),
            "Generation continues for other articles");
        Assert(!rows.Exists(row => row.StartsWith("MAG1HT;GPAOONLY;")),
            "GPAO-only article excluded because ASMPT is the source");
        var emptyRows = await InventoryCsvGenerator.BuildLinesAsync(quantities,
            code => Task.FromResult<Article>(null));
        Assert(emptyRows.Count == 0, "No common articles produces no rows without error");
        bool failed = false;
        try { await InventoryCsvGenerator.BuildLinesAsync(quantities, code => { throw new Exception("API unavailable"); }); }
        catch (Exception) { failed = true; }
        Assert(failed, "API failure aborts generation");
    }
    static void Assert(bool success, string name)
    {
        if (!success) throw new Exception(name);
        Console.WriteLine("PASS: " + name);
    }
}
