dotnet build -p:Configuration=Release
cd bin/Release/net48
if [[ -d "NoclipMod.dll" ]]; then
    rm NoclipMod.dll
fi
cp NoclipMod.dll ../../../
cd ..
cd ..
cd ..