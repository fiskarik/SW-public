using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Imperial.OperationalERTcleaners;

[Serializable, NetSerializable]
public sealed class FaxCentcomReceivedEvent : EntityEventArgs
{
    public NetEntity FaxEntity;
    public NetEntity DocumentEntity;
    public string Content;

    public FaxCentcomReceivedEvent(NetEntity fax, NetEntity document, string content)
    {
        FaxEntity = fax;
        DocumentEntity = document;
        Content = content;
    }
}
