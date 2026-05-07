# Отчёт ЛР15 — CI (GitHub Actions)
## Что сделано
- Добавлен workflow: .github/workflows/dotnet-tests.yml
- Настроен автоматический запуск:
- dotnet restore
- dotnet build
- dotnet test
- (опционально) добавлен бейдж статуса workflow в README
## Как проверить
1) Открыть репозиторий на GitHub
2) Перейти во вкладку Actions
3) Убедиться, что последний прогон завершился успешно
## Проверка работы CI
- Временно изменён тест -> workflow завершился с ошибкой
- Тест исправлен -> workflow снова завершился успешно