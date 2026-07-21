#!/bin/bash
set -e
echo "Installing dotnet-ef..."
dotnet tool install --global dotnet-ef
export PATH="$PATH:/root/.dotnet/tools"

echo "Restoring packages..."
dotnet restore FleetOS.sln

echo "Running migrations..."
dotnet ef migrations add InitialCore --project src/FleetOS.Infrastructure --startup-project src/FleetOS.Api
