# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/InsuraTech.API/InsuraTech.API.csproj", "src/InsuraTech.API/"]
COPY ["src/InsuraTech.Application/InsuraTech.Application.csproj", "src/InsuraTech.Application/"]
COPY ["src/InsuraTech.Domain/InsuraTech.Domain.csproj", "src/InsuraTech.Domain/"]
COPY ["src/InsuraTech.Infrastructure/InsuraTech.Infrastructure.csproj", "src/InsuraTech.Infrastructure/"]

# Restore
RUN dotnet restore "src/InsuraTech.API/InsuraTech.API.csproj"

# Copiar todo el código fuente
COPY . .

# Build y publish
WORKDIR "/src/src/InsuraTech.API"
RUN dotnet build "InsuraTech.API.csproj" -c Release -o /app/build
RUN dotnet publish "InsuraTech.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InsuraTech.API.dll"]