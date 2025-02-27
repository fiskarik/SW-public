using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.Imperial.Zlevels;

/// <summary>
/// Проход, который позволяет по клику, драг дропу или при соприкосновении перемещаться к сваязанному проходу.
/// </summary>
[RegisterComponent]
public sealed partial class LadderComponent : Component
{
    #region Enabled
    /// <summary>
    /// Включен ли люк?
    /// </summary>
    [DataField("enabled"), ViewVariables(VVAccess.ReadWrite)]
    public bool Enabled = true;

    /// <summary>
    /// Включён ли перенос при контакте с люком?
    /// </summary>
    [DataField("moveByCollideEnabled"), ViewVariables(VVAccess.ReadWrite)]
    public bool MoveByCollideEnabled = true;

    /// <summary>
    /// Включён ли перенос при клике по люку?
    /// </summary>
    [DataField("moveByClickEnabled"), ViewVariables(VVAccess.ReadWrite)]
    public bool MoveByClickEnabled = true;

    /// <summary>
    /// Включён ли перенос при драг-дропе на люк?
    /// </summary>
    [DataField("moveByDragDropEnabled"), ViewVariables(VVAccess.ReadWrite)]
    public bool MoveByDragDropEnabled = true;

    /// <summary>
    /// Можно ли блокировать люк
    /// </summary>
    [DataField("canLocked"), ViewVariables(VVAccess.ReadWrite)]
    public bool CanLocked = false;

    /// <summary>
    /// Можно ли закрыть люк
    /// </summary>
    [DataField("canClosed"), ViewVariables(VVAccess.ReadWrite)]
    public bool CanClosed = false;

    #endregion
    #region ID info

    /// <summary>
    /// ID люка
    /// </summary>
    [DataField("ladderID"), ViewVariables(VVAccess.ReadWrite)]
    public string LadderID = "default";

    /// <summary>
    /// ID группы люка
    /// </summary>
    [DataField("groupID"), ViewVariables(VVAccess.ReadWrite)]
    public string GroupID = "default";

    /// <summary>
    /// Этаж. Берётся из карты
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int Level = 0;

    #endregion

