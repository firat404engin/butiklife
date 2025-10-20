@echo off
echo Onceki dotnet sureclerini kapatiliyor...
taskkill /f /im dotnet.exe 2>nul

timeout /t 3 /nobreak

echo.
echo API baslatiliyor...
start "Butik API" cmd /k "cd /d C:\Users\Firat\Desktop\butiklife\ButikProjesi.API && dotnet run"

timeout /t 8 /nobreak

echo.
echo Frontend baslatiliyor...
start "Butik Frontend" cmd /k "cd /d C:\Users\Firat\Desktop\butiklife\ButikProjesi.Istemci && dotnet run --urls http://localhost:5194"

timeout /t 3 /nobreak

echo.
echo ================================================
echo Projeler baslatildi!
echo ================================================
echo API: http://localhost:5154
echo Frontend: http://localhost:5194
echo ================================================
echo.
echo Tarayicinizda http://localhost:5194 adresini acin
echo.
pause
