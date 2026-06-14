using Godot;

public partial class LilyPad : StaticBody2D
{
	[Export]
	bool flipH, flipV;
	
	public bool isUsed;
	
	public override void _Ready()
	{
		Sprite2D lilyPadSprite = GetNode<Sprite2D>("LilyPad");

		isUsed = false;

		if (flipH)
		{
			lilyPadSprite.Scale = new Vector2(-1.0f, 1.0f);
		}
		if (flipV)
		{
			lilyPadSprite.Scale = new Vector2(1.0f, -1.0f);
		}
	}

	public void ShowFrog()
	{
		Sprite2D frogSprite = GetNode<Sprite2D>("Frog");
		AudioStreamPlayer2D lilySplash = GetNode<AudioStreamPlayer2D>("LilySplash");

		isUsed = true;

		frogSprite.Visible = true;
		lilySplash.Play();
	}

	public void HideFrog()
	{
		Sprite2D frogSprite = GetNode<Sprite2D>("Frog");

		frogSprite.Visible = false;
		isUsed = false;
	}
}
