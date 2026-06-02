# 1. Aşama: Uygulamayı Derleme (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Her iki projenin de iskeletini (csproj) kendi klasör isimleriyle içeri al
COPY BlockchainAPI/BlockchainAPI.csproj BlockchainAPI/
COPY BlockchainCore/BlockchainCore.csproj BlockchainCore/

# Bağımlılıkları yükle (Artık köprünün karşı tarafını da görebilecek)
RUN dotnet restore "BlockchainAPI/BlockchainAPI.csproj"

# Tüm şantiye kodlarını içeri kopyala
COPY . .

# Çalışma odasını API'nin içine taşı ve projeyi yayınla (Publish)
WORKDIR "/src/BlockchainAPI"
RUN dotnet publish "BlockchainAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Aşama: Uygulamayı Çalıştırma (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "BlockchainAPI.dll"]