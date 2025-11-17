FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/UrlShortener.Api/UrlShortener.Api.csproj", "src/UrlShortener.Api/"]
COPY ["src/UrlShortener.Application/UrlShortener.Application.csproj", "src/UrlShortener.Application/"]
COPY ["src/UrlShortener.Domain/UrlShortener.Domain.csproj", "src/UrlShortener.Domain/"]
COPY ["src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj", "src/UrlShortener.Infrastructure/"]
COPY ["src/UrlShortener.SharedKernel/UrlShortener.SharedKernel.csproj", "src/UrlShortener.SharedKernel/"]

RUN dotnet restore "src/UrlShortener.Api/UrlShortener.Api.csproj"

COPY src/ ./src/

WORKDIR "/src/src/UrlShortener.Api"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]
