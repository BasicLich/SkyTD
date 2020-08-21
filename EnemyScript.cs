using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class EnemyScript : KinematicBody2D
{
    public static Color White = new Color(1, 1, 1);
    public static Color Red = new Color(1, 0, 0);
    public static Color Green = new Color(1, 0, 0);

    private int _pathIndex = 0;
    private float _rotationAnim = 0f;
    private float _damageAnim = 0f;
    private bool _rotationCW;
    public bool isBurning;
    public bool isInfected;
    public bool motherSpawned;

    private float _summonCooldown = 5;
    private float _undeadLordCooldown = 10;
    public bool isSummoner;
    public bool isMother;
    public bool isEndless;
    public bool isWealthy;
    public bool isGreedy;
    public float submerged;

    public DamageType absorbptionType;
    public float absorbptionEffect = 1;
    public float alphaExtraMS = 0;
    public float koboldTankMSCounter = 0;

    public float burnDPS;

    public EnemyDataBase data;
    public AnimatedSprite SpriteToAttach { get; set; }
    private CPUParticles2D _hitEffect;

    [Export]
    public float health;

    [Export]
    public bool baseObject;

    [Export]
    public float speed;

    public bool cursed;
    public bool undyingTriggered;
    public bool undying2Triggered;

    public Path2D path;
    public List<EnemyEffect> Effects { get; } = new List<EnemyEffect>();

    public AnimatedSprite SelfSprite { get; set; }
    public AnimatedSprite UndeadLordSprite { get; set; }
    public AnimatedSprite SirenCharmSprite { get; set; }
    public AnimatedSprite ResurrectionSprite { get; set; }

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (!this.baseObject)
        {
            foreach (Node n in this.GetChildren())
            {
                if (n is AnimatedSprite asp)
                {
                    if (asp.Name.Contains("Self"))
                    {
                        this.SelfSprite = asp;
                    }

                    if (asp.Name.Contains("UndeadLord"))
                    {
                        this.UndeadLordSprite = asp;
                    }

                    if (asp.Name.Contains("Siren"))
                    {
                        this.SirenCharmSprite = asp;
                    }

                    if (asp.Name.Contains("Resurrection"))
                    {
                        this.ResurrectionSprite = asp;
                    }
                }

                if (n is CPUParticles2D parts)
                {
                    this._hitEffect = parts;
                }
            }

            if (this.data is DataSiren)
            {
                this.SirenCharmSprite.Visible = true;
                ((AnimationPlayer)this.SirenCharmSprite.GetChildren()[0]).Play("idle");
            }

            this.SelfSprite.Frame = this.data.Frame;
            this.health = this.data.Health;
            this.speed = this.data.Speed;
            this.Visible = true;
            if (this.SpriteToAttach != null)
            {
                this.SelfSprite.AddChild(this.SpriteToAttach);
            }
        }
    }

    public void ResetPosition()
    {
        if (this._pathIndex == 0)
        {
            return;
        }
        else
        {
            this.Position = this.path.Curve.GetBakedPoints()[this._pathIndex - 1];
        }
    }

    public void Resurrect()
    {
        this.ResurrectionSprite.Visible = true;
        ((AnimationPlayer)this.ResurrectionSprite.GetChildren()[0]).Play("trigger");
        ((GameScript)this.GetParent()).Sounds[SoundName.EnemyResurrect].Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!this.baseObject)
        {
            base._PhysicsProcess(delta);
            if (this.path != null)
            {
                Vector2 target = this.path.Curve.GetBakedPoints()[this._pathIndex];
                if (this.Position.DistanceTo(target) < 9f)
                {
                    this._pathIndex += 1;
                    this._pathIndex %= this.path.Curve.GetBakedPoints().Length;
                    if (this.data is DataTrueKoboldTank)
                    {
                        this.koboldTankMSCounter = 0;
                        this.UndeadLordSprite.Visible = true;
                        ((AnimationPlayer)this.UndeadLordSprite.GetChildren()[0]).Play("explode");
                        GameScript gs = (GameScript)this.GetParent();
                        gs.Sounds[SoundName.UndeadLordExplode].Play();
                        foreach (BaseTower bt in gs.GetChildren().Cast<Node>().Where(c => c is BaseTower).Cast<BaseTower>())
                        {
                            if (!bt.baseObject && bt.Position.DistanceTo(this.Position) <= 48f)
                            {
                                bt.cooldown = 6;
                            }
                        }
                    }
                }

                float speedmod = this.data is DataUndeadLord ? this._undeadLordCooldown < 1 ? this._undeadLordCooldown : 1 : this.data is DataTrueKoboldTank ? this.koboldTankMSCounter > 6 ? 4 : 0 : 1;
                this.MoveAndCollide((target - this.Position).Normalized() * (this.speed + this.alphaExtraMS) * delta * speedmod);
                if (this.alphaExtraMS > 0)
                {
                    this.alphaExtraMS -= delta;
                }
                else
                {
                    this.alphaExtraMS = 0;
                }
            }
        }
    }

    public void AddEffect(EnemyEffect eff)
    {
        this.Effects.Add(eff);
        eff.Apply(this);
    }

    public void RemoveEffect(EnemyEffect eff)
    {
        this.Effects.Remove(eff);
        eff.Retract(this);
    }

    public override void _Draw()
    {
    }

    private Random _rand = new Random();

    public void ApplyDamage(float damage, DamageType dt, bool triggerAnimation = true)
    {
        if (this.cursed && this._rand.NextDouble() < 0.1)
        {
            this.ApplyDamage(damage, dt, triggerAnimation);
        }

        float resistance = this.data.Resistances.ContainsKey(dt) ? this.data.Resistances[dt] : 1f;
        damage *= resistance;
        this.Effects.ForEach(e => e.HandleDamage(ref damage, ref dt, this));
        GameScript gameScript = ((GameScript)this.GetParent());
        gameScript.totalDamage += damage;
        gameScript.maxDamage = Mathf.Max(gameScript.maxDamage, damage);
        if (this.data is DataTrueKoboldTank && this.koboldTankMSCounter >= 5)
        {
            damage = dt == DamageType.Pure ? damage : 0;
        }

        this.health -= damage;
        if (triggerAnimation && damage > float.Epsilon)
        {
            this._damageAnim = 0.2F;
            this._hitEffect.Visible = true;
            this._hitEffect.OneShot = true;
            this._hitEffect.Color = this.data.BloodColor;
            this._hitEffect.Emitting = true;
        }

        if (this.data is DataAlpha)
        {
            this.alphaExtraMS += 1;
        }

        if (this.health <= 0)
        {
            gameScript.numEnemies -= 1;
            CoinScript kb2d = (CoinScript)gameScript.coin.Duplicate();
            kb2d.Position = this.Position;
            kb2d.Visible = true;
            kb2d.Value = this.data.Value * (this.isWealthy ? 2 : 1);
            gameScript.coinValue += kb2d.Value;
            this.GetParent().AddChild(kb2d);
            gameScript.Sounds[SoundName.EnemyDead].Play();
            if (this.data is DataTheEnd5)
            {
                gameScript.Win();
            }

            if (this.isMother && !this.motherSpawned)
            {
                gameScript.Sounds[SoundName.Mother].Play();
                for (int i = 0; i < this._rand.Next(3, 7); ++i)
                {
                    EnemyScript child = gameScript.SpawnEnemy(this.path, new DataSpiderling(), false);
                    child._pathIndex = this._pathIndex;
                    child.Position = this.Position + (Vector2.Up.Rotated((float)(this._rand.NextDouble() * 2 * Math.PI) * this._rand.Next(3, 8)));
                    child.motherSpawned = true;
                }
            }
            if (!this.data.IsUnlocked)
            {
                this.data.IsUnlocked = true;
                PersistentDataStorage.Save();
            }

            this.QueueFree();
        }
        else
        {
            if (triggerAnimation)
            {
                gameScript.Sounds[SoundName.EnemyHurt].Play();
            }
        }
    }

    public bool HandleDeathAreaEntry()
    {
        if (this.isGreedy)
        {
            GameScript gs = (GameScript)this.GetParent();
            if (!gs.treasureStolen)
            {
                gs.treasureStolen = true;
                if (!this.isEndless)
                {
                    return true;
                }
            }
        }

        if (this.isEndless)
        {
            GameScript gs = (GameScript)this.GetParent();
            gs.TakeLife();
            this.Position = this.path.Curve.GetBakedPoints()[0];
            this._pathIndex = 0;
            return true;
        }

        return false;
    }

    public Color BlendClr(Color c1, Color c2, float a)
    {
        float ainv = 1 - a;
        return new Color(c1.r * a + c2.r * ainv, c1.g * a + c2.g * ainv, c1.b * a + c2.b * ainv);
    }

    private float _upsExisted;

    public override void _Process(float delta)
    {
        if (!this.baseObject)
        {
            this.Effects.ForEach(e => e.HandleTick(delta, this));
            this._upsExisted += delta;
            this.submerged -= delta;
            if (this._damageAnim > -0.1f)
            {
                this._damageAnim -= delta;
                Color c = this.data.ColorMod;
                this.Effects.ForEach(e => e.ModifyColor(ref c, this));
                this.Modulate = this.BlendClr(this.data.ColorMod, Red, Mathf.Min(1, 1 - this._damageAnim * 5f));
            }
            else
            {
                if (this.isInfected)
                {
                    Color c = this.data.ColorMod;
                    this.Effects.ForEach(e => e.ModifyColor(ref c, this));
                    this.Modulate = this.BlendClr(this.data.ColorMod, new Color(0.5f, 0, 0.5f), Mathf.Sin(this._upsExisted % 360));
                }
                else
                {
                    if (this.submerged > float.Epsilon)
                    {
                        Color c = this.data.ColorMod;
                        this.Effects.ForEach(e => e.ModifyColor(ref c, this));
                        this.Modulate = this.BlendClr(this.data.ColorMod, new Color(this.data.ColorMod.r, this.data.ColorMod.g, this.data.ColorMod.b, 0.1f), 1f - this.submerged / 2f);
                    }
                }
            }

            if (this._rotationCW)
            {
                this._rotationAnim += delta * this.speed;
                if (this._rotationAnim > 5F)
                {
                    this._rotationCW = false;
                }
            }
            else
            {
                this._rotationAnim -= delta * this.speed;
                if (this._rotationAnim < -5F)
                {
                    this._rotationCW = true;
                }
            }

            this.Rotation = this.ToRad(this._rotationAnim);
            if (this.isBurning)
            {
                this.ApplyDamage(this.burnDPS * delta, DamageType.Fire, false);
            }

            if (this.isInfected && this._rand.NextDouble() < delta)
            {
                this.ApplyDamage(this.health / 2f, DamageType.Pure, false);
            }

            if (this.isSummoner)
            {
                if ((this._summonCooldown -= delta) <= 0)
                {
                    this._summonCooldown = 5;
                    GameScript gs = ((GameScript)this.GetParent());
                    EnemyScript child = gs.SpawnEnemy(this.path, new DataSkeleton(), false);
                    child._pathIndex = this._pathIndex;
                    child.Position = this.Position + Vector2.Up.Rotated((float)(this._rand.NextDouble() * 2 * Math.PI)) * this._rand.Next(3, 8);
                    gs.Sounds[SoundName.NecromancerRaise].Play();
                }
            }

            if (this.data is DataSiren)
            {
                GameScript gs = (GameScript)this.GetParent();
                foreach (BaseTower bt in gs.GetChildren().Cast<Node>().Where(c => c is BaseTower).Cast<BaseTower>())
                {
                    if (!bt.baseObject && bt.Position.DistanceTo(this.Position) <= 48f)
                    {
                        bt.charmed = 0.5f;
                    }
                }
            }

            if (this.data is DataUndeadLord)
            {
                if ((this._undeadLordCooldown -= delta) <= 0)
                {
                    this._undeadLordCooldown = 10;
                    this.UndeadLordSprite.Visible = true;
                    ((AnimationPlayer)this.UndeadLordSprite.GetChildren()[0]).Play("explode");
                    GameScript gs = (GameScript)this.GetParent();
                    gs.Sounds[SoundName.UndeadLordExplode].Play();
                    foreach (BaseTower bt in gs.GetChildren().Cast<Node>().Where(c => c is BaseTower).Cast<BaseTower>())
                    {
                        if (!bt.baseObject && bt.Position.DistanceTo(this.Position) <= 64f)
                        {
                            bt.cooldown = 4;
                        }
                    }
                }
            }

            if (this.data is DataTrueKoboldTank)
            {
                this.koboldTankMSCounter += delta;
            }
        }
    }

    public float ToRad(float deg) => (float)(deg * (Math.PI / 180F));
}

