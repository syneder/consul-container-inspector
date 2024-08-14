﻿using Consul.Extensions.ContainerInspector.Core.Internal.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Core.Internal;

[JsonSerializable(typeof(AmazonTaskDescriptionRequest))]
[JsonSerializable(typeof(AmazonTaskDescriptionResponse))]
[JsonSerializable(typeof(ContainerCredentials))]
[JsonSerializable(typeof(DockerEventResponse))]
[JsonSerializable(typeof(ServiceRegistration))]
[JsonSerializable(typeof(InspectedDockerResponse))]
[JsonSerializable(typeof(IDictionary<string, string[]>), TypeInfoPropertyName = $"ContainerFilters")]
[JsonSerializable(typeof(IDictionary<string, ServiceRegistrationResponse>), TypeInfoPropertyName = $"{nameof(ServiceRegistrationResponse)}Dictionary")]
[JsonSerializable(typeof(IList<DockerResponse>), TypeInfoPropertyName = $"{nameof(DockerResponse)}Collection")]
internal partial class JsonSerializerGeneratedContext : JsonSerializerContext { }
