FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY BudgetTracker.slnx .
COPY src/BudgetTracker.API/BudgetTracker.API.csproj src/BudgetTracker.API/
COPY src/BudgetTracker.Application/BudgetTracker.Application.csproj src/BudgetTracker.Application/
COPY src/BudgetTracker.Data/BudgetTracker.Data.csproj src/BudgetTracker.Data/
COPY src/BudgetTracker.Domain/BudgetTracker.Domain.csproj src/BudgetTracker.Domain/
COPY src/BudgetTracker.Tests/BudgetTracker.Tests.csproj src/BudgetTracker.Tests/
RUN dotnet restore

# Copy everything and publish
COPY . .
WORKDIR /src/src/BudgetTracker.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Create data directory for SQLite
# In Docker, ContentRootPath=/app, so Path.Combine("/app", "..", "..", "data", "budget.db")
# resolves to /data/budget.db. We create /data so the volume mount target exists.
RUN mkdir -p /data

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "BudgetTracker.API.dll"]
