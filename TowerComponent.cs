using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class TowerComponent : StaticBody2D
{
    public static Dictionary<ComponentType, List<Tuple<TowerComponentDataBase, int>>> WeightedComponents { get; } = new Dictionary<ComponentType, List<Tuple<TowerComponentDataBase, int>>>()
    {
        [ComponentType.Barrel] = new List<Tuple<TowerComponentDataBase, int>>(7)
        {
            new Tuple<TowerComponentDataBase, int>(new TCDBarrelFlame(), 25),
            new Tuple<TowerComponentDataBase, int>(new TCDBarrelIce(), 25),
            new Tuple<TowerComponentDataBase, int>(new TCDBarrelDark(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDBarrelLight(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDBarrelPale(), 500),
            new Tuple<TowerComponentDataBase, int>(new TCDBarrelOmega(), 500),
            new Tuple<TowerComponentDataBase, int>(new TCDBarrelSingularity(), 500),
        },

        [ComponentType.Base] = new List<Tuple<TowerComponentDataBase, int>>(9)
        {
            new Tuple<TowerComponentDataBase, int>(new TCDBaseWooden(), 5),
            new Tuple<TowerComponentDataBase, int>(new TCDBaseSteel(), 25),
            new Tuple<TowerComponentDataBase, int>(new TCDBaseGold(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDBaseDemon(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDBaseSlime(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDBaseAncient(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDBaseTemporal(), 500),
            new Tuple<TowerComponentDataBase, int>(new TCDBaseUnobtanium(), 500),
            new Tuple<TowerComponentDataBase, int>(new TCDBaseClassic(), 500),
        },

        [ComponentType.Core] = new List<Tuple<TowerComponentDataBase, int>>(7)
        {
            new Tuple<TowerComponentDataBase, int>(new TCDCorePrecision(), 120),
            new Tuple<TowerComponentDataBase, int>(new TCDCoreGolden(), 120),
            new Tuple<TowerComponentDataBase, int>(new TCDCoreLuck(), 120),
            new Tuple<TowerComponentDataBase, int>(new TCDCoreTime(), 120),
            new Tuple<TowerComponentDataBase, int>(new TCDCoreSelfAware(), 500),
            new Tuple<TowerComponentDataBase, int>(new TCDCoreQuad(), 1000),
            new Tuple<TowerComponentDataBase, int>(new TCDCoreAlien(), 500),
        },

        [ComponentType.Gun] = new List<Tuple<TowerComponentDataBase, int>>(13)
        {
            new Tuple<TowerComponentDataBase, int>(new TCDGunWooden(), 5),
            new Tuple<TowerComponentDataBase, int>(new TCDGunSteel(), 5),
            new Tuple<TowerComponentDataBase, int>(new TCDGunLaser(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDGunBigLazer(), 120),
            new Tuple<TowerComponentDataBase, int>(new TCDGunSniper(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDGunHoly(), 25),
            new Tuple<TowerComponentDataBase, int>(new TCDGunHolier(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDGunDarkness(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDGunCurse(), 120),
            new Tuple<TowerComponentDataBase, int>(new TCDGunFlamethrower(), 120),
            new Tuple<TowerComponentDataBase, int>(new TCDGunTopaz(), 500),
            new Tuple<TowerComponentDataBase, int>(new TCDGunAquamarine(), 500),
            new Tuple<TowerComponentDataBase, int>(new TCDGunRuby(), 500),
        },

        [ComponentType.Igniter] = new List<Tuple<TowerComponentDataBase, int>>(25)
        {
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterSpace(), 25),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterReactor(), 25),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterCapacitor(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterLightning(), 120),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterMagic(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterFire(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterIce(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterCurse(), 60),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterReverse(), 1000),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterEgg(), 500),
            new Tuple<TowerComponentDataBase, int>(new TCDIgniterInfinity(), 500),
        },
    };

    public static Color White = new Color(1, 1, 1);
    public static Color Green = new Color(0.1f, 0.7f, 0.2f);
    public static Color Blue = new Color(0, 0.3f, 1);
    public static Color Purple = new Color(0.7f, 0, 0.7f);
    public static Color Orange = new Color(0.9f, 0.5f, 0);
    public static Color Red = new Color(1, 0, 0);

    public static TowerComponent Template { get; set; }

    [Export]
    public bool baseObject;

    public bool dragged;
    public ComponentType componentType;
    public string humanReadableName;
    public float damage;
    public float damageMultiplier = 1;
    public float cooldown;
    public float cooldownMultiplier = 1;
    public float range;
    public float rangeMultiplier = 1;
    public DamageType damageTypeOverride = DamageType.Unspecified;
    public string[] humanReadableDescription = new string[0];
    public Color rarityColor = new Color(1, 1, 1);
    public int particleRate = 1;

    public int Frame { get; set; }

    public TowerComponentDataBase data;

    private BaseTower _owner;
    private AnimatedSprite _sprite;
    private CPUParticles2D _particles;
    private Area2D _collider;
    private Vector2 _initialSpriteScale;

    public bool HasOwner => this._owner != null;
    public BaseTower OwningTower => this._owner;

    public override void _Ready()
    {
        base._Ready();
        if (this.baseObject)
        {
            Template = this;
            ((GameScript)this.GetParent()).Todo.Enqueue(() =>
            {
                for (int i = 0; i < 2; ++i)
                {
                    TowerComponent tc = TowerComponent.SpawnComponent(new TCDBaseWooden());
                    tc.Position = new Vector2(308, 300 + 20 * i);
                    this.GetParent().AddChild(tc);
                }

                for (int i = 0; i < 2; ++i)
                {
                    TowerComponent tc = TowerComponent.SpawnComponent(new TCDGunWooden());
                    tc.Position = new Vector2(308, 340 + 20 * i);
                    this.GetParent().AddChild(tc);
                }
            });
        }

        this._sprite = this.GetChildren().Cast<Node>().Where(c => c is AnimatedSprite).Cast<AnimatedSprite>().First();
        this._initialSpriteScale = this._sprite.Scale;
        this._particles = this.GetChildren().Cast<Node>().Where(c => c is CPUParticles2D).Cast<CPUParticles2D>().First();
        this._collider = this.GetChildren().Cast<Node>().Where(c => c is Area2D).Cast<Area2D>().First();
        //this._collider.Connect("mouse_entered", this, "MouseEnter");
        //this._collider.Connect("mouse_exited", this, "MouseLeave");
        this.CollisionMask = this.dragged || this._owner != null ? 0u : uint.MaxValue;
    }

    public void MouseEnter()
    {
        if (this.baseObject)
        {
            return;
        }

        ((GameScript)this.GetParent()).ComponentMouseOver = this;
    }

    public void MouseLeave()
    {
        if (this.baseObject)
        {
            return;
        }

        ((GameScript)this.GetParent()).ComponentMouseOver = null;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (!this.baseObject)
        {
            this._particles.Emitting = this._owner == null;
            this._particles.Color = this.rarityColor;
            this.CollisionMask = this.dragged ? 0u : 2u;
            this.CollisionLayer = this.dragged ? 0u : 2u;
            this._particles.Emitting = !(this.dragged || this.HasOwner);
            if (this.Frame != this._sprite.Frame)
            {
                this._sprite.Frame = this.Frame;
            }

            if (this.dragged)
            {
                this.Position = this.GetGlobalMousePosition();
            }

            this.data.HandleUpdate(this, this._owner, delta);
            if (this.animationTime > 0)
            {
                this.animationTime -= delta;
                this._sprite.Scale = this._initialSpriteScale * (1 + (0.5f * (this.animationTime / this.maxAnimationTime)));
            }
        }
    }

    public void DetachFromTower(BaseTower tower)
    {
        this._owner.Components.Remove(this.componentType);
        this._owner = null;
    }

    public float animationTime;
    public float maxAnimationTime;
    public void AnimateShot(float cooldown) => this.animationTime = this.maxAnimationTime = cooldown;

    public void Attach2Tower(BaseTower tower)
    {
        this._owner = tower;
        switch (this.componentType)
        {
            case ComponentType.Base:
            {
                this.Position = tower.Position;
                break;
            }

            case ComponentType.Gun:
            {
                this.Position = tower.Position + new Vector2(0, -8);
                break;
            }

            case ComponentType.Barrel:
            {
                this.Position = tower.Position + new Vector2(0, -16);
                break;
            }

            case ComponentType.Core:
            {
                this.Position = tower.Position + new Vector2(-8, 0);
                break;
            }

            case ComponentType.Igniter:
            {
                this.Position = tower.Position + new Vector2(8, 0);
                break;
            }

            default:
            {
                break;
            }
        }
    }

    public static TowerComponent SpawnComponent(TowerComponentDataBase data)
    {
        TowerComponent newInst = (TowerComponent)Template.Duplicate();
        newInst.baseObject = false;
        newInst._owner = null;
        newInst.data = data;
        data.Apply(newInst);
        return newInst;
    }
}

public enum ComponentRarity
{
    White,
    Green,
    Blue,
    Purple,
    Orange,
    Red
}

public abstract class TowerComponentDataBase
{
    public abstract void Apply(TowerComponent component);
    public abstract ComponentRarity Rarity { get; }
    public virtual void HandleDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
    }

    public virtual void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
    }

    public virtual void HandleBulletSpawn(BulletScript bullet, TowerComponent tower)
    {
    }

    public virtual void HandleKill(BulletScript bullet, EnemyScript enemy, ref bool saveBullet)
    {
    }

    public virtual void HandleUpdate(TowerComponent owner, BaseTower tower, float delta)
    {
    }
}

public class TCDBaseWooden : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.White;

    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "WOODEN TOWER BASE";
        component.Frame = 139;
        component.humanReadableDescription = new string[]
        {
            "NO SPECIAL FEATURES.",
            "[i]A WOODEN TOWER BASE. ALL LOCALLY SOURCED AND FULLY BIO-DEGRADABLE![/i]"
        };
    }
}
public class TCDBaseSteel : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Green;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "STEEL TOWER BASE";
        component.Frame = 179;
        component.damageMultiplier = 1.1f;
        component.cooldownMultiplier = 0.9f;
        component.rangeMultiplier = 1.1f;
        component.rarityColor = TowerComponent.Green;
        component.humanReadableDescription = new string[]
        {
            "[color=green]x110%[/color] DAMAGE.",
            "[color=green]x110%[/color] RANGE.",
            "[color=green]x90%[/color] COOLDOWN.",
            "[i]MAKES FOR STURDY TOWERS. OR A DECENT ANVIL.[/i]"
        };
    }
}
public class TCDBaseGold : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "GOLD TOWER BASE";
        component.Frame = 229;
        component.damageMultiplier = 1.25f;
        component.cooldownMultiplier = 1.5f;
        component.rangeMultiplier = 2f;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]
        {
            "[color=green]x125%[/color] DAMAGE.",
            "[color=green]x200%[/color] RANGE.",
            "[color=red]x150%[/color] COOLDOWN.",
            "[i]GOLD IS PRETTY HEAVY, BUT SHINY![/i]"
        };
    }
}
public class TCDBaseDemon : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "HELLSTONE TOWER BASE";
        component.Frame = 539;
        component.damageMultiplier = 2f;
        component.cooldownMultiplier = 0.9f;
        component.rangeMultiplier = 0.25f;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]
        {
            "[color=green]x200%[/color] DAMAGE.",
            "[color=red]x25%[/color] RANGE.",
            "[color=green]x90%[/color] COOLDOWN.",
            "[i]ARE WE SURE THIS IS LEGAL?[/i]"
        };
    }
}
public class TCDBaseSlime : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "SLIME TOWER BASE";
        component.Frame = 536;
        component.damageMultiplier = 0.6f;
        component.cooldownMultiplier = 0.2f;
        component.rangeMultiplier = 0.6f;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]
        {
            "[color=red]x60%[/color] DAMAGE.",
            "[color=red]x60%[/color] RANGE.",
            "[color=green]x20%[/color] COOLDOWN.",
            "[i]EWW, WHO WOULD EVEN THINK ABOUT MAKING A TOWER OUT OF THAT?[/i]"
        };
    }
}
public class TCDBaseAncient : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "ANCIENT TOWER BASE";
        component.Frame = 619;
        component.rangeMultiplier = 3f;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]
        {
            "[color=green]x300%[/color] RANGE.",
            "[i]THEY SURE KNEW HOW TO MAKE TOWERS BACK IN THE DAY. HOW DOES THAT EVEN MAKE SENSE?[/i]"
        };
    }
}
public class TCDBaseTemporal : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "TEMPORAL TOWER BASE";
        component.Frame = 621;
        component.damageMultiplier = 1.25f;
        component.cooldownMultiplier = 0.5f;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]
        {
            "[color=red]THEY WON'S SEE WHAT HIT THEM.[/color]",
            "[i]WE ARE UNSIRE WHERE THIS EVEN CAME FROM. IT JUST APPEARED, AND IT SEEMS WE GET A NEW ONE EVERY 22 MINUTES.[/i]"
        };
    }

    public override void HandleBulletSpawn(BulletScript bullet, TowerComponent tower) => bullet.Speed = 10;
}
public class TCDBaseUnobtanium : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "UNOBTANIUM TOWER BASE";
        component.Frame = 719;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]
        {
            "[color=red]WHAT CAME FIRST? THE TOWER OR THE ENEMY?[/color]",
            "[i]IF IT IS SUPPOSEDLY MADE OUT OF UNOBTANIUM, THEN HOW DID WE GET IT?[/i]"
        };
    }

    private bool _hold;
    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy) => this._hold = enemy.health > enemy.data.Health;

    public override void HandleBulletSpawn(BulletScript bullet, TowerComponent tower)
    {
        if (this._hold)
        {
            bullet.Damages.Add(new Tuple<DamageType, float, TowerComponentDataBase>(DamageType.Pure, bullet.Damages[0].Item2, this));
        }
    }
}
public class TCDBaseClassic : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Base;
        component.humanReadableName = "CLASSIC TOWER BASE";
        component.Frame = 983;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]
        {
            "[color=red]GOOD OLD TIMES.[/color]",
            "[i]WE HAVE NO IDEA WHAT THIS DOES. ALTHOUGH IT DOES LOOK CLASSY.[/i]"
        };
    }

    private Random _rand = new Random();
    public override void HandleBulletSpawn(BulletScript bullet, TowerComponent tower)
    {
        if (this._rand.NextDouble() < 0.03)
        {
            Tuple<DamageType, float, TowerComponentDataBase> damages = bullet.Damages[0];
            bullet.Damages[0] = new Tuple<DamageType, float, TowerComponentDataBase>(damages.Item1, 999f, this);
        }
    }
}

