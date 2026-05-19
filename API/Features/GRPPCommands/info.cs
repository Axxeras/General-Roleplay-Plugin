namespace GRPP.API.Features.GRPPCommands;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandSystem;
using EasyTmp;
using Attributes;
using Extensions;
using JetBrains.Annotations;

public abstract class Info
{
    public static bool IsEnabled;

    [UsedImplicitly]
    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;

    private static void WaitingForPlayers() => IsEnabled = false;
}

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class InfoEnable : ICommand
{
    public string Command => "InfoEnable";
    public string[] Aliases => ["InfoOn", "OnInfo", "EnableInfo", "customion", "inf1"];
    public string Description => "Enables the Information command, that allows users to set their customi.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=orange>Information Command is already</color> <color=green>Enabled</color>";
        if (Info.IsEnabled)
            return false;

        Info.IsEnabled = !Info.IsEnabled;

        response = "<color=orange>Information Command is now</color> <color=green>Enabled</color>";
        return true;
    }
}

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class InfoDisable : ICommand
{
    public string Command => "InfoDisable";
    public string[] Aliases => ["InfoOff", "OffInfo", "DisableInfo", "NoInfo", "customioff", "inf0"];
    public string Description => "Disables the Information Command";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=orange>Information Command is already</color> <color=red>Disabled</color>";
        if (!Info.IsEnabled)
            return false;

        Info.IsEnabled = !Info.IsEnabled;

        response = "<color=orange>Information Command is now</color> <color=red>Disabled</color>";
        return true;
    }
}

[UsedImplicitly]
[CommandHandler(typeof(ClientCommandHandler))]
public class InfoClient : ICommand
{
    public string Command => "Info";
    public string[] Aliases => ["Information", "custominfo", "inf"];
    public string Description => "Gives you a custom information tag for RP purposes : Info (customi, use `\\n` to make a newline.)";
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!Info.IsEnabled || Plugin.Singleton == null)
        {
            response = "This feature is currently disabled!";
            return false;
        }

        var player = ExPlayer.Get((CommandSender)sender);
        if (arguments.Count == 0)
        {
            player.CustomInfo = string.Empty;
            response = "Customi reset successfully!";
            return true;
        }
        foreach (var word in arguments)
            foreach (var _ in Plugin.Singleton.Config.Blocklist.Where(target => word.Equals(target, StringComparison.OrdinalIgnoreCase))) 
                player.Ban(1577000000, "Automated ban. Appeal on the discord if you believe this was false.");

        var customInfo = string.Join(" ", arguments);
        player.CustomInfo = customInfo.Length < (Plugin.Singleton.Config.InfoMaxLength ?? Defaults.InfoMaxLength) ? 
            customInfo : 
            customInfo.CutStringToValue(Plugin.Singleton.Config.InfoMaxLength ?? Defaults.InfoMaxLength);
        
        response = EasyArgs.Build().Blue("Custom info")
            .Space().Orange("has been")
            .Space().Green("successfully")
            .Space().Orange("set!")
            .Done();
        return true;
    }
}
