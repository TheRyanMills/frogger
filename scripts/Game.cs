using Godot;
using System;
using System.Threading.Tasks;

public partial class Game : Node2D
{
	int score, level, livesRemaining, safeFrogCount;
	Tween tween;

	public override void _Ready()
	{
		score = 0;
		level = 0; // Zero indexed
		
		livesRemaining = 3;
		safeFrogCount = 0;
		
		CreateLaneManagers();
		ResetPlayer();
	}


	// ---------- Signals ----------

	async void OnTimerTimeout()
	{
		ColorRect timerBarBackground = GetNode<ColorRect>("HUD/TimerBarBackground");
		Timer attemptTimer = GetNode<Timer>("HUD/AttemptTimer");

		// Flash timer bar background
		while (attemptTimer.IsStopped())
		{
			timerBarBackground.Color = new Color(0.427f, 0.459f, 0.553f); // Light gray
			await Task.Delay(TimeSpan.FromSeconds(0.20));
			timerBarBackground.Color = new Color(0.29f, 0.329f, 0.384f); // Dark gray
			await Task.Delay(TimeSpan.FromSeconds(0.20));
		}
	}


	// ---------- Public Util ----------

	public void AddScore(int scored)
	{
		Label scoreLabel = GetNode<Label>("HUD/ScoreLabel");
		
		score += scored;
		scoreLabel.Text = "Score: " + score;
	}

	public async void SafeFrog()
	{
		Timer attemptTimer = GetNode<Timer>("HUD/AttemptTimer");

		attemptTimer.Stop();
		tween.Pause();

		safeFrogCount++;
		AddScore(50 + 10 * (int)attemptTimer.TimeLeft);

		if (safeFrogCount == 1)
		{
			ResetLevel();
		}
		else
		{
			ResetPlayer();
		}
	}

	public void DeadFrog()
	{
		Timer attemptTimer = GetNode<Timer>("HUD/AttemptTimer");

		attemptTimer.Stop();
		tween.Pause();

		UpdateLives(-1);

		if (livesRemaining == 0)
		{
			GameOver();
		}
		else
		{
			ResetPlayer();
		}
	}


	// ---------- Private Util ----------

	void CreateLaneManagers()
	{
		PackedScene carScene = GD.Load<PackedScene>("res://scenes/cars/car.tscn");
		PackedScene turtles3Scene = GD.Load<PackedScene>("res://scenes/turtles/turtles3.tscn");
		PackedScene logs3Scene = GD.Load<PackedScene>("res://scenes/logs/logs3.tscn");
		PackedScene logs6Scene = GD.Load<PackedScene>("res://scenes/logs/logs6.tscn");
		PackedScene turtles2Scene = GD.Load<PackedScene>("res://scenes/turtles/turtles2.tscn");
		PackedScene logs4Scene = GD.Load<PackedScene>("res://scenes/logs/logs4.tscn");

		AddChild(new LaneManager(logs4Scene, 256, true, 125, 0, 2, 4, 4));
		AddChild(new LaneManager(turtles2Scene, 320, false, 125, 0, 2, 9, 4));
		AddChild(new LaneManager(logs6Scene, 384, true, 150, 0, 3, 7, 3));
		AddChild(new LaneManager(logs3Scene, 448, true, 100, 0, 2, 9, 3));
		AddChild(new LaneManager(turtles3Scene, 512, false, 125, 0, 1, 3, 5));

		AddChild(new LaneManager(carScene, 632, false, 125, 5, 3, 7, 2));
		AddChild(new LaneManager(carScene, 696, true,  100, 10, 3, 7, 3));
		AddChild(new LaneManager(carScene, 760, false, 125, 6, 3, 7, 3));
		AddChild(new LaneManager(carScene, 824, true,  100, 4, 3, 7, 3));
		AddChild(new LaneManager(carScene, 888, false, 100, 3, 3, 7, 3));
	}

