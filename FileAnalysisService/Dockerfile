FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Common.Models ./Common.Models
COPY FileAnalysisService/. ./FileAnalysisService
WORKDIR /src/FileAnalysisService
RUN dotnet restore
RUN dotnet clean
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS sdk

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=sdk /usr/share/dotnet /usr/share/dotnet
ENTRYPOINT ["dotnet", "FileAnalysisService.dll"] 