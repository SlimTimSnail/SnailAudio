// -----------------------------------------------------------------------
// Copyright (c) 2026 Tim Donnan. All rights reserved.
// -----------------------------------------------------------------------

using Godot;
using System;
using System.Collections.Generic;

[Icon("uid://dfelah12kagti")]
public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    private Dictionary<int, AudioStreamPlayer> _genericBusRegister = [];

    private Dictionary<Guid, (AudioStreamPlaybackPolyphonic StreamPlayback, long StreamIndex, AudioEvent AudioEvent)> _eventRegister = [];


    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        GenerateGenericAudioStreamPlayers();
    }


    // 
    // Public Methods
    //

    public void PlayOneShotEvent (AudioEvent audioEvent)
    {
        if (audioEvent != null)
        {
            HandleAudioEvent(audioEvent, null, Guid.Empty);
        }
    }

    public void PlayOneShotEventAttached (AudioEvent audioEvent, AudioStreamPlayer2D audioPlayer)
    {
        if (audioEvent != null && GodotObject.IsInstanceValid(audioPlayer))
        {
            HandleAudioEvent(audioEvent, audioPlayer, Guid.Empty);
        }
    }

    public void StartAudioEvent (AudioEvent audioEvent, Guid eventId)
    {
        if (audioEvent != null && eventId != Guid.Empty && !_eventRegister.ContainsKey(eventId))
        {
            HandleAudioEvent(audioEvent, null, eventId);
        }
    }

    public void StartAudioEventAttached (AudioEvent audioEvent, AudioStreamPlayer2D audioPlayer, Guid eventId)
    {
        if (audioEvent != null && GodotObject.IsInstanceValid(audioPlayer) && eventId != Guid.Empty && !_eventRegister.ContainsKey(eventId))
        {
            HandleAudioEvent(audioEvent, audioPlayer, eventId);
        }
    }

    public void StopAudioEvent (Guid eventId)
    {
        if (eventId != Guid.Empty && _eventRegister.ContainsKey(eventId))
        {
            if (_eventRegister.TryGetValue(eventId, out (AudioStreamPlaybackPolyphonic streamPlayback, long streamIndex, AudioEvent audioEvent) value))
            {
                _eventRegister.Remove(eventId);

                if (value.audioEvent.FadeOutLength <= 0f)
                {
                    // No fade out time, just stop
                    CleanUpStream(value.streamPlayback, value.streamIndex);
                }
                else
                {
                    // Perform fade out
                    Tween fadeOutTween = CreateTween();
                    fadeOutTween.TweenMethod(Callable.From((float volume) => value.streamPlayback.SetStreamVolume(value.streamIndex, volume)), value.audioEvent.Volume, -80f, value.audioEvent.FadeOutLength);
                    fadeOutTween.Finished += () => CleanUpStream(value.streamPlayback, value.streamIndex);
                }
            }
        }
    }


    // 
    // Private Methods
    //

    private void HandleAudioEvent (AudioEvent audioEvent, AudioStreamPlayer2D audioPlayer, Guid eventId)
    {
        if (audioEvent == null)
        {
            GD.Print("Not sure how we got here but the audio event is null. Sound will not play.");
            return;
        }

        AudioStreamPlaybackPolyphonic streamPlayback;

        if (GodotObject.IsInstanceValid(audioPlayer))
        {
            if (audioPlayer.Stream is not AudioStreamPolyphonic)
            {
                GD.Print("For some reason this audio stream isn't polyphonic. Sound will not play.");
                return;
            }

            // This is just make sure the stream is running, it doesn't actually play sound
            if (!audioPlayer.IsPlaying())
            {
                audioPlayer.Play();
            }

            streamPlayback = audioPlayer.GetStreamPlayback() as AudioStreamPlaybackPolyphonic;
        }
        else
        {
            // If the event doesn't have an audio player reference then we'll use a generic one
            if (_genericBusRegister.TryGetValue(audioEvent.AudioBusIndex, out AudioStreamPlayer genericAudioPlayer))
            {
                if (genericAudioPlayer.Stream is not AudioStreamPolyphonic)
                {
                    GD.Print("For some reason this audio stream isn't polyphonic. Sound will not play.");
                    return;
                }

                // This is just make sure the stream is running, it doesn't actually play sound
                if (!genericAudioPlayer.IsPlaying())
                {   
                    genericAudioPlayer.Play();
                }

                streamPlayback = genericAudioPlayer.GetStreamPlayback() as AudioStreamPlaybackPolyphonic;
            } 
            else
            {
                GD.Print("The Bus value set on this event was invalid, sound will not play");
                return;
            } 
        }

        // Actually start playing the stream. Grab it's unique identifier as streamIndex.
        long streamIndex = streamPlayback.PlayStream(audioEvent.AudioStream, 0f, -80f, audioEvent.PitchScale, 0, audioEvent.AudioBus);
        
        // Handle fading in
        if (audioEvent.FadeInLength <= 0f)
        {
            // no fade in time, just set volume per the event
            streamPlayback.SetStreamVolume(streamIndex, audioEvent.Volume);
        }
        else
        {
            Tween fadeInTween = CreateTween();
            fadeInTween.TweenMethod(Callable.From((float volume) => streamPlayback.SetStreamVolume(streamIndex, volume)), -80f, audioEvent.Volume, audioEvent.FadeInLength);
        }

        // If the guid is empty, the sound is a oneshot and we can forget about this sound now, otherwise add it to the dict
        if (eventId != Guid.Empty)
        {
            _eventRegister.Add(eventId, (streamPlayback, streamIndex, audioEvent));
        }
    }

    private void CleanUpStream (AudioStreamPlaybackPolyphonic streamPlayback, long streamIndex)
    {
        streamPlayback.StopStream(streamIndex);
        // ToDo: do I need to stop the audio stream player from playing if there are no streams left?
    }

    private void GenerateGenericAudioStreamPlayers ()
    {
        int busCount = AudioServer.BusCount;

        for (int i = 0; i < busCount; i++)
        {
            AudioStreamPlayer newAudioPlayer = new()
            {
                Bus = AudioServer.GetBusName(i),
                MaxPolyphony = 64,
                Stream = new AudioStreamPolyphonic()
            };

            AddChild(newAudioPlayer);

            _genericBusRegister.Add(i, newAudioPlayer);
        }
    }
}
