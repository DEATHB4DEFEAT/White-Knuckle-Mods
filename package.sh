#!/usr/bin/env bash

rm -rf .packages

for d in * ; do
    if [ ! -d "$d" ]; then
        continue
    fi
    cd "$d"
    if [ -f "manifest.json" ]; then
        echo "Found manifest in $d"
        dotnet build -c Release
        mkdir -p ../.packages/$d/BepInEx/plugins/$d
        cp manifest.json icon.png README.md CHANGELOG.md ../.packages/$d/
        cp bin/Release/net472/$d.dll ../.packages/$d/BepInEx/plugins/$d/
    fi
    cd ..
done

if [ ! -d .packages ]; then
    echo "No packages found"
    exit 1
fi

cd .packages
for d in * ; do
    if [ ! -d "$d" ]; then
        continue
    fi
    cd "$d"
    zip -r "../$d.zip" .
    cd ..
    rm -rf "$d"
done
