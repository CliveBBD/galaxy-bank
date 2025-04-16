# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src
COPY ["Api.csproj", "./"]
RUN dotnet restore "./Api.csproj"
COPY . .
RUN dotnet publish "./Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]
