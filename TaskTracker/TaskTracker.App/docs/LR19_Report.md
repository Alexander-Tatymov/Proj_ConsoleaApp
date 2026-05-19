# Отчёт ЛР19 — роли и доступ## Что сделано
- В config.json добавлено поле Role (User/Admin)
- Реализован AccessControl

- Ограничены команды:
- Delete (4)
- Import (11)
- Settings (15)
## Как проверить
1) Role=User -> Delete/Import/Settings недоступны
2) Role=Admin -> все команды доступны