# Production Planning

ASP.NET Core приложение с MySQL/SQLite, Identity и SignalR.

**Репозиторий:** [https://github.com/natalliakhilman-oss/ProductionPlanning](https://github.com/natalliakhilman-oss/ProductionPlanning)

## Важно: где вы открываете приложение

- **Локально** (запуск `run.bat` или `run.ps1` на своём ПК) — в браузере открывается **http://localhost:5185**. Используются файлы с вашего диска. Все правки в коде видны после перезапуска.
- **В облаке** (Railway, Render и т.п.) — открывается URL вида `https://ваш-сервис.railway.app`. Там крутится версия, **собранная из GitHub**. Локальные правки туда не попадают, пока вы не сделаете **git push** в репозиторий. После пуша хостинг пересоберёт приложение, и изменения появятся на сайте.

Если вы смотрите приложение по облачному URL и не видите изменений — закоммитьте и запушьте их в GitHub (см. ниже).

## Локальный запуск

Из корня репозитория запустите `run.bat` (Windows) или `run.ps1` (PowerShell). Откройте в браузере: **http://localhost:5185**.

## GitHub

1. Создайте репозиторий на [GitHub](https://github.com/new).
2. Инициализируйте Git и привяжите удалённый репозиторий:

```bash
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/natalliakhilman-oss/ProductionPlanning.git
git push -u origin main
```

3. При каждом пуше в `main`/`master` запускается CI (сборка в `.github/workflows/ci.yml`). Если приложение задеплоено из этого репозитория (Railway, Render и т.д.), после `git push` хостинг пересоберёт проект и изменения появятся на сайте.

**Отправить локальные изменения в GitHub (чтобы обновился облачный сайт):**
```bash
git add .
git status
git commit -m "Новая заявка: одна форма вместо 3 шагов"
git push origin main
```
После пуша подождите 1–3 минуты, пока хостинг пересоберёт приложение, затем обновите страницу в браузере (лучше Ctrl+F5).

## База данных

### Локально (Production-режим)

1. Скопируйте пример конфига:
   ```bash
   copy ProductionPlanning\hosting.example.json ProductionPlanning\hosting.json
   ```
2. Заполните в `hosting.json`: `db_host`, `db_port`, `db_name`, `db_user`, `db_pass` (или зашифрованный пароль через `Crypt.Encrypt`).  
   Файл `hosting.json` в `.gitignore` — в репозиторий не попадает.

### На сервере / в облаке (Railway, Render и т.п.)

Задайте переменные окружения (пароль — обычный текст, без шифрования):

| Переменная     | Описание        | Пример    |
|----------------|-----------------|-----------|
| `DB_HOST`      | Хост MySQL      | `localhost` или хост провайдера |
| `DB_PORT`      | Порт            | `3306`    |
| `DB_NAME`      | Имя БД          | `ProductPlanning` |
| `DB_USER`      | Пользователь    | `root`    |
| `DB_PASSWORD`  | Пароль          | ваш пароль |
| `DB_TIMEOUT`   | Таймаут (сек)   | `60`      |

Приоритет: переменные окружения перекрывают значения из `hosting.json`.

## Деплой: Vercel и бэкенд .NET

**Vercel** не поддерживает запуск ASP.NET Core. На Vercel обычно разворачивают фронтенд (Next.js, SPA и т.д.).

Для этого бэкенда используйте платформы с поддержкой .NET:

- **[Railway](https://railway.app)** — импорт репозитория из GitHub, добавление MySQL, задание переменных `DB_*`.
- **[Render](https://render.com)** — Web Service из GitHub, образ .NET, переменные окружения для БД.
- **Azure App Service**, **DigitalOcean App Platform** — аналогично: репозиторий + переменные для MySQL.

### Связка: GitHub → облако → БД

1. **GitHub** — исходный код, CI при пуше.
2. **Облачный хостинг** (Railway/Render и т.д.) — подключаете тот же репозиторий, деплой при пуше в `main`.
3. **База** — облачный MySQL (PlanetScale, Railway MySQL, или свой сервер). Параметры подключения задаёте переменными `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`, `DB_TIMEOUT` в настройках сервиса.

Если позже сделаете отдельный фронт на Vercel, его можно подключить к тому же GitHub-репозиторию и настроить API URL на адрес этого .NET-бэкенда.
