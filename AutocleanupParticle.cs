using Godot;

public class AutocleanupParticle : CPUParticles2D
{
	public bool baseObject = true;

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (!this.baseObject && !this.Emitting)
		{
			this.QueueFree();
		}
	}
}
