//-----------------------------------------------------------------------
// <copyright file="ServiceHelper.cs" company="ASM Assembly Systems GmbH & Co. KG">
//     Copyright (c) ASM Assembly Systems GmbH & Co. KG. All rights reserved.
// </copyright>
// <summary>
//    This code is part of the OIB SDK. 
//    Use and redistribution is free without any warranty. 
// </summary>
//-----------------------------------------------------------------------

#region Namespace

using System;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel;

#endregion

namespace OIB.Tutorial
{
    public class ServiceHelper<T> : IDisposable where T : class
    {
        #region Fields

        private bool _Disposed;
        private T _Proxy;
        protected bool UseClientAuthentication;

        #endregion

        #region Properties

        public T Proxy
        {
            get
            {
                if (_Disposed)
                    throw new ObjectDisposedException(GetType().Name);

                return _Proxy;
            }
        }

        public bool Disposed
        {
            get { return _Disposed; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor using net.tcp transport protocol
        /// </summary>
        /// <param name="mexUri">Uri containing the mex address of the service</param>
        /// <param name="useClientAuthentication">If true use client authentication</param>
        public ServiceHelper(Uri mexUri, bool useClientAuthentication)
        {
            if (mexUri == null)
                throw new ArgumentNullException("mexUri");

            UseClientAuthentication = useClientAuthentication;
            InitializeProxy(mexUri, "net.tcp");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mexUri">Uri containing the mex address of the service</param>
        /// <param name="transportProtocolName">The protocol used (e.g. net.tcp or http)</param>
        public ServiceHelper(Uri mexUri, string transportProtocolName)
        {
            if (mexUri == null)
                throw new ArgumentNullException("mexUri");

            if (transportProtocolName == null)
                throw new ArgumentNullException("transportProtocolName");

            InitializeProxy(mexUri, transportProtocolName);
        }

        /// <summary>
        /// The service helper gets the instantiated Web service proxy.
        /// MEX is not used.
        /// </summary>
        /// <param name="proxy">The ready instantiated Web service proxy.</param>
        public ServiceHelper(T proxy)
        {
            _Proxy = proxy;
        }
        #endregion

        #region Private Methods

        private void InitializeProxy(Uri mexUri, string transportProtocolName)
        {
            #region DOC_INSTANTIATE_PROXY_FROM_MEX
            Binding binding;
            // Get all endpoints from the web service with the given mex-address
            if (mexUri.Scheme == Uri.UriSchemeNetTcp)
            {
                binding = new NetTcpBinding(SecurityMode.None);
                AdjustBindingParameters(binding, false);
            }
            else if (mexUri.Scheme == Uri.UriSchemeHttp)
            {
                binding = new WSHttpBinding(SecurityMode.None);
                AdjustBindingParameters(binding, false);
            }
            else
            {
                throw new ArgumentException("mexUri");
            }

            MetadataExchangeClient client = new MetadataExchangeClient(binding);
            MetadataSet mds = client.GetMetadata(mexUri, MetadataExchangeClientMode.MetadataExchange);
            MetadataImporter mdi = new WsdlImporter(mds);

            ServiceEndpoint selectedEndpoint = null;

            // Select the endpoint with the required protocol
            foreach (ServiceEndpoint endpoint in mdi.ImportAllEndpoints())
            {
                if (endpoint.Contract.Name == typeof(T).Name && endpoint.Address.Uri.Scheme.Equals(transportProtocolName))
                {
                    // In case there are more than one endpoint for this contract and this protocol available, 
                    // prefer the reliable endpoint.
                    bool bSelectedBindingIsReliable = false;
                    if (selectedEndpoint != null)
                    {
                        if (selectedEndpoint.Binding is WSHttpBinding)
                        {
                            if (((WSHttpBinding)selectedEndpoint.Binding).ReliableSession.Enabled)
                            {
                                bSelectedBindingIsReliable = true;
                            }
                        }
                        if (selectedEndpoint.Binding is NetTcpBinding)
                        {
                            if (((NetTcpBinding)selectedEndpoint.Binding).ReliableSession.Enabled)
                            {
                                bSelectedBindingIsReliable = true;
                            }
                        }
                    }

                    if (!bSelectedBindingIsReliable)
                    {
                        selectedEndpoint = endpoint;
                    }
                }
            }

            if (selectedEndpoint != null)
            {
                AdjustBindingParameters(selectedEndpoint.Binding, true);

                // Activate secure communication, thereto Check if secure endpoint is used
                var strEndpoint = !selectedEndpoint.Address.ToString().ToLowerInvariant().EndsWith("secure")
                    ? $"{selectedEndpoint.Address}Secure"
                    : selectedEndpoint.Address.ToString();

                // Add endpoint identity to provide information about service identity and open secure communication to service
                EndpointAddress address = new EndpointAddress(new Uri(strEndpoint), EndpointIdentity.CreateDnsIdentity("ASM.SW.Service"));

                var factory = new ChannelFactory<T>(selectedEndpoint.Binding, address);

                // Revocation list not used
                factory.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

                #region Use Client Authentication

                // Usage of client authentication
                if (UseClientAuthentication)
                {
                    // Add client certificate to authenticate to service

                    // **** IMPORTANT ****
                    //- the certificate value [Certificate Value used for OIB installation] must be set to that one selected during OIB installation
                    //- uncomment to use client authentication
                    //factory.Credentials.ClientCertificate.SetCertificate(
                    //    StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySubjectName, "[Certificate Value used for OIB installation]");
                }

                #endregion

                _Proxy = factory.CreateChannel();
            }

            if (_Proxy == null)
            {
                throw new ApplicationException(
                    string.Format("Web service '{0}' does not support an endpoint with contract '{1}' and transport protocol '{2}'",
                        mexUri.AbsoluteUri, typeof(T).Name, transportProtocolName));
            }

            #endregion
        }

        private void AdjustBindingParameters(Binding binding, bool useSecurity)
        {
            WSHttpBinding bindHttp = binding as WSHttpBinding;
            NetTcpBinding bindTcp = binding as NetTcpBinding;
            if (bindHttp != null)
            {
                bindHttp.MaxBufferPoolSize = 524288;
                bindHttp.MaxReceivedMessageSize = 2147483647;
                bindHttp.OpenTimeout = new TimeSpan(0, 0, 0, 10);
                bindHttp.CloseTimeout = new TimeSpan(0, 0, 0, 10);
                bindHttp.SendTimeout = new TimeSpan(0, 0, 1, 0);
                bindHttp.ReaderQuotas.MaxArrayLength = 2147483647;
                bindHttp.ReaderQuotas.MaxBytesPerRead = 2147483647;
                bindHttp.ReaderQuotas.MaxDepth = 2147483647;
                bindHttp.ReaderQuotas.MaxNameTableCharCount = 2147483647;
                bindHttp.ReaderQuotas.MaxStringContentLength = 2147483647;

                // Use message security
                bindHttp.Security.Mode = useSecurity ? SecurityMode.Message : SecurityMode.None;

                // Usage of client authentication  binding properties
                bindHttp.Security.Message.ClientCredentialType = UseClientAuthentication ? MessageCredentialType.Certificate : MessageCredentialType.None;
            }

            if (bindTcp != null)
            {
                bindTcp.MaxBufferPoolSize = 524288;
                bindTcp.MaxBufferSize = 2147483647;
                bindTcp.MaxReceivedMessageSize = 2147483647;
                bindTcp.OpenTimeout = new TimeSpan(0, 0, 0, 10);
                bindTcp.CloseTimeout = new TimeSpan(0, 0, 0, 10);
                bindTcp.SendTimeout = new TimeSpan(0, 0, 1, 0);
                bindTcp.ReaderQuotas.MaxArrayLength = 2147483647;
                bindTcp.ReaderQuotas.MaxBytesPerRead = 2147483647;
                bindTcp.ReaderQuotas.MaxDepth = 2147483647;
                bindTcp.ReaderQuotas.MaxNameTableCharCount = 2147483647;
                bindTcp.ReaderQuotas.MaxStringContentLength = 2147483647;

                // Use transport security
                if (useSecurity)
                {
                    bindTcp.Security.Mode = SecurityMode.Transport;
                    bindTcp.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
                }

                // Usage of client authentication binding properties
                bindTcp.Security.Transport.ClientCredentialType = UseClientAuthentication
                    ? TcpClientCredentialType.Certificate
                    : TcpClientCredentialType.None;
            }

        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _Disposed = true;

            #region DOC_CLOSING_THE_PROXY_CONNECTION

            // Close the proxy. 
            // In case the connection is faulted or an exception has occurred: abort the proxy.
            IClientChannel channel = _Proxy as IClientChannel;
            if (channel == null)
                return;

            if (channel.State != CommunicationState.Faulted)
            {
                try
                {
                    channel.Close();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    try
                    {
                        channel.Abort();
                    }
                    catch
                    {
                        //
                    }
                }
            }
            else
            {
                try
                {
                    channel.Abort();
                }
                catch
                {
                    //
                }
            }
            _Proxy = null;

            #endregion
        }

        #endregion
    }
}