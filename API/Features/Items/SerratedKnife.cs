namespace GRPP.API.Features.Items;

using CustomItems;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.Handlers;
using InventorySystem.Items;

public sealed class SerratedKnife : CustomItemHandler
{
    private SerratedKnife()
    {
    }

    public CustomItemContainer Container { get; } = new();

    public override string Name => "SerratedKnife";
    public override string[] Alias => ["Serrated", "KnifeSerrated"];

    public override void EnableEvents()
    {
        PlayerHandlers.Hurting += Swinging1509;
        PlayerHandlers.ChangingItem += ChangingItem;
    }
    public void ChangingItem(ChangingItemEventArgs ev)
    {
        if (ev.Item == null)
            return;

        if (!HasItem(ev.Item.Base))
            return;

        ev.Player.ShowHint("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|⚡|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=23><space=2.6em><b>Serrated Knife</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    public override bool HasItem(ushort serial) => Container.HasItem(serial);

    public override void ClearItems() => Container.ClearItems();

    private void Swinging1509(HurtingEventArgs ev)
    {
        if (ev.Attacker?.CurrentItem == null)
            return;

        if (!HasItem(ev.Attacker.CurrentItem.Base))
            return;

        if (ev.Attacker == ev.Player)
            return;

        if (ev.DamageHandler.Type != DamageType.Scp1509)
            return;

        if (ev.Player.IsGodModeEnabled)
            return;

        ev.Amount = 5f;

        // ev.Player.ShowHint("", 5f);
        ev.Player.EnableEffect(EffectType.Concussed, 5f);
        ev.Player.EnableEffect(EffectType.Decontaminating, 2f, false);
        //ev.Player.EnableEffect(EffectType.Traumatized, 50f); ermmm why did it make you traumatized LMAOO
    }

    // private void Swinging(SwingingEventArgs ev)
    // {
    //     if (!HasItem(ev.Player.CurrentItem.Base))
    //         return;
    //     
    //     ev.IsAllowed = false;
    // }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.SCP1509);
        Container.RegisterItem(item.Base);
        return item.Base;
    }
}