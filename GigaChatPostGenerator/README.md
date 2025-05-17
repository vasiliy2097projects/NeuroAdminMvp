# GigaChat Post Generator

Этот веб-сервис генерирует посты на заданную тематику с помощью GigaChat API.

## Требования

- .NET 7.0 или выше
- API ключ GigaChat

## Запуск проекта

1. Клонируйте репозиторий
2. Перейдите в директорию проекта
3. Выполните команду:

```bash
dotnet run
```

## Использование API

### Генерация поста

**Endpoint:** POST /api/post/generate

**Request Body:**

```json
{
  "topic": "Ваша тема"
}
```

**Response:**

```json
{
  "post": "Сгенерированный текст поста"
}
```

## Пример использования с curl

```bash
curl -X POST "https://localhost:7001/api/post/generate" \
     -H "Content-Type: application/json" \
     -d '{"topic": "Искусственный интеллект"}'
```
