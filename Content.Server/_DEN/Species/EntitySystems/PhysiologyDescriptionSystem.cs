using Content.Shared._DEN.Species.Components;
using Content.Shared._DEN.Species.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;

#pragma warning disable IDE1006 // Naming Styles
namespace Content.Server._DEN.Species.EntitySystems;
#pragma warning restore IDE1006 // Naming Styles

public sealed partial class PhysiologyDescriptionSystem : SharedPhysiologyDescriptionSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhysiologyDescriptionComponent, ExaminedEvent>(OnPhysiologyDescriptionExamined);
    }

    private void OnPhysiologyDescriptionExamined(Entity<PhysiologyDescriptionComponent> ent, ref ExaminedEvent args)
    {
        var comp = ent.Comp;
        var baseLabel = Loc.GetString(comp.BaseLabel);
        var prefixLabel = comp.PrefixLabel != null
            ? Loc.GetString(comp.PrefixLabel)
            : string.Empty;
        var physiologyLabel = prefixLabel != string.Empty
            ? Loc.GetString(comp.PrefixedPhysiologyDescriptor,
                ("base", baseLabel),
                ("prefix", prefixLabel))
            : Loc.GetString(comp.BasePhysiologyDescriptor,
                ("base", baseLabel));

        var examineText = Loc.GetString(comp.ExamineText,
            ("target", Identity.Entity(ent.Owner, EntityManager)),
            ("physiology", physiologyLabel));

        args.PushMarkup(examineText);
    }
}
