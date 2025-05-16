FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StockGTO.csproj", "./"]
RUN dotnet restore "./StockGTO.csproj"
COPY . .
RUN dotnet publish "./StockGTO.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "StockGTO.dll"]
