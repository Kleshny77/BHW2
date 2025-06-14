# Text Scanner 

## Описание

Микросервисное приложение для загрузки, хранения, анализа текстовых файлов и генерации облака слов. Реализовано на C# с использованием ASP.NET Core и современной микросервисной архитектуры.

### Основные возможности

- **Загрузка файлов** — поддержка различных текстовых форматов
- **Хранение файлов** — надежное хранение с метаданными
- **Анализ текста** — извлечение ключевых слов и статистики
- **Генерация облака слов** — визуализация частоты слов
- **API для интеграции** — RESTful API для внешних систем

## Архитектура

Приложение построено по принципу микросервисной архитектуры и состоит из следующих компонентов:

### Сервисы

- **ApiGateway** — единая точка входа, маршрутизация и проксирование запросов между сервисами
- **FileStorageService** — управление файлами, хранение метаданных, работа с PostgreSQL
- **FileAnalysisService** — анализ текстового содержимого, генерация облака слов, хранение результатов анализа
- **Common.Models** — общие модели данных для обмена между сервисами

### Технологический стек

- **Backend**: C# (.NET 8), ASP.NET Core
- **База данных**: PostgreSQL
- **Контейнеризация**: Docker, Docker Compose
- **Архитектура**: Микросервисы, REST API

## Подробная структура проекта

### ApiGateway
**Назначение**: Центральный шлюз для всех входящих запросов
**Основные функции**:
- Маршрутизация запросов к соответствующим сервисам
- Проксирование HTTP-запросов
- Базовая валидация входящих данных
- Единая точка входа для клиентов

**Структура**:
```
ApiGateway/
├── Controllers/          # Контроллеры для обработки запросов
├── Middleware/          # Промежуточное ПО (логирование, CORS)
├── Configuration/       # Настройки маршрутизации
├── Program.cs          # Точка входа приложения
└── appsettings.json    # Конфигурация
```

### FileStorageService
**Назначение**: Управление файлами и их метаданными
**Основные функции**:
- Загрузка и сохранение файлов
- Управление метаданными файлов (имя, размер, дата загрузки)
- Предоставление файлов для скачивания
- Работа с базой данных PostgreSQL

**Структура**:
```
FileStorageService/
├── Controllers/         # API контроллеры
├── Models/             # Модели данных
├── Services/           # Бизнес-логика
├── Data/               # Слой доступа к данным
│   ├── Entities/       # Сущности базы данных
│   ├── Context/        # DbContext
│   └── Repositories/   # Репозитории
├── Program.cs          # Точка входа
└── appsettings.json    # Конфигурация
```

**Модели данных**:
- `FileEntity` — информация о файле (ID, имя, размер, путь, дата загрузки)
- `FileMetadata` — дополнительные метаданные

### FileAnalysisService
**Назначение**: Анализ текстового содержимого и генерация облака слов
**Основные функции**:
- Извлечение текста из файлов
- Анализ частоты слов
- Генерация облака слов
- Сохранение результатов анализа

**Структура**:
```
FileAnalysisService/
├── Controllers/         # API контроллеры
├── Models/             # Модели данных
├── Services/           # Бизнес-логика
│   ├── TextAnalysis/   # Анализ текста
│   ├── WordCloud/      # Генерация облака слов
│   └── FileProcessing/ # Обработка файлов
├── Data/               # Слой доступа к данным
├── Program.cs          # Точка входа
└── appsettings.json    # Конфигурация
```

**Модели данных**:
- `AnalysisResult` — результат анализа (ID файла, облако слов, статистика)
- `WordFrequency` — частота слов в тексте

### Common.Models
**Назначение**: Общие модели для обмена данными между сервисами
**Содержимое**:
```
Common.Models/
├── FileModels/         # Модели файлов
├── AnalysisModels/     # Модели анализа
└── SharedModels/       # Общие модели
```

## Принципы работы системы

### 1. Загрузка файла
1. Клиент отправляет файл на `/upload` через ApiGateway
2. ApiGateway перенаправляет запрос в FileStorageService
3. FileStorageService сохраняет файл и создает запись в базе данных
4. Возвращается ID файла для дальнейшего использования

### 2. Анализ файла
1. Клиент запрашивает анализ файла по ID через `/analysis`
2. ApiGateway перенаправляет запрос в FileAnalysisService
3. FileAnalysisService:
   - Получает файл от FileStorageService
   - Извлекает текст из файла
   - Анализирует частоту слов
   - Генерирует облако слов
   - Сохраняет результаты в базу данных
4. Возвращается статус выполнения анализа

### 3. Получение результатов
1. Клиент запрашивает результаты анализа через `/get`
2. ApiGateway перенаправляет запрос в FileAnalysisService
3. FileAnalysisService возвращает сохраненные результаты анализа

### 4. Скачивание файла
1. Клиент запрашивает файл по ID через `/download`
2. ApiGateway перенаправляет запрос в FileStorageService
3. FileStorageService возвращает файл для скачивания

