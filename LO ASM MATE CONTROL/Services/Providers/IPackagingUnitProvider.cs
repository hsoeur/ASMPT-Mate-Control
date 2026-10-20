using Asm.As.Oib.SiplaceSetupCenter.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services.Providers
{
    public interface IPackagingUnitProvider
    {
        List<PackagingUnit> GetAllPackagingUnits();
        List<PackagingUnit> GetPackagingUnits(string keyword);

        Task<int> ResetPackagingUnitQuantitiesAsync(string keyword);
        Task<int> UpdatePackagingUnitsMagasinAsync(string codeArticle, string codeMagasin);
    }
}
