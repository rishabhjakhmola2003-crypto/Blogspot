
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Blogspot.csproj", "./"]
RUN dotnet restore "Blogspot.csproj"

COPY . .

# Find the unwanted Downloads reference
RUN grep -RniE 'Downloads|bootstrap\.min\.css' . --exclude-dir=.git --exclude-dir=bin --exclude-dir=obj || true

RUN dotnet publish "Blogspot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Blogspot.dll"]