## Взаимодействие сервисов

### Схема взаимодействия
```
Клиент → ApiGateway → FileStorageService (файлы)
                ↓
            FileAnalysisService (анализ)
                ↓
            PostgreSQL (данные)
```

### API Endpoints

**FileStorageService**:
- `POST /api/files/upload` — загрузка файла
- `GET /api/files/download/{id}` — скачивание файла
- `GET /api/files/{id}` — получение информации о файле

**FileAnalysisService**:
- `POST /api/analysis/{fileId}` — запуск анализа файла
- `GET /api/analysis/{fileId}` — получение результатов анализа
- `GET /api/analysis/{fileId}/wordcloud` — получение облака слов

## База данных

### Схема базы данных
- **Files** — таблица с информацией о файлах
- **AnalysisResults** — таблица с результатами анализа
- **WordFrequencies** — таблица с частотой слов

### Миграции
Каждый сервис управляет своей частью базы данных через Entity Framework Core миграции.

## Конфигурация

### Переменные окружения

Основные настройки можно изменить через переменные окружения в `docker-compose.yml`:

- `POSTGRES_CONNECTION_STRING` — строка подключения к PostgreSQL
- `API_GATEWAY_PORT` — порт для API Gateway (по умолчанию 8080)
- `STORAGE_SERVICE_PORT` — порт для сервиса хранения
- `ANALYSIS_SERVICE_PORT` — порт для сервиса анализа

### Порты по умолчанию

- **API Gateway**: 8080
- **FileStorageService**: 8081
- **FileAnalysisService**: 8082
- **PostgreSQL**: 5432


### Установка и запуск

1. **Клонирование репозитория**
   ```bash
   git clone <repository-url>
   cd BHW_2
   ```

2. **Сборка и запуск через Docker Compose**
   ```bash
   docker compose up --build
   ```

3. **Проверка работоспособности**
   ```bash
   curl http://localhost:8080/health
   ```

## API Endpoints

### Загрузка файлов

**POST** `/upload`
- Загружает текстовый файл в систему
- **Параметры**: `file` (multipart/form-data)
- **Ответ**: JSON с ID загруженного файла

```bash
curl -X POST -F "file=@example.txt" http://localhost:8080/upload
```

### Получение файлов

**GET** `/download`
- Скачивает файл по его ID
- **Параметры**: `id` (query parameter)
- **Ответ**: Файл для скачивания

```bash
curl "http://localhost:8080/download?id=<file_id>"
```

### Анализ файлов

**GET** `/analysis`
- Запускает анализ текстового файла
- **Параметры**: `id` (query parameter)
- **Ответ**: JSON с результатами анализа

```bash
curl -X GET "http://localhost:8080/analysis?id=<file_id>"
```

### Получение результатов анализа

**GET** `/get`
- Получает результаты анализа по ID файла
- **Параметры**: `id` (query parameter)
- **Ответ**: JSON с облаком слов и статистикой

```bash
curl -X GET "http://localhost:8080/get?id=<file_id>"
```

## Структура проекта

```
BHW_2/
├── ApiGateway/              # Шлюз API
│   ├── Controllers/         # Контроллеры
│   ├── Middleware/          # Промежуточное ПО
│   ├── Configuration/       # Настройки
│   └── Program.cs           # Точка входа
├── FileStorageService/       # Сервис хранения файлов
│   ├── Controllers/         # API контроллеры
│   ├── Models/              # Модели данных
│   ├── Services/            # Бизнес-логика
│   ├── Data/                # Слой доступа к данным
│   └── Program.cs           # Точка входа
├── FileAnalysisService/      # Сервис анализа файлов
│   ├── Controllers/         # API контроллеры
│   ├── Models/              # Модели данных
│   ├── Services/            # Бизнес-логика
│   ├── Data/                # Слой доступа к данным
│   └── Program.cs           # Точка входа
├── Common.Models/           # Общие модели
│   ├── FileModels/          # Модели файлов
│   ├── AnalysisModels/      # Модели анализа
│   └── SharedModels/        # Общие модели
├── docker-compose.yml       # Конфигурация Docker Compose
└── README.md               # Документация
```

## Логи

Просмотр логов отдельных сервисов:
```bash
# Логи API Gateway
docker compose logs apigateway

# Логи сервиса хранения
docker compose logs filestorageservice

# Логи сервиса анализа
docker compose logs fileanalysisservice
```

### Используемые паттерны
- **Микросервисная архитектура** — разделение на независимые сервисы
- **API Gateway** — единая точка входа
- **Dependency Injection** — внедрение зависимостей
- **Entity Framework Core** — ORM для работы с базой данных
- **Docker** — контейнеризация приложений
- **REST API** — архитектурный стиль API
- **PostgreSQL** — реляционная база данных
- **Single Responsibility** — каждый сервис отвечает за одну область
- **Loose Coupling** — слабая связанность между сервисами
- **High Cohesion** — высокая связанность внутри сервисов
- **Separation of Concerns** — разделение ответственности
