# Poll System API

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=microsoft&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-A881FF?style=for-the-badge&logo=xunit&logoColor=white)
![Serilog](https://img.shields.io/badge/Serilog-F9A825?style=for-the-badge&logo=nuget&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)



## 📋 Описание проекта

**Poll System API** — это мощный и гибкий бэкенд для системы голосований и опросов, разработанный на **ASP.NET Core**. Проект построен на принципах **Чистой Архитектуры** с использованием паттерна **CQRS (Command Query Responsibility Segregation)** с библиотекой **MediatR**. Это обеспечивает четкое разделение бизнес-логики, данных и представления, делая систему легко расширяемой, тестируемой и поддерживаемой.

API предоставляет полный набор эндпоинтов для управления пользователями, опросами, вариантами ответов и голосованием, с ролевой моделью доступа и JWT-аутентификацией.

---

## ✨ Основной функционал

-   🔐 **Аутентификация и Авторизация**:
    -   Регистрация, вход и смена пароля.
    -   **JWT (JSON Web Token)** для защиты эндпоинтов.
    -   Обновление токенов с помощью **Refresh Tokens**.
    -   Ролевая модель (**Admin**, **User**).

-   📝 **Управление опросами (Polls)**:
    -   Полный CRUD для опросов (создание, чтение, обновление, удаление).
    -   Настройка времени начала и окончания, анонимности, множественного выбора.
    -   Гибкая фильтрация и пагинация списка опросов (по названию, датам, тегам).

-   ✅ **Управление вариантами ответов (Options)**:
    -   Добавление, обновление и удаление вариантов ответов для существующих опросов.

-   🗳️ **Голосование (Voting)**:
    -   Прием голосов с проверкой на активность опроса и повторное голосование.
    -   Поддержка анонимных и неанонимных опросов.

-   📊 **Результаты и Аналитика**:
    -   Получение результатов опроса после его завершения.
    -   Экспорт результатов в **CSV** файл.
    -   Досрочное завершение опроса администратором.

-   🛡️ **Надежность и Качество**:
    -   **Централизованная обработка ошибок** с помощью `IExceptionHandler`.
    -   **Валидация** всех входящих запросов с помощью **FluentValidation**.
    -   **Структурированное логирование** с помощью **Serilog** для удобной отладки и мониторинга.
    -   **Юнит-тесты** на **xUnit** и **Moq**, покрывающие ключевую бизнес-логику.

---

## 🏛️ Архитектура

Проект следует принципам **Чистой Архитектуры** и разделен на следующие слои:

-   📁 `Domain`: Содержит сущности, кастомные исключения и основные бизнес-правила. Не зависит ни от чего.
-   📁 `Application`: Содержит бизнес-логику (CQRS команды и запросы), интерфейсы репозиториев и сервисов, DTO и валидацию. Зависит только от `Domain`.
-   📁 `Infrastructure`: Реализует интерфейсы из `Application`. Содержит конфигурацию EF Core, репозитории, реализацию JWT-сервисов и другие внешние зависимости. Зависит от `Application`.
-   📁 `Api`: Точка входа в приложение. Содержит контроллеры, конфигурацию DI и пайплайна ASP.NET Core. Зависит от `Application` и `Infrastructure`.

---

## 🛠️ Технологии

-   **Backend**: ASP.NET Core 8
-   **Архитектура**: Clean Architecture, CQRS с MediatR
-   **ORM**: Entity Framework Core
-   **База данных**: Microsoft SQL Server
-   **Аутентификация**: JWT, ASP.NET Core Identity
-   **Валидация**: FluentValidation
-   **Логирование**: Serilog
-   **Тестирование**: xUnit, Moq, FluentAssertions
-   **Документация API**: Swagger (OpenAPI)

---

## 🚀 Локальный запуск и настройка

### 1. Предварительные требования

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Microsoft SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (например, Express или Developer Edition)
-   IDE (Visual Studio 2022, JetBrains Rider, VS Code)

### 2. Клонирование репозитория

```bash
git clone https://github.com/Keng77/PollSystemApp.git
cd PollSystemApp
### 3. Настройка базы данных

Откройте файл `appsettings.json` в проекте `PollSystemApp.Api` и измените строку подключения `DefaultConnection` под вашу конфигурацию SQL Server.

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PollSystemDb;Integrated Security=true;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 4. Настройка секретов

Проект использует `user-secrets` для хранения чувствительных данных (JWT-ключ).

Откройте терминал в корневой папке проекта (`PollSystemApp.Api`) и выполните следующие команды:

**Инициализация user-secrets:**
```bash
dotnet user-secrets init --project ./PollSystemApp.Api
```

**Добавление секретов для JWT:**
```bash
dotnet user-secrets set "JwtSettings:Secret" "YOUR_SUPER_SECRET_KEY_THAT_IS_LONG_AND_COMPLEX"
dotnet user-secrets set "JwtSettings:Issuer" "PollSystemApp.Api"
dotnet user-secrets set "JwtSettings:Audience" "PollSystemApp.ApiClient"
dotnet user-secrets set "JwtSettings:ExpiryMinutes" 60
```
> **Важно:** Замените `"YOUR_SUPER_SECRET_KEY..."` на свою собственную длинную и сложную строку для обеспечения безопасности.

### 5. Применение миграций

Выполните миграции для создания схемы базы данных.

*В **Package Manager Console** (убедитесь, что `PollSystemApp.Api` выбран как Startup Project):*
```powershell
Update-Database
```

*Или через **.NET CLI**:*
```bash
dotnet ef database update --project PollSystemApp.Infrastructure --startup-project PollSystemApp.Api
```

### 6. Запуск приложения
Запустите проект `PollSystemApp.Api` из вашей IDE или через терминал:
```bash
dotnet run --project PollSystemApp.Api
```
Приложение будет доступно по адресу `https://localhost:PORT` или `http://localhost:PORT`. Swagger UI будет доступен по адресу `https://localhost:PORT/swagger`.

---

## 🧪 Тестирование
Проект содержит набор юнит-тестов. Для их запуска выполните команду в корневой папке решения:
```bash
dotnet test
```

## 🗂️ API Эндпоинты
Вся документация по API доступна через **Swagger UI** после запуска приложения. Основные группы эндпоинтов:

-   `/api/Auth`: Регистрация, вход, обновление токена, смена пароля.
-   `/api/Polls`: Полное управление опросами и их опциями, голосование и получение результатов.

> **Авторизация:** Некоторые эндпоинты требуют авторизации. Для этого сначала выполните запрос на `/api/Auth/login`, скопируйте полученный токен. Затем в Swagger UI нажмите кнопку **Authorize**, вставьте токен в формате `Bearer YOUR_TOKEN` и нажмите **Authorize**.
