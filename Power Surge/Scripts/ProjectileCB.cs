using Godot;
using System;
using System.Security.Cryptography;

public partial class ProjectileCB : Area2D
{
	private String Direction;
	private bool DoMove = false;
	private float Speed = 300f;
	private AnimatedSprite2D ExplodeAnim;
	private Sprite2D Sprite;

	public override void _Ready()
	{
		ExplodeAnim = GetNode<AnimatedSprite2D>("Explosion");
		Sprite = GetNode<Sprite2D>("Sprite");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (DoMove)
		{
			if (Direction == "left")
				Position += new Vector2(-Speed * (float)delta, 0);
			else if (Direction == "right")
				Position += new Vector2(Speed * (float)delta, 0);
		}
	}

	public void Fire(String dir)
	{
		Direction = dir;
		DoMove = true;

	}

	public void OnBodyEntered(Node2D body)
{
    if (!body.IsInGroup("Enemy"))
    {
        Explode();
        if (body.Name == "Player")
        {
            if (body is PlayerMove player)
            {
                player.Hurt(20);
            }
        }
    }
}

	public void Explode()
	{
		DoMove = false;
		Sprite.Visible = false;
		ExplodeAnim.Visible = true;
		ExplodeAnim.Play();
	}

	public void OnExplodeAnimationFinished()
	{
		// Destroy self
		QueueFree();
	}

}