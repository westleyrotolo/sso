#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/SsoServer/SsoServer.csproj", "src/SsoServer/"]
RUN dotnet restore "src/SsoServer/SsoServer.csproj"
COPY . .
WORKDIR "/src/src/SsoServer"
RUN dotnet build "SsoServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SsoServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SsoServer.dll"]
