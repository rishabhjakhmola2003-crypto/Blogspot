
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Blogspot.csproj", "./"]
RUN dotnet restore "Blogspot.csproj"

COPY . .

# Find the unwanted Downloads reference
RUN echo "===== PROJECT FILE =====" && cat Blogspot.csproj && echo "===== SEARCH RESULTS =====" && grep -nEi 'Downloads|bootstrap\.min\.css' Blogspot.csproj || true
RUN dotnet publish "Blogspot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Blogspot.dll"]
