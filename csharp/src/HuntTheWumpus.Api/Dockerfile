FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HuntTheWumpus.Api/HuntTheWumpus.Api.csproj", "src/HuntTheWumpus.Api/"]
COPY ["src/HuntTheWumpus.Application/HuntTheWumpus.Application.csproj", "src/HuntTheWumpus.Application/"]
COPY ["src/HuntTheWumpus.Domain/HuntTheWumpus.Domain.csproj", "src/HuntTheWumpus.Domain/"]
COPY ["src/HuntTheWumpus.Infrastructure/HuntTheWumpus.Infrastructure.csproj", "src/HuntTheWumpus.Infrastructure/"]
RUN dotnet restore "src/HuntTheWumpus.Api/HuntTheWumpus.Api.csproj"
COPY . .
WORKDIR "/src/src/HuntTheWumpus.Api"
RUN dotnet build "HuntTheWumpus.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HuntTheWumpus.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HuntTheWumpus.Api.dll"]
