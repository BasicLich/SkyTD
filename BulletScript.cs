using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BulletScript : KinematicBody2D
{
    [Export]
    public bool baseObject = true;

    private AnimatedSprite _sprite;

    public List<Tuple<DamageType, float, TowerComponentDataBase>> Damages { get; } = new List<Tuple<DamageType, float, TowerComponentDataBase>>();
    public List<TowerComponentDataBase> AllDamageDealers { get; } = new List<TowerComponentDataBase>();

    public WeakReference<Node2D> Target { get; set; }
    public int Frame { get; set; }
    public float Speed { get; set; } = 1f;

    public override void _Ready()
    {
        this._sprite = this.GetChildren().Cast<Node>().Where(c => c is AnimatedSprite).Cast<AnimatedSprite>().First();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (!this.baseObject)
        {
            if (this._sprite.Frame != this.Frame)
            {
                this._sprite.Frame = this.Frame;
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (this.baseObject)
        {
            return;
        }

        try
        {
            if (this.Target == null || !this.Target.TryGetTarget(out Node2D target) || !(target is EnemyScript sc) || sc.submerged > float.Epsilon)
            {
                this.QueueFree();
                return;
            }

            Vector2 self2target = (target.Position - this.Position).Normalized() * 128 * this.Speed;
            KinematicCollision2D collision = this.MoveAndCollide(self2target * delta);
            if (collision != null && collision.Collider is EnemyScript es)
            {
                foreach (Tuple<DamageType, float, TowerComponentDataBase> dam in this.Damages)
                {
                    float d = dam.Item2;
                    DamageType dt = dam.Item1;
                    dam.Item3.HandleDamage(ref d, ref dt, es);
                    foreach (TowerComponentDataBase tcdb in this.AllDamageDealers)
                    {
                        tcdb.HandleAnyDamage(ref d, ref dt, es);
                    }

                    es.ApplyDamage(d, dt);
                }

                bool save = false;
                if (es.health <= 0)
                {
                    foreach (TowerComponentDataBase tcdb in this.AllDamageDealers)
                    {
                        tcdb.HandleKill(this, es, ref save);
                    }
                }

                if (!save)
                {
                    this.QueueFree();
                }
            }
        }
        catch (ObjectDisposedException ode)
        {
            this.Target = null;
        }
    }
}
