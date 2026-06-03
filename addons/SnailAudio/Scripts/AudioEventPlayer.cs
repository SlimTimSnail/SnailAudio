// -----------------------------------------------------------------------
// Copyright (c) 2026 Tim Donnan. All rights reserved.
// -----------------------------------------------------------------------

using Godot;
using Godot.Collections;
using System;

[Tool]
[Icon("uid://378jdspwdk1x")]
[GlobalClass]
public partial class AudioEventPlayer : Node
{
    [Export]
    public AudioEvent AudioEvent
    {
        get;
        private set 
        { 
            if (value != field)
            {
                field = value;
                NotifyPropertyListChanged();
            }
        }
    }

    // Exported if the event attenuation data != null
    private Node _attachToNode = null;

    [Export]
    public bool PlayOnReady
    {
        get;
        private set 
        { 
            if (value != field)
            {
                field = value;
                NotifyPropertyListChanged();
            }
        }
    }

    // Exported if PlayOnReady = true
    private bool _asOneShot = false;

    //[Export]
    //private bool _stopOnExitTree = true;

    private Guid _eventId = Guid.NewGuid();

    private AudioStreamPlayer2D _audioPlayer = null;


    //
    // Ready / Exit Functions
    //

    public override void _EnterTree()
    {
        base._EnterTree();

        // If the audio event should be positioned in the world, check for or spawn a stream player at the end of the frame.
        if (GodotObject.IsInstanceValid(_attachToNode) && AudioEvent.AttenuationData != null)
        { 
            CallDeferred(MethodName.AssignAudioPlayer);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        if (PlayOnReady == true)
        {
            if (_asOneShot == true)
            {
                CallDeferred(MethodName.PlayOneShot);
            }
            else
            {
                CallDeferred(MethodName.StartAudio);;
            }
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        //if (_stopOnExitTree)
        //{
            //StopAudio();
        //}
    }


    //
    // Public Functions
    //

    public void PlayOneShot()
    {
        if (EventValid())
        {
            if (GodotObject.IsInstanceValid(_audioPlayer))
            {
                AudioManager.Instance.PlayOneShotEventAttached(AudioEvent, _audioPlayer);
            }
            else
            {
                AudioManager.Instance.PlayOneShotEvent(AudioEvent);
            }
        }
    }

    public void StartAudio()
    {
        if (EventValid())
        {
            if (GodotObject.IsInstanceValid(_audioPlayer))
            {
                AudioManager.Instance.StartAudioEventAttached(AudioEvent, _audioPlayer, _eventId);
            }
            else
            {
                AudioManager.Instance.StartAudioEvent(AudioEvent, _eventId);
            }
        }
    }

    public void StopAudio()
    {
        if (EventValid())
        {
            AudioManager.Instance.StopAudioEvent(_eventId);
        }
    }


    //
    // Tool Scripting
    //

    public override Array<Dictionary> _GetPropertyList()
    {
        Array<Dictionary> properties = [];

        //add the one shot bool if play on ready is enabled
        if (PlayOnReady)
        {
            properties.Add(new Dictionary()
            {
                { "name", nameof(_asOneShot) },
                { "type", (int)Variant.Type.Bool },
            });
        }

        // add the attach to object export field if the event contains attenuation data
        if (AudioEvent != null && AudioEvent.AttenuationData != null)
        {
            properties.Add(new Dictionary()
            {
                { "name", nameof(_attachToNode) },
                { "type", (int)Variant.Type.Object },
                { "hint", (int)PropertyHint.NodeType },
                { "hint_string", "Node2D" },
            });
        }

        return properties;
    }


    //
    // Private Functions
    //

    private void AssignAudioPlayer ()
    {
        // Checks if the attached node already has an appropriate audio player. If not, creates one. 
        // An appropriate audio player has the same Bus and attenuation data.
        if (EventValid())
        {
            AudioManager.Instance.SpawnAudioPlayer(AudioEvent, _attachToNode, out AudioStreamPlayer2D audioPlayer);
            
            if (GodotObject.IsInstanceValid(audioPlayer))
            {
                _audioPlayer = audioPlayer;
                _attachToNode.TreeExiting += StopAudio;
            }
        }
    }


    //
    // Bool functions
    //

    private bool CheckForAudioPlayer (out AudioStreamPlayer2D result)
    {
        // Checks if the attached node already has an appropriate audio player. 
        // An appropriate audio player has the same Bus and attenuation data. Otherwise we need to make a new one.
        foreach (Node attachedChild in _attachToNode.GetChildren())
        {
            if (attachedChild is AudioStreamPlayer2D audioPlayer)
            {
                if (AudioEvent.AudioBus == audioPlayer.Bus && CompareAttenuationData(audioPlayer))
                {
                    result = audioPlayer;
                    return true;
                }
            }
        }
        result = null;
        return false;
    }

    private bool CompareAttenuationData (AudioStreamPlayer2D audioPlayer)
    {
        // I want to eventually be able to compare this via the data asset, but this isn't stored on the player currently
        AudioAttenuationData attenuationData = AudioEvent.AttenuationData;
        if (attenuationData.Attenuation == audioPlayer.Attenuation && attenuationData.MaxDistance == audioPlayer.MaxDistance && attenuationData.PanningStrength == audioPlayer.PanningStrength)
        {
            return true;
        }
        return false;
    }

    private bool EventValid()
    {
        if (AudioEvent == null)
        {
            GD.Print("Event resource is missing, no audio played");
            return false;
        }

        if (!GodotObject.IsInstanceValid(AudioManager.Instance))
        {
            GD.Print("I'm trying to play a sound but the Audio Manager doesn't exist for some reason!!!!");
            return false;
        }

        return true;
    }
}
 