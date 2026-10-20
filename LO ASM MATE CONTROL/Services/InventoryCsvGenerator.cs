using LO_ASM_MATE_CONTROL.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services
{
    public static class InventoryCsvGenerator
    {
        public static bool IsEligiblePackagingUnit(string uid, string extra2, string magasin)
        {
            if ((uid ?? string.Empty).Trim().StartsWith("DMY", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(extra2))
                return false;

            var location = (magasin ?? string.Empty).Trim();
            return location.StartsWith("G", StringComparison.OrdinalIgnoreCase)
                || location.StartsWith("C6", StringComparison.OrdinalIgnoreCase)
                || location.StartsWith("C7", StringComparison.OrdinalIgnoreCase)
                || location.StartsWith("C8", StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<List<string>> BuildLinesAsync(
            IDictionary<string, int> quantities, Func<string, Task<Article>> getArticle)
        {
            var rows = new List<string[]>();
            foreach (var entry in quantities.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                var article = await getArticle(entry.Key);
                if (article == null)
                    continue; // Exporter uniquement les références présentes dans ASMPT et GPAO.

                if (article.GpaoQuantity == 0)
                    continue; // Exclure avant comparaison ; un stock ASMPT nul reste pris en compte.

                // Format GPAO sans en-tête : dépôt;article;désignation;0;emplacement;stock GPAO;inventaire;unité.
                rows.Add(new[]
                {
                    "MAG1HT", entry.Key, article.Designation ?? string.Empty, "0",
                    (article.Store ?? string.Empty).Trim(),
                    article.GpaoQuantity.ToString(CultureInfo.InvariantCulture),
                    Math.Min(entry.Value, article.GpaoQuantity).ToString(CultureInfo.InvariantCulture), "UN"
                });
            }
            return rows.OrderBy(row => row[4], StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row[1], StringComparer.OrdinalIgnoreCase)
                .Select(row => string.Join(";", row.Select(EscapeField))).ToList();
        }

        private static string EscapeField(string value)
        {
            return value.IndexOfAny(new[] { ';', '"', '\r', '\n' }) >= 0
                ? "\"" + value.Replace("\"", "\"\"") + "\""
                : value;
        }
    }
}
