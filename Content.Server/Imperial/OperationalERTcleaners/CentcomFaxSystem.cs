using Content.Server.Administration.Logs;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.Fax.Components;
using Content.Shared.Database;
using System.Text.RegularExpressions;
using Robust.Shared.Console;
using Robust.Server.Console;
using Content.Shared.Paper;
using Content.Server.Fax;

namespace Content.Server.Imperial.OperationalERTcleaners;

public sealed class CentcomFaxSystem : EntitySystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly IServerConsoleHost _host = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;

    private readonly Regex _patternForm = new(
        @"^ERT\-REQUEST\-(\d{2})\.(\d{2})\.(\d{4})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly ISawmill _logger = Logger.GetSawmill("CentcomFaxSystem");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FaxMachineComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
    }

    private void OnPacketReceived(EntityUid uid, FaxMachineComponent component, DeviceNetworkPacketEvent args)
    {
        if (!HasComp<DeviceNetworkComponent>(uid) || string.IsNullOrEmpty(args.SenderAddress))
            return;

        if (args.Data.TryGetValue(DeviceNetworkConstants.Command, out string? command))
        {
            if (!args.Data.TryGetValue(FaxConstants.FaxPaperNameData, out string? name) ||
                !args.Data.TryGetValue(FaxConstants.FaxPaperContentData, out string? content))
                return;

            args.Data.TryGetValue(FaxConstants.FaxPaperLabelData, out string? label);
            args.Data.TryGetValue(FaxConstants.FaxPaperStampStateData, out string? stampState);
            args.Data.TryGetValue(FaxConstants.FaxPaperStampedByData, out List<StampDisplayInfo>? stampedBy);
            args.Data.TryGetValue(FaxConstants.FaxPaperPrototypeData, out string? prototypeId);
            args.Data.TryGetValue(FaxConstants.FaxPaperLockedData, out bool? locked);

            var printout = new FaxPrintout(content, name, label, prototypeId, stampState, stampedBy, locked ?? false);
            Receive(uid, printout, args.SenderAddress);
        }
    }

    public void Receive(EntityUid uid, FaxPrintout printout, FaxMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.PrintingQueue.Enqueue(printout);

        if (component.FaxName == "Central Command")
            return;

        _logger.Info("СОБЫТИЕ ПОЛУЧЕНО!");
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"Тестовое сообщение в логах");
        _consoleHost.ExecuteCommand("say Тестовая команда выполнена"); // test

        var sendEntity = component.PaperSlot.Item;

        if (!TryComp<PaperComponent>(sendEntity, out var paper))
            return;

        var cleanContent = paper.Content
            .Replace(" ", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim()
            .ToUpperInvariant();

        var match = _patternForm.Match(cleanContent);

        var day = match.Groups[1].Value;
        var month = match.Groups[2].Value;
        var year = match.Groups[3].Value;

        if (!match.Success)
        {
            _logger.Info($"[CentcomFax] Неверный формат. Получено: {cleanContent}");
            return;
        }

        if (!IsValidDate(day, month, year, out var date))
        {
            _logger.Info($"[CentcomFax] Некорректная дата в документе: {day}.{month}.{year}");
            return;

        }

        _consoleHost.ExecuteCommand("callert ERT-Cleaners");
        _host.ExecuteCommand("callert ERT-Cleaners");   // test 2
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"Вызван отряд быстрого реагирования (уборщики)");
        _logger.Info("[CentcomFax] Команда успешно выполнена");

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
}