public abstract class EnemyEffect
{
    public static List<Tuple<float, float, uint, EnemyEffect>> Effects { get; } = new List<Tuple<float, float, uint, EnemyEffect>>()
    {
        new Tuple<float, float, uint, EnemyEffect>(0.4f, -0.025f, 1, new EffectTough()),
        new Tuple<float, float, uint, EnemyEffect>(0.3f, -0.0125f, 1, new EffectTough2()),
        new Tuple<float, float, uint, EnemyEffect>(0.2f, 0.0f, 1, new EffectTough3()),
        new Tuple<float, float, uint, EnemyEffect>(0.1f, 0.0125f, 1, new EffectTough4()),
        new Tuple<float, float, uint, EnemyEffect>(0.0f, 0.025f, 1, new EffectTough5()),
        new Tuple<float, float, uint, EnemyEffect>(0.4f, -0.025f, 2, new EffectNimble()),
        new Tuple<float, float, uint, EnemyEffect>(0.3f, -0.0125f, 2, new EffectNimble2()),
        new Tuple<float, float, uint, EnemyEffect>(0.2f, 0.0f, 2, new EffectNimble3()),
        new Tuple<float, float, uint, EnemyEffect>(0.1f, 0.0125f, 2, new EffectNimble4()),
        new Tuple<float, float, uint, EnemyEffect>(0.0f, 0.025f, 2, new EffectNimble5()),
        new Tuple<float, float, uint, EnemyEffect>(0.4f, -0.025f, 4, new EffectQuick()),
        new Tuple<float, float, uint, EnemyEffect>(0.3f, -0.0125f, 4, new EffectQuick2()),
        new Tuple<float, float, uint, EnemyEffect>(0.2f, 0.0f, 4, new EffectQuick3()),
        new Tuple<float, float, uint, EnemyEffect>(0.1f, 0.0125f, 4, new EffectQuick4()),
        new Tuple<float, float, uint, EnemyEffect>(0.0f, 0.025f, 4, new EffectQuick5()),
        new Tuple<float, float, uint, EnemyEffect>(0.0f, 0.025f, 6, new EffectUndying()),
        new Tuple<float, float, uint, EnemyEffect>(0.0f, 0.0125f, 6, new EffectUndying2()),
        new Tuple<float, float, uint, EnemyEffect>(0.2f, 0.0f, 0, new EffectUndead()),
        new Tuple<float, float, uint, EnemyEffect>(0.1f, 0.0f, 0, new EffectSummoner()),
        new Tuple<float, float, uint, EnemyEffect>(0.1f, 0.025f, 0, new EffectIncorporeal()),
        new Tuple<float, float, uint, EnemyEffect>(0.15f, 0.025f, 8, new EffectAbsorbption()),
        new Tuple<float, float, uint, EnemyEffect>(0f, 0.035f, 8, new EffectAbsorbption2()),
        new Tuple<float, float, uint, EnemyEffect>(0.1f, 0.0f, 0, new EffectMother()),
        new Tuple<float, float, uint, EnemyEffect>(0.2f, 0.0f, 0, new EffectDarkDweller()),
        new Tuple<float, float, uint, EnemyEffect>(0.2f, 0.0125f, 0, new EffectVengeance()),
        new Tuple<float, float, uint, EnemyEffect>(0.3f, 0.025f, 0, new EffectWealthy()),
        new Tuple<float, float, uint, EnemyEffect>(0.05f, 0.0125f, 0, new EffectGreedy()),
    };

    public abstract string Name { get; }
    public abstract float PurpleValue { get; }
    public abstract float RedValue { get; }

    public virtual void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
    }

    public virtual void ModifyColor(ref Color defaultColor, EnemyScript holder)
    {
    }

    public virtual void HandleTick(float delta, EnemyScript holder)
    {
    }

    public abstract void Apply(EnemyScript holder);
    public abstract void Retract(EnemyScript holder);
}

public class EffectTough : EnemyEffect
{
    public override string Name => "TOUGHNESS";
    public override float PurpleValue => 0.1f;
    public override float RedValue => 0;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure)
        {
            damage *= 0.85f;
        }
    }
}
public class EffectTough2 : EnemyEffect
{
    public override string Name => "TOUGHNESS II";
    public override float PurpleValue => 0.18f;
    public override float RedValue => 0.05f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure)
        {
            damage *= 0.70f;
        }
    }
}
public class EffectTough3 : EnemyEffect
{
    public override string Name => "TOUGHNESS III";
    public override float PurpleValue => 0.23f;
    public override float RedValue => 0.08f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure)
        {
            damage *= 0.5f;
        }
    }
}
public class EffectTough4 : EnemyEffect
{
    public override string Name => "TOUGHNESS IV";
    public override float PurpleValue => 0.26f;
    public override float RedValue => 0.1f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure)
        {
            damage *= 0.35f;
        }
    }
}
public class EffectTough5 : EnemyEffect
{
    public override string Name => "TOUGHNESS V";
    public override float PurpleValue => 0.3f;
    public override float RedValue => 0.12f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure)
        {
            damage *= 0.1f;
        }
    }
}

public class EffectNimble : EnemyEffect
{
    public override string Name => "NIMBLINESS I";
    public override float PurpleValue => 0.04f;
    public override float RedValue => 0f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    private Random _rand = new Random();
    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure && this._rand.NextDouble() < 0.1f)
        {
            damage = 0;
        }
    }
}
public class EffectNimble2 : EnemyEffect
{
    public override string Name => "NIMBLINESS II";
    public override float PurpleValue => 0.09f;
    public override float RedValue => 0f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    private Random _rand = new Random();
    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure && this._rand.NextDouble() < 0.2f)
        {
            damage = 0;
        }
    }
}
public class EffectNimble3 : EnemyEffect
{
    public override string Name => "NIMBLINESS III";
    public override float PurpleValue => 0.12f;
    public override float RedValue => 0.02f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    private Random _rand = new Random();
    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure && this._rand.NextDouble() < 0.3f)
        {
            damage = 0;
        }
    }
}
public class EffectNimble4 : EnemyEffect
{
    public override string Name => "NIMBLINESS IV";
    public override float PurpleValue => 0.17f;
    public override float RedValue => 0.05f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    private Random _rand = new Random();
    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure && this._rand.NextDouble() < 0.4f)
        {
            damage = 0;
        }
    }
}
public class EffectNimble5 : EnemyEffect
{
    public override string Name => "NIMBLINESS V";
    public override float PurpleValue => 0.25f;
    public override float RedValue => 0.1f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    private Random _rand = new Random();
    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt != DamageType.Pure && this._rand.NextDouble() < 0.5f)
        {
            damage = 0;
        }
    }
}

