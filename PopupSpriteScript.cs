using Godot;

public class PopupSpriteScript : AnimatedSprite
{
    public float lifespan;

    public override void _Process(float delta)
    {
        if (this.Visible)
        {
            if ((this.lifespan -= delta) < 0)
            {
                this.QueueFree();
            }
        }
    }
}
