using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class Main : Node2D
{
	[Signal]
	public delegate void DifficultyUpEventHandler();

	private Player Player => GetNode<Player>("/root/Main/Player");
	private Timer DifficultyUpTimer => GetNode<Timer>("DifficultyUpTimer");
	private Timer EnemySpawnTimer => GetNode<Timer>("EnemySpawnTimer");
	private AudioStreamPlayer2D DifficultyUpAudio => GetNode<AudioStreamPlayer2D>("DifficultyUpAudio");
	private AudioStreamPlayer2D MenuAudio => GetNode<AudioStreamPlayer2D>("MenuAudio");
	private AudioStreamPlayer2D SoundtrackAudio => GetNode<AudioStreamPlayer2D>("Soundtrack");
	private PackedScene _triceratopsScene = GD.Load<PackedScene>("res://scenes/triceratops.tscn");
	private PackedScene _eoraptorVScene = GD.Load<PackedScene>("res://scenes/eoraptor_v.tscn");
	private Random _random;
	
	public override void _Ready()
	{
		_random = new Random((int)DateTime.Now.Ticks);
	}

	public override void _Process(double delta)
	{
		if (Globals.IsGameOver)
		{
			var node = GetNode("UI") as UI;
			node.OnGameOver();
			SoundtrackAudio.Stop();
		}
	}

	private void OnEnemySpawnTimerTimeout()
	{
		if (!Globals.IsGameStarted || Globals.DinoSpawnMap.All(x => x.Value) || Globals.IsGameOver)
		{
			return;
		}
		
		int rnd;
		do
		{
			rnd = _random.Next(0, Globals.DinoSpawnMap.Count);
		} while (Globals.DinoSpawnMap[rnd]);
		
		Globals.DinoSpawnMap[rnd] = true;
		var dino = (Node2D)_triceratopsScene.Instantiate();

		var dinoClass = dino as Eoraptor;
		dinoClass.HitPlayer += (damage, x, y) => Player.OnEoraptorHitPlayer(damage, x, y);
		dinoClass.IndexOnMap = rnd;
		dinoClass.IsHorizontal = true;
		dinoClass.Position = new Vector2(880.0f, 35.0f * Math.Max(rnd, 1));
		AddChild(dino);
	}

	private void OnEnemySpawnVerticalTimerTimeout()
	{
		if (!Globals.IsGameStarted || Globals.DinoSpawnVerticalMap.All(x => x.Value) || Globals.IsGameOver)
		{
			return;
		}
		
		int rnd;
		do
		{
			rnd = _random.Next(0, Globals.DinoSpawnVerticalMap.Count);
		} while (Globals.DinoSpawnVerticalMap[rnd]);
		
		Globals.DinoSpawnVerticalMap[rnd] = true;
		var dino = (Node2D)_eoraptorVScene.Instantiate();

		var dinoClass = dino as Eoraptor;
		dinoClass.HitPlayer += (damage, x, y) => Player.OnEoraptorHitPlayer(damage, x, y);
		dinoClass.IndexOnMap = rnd;
		dinoClass.IsHorizontal = false;
		dinoClass.Position = new Vector2(rnd == 0 ? 10 : 35.0f * Math.Max(rnd, 1), -20.0f);
		AddChild(dino);
	}

	private void OnDifficultyUpTimerTimeout()
	{
		if (Globals.IsGameOver || Globals.Difficulty >= 4)
		{
			return;
		}
		
		DifficultyUpAudio.Play();
		Globals.Difficulty += 0.5f;
		EnemySpawnTimer.WaitTime = EnemySpawnTimer.WaitTime / Globals.Difficulty * 1.2f;
		SoundtrackAudio.PitchScale += 0.04f;
		EmitSignal(SignalName.DifficultyUp);
	}

	private void OnUIGameStarted()
	{
		MenuAudio.Stop();
		SoundtrackAudio.Play();
		DifficultyUpTimer.Start();
	}

	private void OnUIGameRestarted()
	{
		DifficultyUpTimer.Stop();
	}
}
