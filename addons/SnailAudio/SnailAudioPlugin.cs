// -----------------------------------------------------------------------
// Copyright (c) 2026 Tim Donnan. All rights reserved.
// -----------------------------------------------------------------------

#if TOOLS
using System.Reflection.Metadata;
using Godot;

[Tool]
public partial class SnailAudioPlugin : EditorPlugin
{
	private const string _autoloadName = "AudioManager";

	private const string _configPath = "res://addons/SnailAudio/SnailSettings.cfg";

	public override void _EnterTree()
	{
		AddAutoloadSingleton(_autoloadName, "res://addons/SnailAudio/Scripts/AudioManager.cs");
		
		if (!FileAccess.FileExists(_configPath))
		{
			CreateConfig();
		}
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton(_autoloadName);
	}


	private void CreateConfig ()
	{
		ConfigFile config = new();
		config.SetValue("Player1", "player_name", "Bob");
		config.SetValue("Player1", "best_score", 10);
		config.SetValue("Player2", "player_name", "Bob2");
		config.SetValue("Player2", "best_score", 9001);

		config.Save(_configPath);
	}
}
#endif
