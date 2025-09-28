using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Stone bridge which crumbles when stood on
//   Made up of tiles that break after their neighbour breaks
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class StoneBridge : Node2D, IWorldObject
{
	private List<StaticBody2D> tiles = new List<StaticBody2D>();

	public override void _Ready()
	{
		// Add tiles to list
		foreach (Node node in GetChildren())
		{
			if (node is StaticBody2D tile)
			{
				tiles.Add(tile);
				var animation = tile.GetNode<AnimatedSprite2D>("Animation");
				animation.Frame = 0;
				animation.AnimationFinished += () => OnAnimationFinished(tile);
				animation.FrameChanged += () => OnAnimationFrameChanged(tile);
				var area = tile.GetNode<Area2D>("Player Detection");
				area.BodyEntered += (Node2D body) => OnBodyEntered(tile, body);
			}
		}
	}

	public override void _Process(double delta)
	{

	}

	public void OnBodyEntered(StaticBody2D tile, Node2D body)
	{
		if (body is Player player)
		{
			CrumbleTile(tile);
		}
	}

	public void OnAnimationFinished(StaticBody2D tile)
	{
		tile.QueueFree();
	}

	public void OnAnimationFrameChanged(StaticBody2D tile)
	{
		if (tile.GetNode<AnimatedSprite2D>("Animation").Frame == 7)
		{
			tile.GetNode<CollisionShape2D>("Collider").Disabled = true;

		}
		else if (tile.GetNode<AnimatedSprite2D>("Animation").Frame == 5)
		{

			int index = tiles.IndexOf(tile);

			// Check next tile
			if (index < tiles.Count - 1)
			{
				var nextTile = tiles[index + 1];
				if (IsInstanceValid(nextTile))
					CrumbleTile(nextTile);
			}

			// Check previous tile
			if (index > 0)
			{
				var prevTile = tiles[index - 1];
				if (IsInstanceValid(prevTile))
					CrumbleTile(prevTile);
			}
		}
	}

	/// <summary>
	/// Start tile crumble animation and sound
	/// </summary>
	/// <param name="tile">Tile to crumble</param>
	private void CrumbleTile(StaticBody2D tile)
	{
		tile.GetNode<AudioStreamPlayer2D>("Crumble").Play();
		tile.GetNode<AnimatedSprite2D>("Animation").Play();
	}

}