public class EffectQuick : EnemyEffect
{
    public override string Name => "QUICKNESS I";
    public override float PurpleValue => 0.06f;
    public override float RedValue => 0f;
    public override void Apply(EnemyScript holder) => holder.speed *= 1.25f;
    public override void Retract(EnemyScript holder) => holder.speed /= 1.25f;
}
public class EffectQuick2 : EnemyEffect
{
    public override string Name => "QUICKNESS II";
    public override float PurpleValue => 0.13f;
    public override float RedValue => 0.01f;
    public override void Apply(EnemyScript holder) => holder.speed *= 1.5f;
    public override void Retract(EnemyScript holder) => holder.speed /= 1.5f;
}
public class EffectQuick3 : EnemyEffect
{
    public override string Name => "QUICKNESS III";
    public override float PurpleValue => 0.18f;
    public override float RedValue => 0.03f;
    public override void Apply(EnemyScript holder) => holder.speed *= 2;
    public override void Retract(EnemyScript holder) => holder.speed /= 2f;
}
public class EffectQuick4 : EnemyEffect
{
    public override string Name => "QUICKNESS IV";
    public override float PurpleValue => 0.27f;
    public override float RedValue => 0.06f;
    public override void Apply(EnemyScript holder) => holder.speed *= 3f;
    public override void Retract(EnemyScript holder) => holder.speed /= 3f;
}
public class EffectQuick5 : EnemyEffect
{
    public override string Name => "QUICKNESS V";
    public override float PurpleValue => 0.37f;
    public override float RedValue => 0.1f;
    public override void Apply(EnemyScript holder) => holder.speed *= 5;
    public override void Retract(EnemyScript holder) => holder.speed /= 5f;
}

public class EffectUndying : EnemyEffect
{
    public override string Name => "UNDYING";
    public override float PurpleValue => 0.3f;
    public override float RedValue => 0.1f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        if (holder.health - damage <= 0 && !holder.undyingTriggered)
        {
            holder.undyingTriggered = true;
            holder.health = holder.data.Health;
            damage = 0;
            holder.Resurrect();
        }
    }

    public override void Retract(EnemyScript holder)
    {
    }
}
public class EffectUndying2 : EnemyEffect
{
    public override string Name => "UNDYING II";
    public override float PurpleValue => 0.4f;
    public override float RedValue => 0.2f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        if (holder.health - damage <= 0)
        {
            if (!holder.undyingTriggered)
            {
                holder.undyingTriggered = true;
                holder.health = holder.data.Health;
                damage = 0;
                holder.Resurrect();
            }
            else
            {
                if (!holder.undying2Triggered)
                {
                    holder.undying2Triggered = true;
                    holder.health = holder.data.Health;
                    damage = 0;
                    holder.Resurrect();
                }
            }
        }
    }

    public override void Retract(EnemyScript holder)
    {
    }
}
public class EffectUndead : EnemyEffect
{
    public override string Name => "UNDEAD";
    public override float PurpleValue => 0.1f;
    public override float RedValue => 0.02f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt == DamageType.Cold || dt == DamageType.Fire)
        {
            damage *= 0.75f;
        }
    }

    public override void HandleTick(float delta, EnemyScript holder)
    {
        base.HandleTick(delta, holder);
        if (holder.isBurning)
        {
            holder.isBurning = false;
        }
    }
}
public class EffectSummoner : EnemyEffect
{
    public override string Name => "SUMMONER";
    public override float PurpleValue => 0;
    public override float RedValue => 0;
    public override void Apply(EnemyScript holder) => holder.isSummoner = true;
    public override void Retract(EnemyScript holder) => holder.isSummoner = false;
}
public class EffectIncorporeal : EnemyEffect
{
    public override string Name => "INCORPOREAL";
    public override float PurpleValue => 0.15f;
    public override float RedValue => 0.05f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt == DamageType.Physical)
        {
            damage = 0;
        }
    }
}
public class EffectAbsorbption : EnemyEffect
{
    public override string Name => "ABSORBPTION I";
    public override float PurpleValue => 0.1f;
    public override float RedValue => 0.03f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (holder.absorbptionType == dt)
        {
            damage *= holder.absorbptionEffect;
            holder.absorbptionEffect /= 1.25f;
        }
        else
        {
            holder.absorbptionType = dt;
            holder.absorbptionEffect = 1;
        }
    }
}
public class EffectAbsorbption2 : EnemyEffect
{
    public override string Name => "ABSORBPTION II";
    public override float PurpleValue => 0.33f;
    public override float RedValue => 0.16f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (holder.absorbptionType == dt)
        {
            damage *= holder.absorbptionEffect;
            holder.absorbptionEffect = 0;
        }
        else
        {
            holder.absorbptionType = dt;
            holder.absorbptionEffect = 1;
        }
    }
}
public class EffectMother : EnemyEffect
{
    public override string Name => "MOTHER";
    public override float PurpleValue => 0.2f;
    public override float RedValue => 0.1f;

    public override void Apply(EnemyScript holder) => holder.isMother = true;
    public override void Retract(EnemyScript holder) => holder.isMother = false;
}
public class EffectEndless : EnemyEffect
{
    public override string Name => "ENDLESS";
    public override float PurpleValue => 0;
    public override float RedValue => 0;

    public override void Apply(EnemyScript holder) => holder.isEndless = true;
    public override void Retract(EnemyScript holder) => holder.isEndless = false;
}
public class EffectDarkDweller : EnemyEffect
{
    public override string Name => "DARK DWELLER";
    public override float PurpleValue => 0.1f;
    public override float RedValue => 0.02f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        if (dt == DamageType.Darkness)
        {
            damage *= 0.75f;
        }
    }

    public override void HandleTick(float delta, EnemyScript holder)
    {
        base.HandleTick(delta, holder);
        if (holder.cursed)
        {
            holder.cursed = false;
        }
    }
}
public class EffectVengeance : EnemyEffect
{
    public override string Name => "VENGEANCE";
    public override float PurpleValue => 0.25f;
    public override float RedValue => 0.13f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder)
    {
        base.HandleDamage(ref damage, ref dt, holder);
        GameScript gs = (GameScript)holder.GetParent();
        foreach (BaseTower bt in gs.GetChildren().Cast<Node>().Where(c => c is BaseTower).Cast<BaseTower>())
        {
            if (!bt.baseObject && bt.Position.DistanceTo(holder.Position) <= 32f)
            {
                bt.VengeanceEffect.Visible = true;
                ((AnimationPlayer)bt.VengeanceEffect.GetChildren()[0]).Play("trigger");
                bt.vengeanceTime = 1.5f;
            }
        }
    }
}
public class EffectWealthy : EnemyEffect
{
    public override string Name => "WEALTHY";
    public override float PurpleValue => 0f;
    public override float RedValue => 0f;

    public override void Apply(EnemyScript holder)
    {
        holder.health *= 10;
        holder.isWealthy = true;
    }

    public override void Retract(EnemyScript holder) => holder.isWealthy = false;
}
public class EffectGreedy : EnemyEffect
{
    public override string Name => "GREEDY";
    public override float PurpleValue => 0.05f;
    public override float RedValue => 0;

    public override void Apply(EnemyScript holder) => holder.isGreedy = true;
    public override void Retract(EnemyScript holder) => holder.isGreedy = false;
}
public class EffectSubmerged : EnemyEffect
{
    public override string Name => "SUBMERGED";
    public override float PurpleValue => 0.1f;
    public override float RedValue => 0f;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript holder) => holder.submerged = 1;

    public override void Retract(EnemyScript holder)
    {
    }
}

public class EffectUndeadLord : EnemyEffect
{
    public override string Name => "???";
    public override float PurpleValue => 0;
    public override float RedValue => 0;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }
}
public class EffectTheAlpha : EnemyEffect
{
    public override string Name => "???";
    public override float PurpleValue => 0;
    public override float RedValue => 0;

    public override void Apply(EnemyScript holder)
    {
    }

    public override void Retract(EnemyScript holder)
    {
    }
}

public abstract class EnemyDataBase
{
    private bool _unlocked;
    public abstract Guid PersistentID { get; }
    public abstract float Speed { get; }
    public abstract float Health { get; }
    public abstract int Frame { get; }
    public abstract int Value { get; }
    public abstract string Name { get; }
    public abstract string BestiaryName { get; }
    public abstract string BestiaryDescription { get; }
    public abstract Color ColorMod { get; }
    public abstract Color BloodColor { get; }
    public virtual Dictionary<DamageType, float> Resistances { get; } = new Dictionary<DamageType, float>();
    public virtual EnemyEffect[] Effect { get; } = new EnemyEffect[0];
    public virtual bool IsUnlocked
    {
        get => PersistentDataStorage.PersistentEnemyDatas[this.PersistentID]._unlocked;
        set => PersistentDataStorage.PersistentEnemyDatas[this.PersistentID]._unlocked = value;
    }
}

public class DataSkeleton : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("9b4ffde7-28a6-4a50-820d-d7e2253fe64f");
    public override float Speed => 6F;
    public override float Health => 5F;
    public override int Frame => 213;
    public override int Value => 1;
    public override string Name => "Skeleton {Undead}";
    public override string BestiaryName => "SKELETON";
    public override string BestiaryDescription => "AH, POOR JORRICK, HOW I KNEW THEE \n THE REMAINS OF DEAD ADVENTURERS CAN BE FOUND IN MANY PLACES, AND IT SEEMS LIKE YOUR ARCH-NEMESIS ABUSED THIS FACT AND THE POOR SKELLIES TO CREATE A TEMPORARY ARMY TO DISTRACT YOU WHILE THEIR MAIN FORCES ARE ON THEIR WAY. CONTRARY TO POPULAR BELIEF SKELETONS DO FEEL PAIN AND HAVE FEELINGS AND EMOTIONS. \n" +
        "\n" +
        "UNDEAD: THIS IS AN UNDEAD CREATURE. IT WILL TAKE LESS DAMAGE FROM DIRECT AND ELEMENTAL ATTACKS AND IS IMMUNE TO BURNING.\n" +
        "\n" +
        "SPEED: EXTREMELY SLOW\n" +
        "HEALTH: EXTREMEPLY POOR\n" +
        "VALUE: THE POOREST\n" +
        "SLIGHTLY WEAK TO LIGHT";
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectUndead() };
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Light] = 1.25f
    };

    public override Color BloodColor => new Color(1, 1, 1);

    public override Guid PersistentID => GID;
}

