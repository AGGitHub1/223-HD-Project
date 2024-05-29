# Use the official .NET image from the Microsoft Container Registry
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Enhanced Reaction Controller/Enhanced Reaction Controller.csproj", "Enhanced Reaction Controller/"]
RUN dotnet restore "Enhanced Reaction Controller/Enhanced Reaction Controller.csproj"
COPY . .
WORKDIR "/src/Enhanced Reaction Controller"
RUN dotnet build "Enhanced Reaction Controller.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Enhanced Reaction Controller.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Enhanced Reaction Controller.dll"]