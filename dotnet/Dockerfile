# 1. Aşama: Build (SDK kullanıyoruz)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala
COPY ["src/Turkcell.BT.Dotnet.Lib/Turkcell.BT.Dotnet.Lib.csproj", "src/Turkcell.BT.Dotnet.Lib/"]
COPY ["samples/Turkcell.BT.Dotnet.Demo/Turkcell.BT.Dotnet.Demo.csproj", "samples/Turkcell.BT.Dotnet.Demo/"]

# Bağımlılıkları geri yükle
RUN dotnet restore "samples/Turkcell.BT.Dotnet.Demo/Turkcell.BT.Dotnet.Demo.csproj"

# Tüm kodu kopyala ve yayınla
COPY . .
WORKDIR "/src/samples/Turkcell.BT.Dotnet.Demo"
RUN dotnet publish "Turkcell.BT.Dotnet.Demo.csproj" -c Release -o /app/publish

# 2. Aşama: Run (Sadece Runtime kullanıyoruz - Hafif imaj)
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "Turkcell.BT.Dotnet.Demo.dll"]