public class DataElSkello : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("5bab62e7-7e5f-4383-91df-566795e0aeb7");
    public override Guid PersistentID => GID;

    public override float Speed => 10F;
    public override Color BloodColor => new Color(1, 1, 1);
    public override float Health => 10F;
    public override int Frame => 219;
    public override int Value => 2;
    public override string Name => "El Skello {Undead}";
    public override string BestiaryName => "EL SKELLO";
    public override string BestiaryDescription => "A TOUGH FELLA, WAS A SPECIAL KIND OF ROGUE BEFORE HIS AFTERLIFE, THE KIND THAT DOESN'T PICK POCKETS OR LOCKS, BUT PICKS THE MINDS OF THEIR OWN CRIMINAL ORGANIZATION. DUE TO THAT THEIR LIFESPANS ARE NOT VERY LONG BUT THEY DO KEEP AN OPTIMISTIC OUTLOOK, ESPECIALLY NOW IN THE AFTERLIFE \n" +
        "\n" +
        "UNDEAD: THIS IS AN UNDEAD CREATURE. IT WILL TAKE LESS DAMAGE FROM DIRECT AND ELEMENTAL ATTACKS AND IS IMMUNE TO BURNING.\n" +
        "\n" +
        "SPEED: VERY SLOW\n" +
        "HEALTH: VERY POOR\n" +
        "VALUE: EXTREMELY POOR\n" +
        "SLIGHTLY WEAK TO LIGHT";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectUndead() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Light] = 1.25f
    };
}

public class DataSlashetor : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("c7225a69-9a10-4d87-b3fc-013329d2ee46");
    public override Guid PersistentID => GID;
    public override float Speed => 16f;
    public override Color BloodColor => new Color(1, 1, 1);
    public override float Health => 10F;
    public override int Frame => 124;
    public override int Value => 2;
    public override string Name => "Slashetor {Undead}";
    public override string BestiaryName => "SLASHETOR";
    public override string BestiaryDescription => "THIS SKELETON BOASTING QUITE AN UNUSUAL NAME AND A QUITE UNUSUAL SET OF TWO CURVED BLADES SEEMS TO HAVE ORIGINATED FROM A NEARBY DESERT. HIS EYES BURN WITH PASSION AND HE SEEMS READY TO RECLAIM HIS LONG LOST LEGACY. HOWEVER IT SEEMS THAT HIS ADVENTURE WAS INTERRUPTED AS HIS SANDS WERE STOLEN BY YOUR ARCH-NEMESIS WHO IS NOW USING HIM AND THEM AGAINST YOU. POOR FELLOW WILL NEVER TRULY BE DEAD AS THE SANDS WILL BRING HIM BACK TO LIFE EVERY TIME HE DIES. \n" +
        "\n" +
        "UNDEAD: THIS IS AN UNDEAD CREATURE. IT WILL TAKE LESS DAMAGE FROM DIRECT AND ELEMENTAL ATTACKS AND IS IMMUNE TO BURNING.\n" +
        "\n" +
        "SPEED: SLIGHTLY SLOW\n" +
        "HEALTH: VERY POOR\n" +
        "VALUE: EXTREMELY POOR\n" +
        "SLIGHTLY WEAK TO LIGHT.";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectUndead() };

    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Light] = 1.25f
    };
}

public class DataNecromancer : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("b72c6873-2cef-46ae-9a68-75a7b3949df1");
    public override Guid PersistentID => GID;
    public override float Speed => 8f;
    public override Color BloodColor => new Color(0.4f, 0, 0.3f);
    public override float Health => 28F;
    public override int Frame => 118;
    public override int Value => 5;
    public override string Name => "Necromancer {Summoner} {Rebirth}";
    public override string BestiaryName => "NECROMANCER";
    public override string BestiaryDescription => "A WICKED FELLOW WHO GOT VERY UPSET OVER THE ACTIONS THAT HAPPENED IN THE PAST - HE GOT OUTED BY HIS OWN TO A TOWN, WHO PROCEEDED TO BURN BOTH HIM AND THE LEADER. HE RESURRECTED HIMSELF LATER OF COURSE. AND SO DID THE LEADER. NOW HE IS IN SEARCH OF VENGEANCE AGAINST THE WITCH THAT SPOILED HIS PLANS. WHAT IS HE DOING HERE? HE IS UNSURE HIMSELF BUT YOU AND YOUR CASTLE INTEREST HIM SOMEHOW. \n" +
        "\n" +
        "SUMMONER: THIS IS A SUMMONER. EVERY 5 SECONDS LEFT ALIVE IT WILL SUMMON A SKELETON ON THE SAME TILE.\n" +
        "REBIRTH: THIS CREATURE IS ABLE TO AVOID DEATH ONCE FULLY RECOVERING HP IN THE PROCESS.\n" +
        "\n" +
        "SPEED: VERY SLOW\n" +
        "HEALTH: BELOW AVERAGE\n" +
        "VALUE: VERY POOR";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectSummoner(), new EffectUndying() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
}

public class DataOgreSkeleton : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("4c6edfad-9f87-4625-8efd-93b888df70c3");
    public override Guid PersistentID => GID;
    public override float Speed => 6f;
    public override Color BloodColor => new Color(1, 1, 1);
    public override float Health => 60F;
    public override int Frame => 165;
    public override int Value => 8;
    public override string Name => "Ogre Skeleton {Undead}";
    public override string BestiaryName => "OGRE SKELETON";
    public override string BestiaryDescription => "WHILE A LESSER KNOWN FACT THIS SKELETON IN FACT DID NOT BELONG TO A MYTHICAL CREATURE KNOWN AS AN OGRE. INSTEAD THE BESTIARY AUTHOR SIMPLY MISSPELLED GORE. THE CREATURE DOESN'T EVEN RESEMBLE A HUMANOID MUCH - IT IS JUST A COLLECTION OF RANDOM BONES CRUDLY SCREWED TOGETHER. AS A REGULAR SKELETON IT IS LIKELY A THINKING FEELING CREATURE, AND SO ARE ALL OTHER CREATURES THAT MAKE IT UP. \n" +
        "\n" +
        "UNDEAD: THIS IS AN UNDEAD CREATURE. IT WILL TAKE LESS DAMAGE FROM DIRECT AND ELEMENTAL ATTACKS AND IS IMMUNE TO BURNING.\n" +
        "\n" +
        "SPEED: EXTREMELY SLOW\n" +
        "HEALTH: NORMAL\n" +
        "VALUE: PRETTY POOR\n" +
        "SLIGHTLY WEAK TO LIGHT\n" +
        "MODERATELY RESISTANT TO PHYSICAL";
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Light] = 1.25f,
        [DamageType.Physical] = 0.65f
    };
}

public class DataGhost : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("d796782a-745a-44bf-9987-36fb83a0f46f");
    public override Guid PersistentID => GID;
    public override float Speed => 32f;
    public override Color BloodColor => new Color(1, 1, 1, 0.5f);
    public override float Health => 5F;
    public override int Frame => 308;
    public override int Value => 10;
    public override string Name => "Ghost {Undead} {Incorporeal}";
    public override string BestiaryName => "GHOST";
    public override string BestiaryDescription => "THESE DON'T LIKE TO BE IN THE OPEN AND USUALLY PREFER TO HAUNT CASTLES. WHICH REALLY EXPLAINS A LOT IF YOU THINK ABOUT IT. THE CREATURE ITSELF IS EXTREMELY FRIENDLY AND PLAYFUL HOWEVER FATE BE CRUEL AS THE CREATURE IS UNABLE TO INTERACT WITH THE PHYSICAL WORLD WHICH MAKES IT VERY VERY SAD." +
        "\n" +
        "UNDEAD: THIS IS AN UNDEAD CREATURE. IT WILL TAKE LESS DAMAGE FROM DIRECT AND ELEMENTAL ATTACKS AND IS IMMUNE TO BURNING.\n" +
        "INCORPOREAL: THIS CREATURE IS INCORPOREAL. IT IS UNABLE TO INTERACT WITH THE PHYSICAL WORLD AND VICE VERSA. PHYSICAL ATTACKS CAN'T DAMAGE THIS CREATURE.\n" +
        "\n" +
        "SPEED: ABOVE AVERAGE\n" +
        "HEALTH: VERY POOR\n" +
        "VALUE: SLIGHTLY POOR\n" +
        "MODERATELY WEAK TO LIGHT\n" +
        "IMMUNE TO PHYSICAL\n" +
        "VERY RESISTANT TO COLD\n" +
        "MODERATELY RESISTANT TO FIRE\n" +
        "RECOVERS FROM DARKNESS";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectIncorporeal(), new EffectUndead() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Light] = 1.5f,
        [DamageType.Physical] = 0f,
        [DamageType.Cold] = 0.2f,
        [DamageType.Fire] = 0.6f,
        [DamageType.Darkness] = -0.2f
    };
}

