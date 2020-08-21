using Godot;
using System.Linq;

public class CoinScript : KinematicBody2D
{
    private KinematicBody2D _oldMan;
    private AnimatedSprite _ownSprite;
    private float _tAlive;
    public int Value { get; set; }

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this._oldMan = (KinematicBody2D)this.GetParent().FindNode("OldMan");
        this._ownSprite = this.GetChildren().Cast<Node>().Where(c => c is AnimatedSprite).Cast<AnimatedSprite>().First();
    }

    public override void _Process(float delta)
    {
        if (!this.Visible)
        {
            return;
        }

        base._Process(delta);
        this._tAlive += delta;
        float t = this._tAlive + 0.8f;
        this._ownSprite.Scale = Vector2.One * Mathf.Max(0.25f, 2 * t * t - (Mathf.Pow(t, 3)));
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!this.Visible)
        {
            return;
        }

        float t = this._tAlive;
        float speed = 32f - 20 * (2 * t * t - (Mathf.Pow(t, 3)));
        KinematicCollision2D collision = this.MoveAndCollide((this._oldMan.Position - this.Position).Normalized() * speed * delta);
        if (this._tAlive > 5 || (collision != null && collision.Collider == this._oldMan))
        {
            ((GameScript)this.GetParent()).Sounds[SoundName.Coin].Play();
            ((GameScript)this.GetParent()).oldManAnimationTimer = 1;
            this.QueueFree();
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
