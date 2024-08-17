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

## Installation using Amazon ECS Console
To run **container inspector** on Amazon ECS Anywhere hosts using Amazon ECS Console, you must first
create a task definition. Below is the task definition in JSON format to use the latest version of
**container inspector**. After creating a task definition, run it as a service in ECS cluster with
external instances.

> [!WARNING]
> For  **container inspector** to work properly, it must be running on each instance, but no more
> than one per instance. To do this, when creating ECS service, select the service scheduler
> strategy (service type) as DAEMON.

```JSON
{
    "family": "container-inspector",
    "containerDefinitions": [
        {
            "name": "container",
            "image": "ghcr.io/syneder/consul-container-inspector:latest",
            "mountPoints": [
                {
                    "sourceVolume": "docker",
                    "containerPath": "/var/run/docker.sock"
                },
                {
                    "sourceVolume": "consul",
                    "containerPath": "/consul/config",
                    "readOnly": true
                },
                {
                    "sourceVolume": "consul-run",
                    "containerPath": "/consul/run"
                },
                {
                    "sourceVolume": "ssm",
                    "containerPath": "/amazon/ssm",
                    "readOnly": true
                }
            ]
        }
    ],
    "networkMode": "bridge",
    "volumes": [
        {
            "name": "docker",
            "host": {
                "sourcePath": "/var/run/docker.sock"
            }
        },
        {
            "name": "consul",
            "host": {
                "sourcePath": "/etc/consul.d"
            }
        },
        {
            "name": "consul-run",
            "host": {
                "sourcePath": "/var/run/consul"
            }
        },
        {
            "name": "ssm",
            "host": {
                "sourcePath": "/var/lib/amazon/ssm"
            }
        }
    ],
    "cpu": "128",
    "memory": "128"
}
```

### Command line arguments and environment variables

| Environment variable                      | Command line argument | <div style="width: 35%">Description</div>
| :---------------------------------------- | :-------------------- | :----------
| `CONSUL_CONFIG_PATH`                      |                       | The path to the file or folder containing Consul configurations
| `CONSUL_CONFIG`                           |                       | The Consul configurations in base64
| `DOCKER_SOCKET_PATH`                      | `--docker:socketPath` | The path to Docker unix socket
| `DOCKER_EXPECTED_CONTAINER_LABELS`        |                       | Expected Docker container labels
| `DOCKER_CONTAINER_LABELS_SERVICE_NAME`    |                       | The name of the Docker container label containing the service name
|                                           | `--consul:address`    | The address of a Consul agent or host that is used as the address of a service connected to the `host` Docker network
|                                           | `--consul:token`      | The Consul token
|                                           | `--consul:socketPath` | The path to Consul unix socket
| `INSPECTOR_DEBUG`                         | `--debug=true`        | Enables debug logs
| `INSPECTOR_VERBOSE`                       | `--verbose=true`      | Enables debug and trace logs
| `MANAGED_INSTANCE_REGISTRATION_REGUIRED`  |                       | Do not allow launch without instance registration information
| `MANAGED_INSTANCE_REGISTRATION_FILE_PATH` |                       | The path to instance registration information

> [!NOTE]
> Multiple expected Docker container labels must be specified on a single line, separated by the
> comma. Labels can be specified either by name only, or by name and value. Example:
> `vendor=example,consul.inspector.service.name`. In this example, the **container inspector** will
> only process Docker containers that have both an `consul.inspector.service.name` label and a
> `vendor` label with a value of `example`.
