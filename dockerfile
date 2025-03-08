FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Миграции се изпълняват по време на build (ако имате миграции)
# RUN dotnet ef migrations bundle --self-contained -r linux-x64 --output /app/migrate

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

# Ако използвате миграции и сте генерирали bundle:
# COPY --from=build /app/migrate .
# RUN ./migrate --connection "Host=your_supabase_host;Port=5432;Database=db_name;Username=user;Password=password"

ENTRYPOINT ["dotnet", "RecipeBookApi.dll"]