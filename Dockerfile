#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["src/MySelfLog.Api/MySelfLog.Api.csproj", "MySelfLog.Api/"]
COPY ["src/MySelfLog.Contracts/MySelfLog.Contracts.csproj", "MySelfLog.Contracts/"]
COPY ["src/MySelfLog.MessageSender.EventStore/MySelfLog.MessageSender.EventStore.csproj", "MySelfLog.MessageSender.EventStore/"]
COPY ["src/MySelfLog.Contracts.Api/MySelfLog.Contracts.Api.csproj", "MySelfLog.Contracts.Api/"]
COPY ["src/MySelfLog.Admin.Model/MySelfLog.Admin.Model.csproj", "MySelfLog.Admin.Model/"]
RUN dotnet restore "MySelfLog.Api/MySelfLog.Api.csproj"
RUN echo 'running COPY . .'
COPY . .
WORKDIR "/src/MySelfLog.Api"
RUN echo 'running RUN dotnet build "MySelfLog.Api.csproj" -c Release -o /app/build'
RUN dotnet build "/src/MySelfLog.Api/MySelfLog.Api.csproj" -c Release -o /app/build

RUN echo 'running RUN dotnet publish "MySelfLog.Api.csproj" -c Release -o /app/publish'
FROM build AS publish
RUN dotnet publish "MySelfLog.Api.csproj" -c Release -o /app/publish

RUN echo 'running final'
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MySelfLog.Api.dll"]