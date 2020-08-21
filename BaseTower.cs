using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BaseTower : StaticBody2D
{
    public float cooldown;

    [Export]
    public bool baseObject = true;


    public float cooldownOverride;
    public float vengeanceTime;
    public float charmed;
    public bool doOverrideCooldown;

    public WeakReference<KinematicBody2D> target;

    private DamageType _lastDamageType;

    public System.Collections.Generic.Dictionary<ComponentType, TowerComponent> Components { get; } = new System.Collections.Generic.Dictionary<ComponentType, TowerComponent>();

    public AnimatedSprite VengeanceEffect { get; set; }
    public CPUParticles2D CharmedEffect { get; set; }

    public static Dictionary<DamageType, int> DamageFrames { get; } = new Dictionary<DamageType, int>()
    {
        [DamageType.Physical] = 77,
        [DamageType.Cold] = 211,
        [DamageType.Darkness] = 555,
        [DamageType.Fire] = 485,
        [DamageType.Light] = 607,
        [DamageType.Pure] = 861
    };

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.VengeanceEffect = this.GetChildren().Cast<Node>().Where(c => c is AnimatedSprite && c.Name.Contains("Vengeance")).Cast<AnimatedSprite>().First();
        this.CharmedEffect = this.GetChildren().Cast<Node>().Where(c => c is CPUParticles2D && c.Name.Contains("Charmed")).Cast<CPUParticles2D>().First();
    }

    public override void _Draw()
    {
        try
        {
            if (this.Components.ContainsKey(ComponentType.Gun) && this.Components[ComponentType.Gun] != null && this.Components[ComponentType.Gun].data is TCDGunLaser && this.target != null && this.target.TryGetTarget(out KinematicBody2D kb2d))
            {
                float f = Mathf.Sin(Mathf.Abs((((GameScript)this.GetParent()).playTime * 180) % 360));
                Color drawClr;
                switch (this._lastDamageType)
                {
                    case DamageType.Fire:
                    {
                        drawClr = new Color(1, f, 0);
                        break;
                    }

                    case DamageType.Cold:
                    {
                        drawClr = new Color(0, f * 0.1f, 0.5f);
                        break;
                    }

                    case DamageType.Darkness:
                    {
                        drawClr = new Color(0.3f, f * 0.1f, 0.3f);
                        break;
                    }

                    case DamageType.Light:
                    {
                        drawClr = new Color(1f, 1f, f * 0.8f);
                        break;
                    }

                    case DamageType.Physical: // What?
                    {
                        drawClr = new Color(0.7f, 0.7f, 0.7f);
                        break;
                    }

                    case DamageType.Pure:
                    {
                        drawClr = new Color(0.6f, 1f, 1f);
                        break;
                    }

                    default:
                    {
                        drawClr = new Color(0, 0, 0, 0);
                        break;
                    }
                }

                this.DrawLine(new Vector2(0, -9), kb2d.GlobalPosition - this.GlobalPosition, drawClr, 2);
            }

            if (this.Components.ContainsKey(ComponentType.Igniter) && this.Components[ComponentType.Igniter] != null && this.Components[ComponentType.Igniter].data is TCDIgniterLightning cap && cap.isActive)
            {
                this.DrawLine(Vector2.Zero, cap.targetPos - this.GlobalPosition, new Color(0, 0.75f, 1), 1);
            }
        }
        catch (ObjectDisposedException e)
        {
            this.target = null;
        }

        base._Draw();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (!this.baseObject)
        {
            this.vengeanceTime -= delta;
            this.charmed -= delta;
            try
            {
                this.Update();
                this.CharmedEffect.Emitting = this.charmed > float.Epsilon;
                if (this.target != null && !this.target.TryGetTarget(out KinematicBody2D kb2d))
                {
                    this.target = null;
                }

                if ((this.cooldown -= delta) <= 0)
                {
                    IEnumerable<TowerComponent> nonnulls = this.Components.Values.Where(c => c != null);
                    int nnc = nonnulls.Count();
                    if (nnc > 0)
                    {
                        float damage = nonnulls.Sum(d => d.damage);
                        foreach (float f in nonnulls.Select(n => n.damageMultiplier))
                        {
                            damage *= f;
                        }

                        if (this.vengeanceTime > float.Epsilon)
                        {
                            damage *= 0.5f;
                        }

                        float range = nonnulls.Sum(c => c.range);
                        foreach (float f in nonnulls.Select(n => n.rangeMultiplier))
                        {
                            range *= f;
                        }

                        DamageType dType = this.Components.ContainsKey(ComponentType.Gun) && this.Components[ComponentType.Gun] != null ? this.Components[ComponentType.Gun].damageTypeOverride : DamageType.Unspecified;
                        dType = this.Components.ContainsKey(ComponentType.Barrel) && this.Components[ComponentType.Barrel] != null ? this.Components[ComponentType.Barrel].damageTypeOverride : dType;
                        IEnumerable<TowerComponent> nonnullsnodamages = nonnulls.Where(c => c.componentType != ComponentType.Gun && c.componentType != ComponentType.Barrel && c.damageTypeOverride != DamageType.Unspecified);
                        if (nonnullsnodamages.Any())
                        {
                            dType = (DamageType)nonnullsnodamages.Select(c => c.damageTypeOverride).Max(v => (int)v);
                        }

                        if (!this.Components.ContainsKey(ComponentType.Base) || this.Components[ComponentType.Base] == null)
                        {
                            dType = DamageType.Unspecified;
                        }

                        if (this.charmed > float.Epsilon)
                        {
                            damage *= -0.1f;
                        }

                        if (dType != DamageType.Unspecified && damage > 0)
                        {
                            if (this.target == null || !this.target.TryGetTarget(out kb2d) || kb2d.Position.DistanceTo(this.Position) <= range || !(kb2d is EnemyScript enemy) || enemy.submerged > float.Epsilon)
                            {
                                KinematicBody2D t = ((GameScript)this.GetParent()).GetChildren().Cast<Node>().Where(c => c is EnemyScript).Cast<EnemyScript>().Where(es => !es.baseObject && es.submerged < float.Epsilon && es.Position.DistanceTo(this.Position) <= range).OrderBy(c => c.Position.DistanceTo(this.Position)).FirstOrDefault();
                                if (this.target == null && t != null)
                                {
                                    this.target = new WeakReference<KinematicBody2D>(t);
                                }

                                if (t != null)
                                {
                                    this.target.SetTarget(t);
                                }

                            }

                            if (this.target != null && this.target.TryGetTarget(out kb2d))
                            {
                                BulletScript bs = (BulletScript)((GameScript)this.GetParent()).baseBullet.Duplicate();
                                bs.Damages.Add(new System.Tuple<DamageType, float, TowerComponentDataBase>(dType, damage, this.Components[ComponentType.Gun].data));
                                bs.AllDamageDealers.AddRange(this.Components.Values.Where(c => c != null).Select(c => c.data));
                                bs.baseObject = false;
                                bs.Visible = true;
                                bs.Frame = DamageFrames[dType];
                                if (bs.Target == null)
                                {
                                    bs.Target = new WeakReference<Node2D>(kb2d);
                                }
                                else
                                {
                                    bs.Target.SetTarget(kb2d);
                                }

                                bs.Position = this.Position - new Vector2(0, 8);
                                this.GetParent().AddChild(bs);
                                this._lastDamageType = dType;
                                ((GameScript)this.GetParent()).Sounds[
                                    dType == DamageType.Cold ? SoundName.ShootCold :
                                    dType == DamageType.Darkness ? SoundName.ShootDark :
                                    dType == DamageType.Fire ? SoundName.ShootFlame :
                                    dType == DamageType.Light ? SoundName.ShootLight :
                                    dType == DamageType.Physical ? SoundName.ShootPhys : SoundName.DialogSkip].Play();
                                foreach (TowerComponent tc in nonnulls)
                                {
                                    tc.data.HandleBulletSpawn(bs, tc);
                                }

                                float cd = nonnulls.Sum(c => c.cooldown);
                                foreach (float f in nonnulls.Select(n => n.cooldownMultiplier))
                                {
                                    cd *= f;
                                }

                                this.cooldown = cd;
                                if (this.doOverrideCooldown)
                                {
                                    this.doOverrideCooldown = false;
                                    this.cooldown = this.cooldownOverride;
                                    this.cooldownOverride = 0;
                                }

                                this.Components[ComponentType.Gun].AnimateShot(this.cooldown);
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException ode) // Workaround for crashes even though weakref is used?
            {
                this.target = null;
            }
        }
    }
}
