@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo .NET 10 SDK bulunamadi. Visual Studio .NET web gelistirme bilesenini kurun.
  pause
  exit /b 1
)
echo Gorev Yonetim Paneli: http://localhost:5237
echo Ilk kurulum yapilmadiysa ILK_ONCE_OKU.md dosyasini okuyun.
echo Bu pencereyi acik birakin. Durdurmak icin Ctrl+C kullanin.
dotnet run --project StajyerTakip.API\StajyerTakip.API.csproj --launch-profile http
pause
