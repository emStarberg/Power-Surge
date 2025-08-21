using Godot;
using System;
using System.Security.Cryptography;

public partial class ProjectileCB : Area2D
{
	private String Direction;
	private bool DoMove = false;
	private float Speed = 300f;
	public override void _Ready()
	{
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

}