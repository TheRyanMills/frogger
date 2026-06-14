using Godot;
using System;
using System.Threading.Tasks;

public partial class Player : CharacterBody2D
{
	const int TILE = 64;

	static Random random;

	bool isLeaping, isDead;
	int floatingCount;
	float farthestForward;
	Tween tween;

	public override void _Ready()
	{
		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");

		random = new Random();
		
		isLeaping = false;
		isDead = false;
		floatingCount = 0;
		farthestForward = GetViewportRect().Size.Y;

		sprite.RotationDegrees = 180;
	}

	public override void _Process(double delta)
	{
		Game game = GetNode<Game>("..");
		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");

		MoveAndSlide();

		// Handle leap
		if (!isLeaping && !isDead)
		{
			// Move in direction of input
			if (Input.IsActionPressed("move_up") && Position.Y > 96)
			{
				// Add score if moved forward
				if (Position.Y < farthestForward)
				{
					game.AddScore(10);
					farthestForward = Position.Y;
				}

				Leap(Vector2.Up);
				sprite.RotationDegrees = 180;
			}
			if (Input.IsActionPressed("move_down") && Position.Y < GetViewportRect().Size.Y - 96)
			{
				Leap(Vector2.Down);
				sprite.RotationDegrees = 0;
			}
			if (Input.IsActionPressed("move_left") && Position.X > 32)
			{
				Leap(Vector2.Left);
				sprite.RotationDegrees = 90;
			}
			if (Input.IsActionPressed("move_right") && Position.X < GetViewportRect().Size.X - 32)
			{
				Leap(Vector2.Right);
				sprite.RotationDegrees = 270;
			}
		}

		// Handle ambient sounds
		

		// Explode if off-screen
		if ((Position.X < -8 || Position.X > GetViewportRect().Size.X) && !isDead)
		{
			Explode();
		}
	}

	// ---------- Signals ----------

	void OnArea2DBodyEntered(Node2D body)
	{		
		switch (body)
		{
			case Car car:
				if (!isDead)
				{
					Squish();
				}
				break;
			case Floater floater:
				floatingCount++;
				Velocity = floater.Velocity;
				break;
			case LilyPad lilyPad:
				if (!lilyPad.isUsed)
				{					
					lilyPad.ShowFrog();
					Safe();
				}
				break;
		}
	}

	void OnArea2DBodyExited(Node2D body)
	{
		switch (body)
		{
			case Floater floater:
				floatingCount--;
				break;
		}
	}


	// ---------- Private Util ----------

	async void Leap(Vector2 direction)
	{
		AudioStreamPlayer2D frogJump = GetNode<AudioStreamPlayer2D>("FrogJump");
		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		isLeaping = true;

		frogJump.PitchScale = 0.8f + 0.4f * (float)random.NextDouble();
		frogJump.Play();

		animationPlayer.Play("hop");

		tween = CreateTween();
		tween.TweenProperty(this, "position", Position + (direction * TILE), 0.15); // TO-DO: Account for velocity?

		// --- Await tween finishing ---
		await ToSignal(tween, Tween.SignalName.Finished);

		if (floatingCount == 0)
		{
			Velocity = Vector2.Zero;
		}

		await Task.Delay(TimeSpan.FromSeconds(0.05));

		isLeaping = false;

		// Check if position is valid
		if (!isDead)
		{
			if (Position.Y < 192)
			{
				Explode();
			}
			else if (Position.Y < 512 && floatingCount == 0)
			{
				Drown();
			}
		}
	}

	async void Safe()
	{
		Game game = GetNode<Game>("..");

		game.SafeFrog();
		isDead = true; // Cancel all asynchronous operations

		QueueFree();
	}

	async void Squish()
	{
		AudioStreamPlayer2D squish = GetNode<AudioStreamPlayer2D>("Squish");
		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");

		if (!isDead)
		{
			Die();
		}

		squish.PitchScale = 0.8f + 0.4f * (float)random.NextDouble();
		squish.Play();
		sprite.Scale = new Vector2(1.0f, 0.4f + 0.4f * (float)random.NextDouble());

		// Sprite Animation
		tween = CreateTween();

		int shakes = random.Next(6, 10);
		for (int i = 0; i < shakes; i++)
		{
			tween
				.TweenProperty(sprite, "position", new Vector2(random.Next(-10, 10), random.Next(-10, 10)), 0.05);
			tween
				.TweenProperty(sprite, "rotation_degrees", random.Next(-8, 8), 0.05)
				.AsRelative();
		}

		tween.TweenProperty(sprite, "modulate:a", 1.0f, 1);
		tween.TweenProperty(sprite, "modulate:a", 0.0f, 2);

		// --- Await tween finishing ---
		await ToSignal(tween, Tween.SignalName.Finished);

		QueueFree();
	}

	async void Drown()
	{
		GpuParticles2D splashParticles = GetNode<GpuParticles2D>("SplashParticles");
		AudioStreamPlayer2D fallen = GetNode<AudioStreamPlayer2D>("Fallen");
		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");

		Die();
		ZIndex = -2; // Move behind floaters

		// Particles
		splashParticles.Emitting = true;
		fallen.PitchScale = 0.8f + 0.4f * (float)random.NextDouble();
		fallen.Play();

		// Sprite Animation
		tween = CreateTween();

		int dir = -1;
		double duration = 0.15;
		for (int i = 0; i < 10; i++)
		{
			duration += 0.03 * i;

			tween
				.TweenProperty(sprite, "position:y", (20 * dir) + i, duration)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut)
				.AsRelative();

			tween
				.Parallel()
				.TweenProperty(sprite, "rotation_degrees", (20 - i) * dir, duration)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out)
				.AsRelative();

			tween
				.Parallel()
				.TweenProperty(sprite, "scale", new Vector2(1.0f - 0.02f * i, 1.0f - 0.02f * i), duration);

			tween
				.Parallel()
				.TweenProperty(sprite, "modulate:a", 1.0f - 0.1f * i, duration);

			dir *= -1;
		}

		// --- Await tween finishing ---
		await ToSignal(tween, Tween.SignalName.Finished);

		QueueFree();
	}

	public async void Explode()
	{
		AudioStreamPlayer2D fuse = GetNode<AudioStreamPlayer2D>("Fuse");
		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
		AudioStreamPlayer2D explosion = GetNode<AudioStreamPlayer2D>("Explosion");
		GpuParticles2D trailParticles = GetNode<GpuParticles2D>("ExplosionTrailParticles");
		GpuParticles2D pointParticles = GetNode<GpuParticles2D>("ExplosionPointParticles");

		Die();

		fuse.Play();

		// Sprite Animation
		tween = CreateTween();

		for (int i = 0; i < 8; i++)
		{
			tween
				.TweenProperty(sprite, "rotation_degrees", 40 + i * 15, 0.10)
				.AsRelative();
		}

		// --- Await tween finishing ---
		await ToSignal(tween, Tween.SignalName.Finished);

		sprite.Hide();

		fuse.Stop();
		explosion.PitchScale = 0.8f + 0.4f * (float)random.NextDouble();
		explosion.Play();

		trailParticles.Emitting = true;
		pointParticles.Emitting = true;

		await Task.Delay(TimeSpan.FromSeconds(1)); // Let particles play

		QueueFree();
	}

	async void Die()
	{
		CollisionShape2D collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
		Game game = GetNode<Game>("..");

		Velocity = Vector2.Zero;
		collisionShape.CallDeferred("set_disabled", true);
		isDead = true;

		await Task.Delay(TimeSpan.FromSeconds(2)); // Pause to watch animation

		game.DeadFrog();
	}
}
