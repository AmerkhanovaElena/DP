cd ..\Valuator\
start dotnet run --urls "http://localhost:5001;http://localhost:5002"

cd ..\nginx\
start nginx.exe

cd ..\RankCalculator\
start "consumer 1" dotnet run
start "consumer 2" dotnet run