// -----------------------------------------------------------------------
// Copyright (c) 2026 Tim Donnan. All rights reserved.
// -----------------------------------------------------------------------

#if TOOLS
using Godot;

[Tool]
public partial class SnailAudioPlugin : EditorPlugin
{
	private const string _autoloadName = "AudioManager";

	public override void _EnterTree()
	{
		AddAutoloadSingleton(_autoloadName, "res://addons/SnailAudio/Scripts/AudioManager.cs");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton(_autoloadName);
	}
}
#endif
