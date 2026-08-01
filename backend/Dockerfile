# ============================================================
# FleetOS Backend – Dockerfile (ASP.NET Core 10)
# ============================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY FleetOS.sln .

COPY src/FleetOS.Api/FleetOS.Api.csproj src/FleetOS.Api/
COPY src/FleetOS.Application/FleetOS.Application.csproj src/FleetOS.Application/
COPY src/FleetOS.Domain/FleetOS.Domain.csproj src/FleetOS.Domain/
COPY src/FleetOS.Infrastructure/FleetOS.Infrastructure.csproj src/FleetOS.Infrastructure/
COPY src/FleetOS.Shared/FleetOS.Shared.csproj src/FleetOS.Shared/

COPY tests/FleetOS.Tests/FleetOS.Tests.csproj tests/FleetOS.Tests/


RUN dotnet restore FleetOS.sln


COPY . .


RUN dotnet build FleetOS.sln -c Release --no-restore


RUN dotnet publish src/FleetOS.Api/FleetOS.Api.csproj \
    -c Release \
    --no-build \
    -o /app/publish



# ============================================================
# Runtime
# ============================================================

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app


RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && groupadd --gid 1001 fleetos \
    && useradd --uid 1001 --gid 1001 --create-home fleetos \
    && rm -rf /var/lib/apt/lists/*


COPY --from=build --chown=fleetos:fleetos /app/publish .


RUN mkdir -p /app/logs \
    && chown fleetos:fleetos /app/logs


USER fleetos


EXPOSE 8080


ENTRYPOINT ["dotnet", "FleetOS.Api.dll"]