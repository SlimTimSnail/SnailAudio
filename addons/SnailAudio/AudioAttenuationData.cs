// -----------------------------------------------------------------------
// Copyright (c) 2026 Tim Donnan. All rights reserved.
// -----------------------------------------------------------------------

using Godot;

[Tool]
[Icon("uid://dfelah12kagti")]
[GlobalClass]
public partial class AudioAttenuationData : Resource
{
    [Export(PropertyHint.Range, "0,,0.01,or_greater")]
    public float PanningStrength { get; private set; } = 1.0f;

    [Export(PropertyHint.Range, "1,4096,1, suffix:px")]
    public float MaxDistance { get; private set; } = 2000.0f;

    [Export(PropertyHint.ExpEasing, "attenuation,inout")]
    public float Attenuation { get; private set; } = 1.0f;
}
