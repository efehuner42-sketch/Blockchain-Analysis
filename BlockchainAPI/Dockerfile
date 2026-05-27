# 1. Aşama: Uygulamayı Derleme (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve bağımlılıkları yükle
COPY ["BlockchainAPI.csproj", "./"]
RUN dotnet restore "BlockchainAPI.csproj"

# Tüm kodları kopyala ve projeyi yayınla (Publish)
COPY . .
RUN dotnet publish "BlockchainAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Aşama: Uygulamayı Çalıştırma (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "BlockchainAPI.dll"]