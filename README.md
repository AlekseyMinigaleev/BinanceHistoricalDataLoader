# Binance Historical Data Loader

## Описание
Binance Historical Data Loader — это ASP.NET Core приложение, которое позволяет загружать исторические данные с Binance по заданным валютным парам и диапазону дат, сохранять их в MongoDB и предоставлять API для мониторинга статуса загрузки.

## Стек технологий
- **.NET 8.0** — основная платформа для разработки
- **MongoDB** — база данных для хранения исторических данных
- **Hangfire** — управление фоновыми задачами и отслеживание состояния загрузок
- **Automapper** — удобное отображение DTO на доменные модели
- **FluentValidation** — валидация входных данных
- **Serilog** — логирование работы приложения
- **Hangfire Console** — логирование состояния фоновых задач
  
### API Binance
Для получения исторических данных используется API Binance:
- **Эндпоинт**: [`/api/v3/klines`](https://developers.binance.info/docs/binance-spot-api-docs/rest-api/market-data-endpoints#klinecandlestick-data)

#### Пример запроса:
```plaintext
GET https://api.binance.com/api/v3/klines?symbol=BTCUSDT&interval=1d&startTime=1609459200000&endTime=1612137600000
```
- `symbol`: Валютная пара (например, BTCUSDT)
- `interval`: Интервал (например, 1d — день)
- `startTime`: Начальная временная метка в миллисекундах
- `endTime`: Конечная временная метка в миллисекундах

## Логирование
Логирование приложения выполнено с помощью **Serilog**:
- В консоль выводятся логи уровня `Information`
- В файл `BinanceHistoricalDataLoader/API/Logs/` записываются логи уровня `Error`
- Процесс выполнения задач логируется в **Hangfire Console**

## Установка и запуск

## Docker
### 1. Создание `docker-compose.yml`
```yaml
services:
  mongo:
    image: mongo:6.0
    container_name: mongo_container
    restart: always
    environment:
      - MONGO_INITDB_ROOT_USERNAME=user
      - MONGO_INITDB_ROOT_PASSWORD=password
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db
    healthcheck:
      test: ["CMD-SHELL", "mongosh --quiet --eval 'db.runCommand({ ping: 1 }).ok || 0' mongo:27017/test"]
      interval: 10s
      timeout: 10s
      retries: 5
      start_period: 20s

  binance_loader:
    image: alekseyminigaleev/binance_loader
    container_name: binance_loader_container
    depends_on:
      mongo:
        condition: service_healthy
    environment:
      - ConnectionStrings__MongoDb=mongodb://user:password@mongo:27017/
      - ConnectionStrings__Hangfire=mongodb://user:password@mongo:27017/
      - MongoDbConfiguration__DatabaseName=BinanceHistoricalData
      - HangfireConfiguration__CompatibilityLevel=170
      - HangfireConfiguration__DatabaseName=BinanceHistoricalData
      - HangfireConfiguration__Prefix=hangfire
      - ASPNETCORE_URLS=http://+:5000;
    ports:
      - "5000:5000"

volumes:
  mongo_data:
```
### 2. Запуск контейнеров
```sh
docker-compose up -d
```
### 3. Остановка контейнеров
```sh
docker-compose down
```

## Сборка проекта
### 1. Клонирование репозитория
```sh
git clone https://github.com/AlekseyMinigaleev/BinanceHistoricalDataLoader.git
cd BinanceHistoricalDataLoader
```
### 2. Настройка `appsettings.json`
Перед запуском приложения убедитесь, что файл конфигурации `appsettings.json` содержит корректные параметры подключения.  

Файл находится по пути:  
`BinanceHistoricalDataLoader/API/appsettings.json`

**Пример содержимого:**
```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://user:password@localhost:27017/",
    "Hangfire": "mongodb://user:password@localhost:27017/"
  },

  "MongoDbConfiguration": {
    "DatabaseName": "BinanceHistoricalData"
  },

  "HangfireConfiguration": {
    "CompatibilityLevel": 170,
    "DatabaseName": "BinanceHistoricalData",
    "Prefix": "hangfire"
  }
}
```

### 2. Локальный запуск MongoDB (если не установлена)
```sh
docker run -d --name mongodb -p 27017:27017 mongo
```

### 3. Запуск приложения
```sh
dotnet restore
dotnet run
```
После успешного запуска:
- **API** доступно по адресу: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger`
- **Hangfire Dashboard**: `http://localhost:5000/hangfire`




