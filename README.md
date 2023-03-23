# Телеграм-бот для учета расходов
API: https://github.com/Relfick/BBCFinanceAPI

# Запуск через docker
1. Клонирование проекта
    ```
    git clone https://github.com/Relfick/BBCFinanceAPI
    ```
2. Установить токен телеграм-бота **BotToken** в _BBCFinanceBot/appsettings.json_
   ```
   "BotToken": "<YOUR_TOKEN>"
   ```
3. Сборка 
    ```
   docker build -f BBCFinanceBot/Dockerfile -t bot .
   ```
4. Запуск
   ```
   docker run -di --rm --name bot1 bot
   ```