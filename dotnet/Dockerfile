FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TurkcellBTDotnetSolution.sln ./
COPY src/Turkcell.BT.Dotnet.Lib/Turkcell.BT.Dotnet.Lib.csproj src/Turkcell.BT.Dotnet.Lib/
COPY tests/Turkcell.BT.Dotnet.Tests/Turkcell.BT.Dotnet.Tests.csproj tests/Turkcell.BT.Dotnet.Tests/
COPY samples/Turkcell.BT.Dotnet.Demo/Turkcell.BT.Dotnet.Demo.csproj samples/Turkcell.BT.Dotnet.Demo/

RUN dotnet restore TurkcellBTDotnetSolution.sln

COPY . ./

RUN dotnet publish samples/Turkcell.BT.Dotnet.Demo/Turkcell.BT.Dotnet.Demo.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish/ ./

ENTRYPOINT ["dotnet", "Turkcell.BT.Dotnet.Demo.dll"]
