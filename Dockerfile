FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "loja de motocao/loja de motocao.csproj"
RUN dotnet publish "loja de motocao/loja de motocao.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 10000
CMD dotnet EssenzStore.dll --urls http://0.0.0.0:${PORT:-10000}