# EStoreAPI.Server/Dockerfile

# build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /out

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
# port
EXPOSE 5100
ENTRYPOINT ["dotnet", "EStoreAPI.Server.dll"]