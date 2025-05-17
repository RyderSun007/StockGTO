@echo off
REM ===== Change to crawler folder and run Python script =====
cd /d D:\StockGTO\StockGTO\wwwroot\crawler
py news_crawler.py

REM ===== Wait a bit to make sure JSON is written =====
timeout /t 2 /nobreak >nul

REM ===== Start HTTP server in wwwroot =====
cd /d D:\StockGTO\StockGTO\wwwroot
start cmd /k "py -m http.server 8000"

REM ===== Open news_viewer.html in browser =====
timeout /t 1 >nul
start http://localhost:8000/viewer/news_viewer.html

REM ===== Done =====
echo Done. Press any key to exit...
pause >nul
