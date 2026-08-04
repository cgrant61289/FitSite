#!/usr/bin/env bash
set -e

if [ ! -f /var/data/fitsite.db ]; then
if [ -f ./fitsite.db ]; then
cp ./fitsite.db /var/data/fitsite.db
fi
fi

export ASPNETCORE_URLS=http://0.0.0.0:${PORT}
exec dotnet ./publish/FitSite.dll