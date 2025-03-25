using Robust.Shared.GameStates;

namespace Content.Shared.Imperial.Medieval.Identity;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class IndentityRequiresKnowledgeComponent : Component
{
    [AutoNetworkedField]
    public int Identifier = 0;

    [AutoNetworkedField]
    public List<int> KnownIds = new();
}
