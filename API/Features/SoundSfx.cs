namespace GRPP.API.Features;

using System;
using System.Collections.Generic;
using System.IO;
using Attributes;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
// using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using MEC;
using UnityEngine;
using Speaker = Speaker;
using Extensions;

public class SoundSfx
{
    [OnPluginEnabled]
    public static void InitEvents()
    {
        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP"));
        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP", "audio")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP", "audio"));
        if (!Directory.Exists(Path.Combine(Paths.Configs, "GRPP", "audio", "PluginSFX")))
            Directory.CreateDirectory(Path.Combine(Paths.Configs, "GRPP", "audio", "PluginSFX"));
        // AudioClipStorage.LoadClip(Path.Combine(Paths.Configs, "GRPP", "audio", "PluginSFX", ""), "scp");
        try 
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Paths.Configs, "GRPP", "audio", "PluginSFX", "*.ogg")))
            {
                if (!file.IsItOgg()) // by the way, this is a fake check. all this checks at the moment is whether the file has the extension `.ogg`.
                {
                    Log.Warn($"The provided file does not have the correct format.{file}");
                    continue;
                }
                AudioClipStorage.LoadClip(file);  // doin it without the load just grabs the file name without the type !
            }
        } 
        catch (Exception e) {Log.Info($"Exception. GRPP/audio/PluginSFX likely empty.\"{e.Message}\"");}
        // public List<String> listoffiles = whateverthereadallfiles()thingis; 

        PlayerHandlers.Shot += Shot;
        // Exiled.Events.Handlers.Map.ExplodingGrenade += ExplodingGrenade;
    }

    // private static float _uses;

    // public static void ExplodingGrenade(ExplodingGrenadeEventArgs ev)
    // {
    //     var audioPlayer = AudioPlayer.CreateOrGet("Grenade", destroyWhenAllClipsPlayed: true, controllerId: +1); // okay when i figure out what controllerid is i can try fixing this further but
    //     audioPlayer?.AddSpeaker("GrenadeSpeaker" + _uses, ev.Position, maxDistance: 259, minDistance: 56);
    //     audioPlayer?.AddClip("GrenadeSFX");
    //     if (_uses >= 500)
    //         _uses = 0;
    //     _uses++;
    // }
    private static Dictionary<ExPlayer, ActiveSpeaker> _speakerIsActive = new();

    private class ActiveSpeaker(bool justActivated, float timing)
    {
        public bool JustActivated = justActivated;
        public float Timing = timing;
    } 
    // ev.Player.GameObject.GetComponent()
    public static void Shot(ShotEventArgs ev)
    {
        if (ev.Firearm.HasAttachment(AttachmentName.SoundSuppressor))
            return;
        if(!ev.Player.ScomPlayer().AudioPlayer.SpeakersByName.TryGetValue("ShotSFX", out var speaker))
            speaker = ev.Player.ScomPlayer().AudioPlayer.AddSpeaker("ShotSFX", ev.Player.Position, minDistance: 40, maxDistance: 400);

        speaker.Position = ev.Player.Position;

        if(!_speakerIsActive.ContainsKey(ev.Player))
            _speakerIsActive.Add(ev.Player, new ActiveSpeaker(true, 0));

        AudioClipPlayback audioClipPlayback;
        switch (ev.Firearm.FirearmType)
        {
            case FirearmType.Com45: // audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.ComSfx); break;
            case FirearmType.Com15: // audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.ComSfx); break;
            case FirearmType.Com18: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.ComSfx); break;

            case FirearmType.Revolver: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.RevolverSfx); break;

            case FirearmType.FSP9: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.Fsp9Sfx); break;
            case FirearmType.Crossvec: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.CrossVecSfx); break;

            case FirearmType.Shotgun: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.ShotgunSfx); break;

            case FirearmType.E11SR: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.E11Sfx); break;
            case FirearmType.FRMG0: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.FrmgSfx); break;

            case FirearmType.Logicer: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.LogicerSfx); break;
            case FirearmType.A7: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.A7Sfx); break;
            case FirearmType.AK: audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.AkSfx); break;

            case FirearmType.ParticleDisruptor:
                audioClipPlayback = speaker.Owner.AddClip(Plugin.Singleton.Config.ParticleSfx);
                break;

            // case FirearmType.None:
            default:
                speaker.Destroy();
                return;
        }

        _speakerIsActive[ev.Player].Timing = (float)audioClipPlayback.Duration.TotalSeconds;
        if(_speakerIsActive[ev.Player].JustActivated)
            Timing.RunCoroutine(DestroySpeakerAfter(ev.Player, speaker));
    }

    public static IEnumerator<float> DestroySpeakerAfter(ExPlayer player, Speaker speaker)
    {
        _speakerIsActive[player].JustActivated = false;
        while (_speakerIsActive[player].Timing > 0)
        {
            _speakerIsActive[player].Timing -= 1 * Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }

        _speakerIsActive.Remove(player);
        speaker.Destroy();
    }
}