public class DataWraith : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("1c2aaf2a-0162-40f5-bc9a-7506328ed043");
    public override Guid PersistentID => GID;
    public override float Speed => 32f;
    public override Color BloodColor => new Color(1, 1, 1, 0.5f);
    public override float Health => 10F;
    public override int Frame => 310;
    public override int Value => 16;
    public override string Name => "Wraith {Undead} {Incorporeal}";
    public override string BestiaryName => "WRAITH";
    public override string BestiaryDescription => "UNLIKE A PREVIOUSLY MENTIONED GHOST THIS CREATURE IS NOT FRIENDLY AT ALL. IF GHOSTS HAD BULLIES THIS ONE PROBABLY WOULD BE A DEFINITION OF ONE. USUALLY NOT HUMANOIDS IN THEIR PREVIOUS LIVES THEY DO TEND TO SHAPE INTO ONE IF ONLY TO GAIN TRUST OF OTHER GHOSTS. SOMETIMES THEY EVEN GET AFRAID OF THEMSELVES. \n" +
        "\n" +
        "UNDEAD: THIS IS AN UNDEAD CREATURE. IT WILL TAKE LESS DAMAGE FROM DIRECT AND ELEMENTAL ATTACKS AND IS IMMUNE TO BURNING.\n" +
        "INCORPOREAL: THIS CREATURE IS INCORPOREAL. IT IS UNABLE TO INTERACT WITH THE PHYSICAL WORLD AND VICE VERSA. PHYSICAL ATTACKS CAN'T DAMAGE THIS CREATURE.\n" +
        "\n" +
        "SPEED: ABOVE AVERAGE\n" +
        "HEALTH: VERY POOR\n" +
        "VALUE: SLIGHTLY POOR\n" +
        "MODERATELY WEAK TO LIGHT\n" +
        "IMMUNE TO PHYSICAL\n" +
        "VERY RESISTANT TO COLD\n" +
        "MODERATELY RESISTANT TO FIRE\n" +
        "RECOVERS FROM DARKNESS";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectIncorporeal(), new EffectUndead() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Light] = 1.5f,
        [DamageType.Physical] = 0f,
        [DamageType.Cold] = 0.2f,
        [DamageType.Fire] = 0.6f,
        [DamageType.Darkness] = -0.2f
    };
}

public class DataUndeadLord : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("335b0132-59db-4490-b5cc-3cdaa64dd3f4");
    public override Guid PersistentID => GID;
    public override float Speed => 10f;
    public override Color BloodColor => new Color(1, 1, 1);
    public override float Health => 100F;
    public override int Frame => 313;
    public override int Value => 1000;
    public override string Name => "Undead Lord {???} {Undead} {Absorbption} {Greater Rebirth} {Mother} {Endless}";
    public override string BestiaryName => "UNDEAD LORD";
    public override string BestiaryDescription => "A TRUE KING TO RULE ALL OF THE UNDEATH. WELL NOW THEY ARE ANYWAY. AS YOU SEE WHEN YOUVE PLACED YOUR FIRST TOWER THEY WERE BUT A REGULAR SKELETON. BUT WHILE THEIR BUDDIES WERE BUSY DYING TO YOUR TOWERS THIS ONE WAS BUSY LIFTING WEIGHTS AND DOING EXCERCISES. LOOK WHERE THIS GOT THEM. YOU ARE LUCKY YOUR NEMESIS CALLED THEM INTO THE FREY RIGHT NOW OR IN A FEW MONTHS THIS ONE WOULD BE ABLE TO DESTROY YOUR ENTIRE CASTLE WITH A SINGLE PUNCH! \n" +
        "\n" +
        "HEAVY SMASH: UNDEAD LORD IS CAPABLE OF CREATING SUCH A POWERFUL BLOW THAT IT TEMPORARY KNOCKS ALL TURRETS OUT IN A MEDIUM RANGE.\n" +
        "UNDEAD: THIS IS AN UNDEAD CREATURE. IT WILL TAKE LESS DAMAGE FROM DIRECT AND ELEMENTAL ATTACKS AND IS IMMUNE TO BURNING.\n" +
        "ABSORBPTION: THIS CREATURE IS CAPABLE OF ADAPTING TO THE SAME ATTACKS. IF HIT WITH THE SAME TYPE OF ATTACK IT WILL GRADUALLY GAIN RESISTANCE TO IT WHICH CAPS AT 100%\n" +
        "GREATER REBIRTH: THIS CREATURE IS ABLE TO SURVIVE A HIT THAT WOULD OTHERWISE HIT IT AND RECOVER TO FULL HEALTH TWICE\n" +
        "MOTHER: THIS CREATURE HOSTS A NEST OF SPIDER INSIDE OF THEM. IF THEY DIE SOME SPIDERLINGS WILL BE SPAWNED\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: VERY SLOW\n" +
        "HEALTH: PRETTY GOOD\n" +
        "VALUE: EXTREMELY RICH\n" +
        "EXTREMELY WEAK TO LIGHT\n" +
        "VERY RESISTANT TO PHYSICAL\n" +
        "VERY RESISTANT TO COLD\n" +
        "MODERATELY RESISTANT TO FIRE\n" +
        "IMMUNE TO DARKNESS";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectUndeadLord(), new EffectUndead(), new EffectAbsorbption(), new EffectUndying2(), new EffectMother(), new EffectEndless() };
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Light] = 2f,
        [DamageType.Physical] = 0.2f,
        [DamageType.Cold] = 0.2f,
        [DamageType.Fire] = 0.5f,
        [DamageType.Darkness] = 0f
    };
}

public class DataSpiderman : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("4b1c0d60-c1eb-4a67-933c-b5a6acfb7b60");
    public override Guid PersistentID => GID;
    public override float Speed => 18f;
    public override Color BloodColor => new Color(0.4f, 0.6f, 0);
    public override float Health => 12F;
    public override int Frame => 122;
    public override int Value => 3;
    public override string Name => "Spiderman {Dark Dweller}";
    public override string BestiaryName => "SPIDERMAN";
    public override string BestiaryDescription => "A HORRIFIC MUTATION MADE IN THE HORRIBLE EXPERIMENTS OF THE ACURSED WITCH - HALF CRAB AND HALF PERSON THIS CREATURE IS TRULY AN ABOMINATION. WHY IS IT CALLED A SPIDERMAN? DO YOU REALLY WANT TO KNOW? TRUTH OF THE MATTER IS - THEY ARE LONG DEAD INSIDE BUT THERE IS OTHER LIFE IN THEM STILL WAITING TO BE RELEASED. \n" +
        "\n" +
        "DARK DWELLER: THIS CREATURE DWELLS IN THE DEEPEST DARKEST DUNGEONS. IT CANT BE CURSED AND IS RESISTANT TO DARK\n" +
        "\n" +
        "SPEED: BELOW AVERAGE\n" +
        "HEALTH: VERY POOR\n" +
        "VALUE: EXTREMELY POOR\n" +
        "MODERATELY RESISTANT TO DARKNESS\n" +
        "SLIGHTLY WEAK TO LIGHT\n" +
        "SLIGHTLY WEAK TO FIRE\n";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectDarkDweller() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Darkness] = 0.5f,
        [DamageType.Light] = 1.1f,
        [DamageType.Fire] = 1.2f
    };
}

public class DataSpiderling : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("bfa71661-bd0a-4b79-974f-171e5c0d1c65");
    public override Guid PersistentID => GID;
    public override float Speed => 18f;
    public override Color BloodColor => new Color(0.4f, 0.6f, 0);
    public override float Health => 16F;
    public override int Frame => 264;
    public override int Value => 3;
    public override string Name => "Spiderling {Dark Dweller}";
    public override string BestiaryName => "SPIDERLING";
    public override string BestiaryDescription => "THE WORST NIGHTMARE OF PRETTY MUCH ANYONE THIS SPIDER RIVALS A HORSE IN SIZE AND MAKES HORRIFYING NOISES. IT IS ALSO A CARNIVORE. \n" +
        "\n" +
        "DARK DWELLER: THIS CREATURE DWELLS IN THE DEEPEST DARKEST DUNGEONS. IT CANT BE CURSED AND IS RESISTANT TO DARK\n" +
        "\n" +
        "SPEED: BELOW AVERAGE\n" +
        "HEALTH: PRETTY POOR\n" +
        "VALUE: EXTREMELY POOR\n" +
        "MODERATELY RESISTANT TO DARKNESS\n" +
        "SLIGHTLY WEAK TO LIGHT\n" +
        "SLIGHTLY WEAK TO FIRE\n";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectDarkDweller() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Darkness] = 0.5f,
        [DamageType.Light] = 1.1f,
        [DamageType.Fire] = 1.2f
    };
}

public class DataBigSpider : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("c48be9b0-56a6-49d5-9e05-d8fbf64d4242");
    public override Guid PersistentID => GID;
    public override float Speed => 14f;
    public override Color BloodColor => new Color(0.4f, 0.6f, 0);
    public override float Health => 26F;
    public override int Frame => 263;
    public override int Value => 5;
    public override string Name => "Armored Spider {Dark Dweller} {Mother}";
    public override string BestiaryName => "ARMORED SPIDER";
    public override string BestiaryDescription => "WHOEVER RULES THOSE DEEPEST DARKEST DUNGEONS MUST BE ONE HELL OF A CRAZY FELLOW AS THEYVE MANAGED TO CATCH ONE OF THE SPIDERLINGS AND MOUNT A FULL SUIT OF PLAIT ARMOR UPON THEM! NOW THIS IS A SIEGE BREAKER. HOWEVER THE ADDED ARMOR MAKES THE SPIDER UNABLE TO ACTUALLY PERFORM THEIR SPIDERY DUTIES SUCH AS ENWEB AND DIGEST THEIR PREY. WOULD HONESTLY PREFER THIS MONSTER TO A REGULAR SPIDERLING EVERY DAY OF THE WEEK. \n" +
        "\n" +
        "DARK DWELLER: THIS CREATURE DWELLS IN THE DEEPEST DARKEST DUNGEONS. IT CANT BE CURSED AND IS RESISTANT TO DARK\n" +
        "MOTHER: THIS CREATURE HOSTS A NEST OF SPIDER INSIDE OF THEM. IF THEY DIE SOME SPIDERLINGS WILL BE SPAWNED\n" +
        "\n" +
        "SPEED: SLIGHTLY POOR\n" +
        "HEALTH: BELOW AVERAGE\n" +
        "VALUE: VERY POOR\n" +
        "MODERATELY RESISTANT TO PHYSICAL\n" +
        "MODERATELY RESISTANT TO DARKNESS\n" +
        "SLIGHTLY WEAK TO LIGHT\n" +
        "SLIGHTLY WEAK TO FIRE\n";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectDarkDweller(), new EffectMother() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.5f,
        [DamageType.Darkness] = 0.5f,
        [DamageType.Light] = 1.1f,
        [DamageType.Fire] = 1.2f
    };
}

