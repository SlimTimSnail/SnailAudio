#if TOOLS
using Godot;
using System;

[Tool]
public partial class SnailAudioPlugin : EditorPlugin
{
	private const string _autoloadName = "AudioManager";

	public override void _EnterTree()
	{
		AddAutoloadSingleton(_autoloadName, "res://addons/SnailAudio/AudioManager.cs");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton(_autoloadName);
	}
}
#endif
