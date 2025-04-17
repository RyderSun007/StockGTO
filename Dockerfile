# 使用 .NET SDK 建構環境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 複製 .csproj 並還原套件
COPY StockGTO.csproj ./
RUN dotnet restore

# 複製其他原始碼並建置
COPY . ./
RUN dotnet publish -c Release -o out

# 使用 .NET 執行環境來啟動網站
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# 執行網站
ENTRYPOINT ["dotnet", "StockGTO.dll"]
