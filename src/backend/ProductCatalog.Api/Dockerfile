FROM mcr.microsoft.com/dotnet/sdk:10.0.202 AS build
WORKDIR /source
COPY global.json ./
COPY src/backend/ProductCatalog.Api/ProductCatalog.Api.csproj src/backend/ProductCatalog.Api/
RUN dotnet restore src/backend/ProductCatalog.Api/ProductCatalog.Api.csproj
COPY src/backend/ProductCatalog.Api/ src/backend/ProductCatalog.Api/
RUN dotnet publish src/backend/ProductCatalog.Api/ProductCatalog.Api.csproj -c Release --no-restore -o /publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12 AS runtime
WORKDIR /app
COPY --from=build /publish ./
RUN mkdir -p /app/data && chown "$APP_UID:$APP_UID" /app/data
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductCatalog.Api.dll"]
