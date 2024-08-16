# Container inspector
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/syneder/consul-container-inspector/docker-image.yml)

The **container inspector** is a small background service that inspects running Docker
containers and monitors Docker events to register or unregister services in Consul. This
service supports container startup, destruction, and health change events, as well as
container network connection and disconnection events.

## Introduction
The **container inspector** is designed as a helper service to implement service discovery
in an Amazon ECS Anywhere cluster. The main requirements are:
- **container inspector** must be running on a host that is running the Docker containers
for which service discovery needs to be enabled;
- Consul agent must be running on the same host, and access to the Consul agent API must
be allowed through a unix socket;
- access to Docker for event monitoring must be allowed through a unix socket.

> [!NOTE]
> If you are running the **container inspector** using Amazon ECS Anywhere, it is recommended
> to run the **container inspector** as DAEMON. This will run the inspector on every host in
> the Amazon ECS Anywhere cluster.

## Configure Consul agent
Before running the **container inspector**, make sure that the Consul agent is accessible
through a unix socket and that the unix socket is accessible to the inspector. To have
the Consul agent handle requests through a unix socket, add the following configuration
to the Consul agent configurations:

```HCL
addresses {
  http = "127.0.0.1 unix:///consul/run/consul.sock"
}
```

You can specify a different path to the Consul unix socket. The **container inspector** can read
the Consul configuration from a shared folder or volume at `/consul/config`.

> [!NOTE]
> The path from which the Consul configuration will be read can be overridden by specifying the
> corresponding path in the `CONSUL_CONFIG_PATH` environment variable.

Also the path to the Consul unix socket can be overridden by passing the Consul configuration in
base64 format in the `CONSUL_CONFIG` environment variable. Or the path to the Consul unix socket
can be overridden using the command line argument `--consul:socket`.

> [!NOTE]
> Just as the path to the Consul unix socket was specified, you can specify the Consul token via
> the configuration or the command line parameter `--consul:token`. The inspector will look for the
> token in the `acl:tokens:inspector` or `acl:tokens:agent` section of the configuration.

## Service name determination
The **container inspector** looks for the `consul.inspector.service.name` label among the container
labels to determine the service name. If this label is found, the **container inspector** uses its
value as the service name. The name of this label can be overridden using the
`DOCKER_CONTAINER_LABELS_SERVICE_NAME` environment variable. If this label is not defined, but the
**container inspector** finds the `com.amazonaws.ecs.task-arn` label that AWS adds to containers it
manages, the **container inspector** will try to get information about this container from AWS.

In order for the **container inspector** to successfully retrieve container information from AWS, it
must first obtain the credentials.

> [!WARNING]
> This credentials can only be obtained if the **container inspector** is running as an ECS service
> and has the appropriate IAM role. Otherwise, the **container inspector** will issue warnings.

Once the credentials is obtained, the **container inspector** will send an API request to retrieve
container information and will use the same service name as the ECS service.
