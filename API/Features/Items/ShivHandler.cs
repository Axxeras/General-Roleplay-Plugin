namespace GRPP.API.Features.Items;

using System;
using Attributes;
using CommandSystem;
using CustomItems;
using EasyTmp;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using Extensions;
using InventorySystem.Items;
using UnityEngine;

public sealed class ShivHandler : CustomItemHandler
{
    private ShivHandler()
    {
    }

    public CustomItemContainer Container { get; } = new();

    public override string Name => "Shiv";
    public override string[] Alias => ["knife"];

    public override void EnableEvents()
    {
        PlayerHandlers.UsingItem += UsedItem;
        PlayerHandlers.ChangingItem += ChangingItem;
    }

    public void ChangingItem(ChangingItemEventArgs ev)
    {
        if (ev.Item == null)
            return;

        if (!HasItem(ev.Item.Base))
            return;

        if (!Shiv.IsEnabled)
            return;

        ev.Player.ShowHint("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|🔪|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=23><space=2.6em><b>ꜱʜɪᴠ ᴍᴀᴅᴇ ᴏꜰ ᴄᴏɴᴄʀᴇᴛᴇ</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    public override bool HasItem(ushort serial) => Container.HasItem(serial);

    public override void ClearItems() =>   Container.ClearItems();

    public void UsedItem(UsingItemEventArgs ev)
    {
        if (!HasItem(ev.Item.Base))
            return;

        if (!Shiv.IsEnabled)
            return;

        ev.IsAllowed = false;

        if (!Physics.Raycast(new Ray(ev.Player.CameraTransform.position, ev.Player.CameraTransform.forward * 0.2f), out var hit, 5f))
            return;
        if (!ExPlayer.TryGet(hit.collider, out var victim))
            return;

        if (victim == ev.Player)
            return;

        ev.Player.RemoveHeldItem();

        if (victim.IsGodModeEnabled || victim.IsScp)
        {
            victim.ShowHint("Their shiv broke... nice....", 5f);
            ev.Player.ShowHint("I broke my shiv...", 5f);
            return;
        }

        foreach (var staff in ExPlayer.List)
            if (staff.RemoteAdminPermissions != 0)
            {
                staff.SendConsoleMessage(
                    "SCOMBAT Shiv Log:\nSTABBER NAME: " + ev.Player.Nickname + "\nSTABBER ROLE: " +
                    ev.Player.Role.Type + "\nSTABBER ID: " + ev.Player.Id + "\nSTABBER S-ID: " + ev.Player.UserId,
                    "yellow");
                staff.SendConsoleMessage(
                    "SCOMBAT Shiv Log:\nSTABBED NAME: " + victim.Nickname + "\nSTABBED ROLE: " +
                    victim.Role.Type + "\nSTABBED ID: " + victim.Id + "\nSTABBED S-ID: " +
                    victim.UserId, "red");
            }

        // var bleeding = GRPPCommands.;
        victim.EnableEffect(EffectType.Concussed, 15, 25);
        victim.EnableEffect(EffectType.Bleeding, 255, 50);
        victim.EnableEffect(EffectType.Blurred, 15, 25);
        victim.EnableEffect(EffectType.Disabled, 255, 50);
        victim.Hurt(35f, DamageType.Bleeding);
        // victim.DropHeldItem(true);
        victim.ShowHint("I can't breathe... I've been stabbed", 5);
    }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.Adrenaline);
        Container.RegisterItem(item.Base);
        return item.Base;
    }
}

public abstract class Shiv
{
    public static bool IsEnabled = true;

    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;

