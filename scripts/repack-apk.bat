@echo off
setlocal
for %%d in (%~dp0..) do set root_dir=%%~fd
for %%d in (%~dp0..\scripts) do set scripts_dir=%%~fd
for %%d in (%~dp0..\build-tools) do set build_tools_dir=%%~fd
for %%d in (%~dp0..\build) do set build_dir=%%~fd

rem check build tools
if not exist %build_tools_dir% (
   echo You should download and unzip the build tools first.
   pause
   exit
)

rem check java environment
java -version >nul 2>&1
if errorlevel 1 (
   echo Check java environment failed. Please install java environment.
   pause
   exit
)

call %scripts_dir%\recompile-dll.bat

cd %root_dir%
copy %build_dir%\Assembly-CSharp.dll %root_dir%\Solarmax3vt3_killer\assets\bin\Data\Managed\Assembly-CSharp.dll
java.exe -jar %build_tools_dir%\apktool.jar b Solarmax3vt3_killer
rem output file is Solarmax3vt3_killer\dist\Solarmax3vt3_killer.Apk
if exist %build_dir%\Solarmax3vt3_killersigned-aligned.apk (
   rm %build_dir%\Solarmax3vt3_killersigned-aligned.apk
)
%build_tools_dir%\android-build-tools\zipalign.exe -v 4 %root_dir%\Solarmax3vt3_killer\dist\Solarmax3vt3_killer.Apk %build_dir%\Solarmax3vt3_killersigned-aligned.apk
call %build_tools_dir%\android-build-tools\apksigner.bat sign -ks %root_dir%\candice.keystore -ks-pass pass:candice %build_dir%\Solarmax3vt3_killersigned-aligned.apk

rem directly install to game_ass.dll
if not "%~1"=="" (
   adb install %build_dir%\Solarmax3vt3_killersigned-aligned.apk
)

echo "Repack apk completed!!!"

endlocal
