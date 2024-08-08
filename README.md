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
the Consul configuration from a shared folder or volume at `/consul/config`. The path from which
the Consul configuration will be read can be overridden by specifying the corresponding path in the
`CONSUL_CONFIG_PATH` environment variable. Or the path to the Consul unix socket can be overridden
by passing the Consul configuration in base64 format in the `CONSUL_CONFIG` environment variable.
Or the path to the Consul unix socket can be overridden using the `--consul:socket` command line
argument.
