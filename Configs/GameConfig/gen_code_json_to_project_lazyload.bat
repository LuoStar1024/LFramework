Cd /d %~dp0
echo %CD%

set WORKSPACE=../..
set LUBAN_DLL=%WORKSPACE%\Tools\Luban\LubanDll\Luban.dll
set CONF_ROOT=.
set DATA_OUTPATH=%WORKSPACE%/UnityProject/Assets/GameRes/Configs/
set CODE_OUTPATH=%WORKSPACE%/UnityProject/Assets/GameScripts/GameConfig/ConfigCode/

REM xcopy /s /e /i /y "%CONF_ROOT%\CustomTemplate\ConfigComponent.cs" "%WORKSPACE%\UnityProject\Assets\GameScripts\GameConfig\ConfigComponent.cs"

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-simple-json ^
    -d json^
    --conf %CONF_ROOT%\luban.conf ^
    --customTemplateDir %CONF_ROOT%\CustomTemplate\CustomTemplate_Client_LazyLoad ^
    -x outputCodeDir=%CODE_OUTPATH% ^
    -x outputDataDir=%DATA_OUTPATH% 
pause