public class DataSkelepider : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("763be497-959e-4f18-977e-0f1d3f57c6d1");
    public override Guid PersistentID => GID;
    public override float Speed => 11f;
    public override Color BloodColor => new Color(1, 1, 1);
    public override float Health => 40F;
    public override int Frame => 262;
    public override int Value => 8;
    public override string Name => "Skelepider {Undead} {Dark Dweller}";
    public override string BestiaryName => "SKELEPIDER";
    public override string BestiaryDescription => "AS IF A REGULAR SPIDERLING WAS NOT BAD ENOUGH THE NECROMANCER HAVE DECIDED IT WOULD BE A SICK PRANK TO RESURRECT SOME SPIDERS IN THEIR HOMETOWN. WELL THEY DID THAT AND THEN SOME AND THESE CREATURES ARE AN UNFORTUNATE RESULT OF THEIR WRONGDOINGS. A CURIOUS QUESTION WOULD BE WHETHER ITS INTELLIGENCE ACTUALLY IMPROVED AFTER DEATH OR NOT\n" +
        "\n" +
        "DARK DWELLER: THIS CREATURE DWELLS IN THE DEEPEST DARKEST DUNGEONS. IT CANT BE CURSED AND IS RESISTANT TO DARK\n" +
        "UNDEAD: THIS IS AN UNDEAD CREATURE. IT WILL TAKE LESS DAMAGE FROM DIRECT AND ELEMENTAL ATTACKS AND IS IMMUNE TO BURNING.\n" +
        "\n" +
        "SPEED: SLIGHTLY POOR\n" +
        "HEALTH: AVERAGE\n" +
        "VALUE: VERY POOR\n" +
        "MODERATELY RESISTANT TO PHYSICAL\n" +
        "MODERATELY RESISTANT TO DARKNESS\n" +
        "MODERATELY WEAK TO LIGHT\n" +
        "SLIGHTLY WEAK TO FIRE\n";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectDarkDweller(), new EffectUndead() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.5f,
        [DamageType.Darkness] = 0.5f,
        [DamageType.Light] = 1.6f,
        [DamageType.Fire] = 1.2f
    };
}

public class DataAlpha : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("0b49ddff-0f83-46f5-81e9-46ea61492603");
    public override Guid PersistentID => GID;
    public override float Speed => 8f;
    public override Color BloodColor => new Color(0.4f, 0.6f, 0);
    public override float Health => 300F;
    public override int Frame => 262;
    public override int Value => 1000;
    public override string Name => "The Alpha {???} {Dark Dweller} {Rebirth} {Vengeance} {Endless}";
    public override string BestiaryName => "THE ALPHA";
    public override string BestiaryDescription => "THIS CREATURE HAS MANY NAMES - THE ALPHA, THE OMEGA, NIGHTCRAWLER, THE FIRST DARKNESS. IT USUALLY RESIDES AT THE BOTTOM OF THE DEEPEST DARKEST DUNGEONS AS A BOSS MONSTER BUT NOW IT IS HERE WHICH IS GOOD SINCE THEY GET DISADVANTAGE ON THEIR ATTACK AND SAVE THROWS WHEN EXPOSED TO SUNLIGHT. UNFORTUNATELY THIS ONLY APPLIES TO THEIR DND CHARACTER.\n" +
        "\n" +
        "ALPHA INSTINCTS: THE ALPHA GETS A SLIGHT SPEED BOOST EVERY TIME IT TAKES DAMAGE. IT STACKS. THERE IS NO LIMIT.\n" +
        "REBIRTH: THIS CREATURE WILL WITHSTAND AN ATTACK THAT WOULD OTHERWISE KILL IT AND RETURN TO FULL HEALTH ONCE.\n" +
        "VENGEANCE: THIS CREATURE WILL RETALIATE VERSUS NEARBY TOWERS WHEN ATTACKED, REDUCING THEIR DAMAGE FOR A SHORT AMOUNT OF TIME\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: VERY POOR\n" +
        "HEALTH: VERY GOOD\n" +
        "VALUE: EXTREMELY RICH\n" +
        "VERY RESISTANT TO PHYSICAL\n" +
        "VERY RESISTANT TO DARKNESS\n" +
        "VERY WEAK TO LIGHT\n" +
        "MODERATELY WEAK TO FIRE\n" +
        "SLIGHTLY RESISTANT TO COLD\n";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectTheAlpha(), new EffectUndying(), new EffectVengeance(), new EffectEndless() };
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.25f,
        [DamageType.Darkness] = 0.25f,
        [DamageType.Light] = 1.75f,
        [DamageType.Fire] = 1.5f,
        [DamageType.Cold] = 0.9f
    };
}

public class DataKobold : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("9118cdd7-bd1a-4e8d-bdf5-8143f0e410b2");
    public override Guid PersistentID => GID;
    public override float Speed => 20F;
    public override Color BloodColor => new Color(0.1f, 0.6f, 0.3f);
    public override float Health => 30F;
    public override int Frame => 123;
    public override int Value => 1;
    public override string Name => "Kobold";
    public override string BestiaryName => "KOBOLD";
    public override string BestiaryDescription => "THE FIRST VANGUARD OF YOUR NEMESIS THESE CREATURES ARE THE BOTTOM OF THE BARREL AS FAR AS CREATURES GO. NOBODY EVEN BOTHERED TO TEACH IT HOW TO SPEAK SO IT JUST BLABBERS GIBBERISH WHEREEVER IT GOES. IT WAS ALSO NOT TAUGHT OTHER MANNERS AND SOCIAL NORMS. WHERE DID YOUR NEMESIS EVEN GET THIS ONE?\n" +
        "\n" +
        "SPEED: AVERAGE\n" +
        "HEALTH: AVERAGE\n" +
        "VALUE: THE POOREST";
    public override Color ColorMod => new Color(1, 1, 1, 1);
}

public class DataGremlin : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("16a9be94-0ed8-4e96-8016-f772151c1756");
    public override Guid PersistentID => GID;
    public override float Speed => 32F;
    public override Color BloodColor => new Color(0.1f, 0.6f, 0.3f);
    public override float Health => 30f;
    public override int Frame => 172;
    public override int Value => 25;
    public override string Name => "Gremlin {Wealthy} {Greedy}";
    public override string BestiaryName => "GREMLIN";
    public override string BestiaryDescription => "GREMLINS AND KOBOLDS ARE COMPLETELY DIFFERENT SPECIES - WHERE AS KOBOLDS DIG IN THE MUD FOR COPPER PENNIES GREMLINS USUALLY RESIDE ATOP OF MOUNTAINS OF GOLD AND TREASURE USING A CLEVER DRAGON ILLUSION TO DISSUADE ADVENTURERS FROM STEALING. OR SOMETIMES THEY JUST HIRE A REGULAR DRAGON. THEY SPEND THEIR LONG LIVES ACCUMULATING MORE WEALTH. THEY DON'T EVEN SLEEP AS EVERY MINUTE WAISTED IS A HUGE DEAL FOR A GREMLIN.\n" +
        "\n" +
        "WEALTHY: THIS CREATURE CARRIES MORE GOLD THAN OTHERS BUT ITS HEALTH IS MASSIVELY INCREASED\n" +
        "GREEDY: THIS CREATURE WILL TAKE ALL CHESTS FOR THIS WAVE INSTEAD OF LIVES.\n" +
        "\n" +
        "SPEED: ABOVE AVERAGE\n" +
        "HEALTH: AVERAGE\n" +
        "VALUE: SLIGHTLY POOR\n";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectWealthy(), new EffectGreedy() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
}

public class DataKoboldPeasant : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("a48ee89e-0de1-4c0c-9c52-cfd23d683fa2");
    public override Guid PersistentID => GID;
    public override float Speed => 18F;
    public override Color BloodColor => new Color(0.1f, 0.6f, 0.3f);
    public override float Health => 80F;
    public override int Frame => 121;
    public override int Value => 2;
    public override string Name => "Kobold Peasant {Vengeance}";
    public override string BestiaryName => "KOBOLD PEASANT";
    public override string BestiaryDescription => "UNLIKE THE SPECIMEN YOU SAW BEFORE THIS ONE ACTUALLY CAN TALK! AND HAS STUFF TO DEFEND THEMSELVES WITH! TOO BAD PITCHFORKS DONT PROTECT AGAINST BULLETS AND LAZERS. ALTHOUGH THEY DID BRING THEIR WIFE ALONG WITH THEM.\n" +
        "\n" +
        "VENGEANCE: THIS CREATURE WILL RETALIATE VERSUS NEARBY TOWERS WHEN ATTACKED, REDUCING THEIR DAMAGE FOR A SHORT AMOUNT OF TIME\n" +
        "\n" +
        "SPEED: BELOW AVERAGE\n" +
        "HEALTH: SLIGHTLY GOOD\n" +
        "VALUE: EXTREMELY POOR\n";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectVengeance() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
}

