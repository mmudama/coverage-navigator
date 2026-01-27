FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CoverageNavigator.Api/CoverageNavigator.Api.csproj", "CoverageNavigator.Api/"]
RUN dotnet restore "CoverageNavigator.Api/CoverageNavigator.Api.csproj"
COPY . .
WORKDIR "/src/CoverageNavigator.Api"
RUN dotnet build "CoverageNavigator.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoverageNavigator.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoverageNavigator.Api.dll"]