//-----------------------------------------------------------------------
// <copyright file="OibConfigurationManagerHelper.cs" company="ASM Assembly Systems GmbH & Co. KG">
//     Copyright (c) ASM Assembly Systems GmbH & Co. KG. All rights reserved.
// </copyright>
// <summary>
//    This code is part of the OIB SDK. 
//    Use and redistribution is free without any warranty. 
// </summary>
//-----------------------------------------------------------------------

#region Namespace

using System;

using www.siplace.com.OIB._2008._05.ConfigurationManager.Contracts.Service;
using www.siplace.com.OIB._2008._05.ConfigurationManager.Contracts.Data;
using System.Security.Policy;

#endregion

namespace OIB.Tutorial
{
    /// <summary>
    /// Service locator helper managing the service locator proxy using http protocol. Use the pattern:
    /// 
    /// using (ConfigurationManagerHelper configurationManagerHelper = new ConfigurationManagerHelper(configurationManagerMexUri))
    /// {
    ///     ...
    /// } // end of using closes open connections (or aborts them if they are faulted.
    /// </summary>
    public class OibConfigurationManagerHelper : ServiceHelper<IConfigurationManager>
    {
        #region Constructor

        public OibConfigurationManagerHelper(Uri mexUri, bool useClientAuthentication) : base(mexUri, useClientAuthentication)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes the OIB Factory Layout and renames the enterprise root node to its default name 'Enterprise'
        /// </summary>
        public void ResetFactoryLayout()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            // 1. Load the configuration manager which holds the OIB configuration
            ConfigurationManager configurationManager = Proxy.Load();

            // 2. Rename the enterprise object
            Enterprise enterprise = configurationManager.Enterprise;
            enterprise.Name = "Enterprise";

            // 3. Delete all nodes below the enterprise node.
            enterprise.Sites.Clear();
            Proxy.Save(configurationManager);
        }

        /// <summary>
        /// Creates the OIB Factory Layout for the tutorial consisting of 
        /// -> Enterprise = Nano Placement Inc.
        /// -> Site = Munich
        /// -> Production Line = Line 1
        /// -> Production Line = Line 2
        /// </summary>
        public void CreateTutorialFactoryLayout()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            // 1. Load the configuration manager which holds the OIB configuration
            ConfigurationManager configurationManager = Proxy.Load();

            // 2. Rename the enterprise object
            Enterprise enterprise = configurationManager.Enterprise;
            enterprise.Name = "Nano Placement Inc.";
            enterprise.Sites.Clear();   // delete old configuration, if any, to be on the save side.

            // 3. Create site 'Munich' and link it to the enterprise element
            www.siplace.com.OIB._2008._05.ConfigurationManager.Contracts.Data.Site site = new www.siplace.com.OIB._2008._05.ConfigurationManager.Contracts.Data.Site();
            site.Name = "Munich";
            site.ProductionLines = new ProductionLineCollection();
            enterprise.Sites.Add(site);

            // 4. Create production line 'Line 1' and link it to the site 'Munich'
            ProductionLine line1 = new ProductionLine();
            line1.Name = "Line 1";
            site.ProductionLines.Add(line1);

            // 5. Create production line 'Line 2' and link it to the site 'Munich'
            ProductionLine line2 = new ProductionLine();
            line2.Name = "Line 2";
            site.ProductionLines.Add(line2);

            // 6. overwrite old config with new configuration.
            Proxy.Save(configurationManager);
        }

        #endregion
    }
}