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

// Функция для создания элемента канала
function createChannelElement(channel) {
    const channelElement = document.createElement('div');
    channelElement.className = 'channel-item';
    
    const avatar = document.createElement('div');
    avatar.className = 'channel-avatar';
    avatar.textContent = channel.title.charAt(0).toUpperCase();
    
    const info = document.createElement('div');
    info.className = 'channel-info';
    
    const name = document.createElement('div');
    name.className = 'channel-name';
    name.textContent = channel.title;
    
    const subscribers = document.createElement('div');
    subscribers.className = 'channel-subscribers';
    subscribers.textContent = `${channel.membersCount || 0} подписчиков`;
    
    info.appendChild(name);
    info.appendChild(subscribers);
    
    channelElement.appendChild(avatar);
    channelElement.appendChild(info);
    
    return channelElement;
}

// Функция для создания элемента слота
function createSlotElement(slot) {
    const slotElement = document.createElement('div');
    slotElement.className = 'slot-item';
    
    const header = document.createElement('div');
    header.className = 'slot-header';
    
    const name = document.createElement('div');
    name.className = 'slot-name';
    name.textContent = slot.name;
    
    const status = document.createElement('div');
    status.className = `slot-status ${slot.isActive ? '' : 'inactive'}`;
    status.textContent = slot.isActive ? 'Активен' : 'Неактивен';
    
    header.appendChild(name);
    header.appendChild(status);
    
    const channels = document.createElement('div');
    channels.className = 'slot-channels';
    channels.textContent = `Из ${slot.sourceChannelTitle} в ${slot.targetChannelTitle}`;
    
    slotElement.appendChild(header);
    slotElement.appendChild(channels);
    
    return slotElement;
}

// Функция для загрузки списка каналов
async function loadChannels() {
    try {
        const response = await fetch('/api/channels');
        if (!response.ok) {
            throw new Error('Failed to load channels');
        }
        
        const channels = await response.json();
        const channelsList = document.getElementById('channelsList');
        channelsList.innerHTML = ''; // Очищаем список
        
        channels.forEach(channel => {
            channelsList.appendChild(createChannelElement(channel));
        });

        // Обновляем списки в форме создания слота
        const sourceChannel = document.getElementById('sourceChannel');
        const targetChannel = document.getElementById('targetChannel');
        
        sourceChannel.innerHTML = '<option value="">Выберите канал-источник</option>';
        targetChannel.innerHTML = '<option value="">Выберите канал назначения</option>';
        
        channels.forEach(channel => {
            const sourceOption = document.createElement('option');
            sourceOption.value = channel.id;
            sourceOption.textContent = channel.title;
            sourceChannel.appendChild(sourceOption);

            const targetOption = document.createElement('option');
            targetOption.value = channel.id;
            targetOption.textContent = channel.title;
            targetChannel.appendChild(targetOption);
        });
    } catch (error) {
        console.error('Error loading channels:', error);
        tg.showAlert('Ошибка при загрузке списка каналов');
    }
}

// Функция для загрузки списка слотов
async function loadSlots() {
    try {
        const response = await fetch('/api/slots');
        if (!response.ok) {
            throw new Error('Failed to load slots');
        }
        
        const slots = await response.json();
        const slotsList = document.getElementById('slotsList');
        slotsList.innerHTML = ''; // Очищаем список
        
        slots.forEach(slot => {
            slotsList.appendChild(createSlotElement(slot));
        });
    } catch (error) {
        console.error('Error loading slots:', error);
        tg.showAlert('Ошибка при загрузке списка слотов');
    }
}

// Функция для проверки формата ID канала
function isValidChannelId(channelId) {
    // Проверяем формат -100xxxxxxxxxx или @username
    return /^-100\d{10}$/.test(channelId) || /^@[\w\d_]{5,32}$/.test(channelId);
}

// Обработчики событий для формы создания слота
document.getElementById('addSlotButton').addEventListener('click', () => {
    document.getElementById('slotForm').classList.add('active');
});

document.getElementById('cancelSlotButton').addEventListener('click', () => {
    document.getElementById('slotForm').classList.remove('active');
    document.getElementById('slotName').value = '';
    document.getElementById('sourceChannel').value = '';
    document.getElementById('targetChannel').value = '';
});

document.getElementById('saveSlotButton').addEventListener('click', async () => {
    const name = document.getElementById('slotName').value;
    const sourceChannelId = document.getElementById('sourceChannel').value;
    const targetChannelId = document.getElementById('targetChannel').value;

    if (!name || !sourceChannelId || !targetChannelId) {
        tg.showAlert('Пожалуйста, заполните все поля');
        return;
    }

    if (!isValidChannelId(sourceChannelId)) {
        tg.showAlert('Неверный формат ID канала-источника');
        return;
    }

    if (!isValidChannelId(targetChannelId)) {
        tg.showAlert('Неверный формат ID канала назначения');
        return;
    }

    if (sourceChannelId === targetChannelId) {
        tg.showAlert('Канал-источник и канал назначения не могут совпадать');
        return;
    }

    try {
        const response = await fetch('/api/slots', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                name,
                sourceChannelId,
                sourceChannelTitle: sourceChannelId,
                targetChannelId,
                targetChannelTitle: targetChannelId,
                isActive: true
            })
        });

        if (!response.ok) {
            throw new Error('Failed to create slot');
        }

        document.getElementById('slotForm').classList.remove('active');
        document.getElementById('slotName').value = '';
        document.getElementById('sourceChannel').value = '';
        document.getElementById('targetChannel').value = '';

        await loadSlots();
        tg.showAlert('Слот успешно создан');
    } catch (error) {
        console.error('Error creating slot:', error);
        tg.showAlert('Ошибка при создании слота');
    }
});

// Загружаем списки при загрузке страницы
loadChannels();
loadSlots();

// Обработчик события готовности приложения
tg.onEvent('viewportChanged', function() {
    tg.expand();
});

// Обработчик события изменения темы
tg.onEvent('themeChanged', function() {
    document.body.style.backgroundColor = tg.themeParams.bg_color;
}); 