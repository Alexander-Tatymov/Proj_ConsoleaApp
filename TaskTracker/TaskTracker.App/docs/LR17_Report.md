# Отчёт ЛР17 — обработка ошибок## Что сделано
- Добавлен отдельный error-log файл logs/errors_YYYY-MM-DD.log
- Реализован SafeRunner для единой обработки ошибок
- Команды backup/export/import/stats/log выполняются через SafeRunner
## Как проверить
1) Запустить приложение
2) Ввести неверный путь при Import
3) Убедиться, что программа не закрылась
4) Проверить logs/app_YYYY-MM-DD.log
5) Проверить logs/errors_YYYY-MM-DD.log (для неожиданных ошибок)