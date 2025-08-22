using Godot;

public partial class ProjectileCB : Area2D
{
    private string direction;
    private bool doMove = false;
    private float speed = 300f;
    private AnimatedSprite2D explodeAnim;
    private Sprite2D sprite;

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

    public void Fire(string dir)
    {
        direction = dir;
        doMove = true;
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
                    player.Hurt(20, 2f, 0.1f);
                }
            }
        }
    }

    public void Explode()
    {
        doMove = false;
        sprite.Visible = false;
        explodeAnim.Visible = true;
        explodeAnim.Play();
    }

    public void OnExplodeAnimationFinished()
    {
        QueueFree();
    }
}