public class DataKoboldTank : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("a0cb489f-a504-46ce-be25-e7374e9f06fc");
    public override Guid PersistentID => GID;
    public override float Speed => 20F;
    public override Color BloodColor => new Color(0.1f, 0.6f, 0.3f);
    public override float Health => 130F;
    public override int Frame => 120;
    public override int Value => 5;
    public override string Name => "Kobold Tank";
    public override string BestiaryName => "KOBOLD TANK";
    public override string BestiaryDescription => "THIS ONE SAW WHAT THE UNDEAD LORD DID AND TRIED TO DO THE SAME. UNFORTUNATELY THE LACK OF SELF-DISCIPLINE AND RESPONSIBILITY MADE SURE THEIR EFFORTS WERE IN VAIN. AS IF TO MAKE THEM MORE OF A LAUGHING STOCK THE OTHER KOBOLDS CALLED THEM A TANK - AS IN A TANK TOP, VERY LIGHT AND EASILY TORN STUFF THAT KOBOLDS ONLY TEND TO WEAR TO SHOW OTHERS HOW MUCH THEY DESPISE THEM.\n" +
        "\n" +
        "SPEED: AVERAGE\n" +
        "HEALTH: GOOD\n" +
        "VALUE: VERY POOR\n";
    public override Color ColorMod => new Color(1, 1, 1, 1);
}

public class DataBugbear : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("1183b19b-1922-4d0e-80e0-feb20de62cf2");
    public override Guid PersistentID => GID;
    public override float Speed => 16F;
    public override Color BloodColor => new Color(0.1f, 0.6f, 0.3f);
    public override float Health => 200f;
    public override int Frame => 448;
    public override int Value => 10;
    public override string Name => "Bugbear";
    public override string BestiaryName => "BUGBEAR";
    public override string BestiaryDescription => "THIS COBOLD LIVES DND. LOOK - THEY EVEN BROUGHT EVERYTHING WITH THEM - THEIR CHARACTER SHEET THE RULEBOOK A LOF OF PEN AND PAPER SOME DICE AND... IS THAT A TABLE? UNFORTUNATELY IT FORGOT TO BRING THE MOST IMPORTANT PART OF THE WHOLE ADVENTURE - THE DUNGEON MASTER. OR ACTUALLY THEY DIDNT. THE DM JUST HAPPENED TO ARRIVE EARLIER TODAY AND YOU HAVE KILLED THEM ALREADY INSISTING THAT THEIR SPEED BOOST FROM DAMAGE IS UNREASONABLE.\n" +
        "\n" +
        "SPEED: BELOW AVERAGE\n" +
        "HEALTH: PRETTY GOOD\n" +
        "VALUE: PRETTY POOR\n" +
        "SLIGHTLY RESISTANT TO PHYSICAL\n";
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.8f
    };
}

public class DataKnobold : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("8a620822-0df7-4747-af0d-049e33023c84");
    public override Guid PersistentID => GID;
    public override float Speed => 16F;
    public override Color BloodColor => new Color(0.1f, 0.6f, 0.3f);
    public override float Health => 300F;
    public override int Frame => 31;
    public override int Value => 20;
    public override string Name => "Knobold {Absorbption}";
    public override string BestiaryName => "KNOBOLD";
    public override string BestiaryDescription => "WHILE OTHERS LOOK AT A KOBOLD IN A KNIGHTLY ARMOR WITH AWE AND INSPIRE EVERYONE AROUND THEM TO TAKE A LOOK WHAT THEY DONT KNOW IS THAT THE CREATURE UNDERNEATH THAT ARMOR IS JUST USING THE ARMOR TO HIDE AWAY THE FACT THAT ITS HANDS HAVE BEEN TURNED INTO DOOR KNOBS BY AN EVIL WITCH.\n" +
        "\n" +
        "ABSORBPTION: THIS CREATURE IS CAPABLE OF ADAPTING TO THE SAME ATTACKS. IF HIT WITH THE SAME TYPE OF ATTACK IT WILL GRADUALLY GAIN RESISTANCE TO IT WHICH CAPS AT 100%\n" +
        "\n" +
        "SPEED: BELOW AVERAGE\n" +
        "HEALTH: VERY GOOD\n" +
        "VALUE: BELOW AVERAGE\n" +
        "RESISTANT TO PHYSICAL\n" +
        "MODERATELY WEAK TO COLD\n";
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectAbsorbption() };
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.5f,
        [DamageType.Cold] = 1.5f
    };
}

public class DataTrueKoboldTank : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("bef76ad6-08f5-4ed8-9cf5-32aa64dd4bd7");
    public override Guid PersistentID => GID;
    public override float Speed => 20F;
    public override Color BloodColor => new Color(0.6f, 0.6f, 0.6f);
    public override float Health => 800F;
    public override int Frame => 951;
    public override int Value => 1000;
    public override string Name => "Kobold Tank {???}";
    public override string BestiaryName => "TRUE KOBOLD TANK";
    public override string BestiaryDescription => "NOW THIS CREATURE... IS NOT ACTUALLY A CREATURE. IT IS AN AUTOMATON THAT MOVES IN SHORT BURSTS HAVING TO REST BETWEEN EACH ONE. THE KOBOLDS WHO MADE IT STRAPPED A SHIELD GENERATOR THEY FOUND IN THE JUNKYARD ATOF OF IT BUT IT DOESNT EVEN WORK AS INTENDED ONLY GIVING A SHIELD DURING THE CHARGE AND NOT INBETWEEN. LIKELY THEY MESSED UP AN IF STATEMENT SOMEWHERE WHILE PROGRAMMING THAT SHIELD.\n" +
        "\n" +
        "ABSORBPTION: THIS CREATURE IS CAPABLE OF ADAPTING TO THE SAME ATTACKS. IF HIT WITH THE SAME TYPE OF ATTACK IT WILL GRADUALLY GAIN RESISTANCE TO IT WHICH CAPS AT 100%\n" +
        "FULL SPEED AHEAD: WHEN THE TANK FINISHES A MOVE EVERY TOWER IN A SMALL RADIUS AROUND IT WILL BE STUNNED\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: AVERAGE\n" +
        "HEALTH: GRAMD\n" +
        "VALUE: EXTREMELY RICH\n" +
        "IMMUNE TO PHYSICAL\n" +
        "EXTREMELY RESISTANT TO FIRE\n" +
        "EXTREMELY RESISTANT TO COLD";
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectEndless() };
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0,
        [DamageType.Fire] = 0.1f,
        [DamageType.Cold] = 0.1f
    };
}

public class DataCrab : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("cf7e4dfa-b132-43cf-9f00-b52e6ae40f7f");
    public override Guid PersistentID => GID;
    public override float Speed => 24F;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 10F;
    public override int Frame => 260;
    public override int Value => 5;
    public override string Name => "Crab {Swimmer}";
    public override string BestiaryName => "CRAB";
    public override string BestiaryDescription => "AN ADVENTURER THAT WAS LURED BY A SIREN. NOW AT LEAST YOU KNOW WHAT HAPPENS TO THOSE WHO WOULD TRY TO SPEND A NIGHT WITH A SIREN.\n" +
        "\n" +
        "SWIMMER: THIS CREATURE ARRIVES AND MOVES THROUGH WATER.\n" +
        "\n" +
        "SPEED: AVERAGE\n" +
        "HEALTH: VERY POOR\n" +
        "VALUE: VERY POOR\n" +
        "SLIGHTLY RESISTANT TO PHYSICAL\n" +
        "MODERATELY RESISTANT TO FIRE\n" +
        "INSANELY WEAK TO COLD";
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.8f,
        [DamageType.Fire] = 0.5f,
        [DamageType.Cold] = 2f
    };
}

public class DataEel : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("cdde7e67-7288-4d82-834e-53189d781c9b");
    public override Guid PersistentID => GID;
    public override float Speed => 48F;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 10F;
    public override int Frame => 404;
    public override int Value => 10;
    public override string Name => "Eel {Swimmer}";
    public override string BestiaryName => "EEL";
    public override string BestiaryDescription => "THIS WAS ONCE A WIZARD OF GREAT RENOWN WHO WAS LURED IN BY A SIREN. THE WIZARD HAD ATTEMTED A GREATEST MAGICAL TRICK KNOWN TO MAN. LOOKS LIKE IT WORKED.\n" +
        "\n" +
        "SWIMMER: THIS CREATURE ARRIVES AND MOVES THROUGH WATER.\n" +
        "\n" +
        "SPEED: FAST\n" +
        "HEALTH: VERY POOR\n" +
        "VALUE: SLIGHTLY POOR\n" +
        "SLIGHTLY RESISTANT TO PHYSICAL\n" +
        "MODERATELY RESISTANT TO FIRE\n" +
        "INSANELY WEAK TO COLD";
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.8f,
        [DamageType.Fire] = 0.5f,
        [DamageType.Cold] = 2f
    };
}

public class DataCrocco : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("6e666556-faec-4e05-b09a-ae5a38d66695");
    public override Guid PersistentID => GID;
    public override float Speed => 20F;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 50F;
    public override int Frame => 405;
    public override int Value => 20;
    public override string Name => "Crocco {Swimmer}";
    public override string BestiaryName => "CROCCO";
    public override string BestiaryDescription => "A GREAT HERO OF THE PAST WHO WAS LURED IN BY A SIREN. FOR THEM IT ALL STARTED WHEN THEY MISREAD THE BOUNTY AND WELL LOOKS LIKE THEIR MOTTO OF NOT TAKING OFF THEIR ARMOR EVER CONTINUES EVEN NOW.\n" +
        "\n" +
        "SWIMMER: THIS CREATURE ARRIVES AND MOVES THROUGH WATER.\n" +
        "\n" +
        "SPEED: AVERAGE\n" +
        "HEALTH: ABOVE AVERAGE\n" +
        "VALUE: AVERAGE\n" +
        "SLIGHTLY RESISTANT TO PHYSICAL\n" +
        "MODERATELY RESISTANT TO FIRE\n" +
        "INSANELY WEAK TO COLD";
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.8f,
        [DamageType.Fire] = 0.5f,
        [DamageType.Cold] = 2f
    };
}

