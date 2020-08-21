using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ChestScript : StaticBody2D
{
    public int Value { get; set; }

    private CPUParticles2D _particles;

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this._particles = this.GetChildren().Cast<Node>().Where(c => c is CPUParticles2D).Cast<CPUParticles2D>().First();
    }

    public override void _Process(float delta)
    {
        if (this.Value > 25 && this.Value < 60)
        {
            this._particles.Color = new Color(0.2f, 0.8f, 0.4f);
        }
        else
        {
            if (this.Value >= 60 && this.Value < 120)
            {
                this._particles.Color = new Color(0.2f, 0.3f, 0.8f);
            }
            else
            {
                if (this.Value >= 120 && this.Value < 500)
                {
                    this._particles.Color = new Color(0.6f, 0.1f, 0.6f);
                }
                else
                {
                    if (this.Value >= 500 && this.Value < 1000)
                    {
                        this._particles.Color = new Color(0.9f, 0.6f, 0.0f);
                    }
                    else
                    {
                        if (this.Value >= 1000)
                        {
                            this._particles.Color = new Color(0.9f, 0.0f, 0.0f);
                        }
                    }
                }
            }
        }

        base._Process(delta);
    }

    public Random Rand { get; set; } = new Random();

    public void Sacrifice()
    {
        GameScript gs = (GameScript)this.GetParent();
        if (this.Value > 100)
        {
            float purpleMod = ((this.Value - 100)) / 2400f;
            gs.PurpleChance = Mathf.Min(1, purpleMod + gs.PurpleChance);
            if (this.Value > 250)
            {
                float redMod = (this.Value - 250) / 9750f;
                gs.RedChance = Mathf.Min(1, redMod + gs.RedChance);
            }

            gs.RedoLuckText();
        }

        gs.voidParticles.Position = this.Position;
        gs.voidEmissionTime = 1;
        gs.voidParticles.Emitting = true;
        gs.Sounds[SoundName.ChestSacrifice].Play();
        this.QueueFree();
    }

    public void Open()
    {
        if (!this.Visible)
        {
            return;
        }

        List<Vector2> positionOffsets = new List<Vector2>(8)
        {
            new Vector2(-16, -16),
            new Vector2(0, -16),
            new Vector2(16, -16),
            new Vector2(16, 0),
            new Vector2(-16, 0),
            new Vector2(-16, 16),
            new Vector2(0, 16),
            new Vector2(16, 16),
        };

        GameScript gs = (GameScript)this.GetParent();
        gs.Sounds[SoundName.ChestOpen].Play();
        bool genAny = false;
        for (int i = 0; i < this.Rand.Next(1, 4); ++i)
        {
            System.Collections.Generic.List<Tuple<TowerComponentDataBase, int>> componentList = TowerComponent.WeightedComponents[(ComponentType)this.Rand.Next(5)];
            System.Collections.Generic.List<Tuple<TowerComponentDataBase, int>> localCopy = new System.Collections.Generic.List<Tuple<TowerComponentDataBase, int>>(componentList.Where(c => c.Item2 <= this.Value));
            if (localCopy.Count <= 0)
            {
                break;
            }

            Tuple<TowerComponentDataBase, int> tcdb = null;
            while (localCopy.Count > 0)
            {
                tcdb = GetWeightedItem(localCopy, this.Rand);
                localCopy.Remove(tcdb);
                if (tcdb.Item1.Rarity == ComponentRarity.Red)
                {
                    bool passes = this.Rand.NextDouble() < gs.RedChance;
                    if (passes)
                    {
                        gs.RedChance = Math.Max(0, gs.RedChance - gs.RedActiveDecay);
                        gs.RedoLuckText();
                        break;
                    }
                    else
                    {
                        gs.RedChance = Math.Max(0, gs.RedChance * gs.RedPassiveDecay);
                        tcdb = null;
                        gs.RedoLuckText();
                        continue;
                    }
                }
                else
                {
                    if (tcdb.Item1.Rarity == ComponentRarity.Purple)
                    {
                        bool passes = this.Rand.NextDouble() < gs.PurpleChance;
                        if (passes)
                        {
                            gs.PurpleChance = Math.Max(0, gs.PurpleChance - gs.PurpleActiveDecay);
                            gs.RedoLuckText();
                            break;
                        }
                        else
                        {
                            gs.PurpleChance = Math.Max(0, gs.PurpleChance * gs.PurplePassiveDecay);
                            tcdb = null;
                            gs.RedoLuckText();
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (tcdb != null)
            {
                this.Value -= (int)Mathf.Ceil(tcdb.Item2 / 3F);
                TowerComponent tc = TowerComponent.SpawnComponent(tcdb.Item1);
                if (tc.rarityColor == TowerComponent.Red)
                {
                    ((GameScript)this.GetParent()).Sounds[SoundName.RedDrop].Play();
                }
                else
                {
                    if (tc.rarityColor == TowerComponent.Purple)
                    {
                        ((GameScript)this.GetParent()).Sounds[SoundName.PurpleDrop].Play();
                    }
                }

                Vector2 pos = positionOffsets[this.Rand.Next(positionOffsets.Count)];
                positionOffsets.Remove(pos);
                tc.Position = this.Position + pos;
                genAny = true;
                this.GetParent().AddChild(tc);
            }
        }

        if (!genAny)
        {
            ((GameScript)this.GetParent()).coinValue += this.Value;
            int rm = (int)Math.Ceiling(this.Value / 250f);
            for (int i = 0; i < 5 + rm; ++i)
            {
                Vector2 basePos = this.Position + Vector2.Up.Rotated((float)(this.Rand.NextDouble() * Math.PI)) * (float)(this.Rand.NextDouble() * 9);
                CoinScript kb2d = (CoinScript)((GameScript)this.GetParent()).coin.Duplicate();
                kb2d.Position = this.Position;
                kb2d.Visible = true;
                kb2d.Value = 1;
                this.GetParent().AddChild(kb2d);
            }
        }

        this.QueueFree();
    }

    public static Tuple<T, int> GetWeightedItem<T>(IEnumerable<Tuple<T, int>> collection, Random rand = null)
    {
        int weight = rand.Next(collection.Sum(w => w.Item2)) + 1;
        Queue<Tuple<T, int>> q = new Queue<Tuple<T, int>>(collection);
        Tuple<T, int> wi = q.Dequeue();
        while (true)
        {
            weight -= wi.Item2;
            if (weight > 0)
            {
                wi = q.Dequeue();
            }
            else
            {
                break;
            }
        }

        return wi;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
