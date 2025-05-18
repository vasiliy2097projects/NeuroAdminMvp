// Инициализация Telegram Mini App
const tg = window.Telegram.WebApp;

// Расширяем окно на всю высоту
tg.expand();

// Устанавливаем основной цвет фона
document.body.style.backgroundColor = tg.themeParams.bg_color;

// Обработчик события готовности приложения
tg.onEvent('viewportChanged', function() {
    tg.expand();
});

// Обработчик события изменения темы
tg.onEvent('themeChanged', function() {
    document.body.style.backgroundColor = tg.themeParams.bg_color;
}); 