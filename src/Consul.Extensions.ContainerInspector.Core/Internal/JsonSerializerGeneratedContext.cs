using Consul.Extensions.ContainerInspector.Core.Internal.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Core.Internal;

[JsonSerializable(typeof(AmazonTaskDescriptionRequest))]
[JsonSerializable(typeof(AmazonTaskDescriptionResponse))]
[JsonSerializable(typeof(ContainerCredentials))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(DockerEventResponse))]
[JsonSerializable(typeof(IDictionary<string, ServiceRegistrationResponse>))]
[JsonSerializable(typeof(IList<DockerResponse>))]
[JsonSerializable(typeof(InspectedDockerResponse))]
[JsonSerializable(typeof(ServiceRegistration))]
internal partial class JsonSerializerGeneratedContext : JsonSerializerContext { }
