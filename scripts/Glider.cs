using Godot;

public abstract partial class Glider : CharacterBody2D
{
    [Export]
    public float width { get; set; }

    public void Init(Vector2 velocity)
    {
        Velocity = velocity;
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }
}
