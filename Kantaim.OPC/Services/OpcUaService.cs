using Opc.Ua.Client;
using Opc.Ua;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Kantaim.OPC.Services
{
    public class OpcSettings
    {
        public string EndpointUrl { get; set; } = string.Empty;
    }

    public class OpcUaService
    {
        private Session _session;
        private readonly string _endpointUrl;

        //public OpcUaService(IOptions<OpcSettings> options)
        public OpcUaService()
        {
            //_endpointUrl = options.Value.EndpointUrl;
            IConfigurationRoot config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();
            _endpointUrl = config["OpcSettings:EndpointUrl"];
        }

        public async Task<bool> ConnectAsync()
        {
            if (string.IsNullOrEmpty(_endpointUrl))
            {
                throw new InvalidOperationException("L'URL de l'endpoint OPC n'est pas configurée.");
            }

            return await ConnectAsync(_endpointUrl);
        }
        //private async Task ConnectAsync(string endpointUrl)
        //{
        //    var config = new ApplicationConfiguration
        //    {
        //        ApplicationName = "BlazorOpcUaClient",
        //        ApplicationType = ApplicationType.Client,
        //        SecurityConfiguration = new SecurityConfiguration
        //        {
        //            ApplicationCertificate = new CertificateIdentifier(),
        //            AutoAcceptUntrustedCertificates = true
        //        },
        //        TransportConfigurations = new TransportConfigurationCollection(),
        //        TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
        //        ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
        //    };

        //    await config.Validate(ApplicationType.Client);

        //    var endpoint = CoreClientUtils.SelectEndpoint(endpointUrl, useSecurity: false);
        //    var endpointConfiguration = EndpointConfiguration.Create(config);
        //    var endpointDescription = new ConfiguredEndpoint(null, endpoint, endpointConfiguration);

        //    _session = await Session.Create(
        //        config,
        //        endpointDescription,
        //        false,
        //        false,
        //        config.ApplicationName,
        //        60000,
        //        new UserIdentity(new AnonymousIdentityToken()),
        //        null);
        //}

        public async Task<bool> ConnectAsync(string endpointUrl)
        {
            try
            {
                var config = new ApplicationConfiguration
                {
                    ApplicationName = "BlazorOpcUaClient",
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier(),
                        AutoAcceptUntrustedCertificates = true
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
                };

                await config.Validate(ApplicationType.Client);

                var endpoint = CoreClientUtils.SelectEndpoint(endpointUrl, useSecurity: false);
                var endpointConfiguration = EndpointConfiguration.Create(config);
                var endpointDescription = new ConfiguredEndpoint(null, endpoint, endpointConfiguration);

                _session = await Session.Create(
                    config,
                    endpointDescription,
                    false,
                    false,
                    config.ApplicationName,
                    60000,
                    new UserIdentity(new AnonymousIdentityToken()),
                    null);

                // ✅ Connexion réussie
                if (_session == null || !_session.Connected)
                    throw new Exception("La session OPC UA est nulle ou non connectée.");

                return true;
            }
            catch (ServiceResultException ex)
            {
                // Lancer une exception spécifique OPC UA
                throw new Exception($"Erreur OPC UA : {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Lancer une exception générique
                throw new Exception($"Erreur de connexion OPC UA : {ex.Message}", ex);
            }
        }



        //public async Task<string> ReadNodeValueAsync(string nodeId)
        //{
        //    var nodeToRead = new ReadValueId
        //    {
        //        NodeId = new NodeId(nodeId),
        //        AttributeId = Attributes.Value
        //    };

        //    var nodesToRead = new ReadValueIdCollection { nodeToRead };
        //    var requestHeader = new RequestHeader();

        //    var response = await _session.ReadAsync(
        //        requestHeader,
        //        0,
        //        TimestampsToReturn.Neither,
        //        nodesToRead,
        //        CancellationToken.None);

        //    if (response.Results[0].StatusCode == Opc.Ua.StatusCodes.Good)
        //    {
        //        return response.Results[0].Value?.ToString();
        //    }

        //    return $"Error: {response.Results[0].StatusCode}";
        //}

        public async Task<string> ReadNodeValueAsync(string nodeId)
        {
            try
            {
                if (_session == null || !_session.Connected)
                    throw new Exception("La session OPC UA est nulle ou non connectée.");

                var nodeToRead = new ReadValueId
                {
                    NodeId = new NodeId(nodeId),
                    AttributeId = Attributes.Value
                };

                var nodesToRead = new ReadValueIdCollection { nodeToRead };
                var requestHeader = new RequestHeader();

                var response = await _session.ReadAsync(
                    requestHeader,
                    0,
                    TimestampsToReturn.Neither,
                    nodesToRead,
                    CancellationToken.None);

                if (response.Results[0].StatusCode == Opc.Ua.StatusCodes.Good)
                {
                    return response.Results[0].Value?.ToString() ?? string.Empty;
                }

                throw new Exception($"Erreur lors de la lecture du nœud {nodeId} : {response.Results[0].StatusCode}");
            }
            catch (ServiceResultException ex)
            {
                throw new Exception($"Erreur OPC UA lors de la lecture du nœud {nodeId} : {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur générale lors de la lecture du nœud {nodeId} : {ex.Message}", ex);
            }
        }

        public async Task<bool> WriteNodeValueAsync(string nodeId, object value)
        {
            var nodeToWrite = new WriteValue
            {
                NodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value,
                Value = new DataValue
                {
                    Value = value,
                    StatusCode = Opc.Ua.StatusCodes.Good,
                    SourceTimestamp = DateTime.UtcNow
                }
            };

            var nodesToWrite = new WriteValueCollection { nodeToWrite };
            var requestHeader = new RequestHeader();

            var response = await _session.WriteAsync(
                requestHeader,
                nodesToWrite,
                CancellationToken.None);

            if (response.Results[0] == Opc.Ua.StatusCodes.Good)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            _session?.Close();
            _session?.Dispose();
        }
    }
}
