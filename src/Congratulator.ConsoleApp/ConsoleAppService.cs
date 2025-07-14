using Congratulator.Application;
using Congratulator.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Congratulator.ConsoleApp;

public class ConsoleAppService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConsoleRenderer _view;
    private readonly PromptHelper _prompt;
    private readonly IHostApplicationLifetime _lifetime;

    public ConsoleAppService(
        IServiceScopeFactory scopeFactory, 
        ConsoleRenderer view, 
        PromptHelper prompt, 
        IHostApplicationLifetime lifetime)
    {
        _scopeFactory = scopeFactory;
        _view = view;
        _prompt = prompt;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IBirthdayRepository>();

            _view.Clear();

            var all = await repo.GetAllAsync();
            _view.ShowUpcomingBirthdays(all);

            switch (_prompt.MainMenu())
            {
                case "Показать всё":
                    _view.ShowAll(all);
                    _view.WaitForKey();
                    break;

                case "Добавить":
                    await HandleAddAsync();
                    break;

                case "Редактировать":
                    await HandleEditAsync();
                    break;

                case "Удалить":
                    await HandleDeleteAsync();
                    break;

                case "Выход":
                    _view.Clear();
                    _lifetime.StopApplication();
                    return;
            }
        }
    }

    /*
     * Операции
     */

    private async Task HandleAddAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBirthdayRepository>();

        try
        {
            var (name, date) = _prompt.AskNewEntry();
            if (date > DateOnly.FromDateTime(DateTime.Now))
            {
                await _view.FlashAsync("[red]Вы не герой фильма[/] [aqua]\"Назад в будущее\"[/]");
                return;
            }

            await repo.AddAsync(new BirthdayEntry(0, name, date));
            await _view.FlashAsync("[green]Запись добавлена![/]");
        }
        catch (OperationCanceledException)
        {
            await _view.FlashAsync("[grey]Добавление отменено[/]");
        }
    }

    private async Task HandleEditAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBirthdayRepository>();

        var entries = await repo.GetAllAsync();
        if (!entries.Any())
        {
            await _view.FlashAsync("[grey]Список пуст - редактировать нечего[/]");
            return;
        }

        try
        {
            var entry = _prompt.ChooseEntry(entries, "Выберите запись для редактирования:");
            var (name, date) = _prompt.AskEditData(entry);

            if (date > DateOnly.FromDateTime(DateTime.Now))
            {
                await _view.FlashAsync("[red]Вы не герой фильма[/] [aqua]\"Назад в будущее\"[/]");
                return;
            }

            entry.Name = name;
            entry.DateOfBirth = date;

            await repo.UpdateAsync(entry);
            await _view.FlashAsync("[grey]Запись обновлена[/]");
        }
        catch (OperationCanceledException)
        {
            await _view.FlashAsync("[grey]Редактирование отменено[/]");
        }
    }

    private async Task HandleDeleteAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBirthdayRepository>();

        var entries = await repo.GetAllAsync();
        if (!entries.Any())
        {
            await _view.FlashAsync("[grey]Список пуст - удалять нечего[/]");
            return;
        }

        try
        {
            var entry = _prompt.ChooseEntry(entries, "Выберите запись для удаления:");
            if (!_prompt.ConfirmDelete(entry.Name))
            {
                await _view.FlashAsync("[grey]Удаление отменено[/]");
                return;
            }

            await repo.DeleteAsync(entry.Id);
            await _view.FlashAsync("[green]Запись удалена[/]");
        }
        catch (OperationCanceledException)
        {
            await _view.FlashAsync("[grey]Удаление отменено[/]");
        }
    }
}