	async void ResetLevel()
	{
		Ambiance trafficAmbiance = GetNode<Ambiance>("TrafficAmbiance");
		Ambiance riverAmbiance = GetNode<Ambiance>("RiverAmbiance");
		Label clearLabel = GetNode<Label>("HUD/ClearLabel");
		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("HUD/ClearLabel/AnimationPlayer");
		AudioStreamPlayer2D clearSound = GetNode<AudioStreamPlayer2D>("HUD/ClearSound");
		Label levelLabel = GetNode<Label>("HUD/LevelLabel");
		LilyPad lilyPad1 = GetNode<LilyPad>("LilyPad1");
		LilyPad lilyPad2 = GetNode<LilyPad>("LilyPad2");
		LilyPad lilyPad3 = GetNode<LilyPad>("LilyPad3");
		LilyPad lilyPad4 = GetNode<LilyPad>("LilyPad4");
		LilyPad lilyPad5 = GetNode<LilyPad>("LilyPad5");

		// Complete Level
		AddScore(1000);

		trafficAmbiance.RemovePlayer();
		riverAmbiance.RemovePlayer();

		clearLabel.Show();
		animationPlayer.Play("wobble");
		clearSound.Play();

		await Task.Delay(TimeSpan.FromSeconds(3)); // Pause for sound

		clearLabel.Hide();
		animationPlayer.Stop();

		// Increment difficulty
		level++;
		levelLabel.Text = "Level " + (level + 1);

		// Reset lily pads
		safeFrogCount = 0;

		lilyPad1.HideFrog();
		lilyPad2.HideFrog();
		lilyPad3.HideFrog();
		lilyPad4.HideFrog();
		lilyPad5.HideFrog();

		// Gain a life
		if (livesRemaining < 3)
		{
			UpdateLives(1);
		}

		// Reset Player
		ResetPlayer();
	}

	void ResetPlayer()
	{
		Ambiance trafficAmbiance = GetNode<Ambiance>("TrafficAmbiance");
		Ambiance riverAmbiance = GetNode<Ambiance>("RiverAmbiance");
		ColorRect attemptTimerBar = GetNode<ColorRect>("HUD/AttemptTimerBar");
		ColorRect attemptTimerBackground = GetNode<ColorRect>("HUD/AttemptTimerBackground");
		Timer attemptTimer = GetNode<Timer>("HUD/AttemptTimer");

		// Reset player
		PackedScene playerScene = GD.Load<PackedScene>("res://scenes/player.tscn");

		Player player = playerScene.Instantiate<Player>();
		player.Position = new Vector2(416, 928);
		CallDeferred("add_child", player);

		trafficAmbiance.SetPlayer(player);
		riverAmbiance.SetPlayer(player);

		// Reset timer
		if (tween != null)
		{
			tween.Kill();
		}

		int timerDuration = 50 - 5 * level;
		attemptTimerBar.Color = new Color(0.349f, 0.757f, 0.208f);
		attemptTimerBar.Scale = new Vector2(-0.1f * timerDuration, 1.0f);
		attemptTimerBackground.Color = new Color(0.29f, 0.329f, 0.384f);
		attemptTimerBackground.Scale = new Vector2(-0.1f * timerDuration, 1.0f);

		// Start timer
		attemptTimer.WaitTime = timerDuration;
		attemptTimer.Timeout += player.Explode;
		attemptTimer.Start();

		tween = CreateTween();
		tween.TweenProperty(attemptTimerBar, "scale:x", 0, timerDuration);
		tween.Parallel().TweenProperty(attemptTimerBar, "color:h", 0.0, timerDuration);
	}

	void UpdateLives(int livesToAdd)
	{
		Sprite2D life1 = GetNode<Sprite2D>("HUD/Life1");
		Sprite2D life2 = GetNode<Sprite2D>("HUD/Life2");
		Sprite2D life3 = GetNode<Sprite2D>("HUD/Life3");

		livesRemaining += livesToAdd;

		life1.Visible = livesRemaining >= 1;
		life2.Visible = livesRemaining >= 2;
		life3.Visible = livesRemaining >= 3;
	}

	void GameOver()
	{
		Label gameOverLabel = GetNode<Label>("HUD/GameOverLabel");
		AudioStreamPlayer2D gameOverSound = GetNode<AudioStreamPlayer2D>("HUD/GameOverSound");

		gameOverLabel.Show();
		gameOverSound.Play();
	}
}
