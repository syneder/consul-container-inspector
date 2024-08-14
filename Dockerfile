FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20 AS build

WORKDIR /src

COPY /src/Consul.Extensions.ContainerInspector.sln \
	 /src/Consul.Extensions.ContainerInspector/Consul.Extensions.ContainerInspector.csproj \
	 /src/Consul.Extensions.ContainerInspector.Core/Consul.Extensions.ContainerInspector.Core.csproj \
	 /src/Consul.Extensions.ContainerInspector.Configurations/Consul.Extensions.ContainerInspector.Configurations.csproj \
	 /src

RUN mkdir -p /src/Consul.Extensions.ContainerInspector \
 && mkdir -p /src/Consul.Extensions.ContainerInspector.Core \
 && mkdir -p /src/Consul.Extensions.ContainerInspector.Configurations \
 && mv /src/Consul.Extensions.ContainerInspector.csproj /src/Consul.Extensions.ContainerInspector \
 && mv /src/Consul.Extensions.ContainerInspector.Core.csproj /src/Consul.Extensions.ContainerInspector.Core \
 && mv /src/Consul.Extensions.ContainerInspector.Configurations.csproj /src/Consul.Extensions.ContainerInspector.Configurations \
 && dotnet restore /src/Consul.Extensions.ContainerInspector.sln

COPY ./src .

WORKDIR /src/Consul.Extensions.ContainerInspector

RUN dotnet publish Consul.Extensions.ContainerInspector.csproj \
	  /p:Configuration=Release \
	  /p:PublishProfile=DockerProfile.pubxml

FROM mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled

COPY --from=build /app/inspector /

ENTRYPOINT ["/inspector"]
