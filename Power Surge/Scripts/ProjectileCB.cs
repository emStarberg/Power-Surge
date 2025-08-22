using Godot;
//------------------------------------------------------------------------------
// <summary>
//   Projectile spawned when Circuit Bug attacks
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class ProjectileCB : Area2D
{
	private string direction; // Direction to fire in
    private bool doMove = false; // Whether to move
    private float speed = 300f; // Move speed of projectile
    private AnimatedSprite2D explodeAnim; // Animation to play when contact made
    private Sprite2D sprite; // Projectile sprite

    public override void _Ready()
    {
        explodeAnim = GetNode<AnimatedSprite2D>("Explosion");
        sprite = GetNode<Sprite2D>("Sprite");
    }

    public override void _Process(double delta)
    {
        if (doMove)
        {
            if (direction == "left")
                Position += new Vector2(-speed * (float)delta, 0);
            else if (direction == "right")
                Position += new Vector2(speed * (float)delta, 0);
        }
    }
	/// <summary>
	/// Fire projectile in given direction
	/// </summary>
	/// <param name="dir">Direction to move in</param>
    public void Fire(string dir)
    {
        direction = dir;
        doMove = true;
    }
	/// <summary>
	/// When collides with other object
	/// </summary>
	/// <param name="body">Object collided with</param>
    public void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Enemy"))
        {
            Explode();
            if (body.Name == "Player")
            {
                if (body is PlayerMove player)
                {
                    player.Hurt(20, 2f, 0.1f);
                }
            }
        }
    }
	/// <summary>
	/// Play explosion animation
	/// </summary>
    public void Explode()
    {
        doMove = false;
        sprite.Visible = false;
        explodeAnim.Visible = true;
        explodeAnim.Play();
    }
	// Destroy self after explosion
    public void OnExplodeAnimationFinished()
    {
        QueueFree();
    }
}