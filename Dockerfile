FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Desafio Técnico - Good Hamburguer/Desafio Técnico - Good Hamburguer.csproj", "Desafio Técnico - Good Hamburguer/"]
RUN dotnet restore "Desafio Técnico - Good Hamburguer/Desafio Técnico - Good Hamburguer.csproj"
COPY . .
WORKDIR "/src/Desafio Técnico - Good Hamburguer"
RUN dotnet publish "Desafio Técnico - Good Hamburguer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Desafio_Técnico_-_Good_Hamburguer.dll"]
