@echo off
@set framework=net6.0-windows
for %%p in (win-x86) do (
    echo "Building " %%p
    pushd ..\EasyCaster.Alarm
    dotnet clean -p:Configuration=Release || goto FAIL
    dotnet build --no-incremental -p:Configuration=Release || goto FAIL
    dotnet publish -r %%p -p:EnableCompressionInSingleFile=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true -p:Configuration=Release || goto FAIL
	popd
	if not exist %%p mkdir %%p
	copy /Y ..\EasyCaster.Alarm\bin\Release\%framework%\%%p\publish\EasyCaster.Alarm.exe %%p\
)
goto :EOF
:FAIL
echo BUILD FAILED!