public class TCDGunWooden : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.White;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "WOODEN GUN";
        component.Frame = 140;
        component.damage = 1;
        component.cooldown = 1;
        component.range = 128;
        component.damageTypeOverride = DamageType.Physical;
        component.humanReadableDescription = new string[]{
            "[color=gray]PHYSICAL[/color]",
            "[color=green]+100%[/color] DAMAGE.",
            "[color=red]+100%[/color] COOLDOWN.",
            "AVERAGE RANGE",
            "[i]A WOODEN GUN. IT LOOKS LIKE A TOY, BUT IT KINDA HURTS.[/i]"
        };
    }
}
public class TCDGunSteel : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.White;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "STEEL GUN";
        component.Frame = 22;
        component.damage = 3;
        component.cooldown = 1;
        component.range = 128;
        component.damageTypeOverride = DamageType.Physical;
        component.humanReadableDescription = new string[]{
            "[color=gray]PHYSICAL[/color]",
            "[color=green]+300%[/color] DAMAGE.",
            "[color=red]+100%[/color] COOLDOWN.",
            "AVERAGE RANGE",
            "[i]A STEEL GUN. THEY MOUNT THESE ON THE FLOOR AND CALL THEM SPIKES SOMETIMES.[/i]"
        };
    }
}
public class TCDGunLaser : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "LASER GUN";
        component.Frame = 39;
        component.damage = 0.2f;
        component.cooldown = 0.1f;
        component.range = 256;
        component.damageTypeOverride = DamageType.Fire;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=orange]FIRE[/color]",
            "[color=green]+20%[/color] DAMAGE.",
            "[color=red]+10%[/color] COOLDOWN.",
            "GOOD RANGE",
            "[i]IT SHOOTS LAZERS! NEAT![/i]"
        };
    }
}
public class TCDGunBigLazer : TCDGunLaser
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "BIG LASER GUN";
        component.Frame = 40;
        component.damage = 0.2f;
        component.cooldown = 0.05f;
        component.range = 256;
        component.damageTypeOverride = DamageType.Fire;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]{
            "[color=orange]FIRE[/color]",
            "[color=green]+20%[/color] DAMAGE.",
            "[color=red]+5%[/color] COOLDOWN.",
            "GOOD RANGE",
            "[i]IF YOU WANT TO FRY YOUR ENEMIES TO A CRISP[/i]"
        };
    }
}
public class TCDGunSniper : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "SNIPER GUN";
        component.Frame = 110;
        component.damage = 1.5f;
        component.cooldown = 2f;
        component.range = 1024;
        component.damageTypeOverride = DamageType.Cold;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=blue]COLD[/color]",
            "[color=green]+150%[/color] DAMAGE.",
            "[color=red]+200%[/color] COOLDOWN.",
            "INSANE RANGE",
            "[i]IF YOU WANT TO HIT SOMETHING AT THE OTHER END OF THE PLANET[/i]"
        };
    }
}
public class TCDGunHoly : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Green;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "HOLY GUN";
        component.Frame = 137;
        component.damage = 0.8f;
        component.cooldown = 1f;
        component.range = 256;
        component.damageTypeOverride = DamageType.Light;
        component.rarityColor = TowerComponent.Green;
        component.humanReadableDescription = new string[]{
            "[color=yellow]LIGHT[/color]",
            "[color=green]+80%[/color] DAMAGE.",
            "[color=red]+100%[/color] COOLDOWN.",
            "GOOD RANGE",
            "[i]BLESS YOUR ENEMIES[/i]"
        };
    }
}
public class TCDGunHolier : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "HOLIER GUN";
        component.Frame = 138;
        component.damage = 1.2f;
        component.cooldown = 0.8f;
        component.range = 256;
        component.damageTypeOverride = DamageType.Light;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=yellow]LIGHT[/color]",
            "[color=green]+120%[/color] DAMAGE.",
            "[color=red]+80%[/color] COOLDOWN.",
            "GOOD RANGE",
            "[i]BLESS YOUR ENEMIES EVEN HARDER[/i]"
        };
    }
}
public class TCDGunDarkness : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "DARK GUN";
        component.Frame = 376;
        component.damage = 2f;
        component.cooldown = 1.4f;
        component.range = 64;
        component.damageTypeOverride = DamageType.Darkness;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=yellow]DARKNESS[/color]",
            "[color=green]+200%[/color] DAMAGE.",
            "[color=red]+140%[/color] COOLDOWN.",
            "BAD RANGE",
            "[i]THIS THING JUST SPEWS SMOKE AT YOUR ENEMIES. HOW BORING.[/i]"
        };
    }
}
public class TCDGunCurse : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "CURSE GUN";
        component.Frame = 385;
        component.damage = 4f;
        component.cooldown = 1.5f;
        component.range = 64;
        component.damageTypeOverride = DamageType.Darkness;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]{
            "[color=yellow]DARKNESS[/color]",
            "[color=green]+400%[/color] DAMAGE.",
            "[color=red]+150%[/color] COOLDOWN.",
            "BAD RANGE",
            "[i]DEVISED BY A WITCH MOST EVIL FAR-FAR AWAY. WE SHOULDN'T BE USING THIS.[/i]"
        };
    }
}
public class TCDGunFlamethrower : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "FLAMETHROWER GUN";
        component.Frame = 484;
        component.damage = 0.2f;
        component.cooldown = 0.05f;
        component.range = 64;
        component.damageTypeOverride = DamageType.Fire;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]{
            "[color=yellow]FIRE[/color]",
            "[color=green]+20%[/color] DAMAGE.",
            "[color=red]+5%[/color] COOLDOWN.",
            "BAD RANGE",
            "[i]THIS THING JUST MELTS EVERYTHING![/i]"
        };
    }
}
public class TCDGunTopaz : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "TOPAZ GUN";
        component.Frame = 502;
        component.damage = 2f;
        component.cooldown = 0.9f;
        component.range = 512;
        component.damageTypeOverride = DamageType.Light;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]TASTE THE RAINBOW![/color]",
            "[i]WHAT WILL IT DO NEXT?[/i]"
        };
    }

    private Random _rand = new Random();
    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        base.HandleDamage(ref damage, ref dt, enemy);
        dt = this._rand.NextDouble() < 0.5 ? DamageType.Light : this._rand.NextDouble() < 0.5 ? DamageType.Fire : DamageType.Physical;
    }
}
public class TCDGunAquamarine : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "AQUAMARINE GUN";
        component.Frame = 503;
        component.damage = 0.9f;
        component.cooldown = 0.1f;
        component.range = 64;
        component.damageTypeOverride = DamageType.Light;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]TASTE THE RAINBOW![/color]",
            "[i]WHAT WILL IT DO NEXT?[/i]"
        };
    }

    private Random _rand = new Random();
    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        base.HandleDamage(ref damage, ref dt, enemy);
        dt = this._rand.NextDouble() < 0.5 ? DamageType.Darkness : this._rand.NextDouble() < 0.5 ? DamageType.Cold : DamageType.Physical;
    }
}
public class TCDGunRuby : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Gun;
        component.humanReadableName = "RUBY GUN";
        component.Frame = 503;
        component.damage = 1f;
        component.cooldown = 1f;
        component.range = 64;
        component.damageTypeOverride = DamageType.Fire;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]THIS MIGHT STINK[/color]",
            "[color=red]WE DRINK THEIR BLOOD![/color]",
            "[i]THIS SEEMS OH EVER SO SLIGHTLY DANGEROUS AND BLOODTHIRSTY.[/i]"
        };
    }

    private Random _rand = new Random();
    public override void HandleDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        base.HandleDamage(ref damage, ref dt, enemy);
        bool crit = this._rand.NextDouble() < 0.05f;
        damage = crit ? damage * 10 : damage;
        dt = crit ? DamageType.Pure : this._rand.NextDouble() < 0.5 ? DamageType.Fire : DamageType.Physical;
    }

    public override void HandleKill(BulletScript bullet, EnemyScript enemy, ref bool saveBullet)
    {
        base.HandleKill(bullet, enemy, ref saveBullet);
        if (this._rand.NextDouble() < 0.01)
        {
            ((GameScript)bullet.GetParent()).Lives += 1;
        }
    }
}

