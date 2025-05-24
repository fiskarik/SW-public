using Content.Shared.Imperial.OperationalERTcleaners;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using System.Text.RegularExpressions;
using Robust.Shared.Console;
using Robust.Server.Player;

namespace Content.Server.Imperial.OperationalERTcleaners;

public sealed class CentcomFaxSystem : EntitySystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CentcomFaxComponent, FaxCentcomReceivedEvent>(OnCentcomFaxReceived);
    }
    private void OnCentcomFaxReceived(EntityUid uid, CentcomFaxComponent component, FaxCentcomReceivedEvent args)
    {
        Log.Debug("[CentcomFax] Начата обработка документа...");
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"тест");

        var cleanContent = args.Content
            .Replace(" ", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim()
            .ToUpperInvariant();

        var match = PatternForm.Match(cleanContent);

        var day = match.Groups[1].Value;
        var month = match.Groups[2].Value;
        var year = match.Groups[3].Value;

        if (!match.Success)
        {
            Log.Debug($"[CentcomFax] Неверный формат. Шаблон: {PatternForm}, Получено: {cleanContent}");
            return;
        }

        if (!IsValidDate(day, month, year, out var date))
        {
            Log.Debug($"[CentcomFax] Некорректная дата в документе: {day}.{month}.{year}");
            return;

        }

        Log.Debug("[CentcomFax] Пытаюсь выполнить команду: callert ERT-Cleaners");

        if (_consoleHost == null)
        {
            Log.Error("[CentcomFax] ConsoleHost не инициализирован!");
            return;
        }

        _consoleHost.ExecuteCommand(shell.Player, "callert ERT-Cleaners");
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"Вызван отряд быстрого реагирования (уборщики)");
        Log.Debug("[CentcomFax] Команда успешно выполнена");

    }

    private bool IsValidDate(string dayStr, string monthStr, string yearStr, out DateOnly date)
    {
        date = default;
        if (!int.TryParse(dayStr, out var day) ||
            !int.TryParse(monthStr, out var month) ||
            !int.TryParse(yearStr, out var year))
        {
            return false;
        }

        try
        {
            date = new DateOnly(year, month, day);

            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var maxDate = currentDate.AddYears(1000);

            if (date > maxDate)
            {
                return false;
            }

            return true;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return false;
        }
    }

    private static readonly Regex PatternForm = new(
        @"^ERT\-REQUEST\-(\d{2})\.(\d{2})\.(\d{4})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
}


