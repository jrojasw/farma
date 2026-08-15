FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Farmacia.Web/Farmacia.Web.csproj Farmacia.Web/
RUN dotnet restore Farmacia.Web/Farmacia.Web.csproj

COPY src/Farmacia.Web/ Farmacia.Web/
RUN dotnet publish Farmacia.Web/Farmacia.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
RUN mkdir -p App_Data

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet Farmacia.Web.dll"]