public class TCDIgniterSpace : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Green;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "SPACE IGNITER";
        component.Frame = 32;
        component.damage = 0.3f;
        component.cooldown = 0.05f;
        component.rarityColor = TowerComponent.Green;
        component.humanReadableDescription = new string[]{
            "[color=green]+30%[/color] DAMAGE.",
            "[color=red]+5%[/color] COOLDOWN",
            "[i]THIS WILL MAKE THE BULLETS GO PEW-PEW STRONGER.[/i]"
        };
    }
}
public class TCDIgniterReactor : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Green;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "REACTOR IGNITER";
        component.Frame = 37;
        component.damage = -0.1f;
        component.cooldown = -0.1f;
        component.rarityColor = TowerComponent.Green;
        component.humanReadableDescription = new string[]{
            "[color=red]-10%[/color] DAMAGE.",
            "[color=green]-10%[/color] COOLDOWN",
            "[i]INEFFICIENT, BUT FAST.[/i]"
        };
    }
}
public class TCDIgniterCapacitor : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "CAPACITOR IGNITER";
        component.Frame = 46;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=green]STORES ENERGY BETWEEN SHOTS[/color]",
            "[i]INEFFICIENT, BUT FAST.[/i]"
        };
    }

    private float _eStored;

    public override void HandleUpdate(TowerComponent owner, BaseTower tower, float delta)
    {
        base.HandleUpdate(owner, tower, delta);
        if (tower != null)
        {
            if (((GameScript)tower.GetParent()).IsWaveActive)
            {
                this._eStored += delta;
                this._eStored = Mathf.Min(5, this._eStored);
            }
        }
    }

    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        base.HandleAnyDamage(ref damage, ref dt, enemy);
        damage *= (1 + this._eStored);
        this._eStored = 0;
    }
}
public class TCDIgniterLightning : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "LIGHTNING IGNITER";
        component.Frame = 46;
        component.cooldown = -0.2f;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]{
            "[color=green]-20%[/color] COOLDOWN",
            "[color=green]WILL SHOOT ENEMIES ON ITS OWN[/color]",
            "[i]THIS THING IS PRETTY NEAT.[/i]"
        };
    }

    public bool isActive;
    public float activeTicks;
    public Vector2 targetPos;

    public override void HandleUpdate(TowerComponent owner, BaseTower tower, float delta)
    {
        base.HandleUpdate(owner, tower, delta);
        if (tower != null)
        {
            if ((this.activeTicks -= delta) > 0)
            {
                KinematicBody2D t = ((GameScript)owner.GetParent()).GetChildren().Cast<Node>().Where(c => c is EnemyScript es && !es.baseObject && es.Position.DistanceTo(owner.Position) <= 64).Cast<EnemyScript>().OrderBy(c => c.Position.DistanceTo(owner.Position)).FirstOrDefault();
                this.isActive = t != null;
                if (this.isActive)
                {
                    this.targetPos = t.GlobalPosition;
                    ((EnemyScript)t).ApplyDamage(1 * delta, DamageType.Fire);
                }
            }
            else
            {
                this.isActive = false;
                this.activeTicks += delta;
                this.activeTicks = Mathf.Min(this.activeTicks, 3);
            }
        }
    }
}
public class TCDIgniterMagic : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "MAGIC IGNITER";
        component.Frame = 136;
        component.damageMultiplier = 1.2f;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=green]x120%[/color] DAMAGE.",
            "[i]FANCY.[/i]"
        };
    }
}
public class TCDIgniterFire : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "FLAMING IGNITER";
        component.Frame = 485;
        component.damageMultiplier = 0.9f;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=red]x90%[/color] DAMAGE.",
            "[color=green]WILL IGNITE ENEMIES HIT[/color]",
            "[i]DO YOU LIKE TO PLAY WITH FIRE?[/i]"
        };
    }

    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        if (!enemy.isBurning)
        {
            enemy.isBurning = true;
            enemy.burnDPS = damage;
            CPUParticles2D parts = (CPUParticles2D)((GameScript)enemy.GetParent()).spawnParticles.Duplicate();
            parts.Position = Vector2.Zero;
            parts.Visible = true;
            parts.Emitting = true;
            parts.Amount = 3;
            parts.Color = TowerComponent.Orange;
            enemy.AddChild(parts);
        }
    }
}
public class TCDIgniterIce : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "CHILLING IGNITER";
        component.Frame = 517;
        component.damageMultiplier = 0.9f;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=red]x90%[/color] DAMAGE.",
            "[color=green]WILL SLOW DOWN ENEMIES HIT[/color]",
            "[i]CROWD CONTROL?[/i]"
        };
    }

    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        if (enemy.speed.Equals(enemy.data.Speed))
        {
            enemy.speed *= 0.75f;
            CPUParticles2D parts = (CPUParticles2D)((GameScript)enemy.GetParent()).spawnParticles.Duplicate();
            parts.Position = Vector2.Zero;
            parts.Visible = true;
            parts.Emitting = true;
            parts.Amount = 3;
            parts.Color = TowerComponent.Blue;
            enemy.AddChild(parts);
        }
    }
}
public class TCDIgniterCurse : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "CURSED IGNITER";
        component.Frame = 558;
        component.damageMultiplier = 0.9f;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=red]x90%[/color] DAMAGE.",
            "[color=green]CURSES ENEMIES THAT HAD BEEN HIT[/color]",
            "[i]MADE BY THE SAME WITCH THAT MANUFACTURED THE CURSED GUN. VERY DANGEROUS.[/i]"
        };
    }

    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        if (!enemy.cursed)
        {
            dt = DamageType.Darkness;
            enemy.cursed = true;
            CPUParticles2D parts = (CPUParticles2D)((GameScript)enemy.GetParent()).spawnParticles.Duplicate();
            parts.Position = Vector2.Zero;
            parts.Visible = true;
            parts.Emitting = true;
            parts.Amount = 3;
            parts.Color = TowerComponent.Purple;
            enemy.AddChild(parts);
        }
    }
}
public class TCDIgniterReverse : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "REVERSE IGNITER";
        component.Frame = 709;
        component.cooldownMultiplier = 0.01f;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]HAHA, BULLETS GO BRRRRRRR[/color]",
            "[i]PURE MADNESS.[/i]"
        };
    }
}
public class TCDIgniterEgg : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "EGG IGNITER";
        component.Frame = 833;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]HANDLE WITH CARE[/color]",
            "[i]IT MIGHT SPOIL SOON.[/i]"
        };
    }

    private bool _preventRecursion;
    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        base.HandleAnyDamage(ref damage, ref dt, enemy);
        if (!this._preventRecursion)
        {
            this._preventRecursion = true;
            foreach (EnemyScript es in ((GameScript)enemy.GetParent()).GetChildren().Cast<Node>().Where(c => c is EnemyScript es && !es.baseObject && es.Position.DistanceTo(enemy.Position) <= 64).Cast<EnemyScript>())
            {
                es.ApplyDamage(damage * 0.5f, dt);
            }

            this._preventRecursion = false;
        }
    }
}
public class TCDIgniterInfinity : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Igniter;
        component.humanReadableName = "INFINITY IGNITER";
        component.Frame = 967;
        component.rangeMultiplier = 100;
        component.cooldown = 8;
        component.damage = 80;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]THIS MAGICAL BULLET CAN TRULY HIT ANYONE YOU SAY![/color]",
            "[i]HOW DID YOU EVEN GET THIS?[/i]"
        };
    }
}

