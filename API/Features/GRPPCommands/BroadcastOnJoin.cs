namespace GRPP.API.Features.GRPPCommands;

using System;
using System.Globalization;
using System.Linq;
using Attributes;
using CommandSystem;
using EasyTmp;
using Extensions;
using JetBrains.Annotations;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;

public abstract class BroadcastJoin
{
    internal static readonly string Arguments = EasyArgs.Build().CmdArguments("bcoj seconds broadcast").Done();
    public static bool BroadcastSet;
    public static bool WarningSet;
    public static ushort BroadcastJoinTime;
    public static string? BroadcastJoinText;
    public static string? WarningText;
    public static ushort WarningTime;

    [UsedImplicitly]
    [OnPluginEnabled]
    public static void InitEvents()
    {
        BroadcastJoinTime = 0;
        BroadcastJoinText = null;
        WarningSet = false;
        BroadcastSet = false;
        ServerEvents.WaitingForPlayers += WaitingForPlayers;
        PlayerEvents.Joined += BroadcastJoiner;
        PlayerEvents.Spawned += BroadcastSpawner;
        // tryna use more labapi in our cute little weird hybrid exiled labapi plugin !
    }

    private static void WaitingForPlayers()
    {
        BroadcastJoinTime = 0;
        BroadcastJoinText = null;
        WarningSet = false;
        BroadcastSet = false;
    }

    private static void BroadcastJoiner(PlayerJoinedEventArgs ev)
    {
        if (BroadcastSet && BroadcastJoinTime != 0 && BroadcastJoinText != null)
            ev.Player.SendBroadcast(BroadcastJoinText, BroadcastJoinTime);

        if (!WarningSet) return;
        if (BroadcastSet) return;
        
        ev.Player.SendBroadcast(EasyArgs.Build().Red($"A trigger warning has been set: \"{WarningText}\"").Done(), WarningTime);
    }
    private static void BroadcastSpawner(PlayerSpawnedEventArgs ev)
    {
        if (WarningSet)
            ev.Player.SendHint(EasyArgs.Build().Red($"TW: \"{WarningText}\"").Done(), WarningTime);
    }
}

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class BroadcastOnJoin : ICommand
{
    public string Command => "broadcastonjoin";
    public string[] Aliases => ["joinbroadcast", "broadcastjoin", "bcoj"];
    public string Description => "Shows a broadcast when players join the server. Run command with no arguments for arguments.";


    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;
        
        if (arguments.Count <= 2 || arguments.At(0) == null || !ushort.TryParse(arguments.At(0), NumberStyles.Number, CultureInfo.CurrentCulture, out ushort timeToShow))
        {
            response = BroadcastJoin.Arguments;
            return false;
        }
        
        string stringToShow = string.Join(" ", arguments.Skip(1));
        BroadcastJoin.BroadcastSet = true;
        BroadcastJoin.BroadcastJoinTime = timeToShow;
        BroadcastJoin.BroadcastJoinText = stringToShow;
        response = 
            EasyArgs.Build()
                .Green("Success").Orange("!")
                .NewLine().Orange($"The")
                .Space().Blue("broadcast")
                .Space().Orange("has been set to:")
                .Space().Blue(stringToShow).Orange("!")
                .NewLine().Orange("The")
                .Space().Blue("time")
                .Space().Orange("to show has been set to:")
                .Space().Blue(timeToShow.ToString())
                .Space().Blue("seconds").Orange("!")
                .Done();
        Logger.Info($"A broadcast on join has been set by {ExPlayer.Get(sender).UserId}/{ExPlayer.Get(sender).Nickname}, broadcast: {stringToShow}");
        return true;
    }
}

[UsedImplicitly]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class BroadcastOnJoinConsole : ICommand
{
    public string Command => "broadcastonjoin";
    public string[] Aliases => ["joinbroadcast", "broadcastjoin", "bcoj"];
    public string Description => "Shows a broadcast when players join the server. Run command with no arguments for arguments.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count <= 2 || arguments.At(0) == null || !ushort.TryParse(arguments.At(0), NumberStyles.Number, CultureInfo.CurrentCulture, out ushort timeToShow))
        {
            response= BroadcastJoin.Arguments;
            return false;
        }


        string stringToShow = string.Join(" ", arguments.Skip(1));
        BroadcastJoin.BroadcastSet = true;
        BroadcastJoin.BroadcastJoinTime = timeToShow;
        BroadcastJoin.BroadcastJoinText = stringToShow;
        // response = 
        //     EasyArgs.Build()
        //         .Green("Success").Orange("!")
        //         .NewLine().Orange($"The")
        //         .Space().Blue("broadcast")
        //         .Space().Orange("has been set to:")
        //         .Space().Blue(stringToShow).Orange("!")
        //         .NewLine().Orange("The")
        //         .Space().Blue("time")
        //         .Space().Orange("to show has been set to:")
        //         .Blue(timeToShow.ToString())
        //         .Space().Blue("seconds").Orange("!")
        //         .Done();
        response = $"Done. {stringToShow}, time: {timeToShow}"; // reduced formatting cuz consoles don't like my beauty formatting
        return true;
    }
}

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class WarnOnJoin : ICommand
{
    public string Command => "warnonjoin";
    public string[] Aliases => ["joinwarn", "warnjoin", "woj"];
    public string Description => "Shows a TW when players join the server. Run command with no arguments for arguments.";


    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;
        
        if (arguments.Count <= 2 || arguments.At(0) == null || !ushort.TryParse(arguments.At(0), NumberStyles.Number, CultureInfo.CurrentCulture, out ushort timeToShow))
        {
            response = EasyArgs.Build().CmdArguments("woj seconds warning").NewLine().Orange("Example output:").NewLine().Red("TW: warning").Done();
            return false;
        }


        string stringToShow = string.Join(" ", arguments.Skip(1));
        BroadcastJoin.BroadcastSet = true;
        BroadcastJoin.WarningSet = true;
        BroadcastJoin.WarningTime = (ushort)(timeToShow + 5); // i hate you C#. why did this take me so long to find.
        BroadcastJoin.WarningText = stringToShow;
        response = 
            EasyArgs.Build()
                .Green("Success").Orange("!")
                .NewLine().Orange($"The")
                .Space().Blue("broadcast")
                .Space().Orange("has been set to:")
                .Space().Blue(stringToShow).Orange("!")
                .NewLine().Orange("The")
                .Space().Blue("time")
                .Space().Orange("to show has been set to:")
                .Blue(timeToShow.ToString())
                .Space().Blue("seconds").Orange("!")
                .Done();
        Logger.Info($"A trigger warning on join has been set by {ExPlayer.Get(sender).UserId}/{ExPlayer.Get(sender).Nickname}, warning: {stringToShow}");
        return true;
    }
}