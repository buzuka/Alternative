@echo off

msbuild ..\BWLib\BWLib.csproj /t:Rebuild /p:OutputPath=..\Installer\bin;Configuration=Release
msbuild ..\infragen\infragen.csproj /t:Rebuild /p:OutputPath=..\Installer\bin;Configuration=Release
msbuild ..\infraconv\infraconv.csproj /t:Rebuild /p:OutputPath=..\Installer\bin;Configuration=Release

del /q bin\*.pdb

makensis installer.nsi