public class DataSquid : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("f4d0da6b-d833-433e-a1ff-c995b41039ce");
    public override Guid PersistentID => GID;
    public override float Speed => 32F;
    public override Color BloodColor => new Color(0f, 0f, 0f);
    public override float Health => 40F;
    public override int Frame => 401;
    public override int Value => 30;
    public override string Name => "Squid {Swimmer} {Submerge}";
    public override string BestiaryName => "SQUID";
    public override string BestiaryDescription => "UNLIKE ALL OTHER SERVANTS OF THE SIREN THIS ONE WAS NEVER A HUMAN AND WAS BORN IN THIS FORM. LOOKS LIKE THE SIREN FOUND THEM IN THE WILD AND DECIDED TO KEEP THEM AS A PET.\n" +
        "\n" +
        "SWIMMER: THIS CREATURE ARRIVES AND MOVES THROUGH WATER.\n" +
        "SUBMERGE: THIS CREATURE WILL HIDE UNDERWATER FOR A MOMENT UPON BEING ATTACKED.\n" +
        "\n" +
        "SPEED: ABOVE AVERAGE\n" +
        "HEALTH: ABOVE AVERAGE\n" +
        "VALUE: AVERAGE\n" +
        "SLIGHTLY RESISTANT TO PHYSICAL\n" +
        "MODERATELY RESISTANT TO FIRE\n" +
        "INSANELY WEAK TO COLD";
    public override Color ColorMod => new Color(1, 1, 1, 1);
    public override EnemyEffect[] Effect => new EnemyEffect[] { new EffectSubmerged() };
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.8f,
        [DamageType.Fire] = 0.5f,
        [DamageType.Cold] = 2f
    };
}

public class DataSiren : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("a087354c-c79e-4b45-93b6-92e42086b6b1");
    public override Guid PersistentID => GID;
    public override float Speed => 50F;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 400F;
    public override int Frame => 454;
    public override int Value => 1000;
    public override string Name => "Siren {???} {Swimmer} {Endless}";
    public override string BestiaryName => "SIREN";
    public override string BestiaryDescription => "WELL HERE SHE BE HERSELF IN THE FLESH - NOT QUITE AS THE SEAFARERS TELL IN THEIR STORIES. THE QUEEN OF THE SEA GOT VERY INTERESTED IN THE PERSON WHO IS KILLING HER COLLECTION AND DECIDED TO PAY YOU A VISIT HERSELF. LOOKS LIKE YOU WERENT A FINE ADDITION TO HER COLLECTION AFTER ALL.\n" +
        "\n" +
        "SIRENS CHARM: THE SIREN WILL CONSTANTLY CHARM ALL TOWERS IN A SMALL RADIUS. CHARMED TOWERS HEAL THEIR TARGETS INSTEAD OF DEALING DAMAGE.\n" +
        "SWIMMER: THIS CREATURE ARRIVES AND MOVES THROUGH WATER.\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: AVERAGE\n" +
        "HEALTH: ABOVE AVERAGE\n" +
        "VALUE: AVERAGE\n" +
        "SLIGHTLY RESISTANT TO PHYSICAL\n" +
        "MODERATELY RESISTANT TO FIRE\n" +
        "INSANELY WEAK TO COLD";
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.4f,
        [DamageType.Fire] = 0.3f,
        [DamageType.Cold] = 3f
    };
}

public class DataTheEnd : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("7fdc600b-8803-4361-96d8-e2cad49ae59b");
    public override Guid PersistentID => GID;
    public override float Speed => 26F;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 700F;
    public override int Frame => 71;
    public override int Value => 1000;
    public override string Name => "The End {???} {True Rebirth} {Endless}";
    public override string BestiaryName => "THE END, STAGE 1";
    public override string BestiaryDescription => "UNSATISFIED WITH YOUR SLAUGHTER I HAVE DECIDED TO VISIT YOU MYSELF IN MY OWN FLESH AND BLOOD SO TO SPEAK. I SEE YOUVE BEEN DOING JUST FINE WITHOUT ME SETTING UP ALL THESE CRAZY TOWERS AND STUFF. WELL LETS SEE HOW YOU FAIR AGAINST ME. YOUVE TRIED SO HARD AFTER ALL.\n" +
        "\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: ABOVE AVERAGE\n" +
        "HEALTH: GRAND\n" +
        "VALUE: EXTREMELY RICH\n";
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>();
}

public class DataTheEnd2 : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("f75e2afc-b5b1-4c56-8a56-646e149830b7");
    public override Guid PersistentID => GID;
    public override float Speed => 34F;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 600F;
    public override int Frame => 452;
    public override int Value => 2000;
    public override string Name => "The End? {???} {True Rebirth} {Endless}";
    public override string BestiaryName => "THE END, STAGE 2";
    public override string BestiaryDescription => "WELL NOW YOU HAVE MANAGED TO BEAT MY FRAIL HUMAN FORM. HOW ABOUT YOU TRY BEATING MY LESS FRAIL AND LESS HUMAN FORM NOW? AS YOUVE MELTED AWAY THE FLESH THERE ARE STILL SOME BONES LEFT YOU KNOW. YOU DID GET SO FAR AFTER ALL SO WHY STOP HERE?\n" +
        "\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: ABOVE AVERAGE\n" +
        "HEALTH: EXTREMELY GOOD\n" +
        "VALUE: INSANELY RICH\n" +
        "IMMUNE TO PHYSICAL";
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0
    };
}

public class DataTheEnd3 : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("9a2c8b53-ec56-4256-8bff-29315f2202b1");
    public override Guid PersistentID => GID;
    public override float Speed => 20f;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 1000F;
    public override int Frame => 266;
    public override int Value => 3000;
    public override string Name => "The End... {???} {True Rebirth} {Endless}";
    public override string BestiaryName => "THE END, STAGE 3";
    public override string BestiaryDescription => "THATS TWO OUT OF THREE DONE. GOOD JOB ON THAT. MY BONES ARE UNOBTANIUM AFTER ALL BOTH FIGURATIVELY AND LITERALLY. THERE IS JUST MY SOUL LEFT NOW. COME ON. WE ARE IN THE END HERE.\n" +
        "\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: BELOW AVERAGE\n" +
        "HEALTH: THE GRANDEST\n" +
        "VALUE: BEYOUND RICH\n" +
        "IMMUNE TO FIRE\n" +
        "IMMUNE TO COLD\n" +
        "IMMUNE TO DARKNESS\n" +
        "IMMUNE TO LIGHT\n";
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Fire] = 0,
        [DamageType.Cold] = 0,
        [DamageType.Darkness] = 0,
        [DamageType.Light] = 0
    };
}

public class DataTheEnd4 : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("20c4191f-e0bc-42a6-85bf-8f9a28b7393b");
    public override Guid PersistentID => GID;
    public override float Speed => 64F;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 500F;
    public override int Frame => 553;
    public override int Value => 4000;
    public override string Name => "Please be The End {???} {True Rebirth} {Endless}";
    public override string BestiaryName => "THE END, STAGE 4";
    public override string BestiaryDescription => "NOW THAT YOU HAVE KILLED ALL THAT TIED ME TO MY MORTAL COIL! FINALLY I CAN TRULY BE REBORN! ALL THANKS TO YOU AND YOU ALONE! YOU HAVE PUT SO MUCH EFFORT DEFEATING ME BUT IT DOESNT EVEN MATTER. COME. LETS FINISH THIS.\n" +
        "\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: EXTREMELY FAST\n" +
        "HEALTH: VERY GOOD\n" +
        "VALUE: COSMIC\n" +
        "IMMUNE TO PHYSICAL\n" +
        "IMMUNE TO DARKNESS\n" +
        "IMMUNE TO LIGHT";
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0,
        [DamageType.Darkness] = 0,
        [DamageType.Light] = 0
    };
}

public class DataTheEnd5 : EnemyDataBase
{
    public static Guid GID { get; } = Guid.Parse("47d876d1-90b6-475e-b25b-fecc3b4c84f6");
    public override Guid PersistentID => GID;
    public override float Speed => 10F;
    public override Color BloodColor => new Color(0.8f, 0.3f, 0.3f);
    public override float Health => 5000F;
    public override int Frame => 400;
    public override int Value => 10000;
    public override string Name => "The End {???} {Endless}";
    public override string BestiaryName => "THE END, FINAL STAGE";
    public override string BestiaryDescription => "... WHY ARE YOU STILL HERE? WELL, YOU HAVE MANAGED TO DEFEAT ME BEFORE I COULD BE REBORN AND NOW YOU HAVE ALL MY POWER. AND MY BOOK. I GUESS IVE ANTICIPATED IT SINCE I HAVE WROTE THIS PASSAGE. RULE WISELY GOOD KING AND NEVER FORGET THE ONES WHO TRULY MATTER.\n" +
        "\n" +
        "ENDLESS: THIS CREATURE WILL BE RESET TO THEIR STARTING POSITION UPON REACHING YOUR CASTLE INSTEAD OF DYING\n" +
        "\n" +
        "SPEED: EXTREMELY SLOW\n" +
        "HEALTH: UNIMAGINABLE\n" +
        "VALUE: UNIMAGINABLE\n" +
        "IMMUNE TO ALL DAMAGE... OR AM I?";
    public override Color ColorMod => new Color(1, 0, 0, 1);
    public override Dictionary<DamageType, float> Resistances => new Dictionary<DamageType, float>()
    {
        [DamageType.Physical] = 0.01f,
        [DamageType.Darkness] = 0.01f,
        [DamageType.Light] = 0.01f,
        [DamageType.Cold] = 0.01f,
        [DamageType.Fire] = 0.01f
    };
}