using Congratulator.Domain;
using Spectre.Console;

namespace Congratulator.ConsoleApp;

public class ConsoleRenderer
{
    public void Clear() => Console.Clear();

    public void ShowUpcomingBirthdays(IEnumerable<BirthdayEntry> entries)
    {
        if (!entries.Any()) return;

        WriteRule("Ближайшие дни рождения");

        var today = DateOnly.FromDateTime(DateTime.Now);
        var upcoming = entries
            .Select(e =>
            {
                var thisYear = e.DateOfBirth.AddYears(today.Year - e.DateOfBirth.Year);
                var next = thisYear < today ? thisYear.AddYears(1) : thisYear;
                return new { e, next };
            })
            .Where(x => x.next.Year == today.Year)
            .OrderBy(x => x.next)
            .Take(5);

        if (!upcoming.Any()) return;

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Имя")
            .AddColumn("Дата (в этом году)")
            .AddColumn("Через дней");

        foreach (var x in upcoming)
        {
            var days = (x.next.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;
            table.AddRow(x.e.Name, x.next.ToString("dd.MM.yyyy"), days.ToString());
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void ShowAll(IEnumerable<BirthdayEntry> entries)
    {
        WriteRule("Все записи");

        if (!entries.Any())
        {
            AnsiConsole.MarkupLine("[grey]Список пуст[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Id")
            .AddColumn("Имя")
            .AddColumn("Дата рождения");

        foreach (var e in entries.OrderBy(e => e.DateOfBirth))
            table.AddRow(e.Id.ToString(), e.Name, e.DateOfBirth.ToString("dd.MM.yyyy"));

        AnsiConsole.Write(table);
    }

    public async Task FlashAsync(string markup, int ms = 1000)
    {
        AnsiConsole.MarkupLine(markup);
        await Task.Delay(ms);
    }

    public void WaitForKey()
    {
        AnsiConsole.MarkupLine("\n[grey](Нажмите любую клавишу, чтобы вернуться в меню)[/]");
        Console.ReadKey();
    }

    private static void WriteRule(string title) =>
        AnsiConsole.Write(new Rule($"[yellow]{title}[/]").RuleStyle("grey").Justify(Justify.Left));
}
