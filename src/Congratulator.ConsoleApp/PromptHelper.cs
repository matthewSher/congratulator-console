using Congratulator.Domain;
using Spectre.Console;

namespace Congratulator.ConsoleApp;

public class PromptHelper
{
    public string MainMenu() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите действие:")
                .AddChoices("Показать всё", "Добавить", "Редактировать", "Удалить", "Выход"));

    public (string Name, DateOnly Date) AskNewEntry()
    {
        ShowCancelHint();

        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Введите [green]имя[/]: ")
                .AllowEmpty()
                .ValidationErrorMessage("[red]Имя не может быть пустым[/]")
                .Validate(CancelOrNotEmpty));

        var rawDate = AnsiConsole.Prompt(
            new TextPrompt<string>("Введите [green]дату рождения[/] (ДД.ММ.ГГГГ): ")
                .AllowEmpty()
                .ValidationErrorMessage("[red]Некорректная дата[/]")
                .Validate(CancelOrValidDate));

        return (name, DateOnly.ParseExact(rawDate, "dd.MM.yyyy"));
    }

    public BirthdayEntry ChooseEntry(IEnumerable<BirthdayEntry> entries, string title)
    {
        const string cancel = "[red](X) Отмена[/]";

        var pick = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .AddChoices(entries.Select(e => $"{e.Id}. {e.Name} ({e.DateOfBirth:dd.MM.yyyy})"))
                .AddChoices(cancel));

        if (pick == cancel) throw new OperationCanceledException();

        return entries.First(e => pick.StartsWith($"{e.Id}.", StringComparison.Ordinal));
    }

    public (string Name, DateOnly Date) AskEditData(BirthdayEntry entry)
    {
        ShowCancelHint();

        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Новое имя: ")
                .AllowEmpty()
                .ValidationErrorMessage("[red]Имя не может быть пустым[/]")
                .Validate(CancelOrNotEmpty)
                .DefaultValue(entry.Name));

        var rawDate = AnsiConsole.Prompt(
            new TextPrompt<string>($"Новая дата ([grey]{entry.DateOfBirth:dd.MM.yyyy}[/]): ")
                .AllowEmpty()
                .ValidationErrorMessage("[red]Некорректная дата[/]")
                .Validate(CancelOrValidDate)
                .DefaultValue(entry.DateOfBirth.ToString("dd.MM.yyyy")));

        return (name, DateOnly.ParseExact(rawDate, "dd.MM.yyyy"));
    }

    public bool ConfirmDelete(string name) =>
        AnsiConsole.Confirm($"Вы уверены, что хотите удалить [red]{name}[/]?");

    private static ValidationResult CancelOrValidDate(string arg)
    {
        if (IsCancel(arg)) throw new OperationCanceledException();
        return DateOnly.TryParseExact(arg, "dd.MM.yyyy", out _)
            ? ValidationResult.Success()
            : ValidationResult.Error("[red]Некорректная дата[/]");
    }

    private static ValidationResult CancelOrNotEmpty(string arg)
    {
        if (IsCancel(arg)) throw new OperationCanceledException();
        return !string.IsNullOrWhiteSpace(arg)
            ? ValidationResult.Success()
            : ValidationResult.Error("[red]Имя не может быть пустым[/]");
    }

    private static void ShowCancelHint() =>
        AnsiConsole.MarkupLine("[grey](Введите 'q' в любой момент, чтобы отменить операцию)[/]\n");

    private static bool IsCancel(string s) =>
        string.Equals(s, "q", StringComparison.OrdinalIgnoreCase);
}
