using Asm.As.Oib.Common.Utilities;
using Asm.As.Oib.SiplaceSetupCenter.Proxy.Business.Objects;
using OIB.Tutorial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services.Managers
{
    /// <summary>
    /// Classe permettant d'utiliser le Framework OIB pour effectuer des opérations dans l'écosystème ASMPT.
    /// </summary>
    public class SetupCenterManager
    {
        private const string s_SetupCenterServiceName = "SIPLACE.SetupCenter.MaterialControl";
        private static SiplaceSetupCenter _setupCenterProxy;
        public static SiplaceSetupCenter GetSetupCenterProxy()
        {
            if (_setupCenterProxy == null)
            {
                // Get the OIB Setup Center Adapter MEX Uri.
                Uri setupCenterMexUri = OibDiscoveryHelper.GetServiceMexUri(
                    new Uri("http://srv-asmpt:1405/Asm.As.Oib.ServiceLocator/mex"),
                    s_SetupCenterServiceName,
                    "HorizonTelecom",
                    "Site",
                    false);

                string scEndpoint = setupCenterMexUri.ToString().Replace(@"/mex", "");

                // Activate secure communication, thereto Check if secure endpoint is used
                var strEndpoint = !scEndpoint.ToLowerInvariant().EndsWith("secure")
                    ? $"{scEndpoint}Secure"
                    : scEndpoint;

                _setupCenterProxy = new SiplaceSetupCenter(new EndpointAddress(strEndpoint), EndpointHelper.CreateBindingFromEndpointString(strEndpoint));

            }
            return _setupCenterProxy;
        }
    }
}
