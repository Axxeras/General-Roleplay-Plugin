namespace GRPP.API.Features.GRPPCommands;

using System;
using CommandSystem;
using EasyTmp;
using Exiled.Permissions.Extensions;
using JetBrains.Annotations;

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Blackout : ICommand
{
    public string Command => "grppblackout";
    public string[] Aliases => ["facilityblackout"];
    public string Description => "Blackout. In testing, do not use.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = "This command is in testing.";
        if (!sender.CheckPermission("grpp.blackout")) 
            return false;

        response = EasyArgs.Build().CmdArguments("grppblackout").Done();
        if (arguments.Count != 0) return false;
        response = "<color=green>Reset Lights Successfully</color>";
        return true;
    }
}