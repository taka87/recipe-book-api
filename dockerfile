# Коригиран Dockerfile за .NET 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build   # ← Променено на 8.0
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime  # ← Променено на 8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "RecipeBookApi.dll"]