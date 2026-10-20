//-----------------------------------------------------------------------
// <copyright file="OibSerivceLocatorHelper.cs" company="ASM Assembly Systems GmbH & Co. KG">
//     Copyright (c) ASM Assembly Systems GmbH & Co. KG. All rights reserved.
// </copyright>
// <summary>
//    This code is part of the OIB SDK. 
//    Use and redistribution is free without any warranty. 
// </summary>
//-----------------------------------------------------------------------

#region using

using System;
using www.siplace.com.OIB._2008._05.ConfigurationManager.Contracts.Data;
using www.siplace.com.OIB._2008._05.ServiceLocator.Contracts.Data;
using www.siplace.com.OIB._2008._05.ServiceLocator.Contracts.Service;
using ServiceDescription = www.siplace.com.OIB._2008._05.ServiceLocator.Contracts.Data.ServiceDescription;

#endregion

namespace OIB.Tutorial
{
    /// <summary>
    /// Service locator helper managing the service locator proxy using the http protocol. Use the pattern:
    /// 
    /// using (SerivceLocatorHelper serivceLocatorHelper = new SerivceLocatorHelper(serviceLocatorMexUri))
    /// {
    ///     ...
    /// } // end of using closes connection (or aborts it if they are faulted).
    /// </summary>
    public class OibServiceLocatorHelper : ServiceHelper<IServiceLocator>
    {
        #region Constructor

        public OibServiceLocatorHelper(Uri mexUri, bool useClientAuthentication)
            : base(mexUri, useClientAuthentication)
        {
        }

        #endregion

        /// <summary>
        /// Returns the MEX address for a service name. If the service is installed more than one time, the first service is returned.
        /// </summary>
        /// <param name="serviceName">Name of the service, e.g. "ConfigurationManager"</param>
        /// <returns>The mex address, e.g. http://localhost:1405/Asm.As.Oib.ConfigurationManager/mex or null, if the service is not known by OIB.</returns>
        public Uri GetMexUriForServiceName(string serviceName)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);


            ServiceMatchCriteria criteria = new ServiceMatchCriteria();
            criteria.ServiceName = serviceName;
            ServiceDescription[] serviceDescriptions = Proxy.FindServices(criteria);

            if (serviceDescriptions.Length == 0 || serviceDescriptions[0].MetadataEndpoints.Length == 0)
                return null;

            foreach (Uri mexUri in serviceDescriptions[0].MetadataEndpoints)
            {
                if (mexUri.Scheme == Uri.UriSchemeHttp)
                {
                    return mexUri;
                }
            }

            return serviceDescriptions[0].MetadataEndpoints[0];
        }

        public ServiceDescription[] GetAllServiceDescriptionFroServiceName(string serviceName)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            ServiceMatchCriteria criteria = new ServiceMatchCriteria();
            criteria.ServiceName = serviceName;
            return Proxy.FindServices(criteria);
        }

        #region DOC_MEX_FOR_URI

        public Uri GetMexUriForFactoryLayoutElement(
            string serviceName,
            ConfigurationManager configurationManager,
            Isa95Base factoryLayoutElement)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            // 1. Get all services for the given service name.
            ServiceMatchCriteria criteria = new ServiceMatchCriteria();
            criteria.ServiceName = serviceName;
            ServiceDescription[] services = Proxy.FindServices(criteria);

            while (factoryLayoutElement != null)
            {
                // 2. Check one of the service has the factory layout element assigned
                foreach (ServiceDescription description in services)
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

        #endregion

        /// <summary>
        /// Resets the service - factory layout element assignments.
        /// </summary>
        public void ResetConfiguration()
        {
            foreach (ServiceDescription serviceDescription in Proxy.GetAllServices())
            {
                if (serviceDescription.FactoryElementId != 0)
                {
                    // need to clear both properties (legacy Configuration as well as newer FactoryElementId)
                    // the change is valid from R18-1, OIB 5.3
                    serviceDescription.Configuration = null;
                    serviceDescription.FactoryElementId = 0;

                    // implement changes
                    Proxy.SaveServiceDescription(serviceDescription);
                }
            }
        }

        /// <summary>
        /// Create the service - factory element assignments.
        /// </summary>
        public void CreateTutorialServiceConfiguration()
        {
            using (OibConfigurationManagerHelper configurationManagerHelper =
                new OibConfigurationManagerHelper(GetMexUriForServiceName("ConfigurationManager"), UseClientAuthentication))
            {
                // assign the SPI service to the site 'Munich' node
                AssignFactoryElementToService(configurationManagerHelper, "SIPLACE.Pro.SPI", "Munich", ObjectType.Site);

                // assign the SPI.Optimizer service to the site 'Munich' node
                AssignFactoryElementToService(configurationManagerHelper, "SIPLACE.Pro.Optimizer", "Munich", ObjectType.Site);

                // assign the SPI.LineControl service to the production line 'Line 1' node
                AssignFactoryElementToService(configurationManagerHelper, "SIPLACE.Pro.LineControl", "Line 1", ObjectType.ProductionLine);

                // assign the Setup Center service to the production line 'Line 2' node
                AssignFactoryElementToService(configurationManagerHelper, "SIPLACE.SetupCenter.MaterialControl", "Line 2", ObjectType.ProductionLine);
            }
        }

        /// <summary>
        /// Assigns one service to a factory layout element.
        /// </summary>
        /// <param name="configurationManagerHelper">The helper wrapping the configuration manager</param>
        /// <param name="serviceName">The service to be assigned.</param>
        /// <param name="factoryElementName">The name of the factory layout element.</param>
        /// <param name="factoryElementType">The type of the factory layout element.</param>
        private void AssignFactoryElementToService(OibConfigurationManagerHelper configurationManagerHelper, string serviceName, string factoryElementName, ObjectType factoryElementType)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            // 1. Get the service description for the service
            ServiceMatchCriteria criteria = new ServiceMatchCriteria();
            criteria.ServiceName = serviceName;
            ServiceDescription[] services = Proxy.FindServices(criteria);

            if (services.Length >= 1)
            {
                // 2. Get the factory layout element object ID for its name and type
                int nObjectId = configurationManagerHelper.Proxy.GetObjectId(factoryElementName, factoryElementType);
                if (nObjectId > 0)
                {
                    // 3. Get the factory layout element by its object ID
                    Isa95Base factoryElement = configurationManagerHelper.Proxy.GetObject(nObjectId);

                    // 4. Assign the service to the layout element
                    services[0].Configuration = factoryElement;

                    // 5. Save the assignment
                    Proxy.SaveServiceDescription(services[0]);
                }
            }
            else
            {
                throw new ApplicationException(string.Format("Service '{0}' not known by OIB", serviceName));
            }
        }
        #region DOC_FIND_PARENT

        /// <summary>
        /// Finds the parent of the given factory layout element.
        /// </summary>
        /// <param name="configurationManager">The full factory layout</param>
        /// <param name="factoryLayoutElement">The factory layout element</param>
        /// <returns>The parent of the given factory layout element or null, on no parent was found.</returns>
        public Isa95Base FindParent(ConfigurationManager configurationManager, Isa95Base factoryLayoutElement)
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

        #endregion
    }
}