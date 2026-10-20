using LO_ASM_MATE_CONTROL.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace LO_ASM_MATE_CONTROL.Services.AsmXmlFiles
{
    /// <summary>
    /// Classe responsable de la génération des fichiers XMLS pour les Localisations Magasins des bobines.
    /// </summary>
    public class AsmFileGen : IAsmFileGen
    {
        /// <summary>
        /// Méthode de génération des xmls pour les emplacements magasins. 
        /// </summary>
        /// <param name="componentStockLocations"></param>
        /// <param name="filePath"></param>
        public void GenerateXmlFile(List<ComponentStockLocation> componentStockLocations, string filePath)
        {
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("LineStockLocations",
                    new XAttribute("fileVersion", "1.0.1.1"),
                    new XElement("LineStockLocation",
                        new XAttribute("line", "HORIZON TELECOM\\MO08"),
                        new XElement("ComponentStockLocations",
                            from c in componentStockLocations
                            select new XElement("ComponentStockLocation",
                                new XAttribute("component", c.Component),
                                new XAttribute("stocklocation", c.StockLocation)
                            )
                        )
                    )
                )
            );

            doc.Save(filePath);
        }
    }
}
