namespace GRPP.API.Features.Items;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandSystem;
using Core.Webhooks;
using CustomItems;
using EasyTmp;
using Exiled.Events.EventArgs.Player;
using Exiled.Permissions.Extensions;
using Extensions;
using InventorySystem.Items;

public sealed class CustomHandler : CustomItemHandler
{
    private CustomHandler() { }
    public CustomItemContainer<Item> Container { get; } = new();

    public override string Name => "";
    public override string[] Alias => [];

    public override void EnableEvents()
    {
        PlayerHandlers.ChangingItem += ChangingItems;
        PlayerHandlers.UsingItem += ItemUse;
    }

    public override bool HasItem(ushort serial)
    {
        return Container.HasItem(serial);
    }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.Coin);
        Container.RegisterItem(item.Base, new Item("Radical Coin"));
        return item.Base;
    }

    public void ChangingItems(ChangingItemEventArgs ev)
    {
        if (ev.Item == null)
            return;

        if (!Container.HasItem(ev.Item.Base, out var item))
            return;

        ev.Player.ShowHint($"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|✨|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=21><space=2.6em><b>{item.Name.ReplaceLetters()}</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    private void ItemUse(UsingItemEventArgs ev)
    {
        if (!Container.HasItem(ev.Item.Base))
            return;

        if (ev.Item.Type != ItemType.Medkit)
            return;

        ev.IsAllowed = false;
        ev.Player.RemoveHeldItem();
        ev.Player.ShowHint("Mmmm, that tasted good!", 5f);
    }

    public ItemBase GiveItem(ExPlayer player, string name, ItemType type)
    {
        var item = player.AddItem(type);
        Container.RegisterItem(item.Base, new Item(name));
        return item.Base;
    }

    public override void ClearItems()
    {
        Container.ClearItems();
    }

    public sealed class Item(string name)
    {
        public readonly string Name = name;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class GiveCustomItem : ICommand
{
    public string Command => "givecustomitem";
    public string[] Aliases { get; } = ["gic"];
    public string Description => EasyArgs.Build().CmdArguments("gic itemID itemName").Done();

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        if (arguments.Count < 1 || !Enum.TryParse(arguments.At(0), true, out ItemType itemType))
        {
            response = "<color=orange>Invalid item ID. Please try again.</color>";
            return false;
        }

        var itemName = string.Join(" ", arguments.Skip(1));
        
        CustomItemsManager.Get<CustomHandler>().GiveItem(ExPlayer.Get(sender), itemName, itemType);

        response = $"<color=green>You gave yourself {itemName}";
        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public class PrintCommand : ICommand
{
    public string Command => "print";
    public string[] Aliases { get; } = ["printf"];
    public string Description => EasyArgs.Build().CmdArguments("print itemDescription").Done();

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var plr = ExPlayer.Get(sender);
        response = EasyArgs.Build().Orange("Sorry, but this command is currently").Space().Red("disabled").Orange(". Please try again later.").Done();
        if ((!Plugin.Singleton?.Config.AllowClientCreateCommand ?? true) || (!Plugin.Singleton.GlobalConfig?.PrintEnabled ?? true) || plr.IsDead || !plr.IsConnected || !plr.GameObject)
            return false;
        
        response = EasyArgs.Build().Red("Error!").Space().Orange("Invalid amount of arguments.").Done();
        if (arguments.Count < 1 || arguments.Sum(arg => arg.Length) > (Plugin.Singleton.Config.MaximumCreateDescription ?? Defaults.MaximumCreateDescription))
            return false;
        
        var itemDescription = string.Join(" ", arguments);

        CustomItemsManager.Get<CustomHandler>().GiveItem(plr, itemDescription, ItemType.KeycardJanitor);

        if (!Plugin.Singleton.Config.PrintCommandWebhookUrl.IsEmpty())
            _ = AsyncWebhookHandler.LogMessage(
                webhookNameToUse:"PrintLogger", 
                webhookUrl:Plugin.Singleton.Config.PrintCommandWebhookUrl, 
                title:"Printed.", 
                description:$"A user has printed an item.\nName: \"{plr.DisplayNickname}\"\nSteamID64: \"{plr.UserId}\"\nItemDescription: \"{itemDescription}\"", 
                color:"880808");
        
        response = $"<color=orange>Printed</color> <color=blue>{itemDescription}</color><color=orange>!</color>";
        return true;
    }
}

[CommandHandler((typeof(RemoteAdminCommandHandler)))]
public class PrintToggle : ICommand
{
    public string Command { get; set; } = "PrintToggle";
    public string[] Aliases { get; set; } = ["PrintMod", "ModPrint", "TogglePrint"];
    public string Description { get; set; } = "Toggles (or enables/disables) print.";
    private string Usage { get; set; } = EasyArgs.Build().CmdArguments("printtoggle opt-true/false").Done();
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = "Plugin.Singleton is null. Contact z5tm.";
        if (Plugin.Singleton?.GlobalConfig == null)
            return false;
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = EasyArgs.Build().Orange("Sorry, but you do not have the").Space().Blue("grpp.print").Space().Orange("permission.").Done();
        if (!sender.CheckPermission("grpp.print"))
            return false;

        response = Usage;
        if (arguments.Count > 1)
            return false;
        var emptyArgs = arguments.IsEmpty();

        Plugin.Singleton.GlobalConfig.PrintEnabled = emptyArgs switch
        {
            true => !Plugin.Singleton.GlobalConfig.PrintEnabled,
            false when bool.TryParse(arguments.At(0), out var status) => !status,
            _ => Plugin.Singleton.GlobalConfig.PrintEnabled
        } || (!emptyArgs && arguments.At(0) == "t");

        if (!emptyArgs && arguments.At(0) == "f") Plugin.Singleton.GlobalConfig.PrintEnabled = false;
        
        var responseBuilder = EasyArgs.Build().Orange($"PrintEnabled has been set to").Space();
        response = Plugin.Singleton.GlobalConfig.PrintEnabled ? responseBuilder.Green("true!").Done() : responseBuilder.Red("false!").Done();
        return true;
    }
}