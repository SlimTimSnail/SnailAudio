// -----------------------------------------------------------------------
// Copyright (c) 2026 Tim Donnan. All rights reserved.
// -----------------------------------------------------------------------

using Godot;
using Godot.Collections;

[Tool]
[Icon("uid://dhloy1kg42olc")]
[GlobalClass]
public partial class AudioEvent : Resource
{
    //
    [ExportCategory("Main")]
    //
    [Export]
    public AudioStream AudioStream
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

    [Export(PropertyHint.Enum)]
    public string AudioBus { get; private set; } = AudioServer.GetBusName(0);

    //
    [ExportCategory("Volume Settings")]
    //
    [Export(PropertyHint.Range, "-72,6,0.1, suffix:dB")]
    public float Volume { get; private set; } = 0.0f;
    
    [ExportGroup("Fades")]
    [Export(PropertyHint.Range, "0,,0.01,or_greater suffix:Seconds")]
    public float FadeInLength { get; private set; } = 0.0f;

    [Export(PropertyHint.Range, "0,,0.01,or_greater suffix:Seconds")]
    public float FadeOutLength { get; private set; } = 0.0f;

    //
    [ExportCategory("Other")]
    //
    [Export(PropertyHint.Range, "0.01,10,0.01,")]
    public float PitchScale { get; private set; } = 1.0f;

    //
    [ExportCategory("Attenuation")]
    //
    [Export]
    public AudioAttenuationData AttenuationData { get; private set; } = null;


    public int AudioBusIndex
    {
        get => AudioServer.GetBusIndex(AudioBus);
    }


    public override void _ValidateProperty(Dictionary property)
    {
        // Setup the audio bus enum
        if (property["name"].AsString() == "AudioBus")
        {
            int busCount = AudioServer.BusCount;
            string busNames = "";

            for (int i = 0; i < busCount; i++)
            {
                if (i > 0)
                {
                    busNames += ",";
                }

                busNames += AudioServer.GetBusName(i);
            }
            
            property["hint_string"] = busNames;
        }

        // adjust the range of the fade ins and outs based on the actual stream length
        if (property["name"].AsString() == "FadeInLength" || property["name"].AsString() == "FadeOutLength")
        {
            if (AudioStream != null)
            {
                string streamLength = "";

                if (AudioStream is AudioStreamRandomizer randomAudioStream)
                {
                    int streamCount = randomAudioStream.StreamsCount;;

                    if (streamCount <= 0)
                    {
                        return;
                    }

                    double greatestStreamLength = 0;

                    for (int i = 0; i < streamCount; i++)
                    {
                        double currentStreamLength = randomAudioStream.GetStream(i).GetLength();
                        if (currentStreamLength > greatestStreamLength)
                        {
                            greatestStreamLength = currentStreamLength;
                        }
                    }
                    
                    streamLength = greatestStreamLength.ToString();
                }

                if (AudioStream is not AudioStreamRandomizer)
                {
                    streamLength = AudioStream.GetLength().ToString();
                }

                property["hint_string"] = "0," + streamLength + "0.01,";
            }
        }
    }
}