public class TCDCorePrecision : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Core;
        component.humanReadableName = "PRECISION CORE";
        component.Frame = 132;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]{
            "[color=green]THE TURRET CAN NOW CRITICALLY RELOAD[/color]",
            "[i]LIGHT EM' UP, KNOCK EM' DOWN[/i]"
        };
    }

    private Random _rand = new Random();
    public override void HandleBulletSpawn(BulletScript bullet, TowerComponent tower)
    {
        if (this._rand.NextDouble() < 0.15f)
        {
            tower.OwningTower.cooldownOverride = 0.02f;
            tower.OwningTower.doOverrideCooldown = true;
            ((GameScript)tower.OwningTower.GetParent()).CreatePopupSprite(tower, 677, 1, "popup");
        }
    }
}
public class TCDCoreGolden : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Core;
        component.humanReadableName = "GOLDEN CORE";
        component.Frame = 327;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]{
            "[color=green]EACH SHOT GENERATES GOLD[/color]",
            "[i]THERE IS A PHILOSOPHER'S STONE SOMEWHERE IN THERE. OR MAYBE MIDAS IS TRAPPED THERE. WHO KNOWS.[/i]"
        };
    }

    private Random _rand = new Random();

    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        if (this._rand.NextDouble() < 0.05f)
        {
            CoinScript kb2d = (CoinScript)((GameScript)enemy.GetParent()).coin.Duplicate();
            kb2d.Position = enemy.Position;
            kb2d.Visible = true;
            kb2d.Value = (int)Mathf.Ceil(damage);
            ((GameScript)enemy.GetParent()).coinValue += kb2d.Value;
            enemy.GetParent().AddChild(kb2d);
        }
    }
}
public class TCDCoreSelfAware : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Core;
        component.humanReadableName = "SELF AWARE CORE";
        component.Frame = 469;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]HELLO? ARE YOU STILL THERE?[/color]",
            "[i]THIS THING CREEPS ME OUT.[/i]"
        };
    }

    private Random _rand = new Random();
    private float _exp;
    private int _level = 1;
    private float[] damMuls = { 1f, 1.2f, 1.3f, 1.5f, 2f, 2.5f, 3f, 3.5f, 5f, 7f, 9f, 12, 15, 18, 21, 23, 27, 32, 40 };

    public override void HandleKill(BulletScript bullet, EnemyScript enemy, ref bool saveBullet)
    {
        this._exp += enemy.data.Health;
        if (this._exp > this._level * this._level)
        {
            this._exp -= this._level * this._level;
            this._level += 1;
            foreach (TowerComponent tc in ((GameScript)enemy.GetParent()).GetChildren().Cast<Node>().Where(c => c is TowerComponent).Cast<TowerComponent>().Where(c => c.data is TCDCoreSelfAware))
            {
                ((GameScript)enemy.GetParent()).CreatePopupSprite(tc, 968, 1, "popup");
            }
        }
    }

    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        damage *= this.damMuls[Math.Min(this.damMuls.Length - 1, this._level)];
        if (this._level > 5)
        {
            float maxRes = 0;
            DamageType replacedType = DamageType.Unspecified;
            foreach (KeyValuePair<DamageType, float> kvp in enemy.data.Resistances)
            {
                if (kvp.Value > maxRes)
                {
                    maxRes = kvp.Value;
                    replacedType = kvp.Key;
                }
            }

            if (replacedType != DamageType.Unspecified && replacedType != DamageType.Pure)
            {
                dt = replacedType;
            }
        }
    }
}
public class TCDCoreLuck : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Core;
        component.humanReadableName = "LUCK CORE";
        component.Frame = 699;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]{
            "[color=green]THE TOWER MAY NOW SCORE CRITICAL HITS![/color]",
            "[color=green]THE TOWER MAY NOW SCORE CRITICAL FAILURES![/color]",
            "[i]JUST ONE MORE GAME, PLEASE![/i]"
        };
    }

    private Random _rand = new Random();

    public override void HandleBulletSpawn(BulletScript bullet, TowerComponent tower)
    {
        base.HandleBulletSpawn(bullet, tower);
        int randNumber = this._rand.Next(1, 21);
        if (randNumber == 1)
        {
            ((GameScript)tower.OwningTower.GetParent()).CreatePopupSprite(tower, 700, 1.5f, "excite");
            for (int i = 0; i < bullet.Damages.Count; ++i)
            {
                Tuple<DamageType, float, TowerComponentDataBase> t = bullet.Damages[i];
                bullet.Damages[i] = new Tuple<DamageType, float, TowerComponentDataBase>(t.Item1, -5, t.Item3);
            }
        }
        else
        {
            if (randNumber == 20)
            {
                ((GameScript)tower.OwningTower.GetParent()).CreatePopupSprite(tower, 704, 1.5f, "excite");
                for (int i = 0; i < bullet.Damages.Count; ++i)
                {
                    Tuple<DamageType, float, TowerComponentDataBase> t = bullet.Damages[i];
                    bullet.Damages[i] = new Tuple<DamageType, float, TowerComponentDataBase>(t.Item1, t.Item2 * 5, t.Item3);
                }
            }
            else
            {
                if (randNumber >= 15 && randNumber < 20)
                {
                    ((GameScript)tower.OwningTower.GetParent()).CreatePopupSprite(tower, 703, 0.25f, "excite");
                    for (int i = 0; i < bullet.Damages.Count; ++i)
                    {
                        Tuple<DamageType, float, TowerComponentDataBase> t = bullet.Damages[i];
                        bullet.Damages[i] = new Tuple<DamageType, float, TowerComponentDataBase>(t.Item1, t.Item2 * 1.25f, t.Item3);
                    }
                }
                else
                {
                    if (randNumber <= 5)
                    {
                        ((GameScript)tower.OwningTower.GetParent()).CreatePopupSprite(tower, 701, 0.25f, "excite");
                        for (int i = 0; i < bullet.Damages.Count; ++i)
                        {
                            Tuple<DamageType, float, TowerComponentDataBase> t = bullet.Damages[i];
                            bullet.Damages[i] = new Tuple<DamageType, float, TowerComponentDataBase>(t.Item1, t.Item2 * 1.75f, t.Item3);
                        }
                    }
                    else
                    {
                        ((GameScript)tower.OwningTower.GetParent()).CreatePopupSprite(tower, 702, 0.1f, "excite");
                    }
                }
            }
        }

    }
}
public class TCDCoreTime : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Purple;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Core;
        component.humanReadableName = "TIME CORE";
        component.Frame = 606;
        component.rarityColor = TowerComponent.Purple;
        component.humanReadableDescription = new string[]{
            "[color=green]THE TOWER MAY REVERT ENEMIES BACK IN TIME.[/color]",
            "[i]I CAN'T BELIEVE YOU'VE DONE THIS.[/i]"
        };
    }

    private Random _rand = new Random();
    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        base.HandleAnyDamage(ref damage, ref dt, enemy);
        if (this._rand.NextDouble() < 0.025f)
        {
            ((GameScript)enemy.GetParent()).CreatePopupSprite(enemy, 605, 1f, "rotate");
            enemy.ResetPosition();
            ((GameScript)enemy.GetParent()).CreatePopupSprite(enemy, 605, 1f, "rotate");
        }
    }
}
public class TCDCoreQuad : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Core;
        component.humanReadableName = "QUAD CORE";
        component.Frame = 655;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]FUNCTIONALITY DEPRECATED! USE TRIANGLE INSTEAD![/color]",
            "[i]IT SEEMS TO BE A TAD UNSTABLE...[/i]"
        };
    }

    private Random _rand = new Random();
    public override void HandleBulletSpawn(BulletScript bullet, TowerComponent tower)
    {
        foreach (EnemyScript es in ((GameScript)tower.OwningTower.GetParent()).GetChildren().Cast<Node>().Where(c => c is EnemyScript).Cast<EnemyScript>())
        {
            if (es.Position.DistanceTo(tower.OwningTower.Position) <= 128)
            {
                BulletScript bsC = (BulletScript)bullet.Duplicate();
                bsC.baseObject = false;
                bsC.Position = es.Position + Vector2.One.Rotated((float)(this._rand.NextDouble() * 2 * Math.PI)) * this._rand.Next(32, 128);
                bsC.Target = new WeakReference<Node2D>(es);
                bsC.Damages.AddRange(bullet.Damages);
                bsC.Frame = bullet.Frame;
                bsC.AllDamageDealers.AddRange(bullet.AllDamageDealers);
                ((GameScript)tower.OwningTower.GetParent()).AddChild(bsC);
            }
        }
    }
}
public class TCDCoreAlien : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Core;
        component.humanReadableName = "ALIEN CORE";
        component.Frame = 908;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]I CAN STILL SEE THEM IN MY DREAMS[/color]",
            "[i]IT CAME FROM OUTER SPACE.[/i]"
        };
    }

    private Random _rand = new Random();
    public override void HandleAnyDamage(ref float damage, ref DamageType dt, EnemyScript enemy)
    {
        base.HandleAnyDamage(ref damage, ref dt, enemy);
        if (!enemy.isInfected)
        {
            enemy.isInfected = true;
            ((GameScript)enemy.GetParent()).CreatePopupSprite(enemy, 594, 1f, "nasty");
        }
    }
}

