# 1. Build Aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Tüm solution dosyalarını kopyalayarak bağımlılıkları çözüyoruz
COPY TurkcellBTDotnetSolution.sln ./
COPY src/Turkcell.BT.Dotnet.Lib/*.csproj ./src/Turkcell.BT.Dotnet.Lib/
COPY samples/Turkcell.BT.Dotnet.Demo/*.csproj ./samples/Turkcell.BT.Dotnet.Demo/

# Bağımlılıkları geri yükle
RUN dotnet restore

# Tüm kodları kopyala
COPY . .

# Demo uygulamasını yayınla
WORKDIR /app/samples/Turkcell.BT.Dotnet.Demo
RUN dotnet publish -c Release -o /app/publish

# 2. Çalıştırma Aşaması (OpenShift güvenliği için düşük yetkili kullanıcı uyumlu)
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .

# OpenShift'te container'ların root yetkisiyle çalışmaması için dosya izinlerini ayarla
RUN chown -R 1001:0 /app && chmod -R g+w /app

USER 1001

ENTRYPOINT ["dotnet", "Turkcell.BT.Dotnet.Demo.dll"]