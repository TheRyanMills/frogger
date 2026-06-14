using Godot;
using System;

public partial class LaneManager : Node2D
{
	static Random random;

	PackedScene gliderScene;
	float yPos, speed, gliderWidth, secondsUntilNextCar;
	bool isFacingRight;
	int initialOffset, smallGap, largeGap, largeGapFreq, gliderCount;

	public LaneManager(PackedScene gliderScene, float yPos, bool isFacingRight, float speed, int initialOffset, int smallGap, int largeGap, int largeGapFreq)
	{
		random = new Random();

		this.gliderScene = gliderScene;
		this.yPos = yPos;
		this.isFacingRight = isFacingRight;
		this.speed = speed;
		this.initialOffset = initialOffset * 64;
		this.smallGap = smallGap * 64;
		this.largeGap = largeGap * 64;
		this.largeGapFreq = largeGapFreq;

		// Get glider's width
		Glider glider = gliderScene.Instantiate<Glider>();
		CollisionShape2D collisionShape = glider.GetNode<CollisionShape2D>("CollisionShape2D");

        gliderWidth = collisionShape.Shape.GetRect().Size.X;

		glider.Free();
	}

	public override void _Ready()
	{
		// Place initial gliders
		if (isFacingRight)
		{
			PlaceInitialGlidersRight();
		}
		else
		{
			PlaceInitialGlidersLeft();
		}
	}

	public override void _Process(double delta)
	{
		// Place new cars at random interval
		secondsUntilNextCar -= (float)delta;

		if (secondsUntilNextCar <= 0)
		{
			PlaceGlider(isFacingRight ? 0 : GetViewportRect().Size.X);

			secondsUntilNextCar = GapDist() / speed;
		}
	}


	// ---------- Private Util ----------

	void PlaceInitialGlidersLeft()
	{
		float xPos = initialOffset;
		while (xPos < GetViewportRect().Size.X)
		{
			PlaceGlider(xPos);
			xPos += GapDist();
		}

		secondsUntilNextCar = (xPos - GetViewportRect().Size.X) / speed;
	}

	void PlaceInitialGlidersRight()
	{
		float xPos = GetViewportRect().Size.X - initialOffset;
		while (xPos > 0)
		{
			PlaceGlider(xPos);
			xPos -= GapDist();
		}

		secondsUntilNextCar = Math.Abs(xPos) / speed;
	}

	void PlaceGlider(float pos)
	{
		Glider glider;
		glider = gliderScene.Instantiate<Glider>();
		glider.Init((isFacingRight ? Vector2.Right : Vector2.Left) * speed);

		if (isFacingRight)
		{
			glider.Position = new Vector2(pos - gliderWidth, yPos);
		}
		else
		{
			glider.Position = new Vector2(pos + gliderWidth, yPos);
			glider.ApplyScale(new Vector2(-1.0f, 1.0f)); // Mirror horizontally
		}

		gliderCount++;
		AddChild(glider);
	}

	float GapDist()
	{
		// Choose 'spallGap' or 'largeGap'
		if (gliderCount == largeGapFreq)
		{
			gliderCount = 0;
			return gliderWidth + largeGap;
		}
		else
		{
			return gliderWidth + smallGap;
		}
	}
}