    private static void WaitingForPlayers() => IsEnabled = true;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ShivsEnable : ICommand
{
    public string Command => "turnthemshivson";
    public string[] Aliases => ["ShivsOn", "shivon", "shivenable", "enableshivs"];
    public string Description => "Enables the shiv item. As soon as this is disabled, all shivs turn into adrenaline.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=blue>Shiv</color><color=orange>s are already</color> <color=green>enabled</color>";
        if (Shiv.IsEnabled)
            return false;

        Shiv.IsEnabled = !Shiv.IsEnabled;

        response = "<color=blue>Shiv</color><color=orange>s are now</color> <color=green>enabled</color>";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ShivsDisable : ICommand
{
    public string Command => "turnthemshivsoff";
    public string[] Aliases => ["ShivsOff", "shivoff", "disableshivs", "shivdisable", "shivsdisable"];
    public string Description => "Disables the shiv item. If any shivs exist, they turn into adrenaline.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = EasyArgs.Build().Blue("Shivs").Space().Orange("are already").Space().Red("disabled").Orange(".").NewLine().Orange($"Note:").Space().Blue("mining").Orange($"is currently {Mining.IsEnabled}.").Done();
        if (!Shiv.IsEnabled)
            return false;

        Shiv.IsEnabled = !Shiv.IsEnabled;

        response = EasyArgs.Build().Blue("Shivs").Space().Orange("are now").Space().Red("disabled").Orange(".").NewLine().Orange($"Note:").Space().Blue("mining").Orange($"is currently {Mining.IsEnabled}.").Done();
        return true;
    }
}

public abstract class Mining
{
    public static bool IsEnabled = true;

    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;
    private static void WaitingForPlayers() => IsEnabled = true;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class MiningEnable : ICommand
{
    public string Command => "turnminingthemshivson";
    public string[] Aliases => ["MiningOn", "enableminingshivs", "EnableMining", "shivminingon"];
    public string Description => "Enables the mining of shivs.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Mining is already Enabled";
        if (Mining.IsEnabled)
            return false;

        Mining.IsEnabled = !Mining.IsEnabled;

        response = "<color=green>Mining is now Enabled";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class MiningDisable : ICommand
{
    public string Command => "turnminingthemshivsoff";
    public string[] Aliases => ["MiningOff", "DisableMining", "disableminingshivs", "shivminingoff"];
    public string Description => "Disables the mining of shivs.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=blue>Mining</color> <color=orange>is already</color> <color=red>disabled</color><color=orange>.</color>";
        if (!Mining.IsEnabled)
            return false;

        Mining.IsEnabled = !Mining.IsEnabled;

        response = "<color=blue>Mining</color> <color=orange>is now</color> <color=red>disabled</color><color=orange>.</color>";
        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public class ShivClient : ICommand
{
    public string Command => "shiv";
    public string[] Aliases => ["mine"];
    public string Description => "Punches in the wall to try, to try and break off a piece.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = ExPlayer.Get(sender);

        response = "This feature is currently disabled!";
        if (!Mining.IsEnabled)
            return false;

        response = "<color=red>I can't do that...";
        if (player.IsCuffed)
            return false;

        var ray = new Ray(player.CameraTransform.position + (player.CameraTransform.forward * 0.1f), player.CameraTransform.forward);

        if (!Physics.Raycast(ray, out var hit, 2))
        {
            response = "<color=orange>You must be at a wall</color>";
            return false;
        }

        if (ExPlayer.TryGet(hit.collider, out var victim))
        {
            response = $"<color=orange>You tried to mine</color> <color=blue>{victim.DisplayNickname}</color><color=orange>, but failed.</color>";
            return false;
        }

        response = "<color=orange>I wouldn't be able to carry it anyways</color>";
        if (player.InventoryFull())
            return false;

        if (URandom.Range(1, 100) <= 85)
        {
            player.ShowHint("<color=orange><b>Damnit... not this time.</b></color>");
            player.Health -= 10f;

            if (player.Health <= 0) player.Kill("Their hands are bloody, looks like they were chipping at the wall");
            response = "<color=orange>Damnit... not this time.</color>";
            return false;
        }

        CustomItemsManager.Get<ShivHandler>().GiveItem(player);

        response = "<color=green>Perfect...";
        return true;
    }
}