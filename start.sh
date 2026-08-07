#!/usr/bin/env bash
set -e

mkdir -p /var/data
if [ ! -f /var/data/fitsite.db ] && [ -f ./fitsite.db ]; then
  cp ./fitsite.db /var/data/fitsite.db
fi

export ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000}
exec dotnet ./FitSite.dll