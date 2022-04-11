using System.Collections.Generic;

namespace WebApplicationTechSale.HelperServices
{
    public static class RedirectionMessageProvider
    {
        public static List<string> LotAcceptedMessages()
        {
            return new List<string>()
            {
                "Модерация прошла успешно",
                "Лот опубликован и теперь виден всем пользователям.",
                "Сейчас вы будете перенаправлены на страницу с лотами"
            };
        }

        public static List<string> LotRejectedMessages()
        {
            return new List<string>()
            {
                "Вы отклонили публикацию лота",
                "Пользователь получит уведомление и сможет отредактировать данные для повторной публикации.",
                "Сейчас вы будете перенаправлены на страницу c лотами",
            };
        }

        public static List<string> LotCreatedMessages()
        {
            return new List<string>()
            {
                "Лот успешно создан",
                "Сейчас лот будет отправлен на модерацию. Модератор примет решение о публикации. Проверяйте статус лота в личном кабинете",
                "Если вы подписаны на нашего Telegram-бота, то будете уведомлены о публикации",
                "Сейчас вы будете перенаправлены на главную страницу"
            };
        }

        public static List<string> LotUpdatedMessages()
        {
            return new List<string>()
            {
                "Данные обновлены",
                "Лот повторно отправлен на модерацию. Проверяйте статус лота в личном кабинете",
                "Если вы подписаны на нашего Telegram-бота, то будете уведомлены о публикации",
                "Сейчас вы будете перенаправлены на главную страницу"
            };
        }

        public static List<string> AccountCreatedMessages()
        {
            return new List<string>()
            {
                "Регистрация прошла успешно",
                "Сейчас вы будете перенаправлены на главную страницу нашего сайта"
            };
        }

        public static List<string> BidPlacedMessages()
        {
            return new List<string>()
            {
                "Ставка принята",
                "Сейчас вы будете перенаправлены на страницу лота, где сможете увидеть вашу ставку"
            };
        }

        public static List<string> AccountUpdatedMessages()
        {
            return new List<string>()
            {
                "Изменения сохранены",
                "Сейчас вы будете перенаправлены в личный кабинет"
            };
        }

        public static List<string> AuctionTimeUpMessages()
        {
            return new List<string>()
            {
                "К сожалению, торги окончены...",
                "Ваша ставка не была засчитана",
                "Сейчас вы будете перенаправлены на страницу лота"
            };
        }
    }
}
