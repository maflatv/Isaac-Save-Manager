@echo off
rem Compila dist\IsaacSaveManager.exe (portable, .NET Framework 4.8, sin dependencias).
rem Necesita el compilador de C# de Visual Studio o de Build Tools (Roslyn).
setlocal
cd /d "%~dp0"

set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
set "CSC="
if exist "%VSWHERE%" for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -find MSBuild\**\Bin\Roslyn\csc.exe`) do set "CSC=%%i"
rem vswhere no siempre registra Build Tools: buscar tambien en las carpetas de Visual Studio.
if not defined CSC for /d %%v in ("%ProgramFiles%\Microsoft Visual Studio\*" "%ProgramFiles(x86)%\Microsoft Visual Studio\*") do (
  for /d %%e in ("%%~v\*") do if exist "%%~e\MSBuild\Current\Bin\Roslyn\csc.exe" set "CSC=%%~e\MSBuild\Current\Bin\Roslyn\csc.exe"
)
if not defined CSC (
  echo No se encontro csc.exe de Roslyn. Instala "Build Tools para Visual Studio" con la carga ".NET desktop build tools".
  exit /b 1
)

if not exist dist mkdir dist
if not exist build mkdir build

"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu /out:dist\IsaacSaveManager.exe ^
  /win32manifest:src\app.manifest /win32icon:src\app.ico /resource:src\app.ico,IsaacSaveManager.app.ico /r:System.Windows.Forms.dll /r:System.Drawing.dll ^
  src\*.cs || exit /b 1

"%CSC%" /nologo /target:exe /out:build\SelfTest.exe ^
  src\SaveFile.cs src\SaveWriter.cs tests\SelfTest.cs || exit /b 1

echo Listo: dist\IsaacSaveManager.exe