public class TCDBarrelFlame : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Green;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Barrel;
        component.humanReadableName = "FLAME BARREL";
        component.Frame = 548;
        component.damageTypeOverride = DamageType.Fire;
        component.rarityColor = TowerComponent.Green;
        component.humanReadableDescription = new string[]{
            "[color=orange]FIRE[/color]",
            "[i]HEATS UP THE BULLETS UNTILL THEY LITERALLY SET ON FIRE[/i]"
        };
    }
}
public class TCDBarrelIce : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Green;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Barrel;
        component.humanReadableName = "ICE BARREL";
        component.Frame = 554;
        component.damageTypeOverride = DamageType.Cold;
        component.rarityColor = TowerComponent.Green;
        component.humanReadableDescription = new string[]{
            "[color=blue]COLD[/color]",
            "[i]A COMPLICATED DEVICE SUCKS ALL HEAT OUT OF THE OUTGOING BULLET.[/i]"
        };
    }
}
public class TCDBarrelLight : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Barrel;
        component.humanReadableName = "LIGHT BARREL";
        component.Frame = 608;
        component.damageTypeOverride = DamageType.Light;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=yellow]LIGHT[/color]",
            "[i]THE BULLETS NO LONGER SHOOT OUT. INSTEAD THE GUN SIMPLY BLESSES THE ENEMIES TO DEATH.[/i]"
        };
    }
}
public class TCDBarrelDark : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Blue;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Barrel;
        component.humanReadableName = "DARK BARREL";
        component.Frame = 741;
        component.damageTypeOverride = DamageType.Darkness;
        component.rarityColor = TowerComponent.Blue;
        component.humanReadableDescription = new string[]{
            "[color=purple]DARKNESS[/color]",
            "[i]THAT SAME WITCH DEVISED THIS DEVICE TOO. TRULY AN EVIL BEING.[/i]"
        };
    }
}
public class TCDBarrelPale : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Barrel;
        component.humanReadableName = "PALE BARREL";
        component.Frame = 861;
        component.damageMultiplier = 0.85f;
        component.damageTypeOverride = DamageType.Pure;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]SO, HOW DO YOU LIKE IT HERE, A?[/color]",
            "[i]THIS BARREL COMES FROM AN ENTIRELY DIFFERENT DIMENSION. A WHITE FEATHER WAS DISCOVERED ALONGSIDE IT.[/i]"
        };
    }
}
public class TCDBarrelOmega : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Barrel;
        component.humanReadableName = "OMEGA BARREL";
        component.Frame = 726;
        component.damageMultiplier = 8f;
        component.cooldownMultiplier = 3;
        component.damageTypeOverride = DamageType.Physical;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]OVER 9000.[/color]",
            "[i]THIS CAN MAKE QUITE A BOOM.[/i]"
        };
    }

    public override void HandleBulletSpawn(BulletScript bullet, TowerComponent tower)
    {
        base.HandleBulletSpawn(bullet, tower);
        bullet.Damages.Add(new Tuple<DamageType, float, TowerComponentDataBase>(DamageType.Pure, 5, this));
    }
}
public class TCDBarrelSingularity : TowerComponentDataBase
{
    public override ComponentRarity Rarity => ComponentRarity.Red;
    public override void Apply(TowerComponent component)
    {
        component.componentType = ComponentType.Barrel;
        component.humanReadableName = "SINGULARITY BARREL";
        component.Frame = 727;
        component.damageTypeOverride = DamageType.Physical;
        component.rarityColor = TowerComponent.Red;
        component.humanReadableDescription = new string[]{
            "[color=red]AND SO THE DARKNESS CAME[/color]",
            "[i]IT BUZZES SLIGHTLY WHEN HELD.[/i]"
        };
    }

    public override void HandleBulletSpawn(BulletScript bullet, TowerComponent tower)
    {
        base.HandleBulletSpawn(bullet, tower);
        float cumulativeDamage = 0;
        for (int i = 0; i < bullet.Damages.Count; i++)
        {
            Tuple<DamageType, float, TowerComponentDataBase> t = bullet.Damages[i];
            cumulativeDamage += t.Item2;
            bullet.Damages[i] = new Tuple<DamageType, float, TowerComponentDataBase>(t.Item1, t.Item2 / 2, t.Item3);
        }

        bullet.Damages.Add(new Tuple<DamageType, float, TowerComponentDataBase>(DamageType.Pure, cumulativeDamage / 2f, this));
    }
}


public enum DamageType
{
    Unspecified,
    Physical,
    Cold,
    Fire,
    Darkness,
    Light,
    Pure
}

public enum ComponentType
{
    Gun,
    Base,
    Igniter,
    Core,
    Barrel
}