using LO_ASM_MATE_CONTROL.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services.AsmXmlFiles
{
    public interface IAsmFileGen
    {
        void GenerateXmlFile(List<ComponentStockLocation> componentStockLocations, string filePath);
    }
}
