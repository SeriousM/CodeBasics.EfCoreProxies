@echo off

set scriptdir=%~dp0

pushd %scriptdir%..\Command.Test\
dotnet test -v normal
popd

set /P VERSION=Version to Build (eg. 1.2.4): 
if "%VERSION%"=="" GOTO ERROR

set /P APIKEY=Enter your Api Key from https://www.nuget.org/account/ApiKeys: 
if "%APIKEY%"=="" GOTO ERROR

pushd %scriptdir%..\Command\

del bin\Release\*.nupkg
dotnet pack -c Release /p:Version=%VERSION%
dotnet nuget push bin\Release\CodeBasics.Command.%VERSION%.nupkg --source https://nuget.org/ --api-key %APIKEY%
GOTO END

:ERROR
echo EXIT: No version or api key was entered

:END
popd

pause
