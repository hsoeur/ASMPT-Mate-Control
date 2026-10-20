//-----------------------------------------------------------------------
// <copyright file="OibDiscoveryHelper.cs" company="ASM Assembly Systems GmbH & Co. KG">
//     Copyright (c) ASM Assembly Systems GmbH & Co. KG. All rights reserved.
// </copyright>
// <summary>
//    This code is part of the OIB SDK. 
//    Use and redistribution is free without any warranty. 
// </summary>
//-----------------------------------------------------------------------

#region Namespace

using System;

using www.siplace.com.OIB._2008._05.ConfigurationManager.Contracts.Data;
using www.siplace.com.OIB._2008._05.ServiceLocator.Contracts.Data;

#endregion

namespace OIB.Tutorial
{
    #region OibDiscoryHelperException

    public class OibDiscoryHelperException : Exception
    {
        public OibDiscoryHelperException(string message)
            : base(message)
        {
        }
        public OibDiscoryHelperException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    #endregion

    /// <summary>
    /// Helper class to discover a service depending on a factory layout element.
    /// </summary>
    public class OibDiscoveryHelper
    {
        private const string s_ConfigurationManagerServiceName = "ConfigurationManager";

        /// <summary>
        /// Returns the MEX uri of the service
        /// </summary>
        /// <param name="serviceLocatorMexUri">The MEX uri of the OIB Service Locator</param>
        /// <param name="serviceName">The name of the service, e.g. "SiplaceSetupCenter.SetupCenterControl"</param>
        /// <param name="factoryLayoutElementName">The factory layout element name, e.g. "Line 1"</param>
        /// <param name="factoryLayoutElementType">The factory layout element type, e.g. "ProductionLine"</param>
        /// <param name="useClientAuthentication">If true use client authentication</param>
        /// <returns></returns>
        public static Uri GetServiceMexUri(
          Uri serviceLocatorMexUri,
          string serviceName,
          string factoryLayoutElementName,
          string factoryLayoutElementType,
          bool useClientAuthentication)
        {
            ServiceDescription[] serviceDescriptions;
            ConfigurationManager configurationManager;
            Isa95Base factoryElement = null;
            Uri configurationManagerMexUri;

            // Access OIB Service Locator Service for the configuration manager mex uri
            // and for all registered services of the requested type.
            try
            {
                using (OibServiceLocatorHelper serivceLocatorHelper =
                    new OibServiceLocatorHelper(serviceLocatorMexUri, useClientAuthentication))
                {
                    configurationManagerMexUri = serivceLocatorHelper.GetMexUriForServiceName(s_ConfigurationManagerServiceName);
                    serviceDescriptions = serivceLocatorHelper.GetAllServiceDescriptionFroServiceName(serviceName);
                }
            }
            catch (Exception ex)
            {
                throw new OibDiscoryHelperException("Failure accessing OIB Service Locator: " + ex.Message, ex);
            }

            if (serviceDescriptions.Length == 0)
            {
                throw new OibDiscoryHelperException(string.Format("No OIB Adapter of type '{0}' is known by OIB Service Locator", serviceName));
            }

            // Access OIB Configuration Manager Service for the OIB Factory Layout
            // and to translate name and type the Factory Layout element itself.
            try
            {
                using (OibConfigurationManagerHelper configurationManagerHelper
                        = new OibConfigurationManagerHelper(configurationManagerMexUri, useClientAuthentication))
                {
                    configurationManager = configurationManagerHelper.Proxy.Load();

                    int objectId = configurationManagerHelper.Proxy.GetObjectId(
                        factoryLayoutElementName,
                        (ObjectType)Enum.Parse(typeof(ObjectType),
                        factoryLayoutElementType));

                    if (objectId >= 0)
                        factoryElement = configurationManagerHelper.Proxy.GetObject(objectId);
                }
            }
            catch (Exception ex)
            {
                throw new OibDiscoryHelperException("Failure accessing OIB Configuration Manager to load the factory model: " + ex.Message, ex);
            }

            if (factoryElement == null)
                throw new OibDiscoryHelperException(string.Format("Factory Layout element with name '{0}' and type '{1}' not known.", factoryLayoutElementName, factoryLayoutElementType));


            return GetServiceMex(configurationManager, factoryElement, serviceDescriptions);
        }

        /// <summary>
        /// Get the MES Uri for a factory layout element.
        /// </summary>
        /// <param name="configurationManager">The configuration manager from OIB Configuration Manager.</param>
        /// <param name="factoryLayoutElement">The factory layout element.</param>
        /// <param name="serviceDescriptions">The service descriptions of all services of the required type
        /// managed by OIB Service Locator</param>
        /// <returns></returns>
        public static Uri GetServiceMex(ConfigurationManager configurationManager, Isa95Base factoryLayoutElement, ServiceDescription[] serviceDescriptions)
        {
            while (factoryLayoutElement != null)
            {
                // 2. Check one of the service has the factory layout element assigned
                foreach (ServiceDescription description in serviceDescriptions)
                {
                    foreach (Isa95Base isa95Base in description.Configurations)
                    {
                        if (isa95Base.ID == factoryLayoutElement.ID)
                        {
                            return description.MetadataEndpoints[0];
                        }
                    }
                }

                // 3. None of the services has this factory layout element assigned: 
                //   Try again with its parent element.
                factoryLayoutElement = FindParent(configurationManager, factoryLayoutElement);
            }

            return null;
        }

        /// <summary>
        /// Finds the parent of the given factory layout element.
        /// </summary>
        /// <param name="configurationManager">The full factory layout</param>
        /// <param name="factoryLayoutElement">The factory layout element</param>
        /// <returns>The parent of the given factory layout element or null, on no parent was found.</returns>
        private static Isa95Base FindParent(ConfigurationManager configurationManager, Isa95Base factoryLayoutElement)
        {
            if (configurationManager == null)
                throw new ArgumentNullException("configurationManager");

            if (factoryLayoutElement == null)
                throw new ArgumentNullException("factoryLayoutElement");

            if (configurationManager.Enterprise == null)
            {
                return null;
            }

            foreach (Site site in configurationManager.Enterprise.Sites)
            {
                if (site.ID == factoryLayoutElement.ID)
                    return configurationManager.Enterprise;

                foreach (Area area in site.Areas)
                {
                    if (area.ID == factoryLayoutElement.ID)
                        return site;

                    foreach (ProductionLine productionLine in area.ProductionLines)
                    {
                        if (productionLine.ID == factoryLayoutElement.ID)
                            return area;

                        foreach (WorkCell workCell in productionLine.WorkCells)
                        {
                            if (workCell.ID == factoryLayoutElement.ID)
                            {
                                return productionLine;
                            }

                            foreach (WorkCell subWorkCell in workCell.SubWorkCells)
                            {
                                if (subWorkCell.ID == factoryLayoutElement.ID)
                                    return workCell;
                            }
                        }
                    }
                }
                foreach (ProductionLine productionLine in site.ProductionLines)
                {
                    if (productionLine.ID == factoryLayoutElement.ID)
                        return site;

                    foreach (WorkCell workCell in productionLine.WorkCells)
                    {
                        if (workCell.ID == factoryLayoutElement.ID)
                        {
                            return productionLine;
                        }

                        foreach (WorkCell subWorkCell in workCell.SubWorkCells)
                        {
                            if (subWorkCell.ID == factoryLayoutElement.ID)
                                return workCell;
                        }
                    }
                }
            }

            return null;
        }
    }
}