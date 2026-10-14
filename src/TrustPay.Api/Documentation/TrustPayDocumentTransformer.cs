using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TrustPay.Api.Documentation;

public sealed class TrustPayDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string ApiDescription = """
        Биржа цифровых услуг: покупка и продажа игровых аккаунтов, внутриигровых товаров и услуг (например, буст аккаунтов).
        Продавцы публикуют лоты, покупатели оформляют заказы, спорные заказы разбирает арбитр. У каждого пользователя есть внутренний кошелёк: пополнение, вывод и переводы.

        ## Как начать работу

        1. Войдите: `POST /api/auth/login` (нового пользователя создаёт `POST /api/auth/register`).
        2. Скопируйте значение поля `token` из ответа.
        3. Справа откройте блок **Authentication**, выберите **Bearer** и вставьте токен. Слово `Bearer` писать не нужно.

        Токен действует ограниченное время (по умолчанию 15 минут). Новый можно получить без пароля через `POST /api/auth/refresh`.
        Отправляйте запросы на https-адрес сервера: при редиректе с http некоторые клиенты теряют заголовок авторизации.

        ## Роли

        | Роль | Что может |
        | --- | --- |
        | `User` | Лоты, заказы, отзывы, споры и кошелёк. Только свои данные. |
        | `Arbitrator` | Разбирает споры, замораживает и размораживает кошельки. |
        | `Admin` | Справочники (категории, подкатегории, теги), пользователи и их роли, просмотр чужих данных. |

        ## Формат ошибок

        Ошибки приходят в формате ProblemDetails: `title`, `status`, `detail` и `code`.

        | Код | Значение |
        | --- | --- |
        | 400 | Данные не прошли проверку |
        | 401 | Нет токена или он недействителен |
        | 403 | Токен верный, но прав недостаточно |
        | 404 | Объект не найден |
        | 409 | Конфликт (например, email уже занят) |
        """;

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "TrustPay API",
            Version = "v1",
            Description = ApiDescription
        };

        document.Tags = new HashSet<OpenApiTag>
        {
            new() { Name = "Auth", Description = "Регистрация, вход, обновление токена и выход." },
            new() { Name = "Users", Description = "Профили пользователей. Публичный профиль виден всем, полный только владельцу и администратору." },
            new() { Name = "Wallets", Description = "Кошельки: баланс, пополнение, вывод, переводы, заморозка." },
            new() { Name = "Transactions", Description = "История операций по кошелькам." },
            new() { Name = "Categories", Description = "Категории каталога. Изменяет только администратор." },
            new() { Name = "SubCategories", Description = "Подкатегории внутри категорий. Изменяет только администратор." },
            new() { Name = "Tags", Description = "Теги для поиска. Изменяет только администратор." },
            new() { Name = "Lots", Description = "Лоты продавцов: создание, изменение, поиск по подкатегории и пользователю." },
            new() { Name = "Orders", Description = "Заказы: создание, начало выполнения, завершение и отмена." },
            new() { Name = "Reviews", Description = "Отзывы покупателей к завершённым заказам." },
            new() { Name = "Disputes", Description = "Споры по заказам и их разрешение арбитром." },
            new() { Name = "Webhook", Description = "Приём уведомлений от платёжной системы. Защищён подписью, не для пользователей." }
        };

        return Task.CompletedTask;
    }
}
