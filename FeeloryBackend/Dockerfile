FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY FeeloryBackend/FeeloryBackend.csproj FeeloryBackend/
RUN dotnet restore FeeloryBackend/FeeloryBackend.csproj

COPY . .

WORKDIR /app/FeeloryBackend

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]