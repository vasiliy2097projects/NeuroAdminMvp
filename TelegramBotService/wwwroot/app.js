// Инициализация Telegram Mini App
const tg = window.Telegram.WebApp;

// Расширяем окно на всю высоту
tg.expand();

// Устанавливаем основной цвет фона
document.body.style.backgroundColor = tg.themeParams.bg_color;

// Получаем данные пользователя
const user = tg.initDataUnsafe?.user;
if (user) {
    // Обновляем информацию о пользователе
    document.getElementById('userName').textContent = `${user.first_name} ${user.last_name || ''}`;
    document.getElementById('userUsername').textContent = user.username ? `@${user.username}` : '';
    
    // Устанавливаем аватар пользователя
    const userAvatar = document.getElementById('userAvatar');
    if (user.photo_url) {
        userAvatar.src = user.photo_url;
    } else {
        // Если фото нет, показываем первую букву имени
        userAvatar.style.display = 'none';
    }
}

// Обработчик события готовности приложения
tg.onEvent('viewportChanged', function() {
    tg.expand();
});

// Обработчик события изменения темы
tg.onEvent('themeChanged', function() {
    document.body.style.backgroundColor = tg.themeParams.bg_color;
}); 