# Text Scanner (Word Cloud Generator)

## Описание

Микросервисное приложение для загрузки, хранения, анализа текстовых файлов и генерации облака слов. Реализовано на C# (ASP.NET Core).

### Архитектура

- **ApiGateway** — маршрутизация и проксирование запросов.
- **FileStorageService** — хранение файлов и метаданных, работа с PostgreSQL.
- **FileAnalysisService** — анализ файлов, генерация word cloud, хранение результатов, работа с PostgreSQL.
- **Common.Models** — общие модели для обмена между сервисами.

### Запуск

1. Установить .NET 8 SDK и Docker.
2. Склонировать репозиторий и перейти в корень решения.
3. Собрать и запустить через Docker Compose:

```bash
docker compose up --build
```

### Примеры запросов

- Загрузка файла:
```bash
curl -X POST -F "file=@file.txt" http://localhost:8080/upload
```
- Получение файла по id:
```bash
curl "http://localhost:8080/download?id=<id>"
```
- Анализ файла по id:
```bash
curl -X GET "http://localhost:8080/analysis?id=<id>"
```
- Получение результата анализа:
```bash
curl -X GET "http://localhost:8080/get?id=<id>"
```

---

**Структура решения:**

- ApiGateway
- FileStorageService
- FileAnalysisService
- Common.Models
- docker-compose.yml