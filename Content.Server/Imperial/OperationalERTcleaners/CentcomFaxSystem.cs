using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Fax;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Database;
using Content.Shared.Fax.Components;
using Content.Shared.Paper;
using Robust.Shared.Console;
using System.Text.RegularExpressions;

namespace Content.Server.Imperial.OperationalERTcleaners;

public sealed class CentcomFaxSystem : EntitySystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    private readonly Regex _patternERTRequest1 = new(
        @"ЗАПРОСНАВЫЗОВ",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Regex _patternERTRequest2 = new(
        @"ОБР-УБОРЩИКОВ",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Regex _patternShape = new(
        @"ФОРМА:NT\-([A-Z]{3})\-SOD-REQ",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Regex _patternStationName = new(
        @"СТАНЦИЯ:NT14\-([A-Z]{2})\-(\d{3})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Regex _patternDate = new(
        @"ДАТА\:(0[1-9]|[12][0-9]|3[01])\/(0[1-9]|1[0-2])\/([0-9]{4})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Regex _patternAccountablePerson = new(
        @"ПОДОТЧЁТНОЕЛИЦО\:[а-яА-ЯёЁ\s-]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Regex _patternrole = new(
        @"ДОЛЖНОСТЬ\:[а-яА-ЯёЁ\s]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly ISawmill _logger = Logger.GetSawmill("CentcomFaxSystem");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CentcomFaxComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
    }

    private void OnPacketReceived(EntityUid uid, CentcomFaxComponent component, DeviceNetworkPacketEvent args)
    {
        if (!HasComp<DeviceNetworkComponent>(uid) || string.IsNullOrEmpty(args.SenderAddress))
            return;

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

    public void Receive(EntityUid uid, FaxPrintout printout, string? fromAddress = null, CentcomFaxComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _logger.Info("СОБЫТИЕ ПОЛУЧЕНО!");
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"Тестовое сообщение в логах"); // change!
        _chat.SendAdminAnnouncement("этап 1"); // change!

        var text = printout.Content
            .Replace(" ", "")
            .ToUpperInvariant();

        // вызов именно ОБР-уборщиков
        var ERTRequest1 = _patternERTRequest1.Match(text);
        if (!ERTRequest1.Success)
            return;
        var ERTRequest2 = _patternERTRequest2.Match(text);
        if (!ERTRequest2.Success)
            return;

        // форма документа
        var shape = _patternShape.Match(text);
        if (!shape.Success)
        {
            _logger.Info($"[CentcomFax] Неверная ФОРМА документа. Получено: {text}");
            return;
        }

        // Номер станции
        var stationName = _patternStationName.Match(text);
        if (!stationName.Success)
        {
            _logger.Info($"[CentcomFax] Неверный формат СТАНЦИИ. Получено: {text}");
            return;
        }

        // Дата
        var date = _patternDate.Match(text);
        if (!date.Success)
        {
            _logger.Info($"[CentcomFax] Неверный формат ДАТЫ. Получено: {text}");
            return;
        }

        // Подотчётное лицо
        var accountablePerson = _patternAccountablePerson.Match(text);
        if (!accountablePerson.Success)
        {
            _logger.Info($"[CentcomFax] Неверное подотчетное лицо. Получено: {text}");
            return;
        }

        // Должность
        var role = _patternrole.Match(text);
        if (!role.Success)
        {
            _logger.Info($"[CentcomFax] Неверная должность. Получено: {text}");
            return;
        }

        // Если проверка успешная
        _consoleHost.ExecuteCommand("callert ERT-Cleaners");
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"Вызван отряд быстрого реагирования (уборщики) от игрока ..."); // change!
        _logger.Info("[CentcomFax] Команда успешно выполнена");
        _chat.SendAdminAnnouncement("последний этап"); // change!
    }
}


