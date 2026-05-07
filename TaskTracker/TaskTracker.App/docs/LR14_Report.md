# Отчёт ЛР14 — юнит-тесты
## Что сделано
- Создан проект TaskTracker.Tests (MSTest)
- Добавлены тесты TaskValidator:
- пустой Title -> ошибка
- слишком длинный Title -> ошибка
- слишком длинное Description -> ошибка
- валидная задача -> ok
- Добавлены тесты TaskService:
- Add валидной задачи
- Add пустого Title -> исключение
- Delete существующей задачи
- Update меняет Title и Description
## Как запускать тесты
Visual Studio -> Test -> Test Explorer -> Run All
## Результат
Все тесты проходят успешно.