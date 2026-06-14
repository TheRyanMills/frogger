using Godot;

public partial class Ambiance : AudioStreamPlayer2D
{
	const int TILE = 64;

	[Export]
	int tileHeight;

	Player player;
	Tween tween;

	public override void _Ready()
	{
		Timer ambienceUpdateTimer = GetNode<Timer>("../AmbienceUpdateTimer");
		ambienceUpdateTimer.Timeout += UpdateVolume;

		player = null;
	}


	// ---------- Public Util ----------

	public void SetPlayer(Player player)
	{		
		this.player = player;

		if (tween != null)
		{
			tween.Pause();
		}

		UpdateVolume();
	}

	public void RemovePlayer()
	{		
		player = null;
		Silence();
	}


	// ---------- Private Util ----------

	void UpdateVolume()
	{
		if (player == null)
		{
			Silence();
			return;
		}

		// Update volume based on vertical distance from node
		float distance = Mathf.Abs(player.Position.Y - Position.Y);

		if (distance <= tileHeight * TILE)
		{
			tween = CreateTween();
			tween.TweenProperty(this, "volume_db", -10 + -10 * (distance - TILE) / ((tileHeight - 1) * TILE), 0.5f);
		}
		else if (VolumeDb >= -20)
		{
			Silence();
		}
	}

	void Silence()
	{
		if (VolumeDb >= -20)
		{
			tween = CreateTween();
			tween.TweenProperty(this, "volume_db", -70.0f, 5.0f);
		}	
	}
}