    #region Move utility
    /// <summary>
    /// Список объектов, которые должны игнорироваться коллайдером целевого люка после переноса
    /// </summary>
    [DataField("ignoreObjects"), ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> IgnoreObjects = new();

    /// <summary>
    /// Время активации люка
    /// </summary>
    [DataField("activationTime"), ViewVariables(VVAccess.ReadWrite)]
    public float ActivationTime = 0f;

    #endregion

    #region Access

    /// <summary>
    /// Проверять ли доступ к блокировке люка
    /// </summary>
    [DataField("checkAccessForLock"), ViewVariables(VVAccess.ReadWrite)]
    public bool CheckAccessForLock = false;

    #endregion

    #region Graphics

    /// <summary>
    /// Состояние люка
    /// </summary>
    [DataField("ladderDoorState"), ViewVariables(VVAccess.ReadOnly)]
    public LadderDoorState LadderDoorState = LadderDoorState.Opened;
    [DataField("ladderIndicatorState"), ViewVariables(VVAccess.ReadOnly)]
    public LadderIndicatorState LadderIndicatorState = LadderIndicatorState.Unlocked;

    /// <summary>
    /// Состояние двери люка при старте игры
    /// </summary>
    [DataField("startLadderDoorState")]
    public string StartLadderDoorState = "Opened";

    /// <summary>
    /// Состояние индикатора люка при старте игры
    /// </summary>
    [DataField("startLadderIndicatorState")]
    public string StartLadderIndicatorState = "Unlocked";

    #endregion

    #region Sound

    /// <summary>
    /// Включено ли звуки люка
    /// </summary>\
    [DataField("soundEnabled"), ViewVariables(VVAccess.ReadWrite)]
    public bool SoundEnabled = true;

    /// <summary>
    /// Звук при блокировке люка
    /// </summary>
    [DataField("soundLocked")]
    public SoundSpecifier SoundLocked = new SoundPathSpecifier("/Audio/Machines/door_lock_on.ogg");

    /// <summary>
    /// Звук при разблокировке люка
    /// </summary>
    [DataField("soundUnlocked")]
    public SoundSpecifier SoundUnlocked = new SoundPathSpecifier("/Audio/Machines/door_lock_off.ogg");

    /// <summary>
    /// Звук при закрытии люка
    /// </summary>
    [DataField("soundLockedClosed")]
    public SoundSpecifier SoundClosed = new SoundPathSpecifier("/Audio/Effects/closetclose.ogg");

    /// <summary>
    /// Звук при открытии люка
    /// </summary>
    [DataField("soundUnlockedClosed")]
    public SoundSpecifier SoundOpened = new SoundPathSpecifier("/Audio/Effects/closetopen.ogg");

    #endregion

    #region Light
    /// <summary>
    /// Включено ли управление светом люка
    /// </summary>
    [DataField("lightEnabled"), ViewVariables(VVAccess.ReadWrite)]
    public bool LightEnabled = true;

    /// <summary>
    /// Включено ли управление цветом света люка через систему DayTime
    /// </summary>
    [DataField("lightColorByDayTime"), ViewVariables(VVAccess.ReadWrite)]
    public bool LightColorByDayTime = false;

    /// <summary>
    /// Цвет света люка при открытом состоянии
    /// </summary>
    [DataField("lightColorOpen"), ViewVariables(VVAccess.ReadWrite)]
    public string LightColorOpen = "#FFFFFFFF";

    /// <summary>
    /// Радиус света люка при открытом состоянии
    /// </summary>
    [DataField("lightRadiusOpen"), ViewVariables(VVAccess.ReadWrite)]
    public float LightRadiusOpen = 2f;

    /// <summary>
    /// Энергия света люка при открытом состоянии
    /// </summary>
    [DataField("lightEnergyOpen"), ViewVariables(VVAccess.ReadWrite)]
    public float LightEnergyOpen = 2f;

    /// <summary>
    /// Цвет света люка при закрытом состоянии
    /// </summary>
    [DataField("lightColorClosed"), ViewVariables(VVAccess.ReadWrite)]
    public string LightColorClosed = "#FFFFFFFF";

    /// <summary>
    /// Радиус света люка при закрытом состоянии
    /// </summary>
    [DataField("lightRadiusClosed"), ViewVariables(VVAccess.ReadWrite)]
    public float LightRadiusClosed = 1.2f;

    /// <summary>
    /// Энергия света люка при закрытом состоянии
    /// </summary>
    [DataField("lightEnergyClosed"), ViewVariables(VVAccess.ReadWrite)]
    public float LightEnergyClosed = 0.75f;

    #endregion

    #region Popups

    /// <summary>
    /// Управление показом попапов
    /// </summary>
    [DataField("popupEnabled"), ViewVariables(VVAccess.ReadWrite)]
    public bool PopupEnabled = true;

    /// <summary>
    /// Сообщения для показа в попапе при открытии люка
    /// </summary>
    [DataField("popupDoorOpened"), ViewVariables(VVAccess.ReadWrite)]
    public string PopupDoorOpened = "";

    /// <summary>
    /// Сообщения для показа в попапе при закрытии люка
    /// </summary>
    [DataField("popupDoorCloseed"), ViewVariables(VVAccess.ReadWrite)]
    public string PopupDoorCloseed = "";

    /// <summary>
    /// Сообщения для показа в попапе при блокировке люка
    /// </summary>
    [DataField("popupLocked"), ViewVariables(VVAccess.ReadWrite)]
    public string PopupLocked = "";

    /// <summary>
    /// Сообщения для показа в попапе при разблокировке люка
    /// </summary>
    [DataField("popupUnlocked"), ViewVariables(VVAccess.ReadWrite)]
    public string PopupUnlocked = "";

    /// <summary>
    /// Сообщения для показа в попапе при невозможности перемещения пользователя через люк
    /// </summary>
    [DataField("popupCantPass"), ViewVariables(VVAccess.ReadWrite)]
    public string PopupCantPass = "";

    /// <summary>
    /// Сообщения для показа в попапе при перемещении пользователя через люк
    /// </summary>
    [DataField("popupPass"), ViewVariables(VVAccess.ReadWrite)]
    public string PopupPass = "";

    /// <summary>
    /// Сообщения для показа в попапе при отказе в доступе к люку
    /// </summary>
    [DataField("popupAccessDenied"), ViewVariables(VVAccess.ReadWrite)]
    public string PopupAccessDenied = "";

    #endregion
}



/// <summary>
/// Состояние двери люка
/// </summary>
#region Enums
[Serializable, NetSerializable]
public enum LadderDoorState : byte
{
    Closed,
    Opened,
}


/// <summary>
/// Состояние индикатора люка
/// </summary>
[Serializable, NetSerializable]
public enum LadderIndicatorState : byte
{
    Locked,
    Unlocked,
    NotWorking
}

[Serializable, NetSerializable]
public enum LadderVisualKey : byte
{
    DoorState,
    IndicatorState
}

#endregion

#region Events
[Serializable, NetSerializable]
public sealed partial class LadderMoveDoAfterEvent : DoAfterEvent
{
    /// <summary>
    /// Целевой люк
    /// </summary>
    public NetEntity Ladder;

    /// <summary>
    /// Цель для переноса
    /// </summary>
    public NetEntity LadderUser;

    public LadderMoveDoAfterEvent(NetEntity ladder, NetEntity user)
    {
        Ladder = ladder;
        LadderUser = user;
    }

    public override DoAfterEvent Clone() => this;
}
#endregion
