cd ..\Valuator\
dotnet build

cd ..\RankCalculator\
dotnet build

cd ..\EventsLogger\
dotnet build


cd ..\Valuator\
start "web-app 1" dotnet run --no-build --urls "http://localhost:5001"
start "web-app 2" dotnet run --no-build --urls "http://localhost:5002"

cd ..\nginx\
start nginx.exe

cd ..\RankCalculator\
start "consumer 1" dotnet run --no-build
start "consumer 2" dotnet run --no-build

cd ..\EventsLogger\
start "logger 1" dotnet run --no-build
start "logger 2" dotnet run --no-build