# Container inspector
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/syneder/consul-container-inspector/docker-image.yml)

The **container inspector** is a small background service that inspects running Docker
containers and monitors Docker events to register or unregister services with Consul.
This service supports container startup, destruction, and health change events, as well
as container network connection and disconnection events.

## Introduction
The **container inspector** is designed as a helper service to implement service discovery
in an ECS Anywhere cluster. The main conditions for the service to function as a service
discovery are:
- **container inspector** must be running on a host that is running the Docker containers
for which service discovery needs to be enabled;
- access to Docker for event monitoring must be allowed through a unix socket;
- Consul agent must be running on the same host, and access to the Consul agent API must
be allowed through a unix socket.

> [!NOTE]
> If you are running the **container inspector** via ECS Anywhere, it is recommended to run
> the inspector as DAEMON. This will run the inspector on every host in the ECS Anywhere
> cluster.

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
