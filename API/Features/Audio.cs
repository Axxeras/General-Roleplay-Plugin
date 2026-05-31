namespace GRPP.API.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Extensions;
using MEC;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Audio : ICommand
{
    public string Command => "ScombatAudio";
    public string[] Aliases => ["Au", "AudioPlayer", "PlayAudio", "AudioPlay"];
    public string Description => "Plays an audio using an ID";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response) && !Plugin.Singleton.Config.ClientAu)
            return false;
        
        var player = ExPlayer.Get(sender);
        response = "<color=red>No Permission.";
        if (!sender.CheckPermission("grpp.au"))
            return false;

        response = "<color=red>Invalid Usage\n" +
                   "<color=yellow>> Play {ID} {Loop?} {Local?} {SoundName}\n" +
                   "<color=yellow>> Stop {ID}";

        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP"));
        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP", "audio")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP", "audio"));
        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP", "audio", "PluginSFX")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP", "audio", "PluginSFX"));

        try
        {
            if (arguments.Count == 0)
            {
                foreach (var file in Directory.GetFiles(Path.Combine(Paths.Configs, "GRPP", "audio"), "*.ogg",
                             SearchOption.AllDirectories))
                    sender.Respond(
                        $"<color=green>> {file.Replace(".ogg", "").Replace(Path.Combine(Paths.Configs, "GRPP", "audio") + "\\", "")}");

                response = $"{response}\n<color=green>> Task Completed";
                return true;
            }
        }
        catch(Exception e)
        {
            Log.Info($"The directory at `GRPP\\audio\\` is likely empty. {e.Message}");
        }

        response = arguments.At(0).ToLower() switch
        {
            "play" or "playaudio" => PlayAudio(arguments, player),
            "stop" or "stopaudio" => StopAudio(arguments),
            _ => response
        };
        return true;
    }

    private readonly HashSet<byte> _audioPlayerIds = [];

    public string PlayAudio(ArraySegment<string> arguments, ExPlayer player)
    {
        if (arguments.Count < 5)
            return "<color=red>Invalid Usage\n<color=yellow>> Play {ID} {Loop?} {Local?} {SoundName}";

        var usedIds = new HashSet<byte>();
        foreach (var obj in AudioPlayer.AudioPlayerByName)
            usedIds.Add(obj.Value.ControllerID);

        Log.Info(string.Join(" ", usedIds));

        if (!byte.TryParse(arguments.At(1), out var controllerId))
            return $"<color=red>Invalid ID. Here's an ID {SpeakerExtensions.GetFreeId()}";

        if (usedIds.Contains(controllerId))
            return $"<color=red>ID is already in use, here's a FreeID {SpeakerExtensions.GetFreeId()}";

        if (!bool.TryParse(arguments.At(2).ToLower(), out var isLoop))
            return "<color=red>Invalid Value Loop is invalid. It's not a boolean (true or false)";

        if (!bool.TryParse(arguments.At(3).ToLower(), out var isSpatial))
            return "<color=red>Invalid Value Local is invalid. It's not a boolean (true or false)";

        var offset = new ArraySegment<string>(arguments.Array!, arguments.Offset + 4, arguments.Count - 4);
        var audioToPlay = string.Join(" ", offset);
        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP"));
        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP", "audio")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP", "audio"));
        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP", "audio", "PluginSFX")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP", "audio", "PluginSFX"));
        try 
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Paths.Configs, "GRPP", "audio"), "*.ogg", SearchOption.AllDirectories)) 
            {
                var fileName = file.Replace(".ogg", "").Replace(Path.Combine(Paths.Configs, "GRPP", "audio") + "\\", "");
                if (fileName != audioToPlay) continue;
                AudioClipStorage.LoadClip(file, fileName);
                var audioPlayer = AudioPlayer.Create($"AudioPlayer {controllerId}", controllerId: controllerId);
                var speaker = audioPlayer.AddSpeaker("Speaker", player.Position, isSpatial: isSpatial, maxDistance: isSpatial ? 15f : 8000);

                var clip = audioPlayer.AddClip(fileName, loop: isLoop);
                _audioPlayerIds.Add(controllerId);

                if (!isLoop)
                    Timing.CallDelayed((float)clip.Duration.TotalSeconds, () =>
                    {
                        _audioPlayerIds.Remove(controllerId);
                        audioPlayer.Destroy();
                        speaker.Destroy();
                    });

                return $"<color=green>Now playing as {controllerId} audioFile: {fileName}";
            }
        }
        catch(Exception e)
        {
            Log.Info($"The directory at `GRPP\\audio\\` is likely empty. {e.Message}");
        }

        return $"<color=red>Attempted to play {audioToPlay}, but no audio was found.\nIf you believe this is an issue please talk to @sticksdev.";
    }

    public string StopAudio(ArraySegment<string> arguments)
    {
        if (arguments.Count != 2)
            return "<color=red>Invalid Usage\n<color=yellow>> Stop {ID}";
        if (!byte.TryParse(arguments.At(1), out var controllerId) || !_audioPlayerIds.Contains(controllerId))
            return $"<color=red>Invalid ID. Here's a list of ID's {string.Join(" ", _audioPlayerIds)}";
        if (!AudioPlayer.AudioPlayerByName.TryGetValue($"AudioPlayer {controllerId}", out var audioPlayer))
            return "<color=red>How did this happen...";

        audioPlayer.RemoveSpeaker("Speaker");
        audioPlayer.Destroy();

        _audioPlayerIds.Remove(controllerId);
        return $"<color=green>Stopped all audio from {controllerId}";
    }
}