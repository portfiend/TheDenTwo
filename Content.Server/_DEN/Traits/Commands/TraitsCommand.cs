using System.Linq;
using Content.Server._DEN.Traits.EntitySystems;
using Content.Server.Administration;
using Content.Shared._DEN.Traits.Components;
using Content.Shared._DEN.Traits.Prototypes;
using Content.Shared.Administration;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Server._DEN.Traits.Commands;
#pragma warning restore IDE1006 // Naming Styles

[ToolshedCommand, AdminCommand(AdminFlags.VarEdit)]
public sealed partial class TraitsCommand : ToolshedCommand
{
    private TraitSystem? _traits;

    [CommandImplementation("add")]
    public EntityUid Add([PipedArgument] EntityUid @target, ProtoId<EntityTraitPrototype> trait)
    {
        _traits ??= GetSys<TraitSystem>();
        _traits.TryAddTrait(@target, trait, out var _);
        return @target;
    }

    [CommandImplementation("remove")]
    public EntityUid Remove([PipedArgument] EntityUid @target, ProtoId<EntityTraitPrototype> trait)
    {
        _traits ??= GetSys<TraitSystem>();
        _traits.TryRemoveTrait(@target, trait);
        return @target;
    }

    [CommandImplementation("get")]
    public List<ProtoId<EntityTraitPrototype>> Get([PipedArgument] EntityUid @target)
    {
        _traits ??= GetSys<TraitSystem>();
        _traits.TryGetTraits(@target, out var traits);

        return traits ?? new();
    }
}
