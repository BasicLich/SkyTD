using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameScript : Node2D
{
	private KinematicBody2D _baseEnemy;
	private StaticBody2D _baseTower;
	public CPUParticles2D spawnParticles;
	public CPUParticles2D voidParticles;
	private CPUParticles2D _buildDoneParticle;
	private CPUParticles2D _buildFailedParticle;
	private AnimatedSprite _basePopupSprite;
	private AnimatedSprite _minecartSprite;

	private AnimatedSprite[] _timeSpeedSprites = new AnimatedSprite[4];

	public ColorRect _damageSplash;
	private RichTextLabel _towerDescription;
	private RichTextLabel _waveDescription;
	private RichTextLabel _luckDescription;
	private RichTextLabel _timeText;
	public Camera2D _camera;

	private SplitContainer _victoryScreen;
	private SplitContainer _defeatScreen;

	public KinematicBody2D baseBullet;
	public KinematicBody2D coin;
	public KinematicBody2D oldMan;

	public float RedChance { get; set; } = 0f;
	public float PurpleChance { get; set; } = 0.12f;
	public float MinPurpleChance { get; } = 0.12f;
	public float MinRedChance { get; } = 0f;
	public float RedPassiveDecay { get; } = 0.9f;
	public float PurplePassiveDecay { get; } = 1f;
	public float RedActiveDecay { get; } = 0.5f;
	public float PurpleActiveDecay { get; } = 0.25f;

	public System.Collections.Generic.Dictionary<SoundName, AudioStreamPlayer> Sounds { get; } = new System.Collections.Generic.Dictionary<SoundName, AudioStreamPlayer>();

	public Queue<Action> Todo { get; } = new Queue<Action>();

	public float updatesExisted;
	public float oldManAnimationTimer;
	private float _emissionTime;
	public float playTime;
	public float voidEmissionTime;
	private float _waveTimer;
	private int _waveSpawnsLeft;
	private EnemyEffect[] _currentEffects;
	private bool _areEffectsActive;
	public EnemyWave currentWave;
	public float waveTimeDelay = 0;
	public int waveIndex = -1;
	public int waveSpawnIndex;

	public int kills;
	public int towersBuilt;
	public int towersDestroyed;
	public float totalDamage;
	public float maxDamage;
	public int secrets;

	public bool isMinecartWave;

	public bool isFadeTutorialLocked;
	public bool isCameraTutorialLocked;
	public bool isPlacementTutorialLocked;
	public bool isTutorial;
	public bool treasureStolen;

	private EnemyDataBase _spawnEnemy;
	private Path2D _pathMinecart1;
	private Path2D _pathMinecart2;
	private Path2D _pathLand;
	private Path2D _pathWater;

	private Path2D[] _spawns;

	private Area2D _buildableArea;
	private StaticBody2D _chest;
	public float _damageTime;
	private bool _pauseWaves;
	private Node2D _mainMenuNode;

	public int numEnemies;
	public int coinValue;
	public bool canSpawnNextWave;
	public Random Rand { get; set; } = new Random();

	public bool isMouseInBuildableArea;
	public bool IsWaveActive => this._waveSpawnsLeft > 0 || this.numEnemies > 0;

	public TowerComponent CurrentlyDraggedTower { get; set; }
	public TowerComponent ComponentMouseOver
	{
		get => this.componentMouseOver;
		set
		{
			this.componentMouseOver = value;
			this._towerDescription.BbcodeText = string.Empty;
			if (value != null)
			{
				this._towerDescription.AppendBbcode("[right]");
				this._towerDescription.AppendBbcode(this.componentMouseOver.humanReadableName.Replace(" ", "   ") + '\n');
				foreach (string line in this.componentMouseOver.humanReadableDescription)
				{
					this._towerDescription.AppendBbcode(line.Replace(" ", "   ") + '\n');
				}
			}
		}
	}

	public int Lives
	{
		get => this.lives;
		set
		{
			this.lives = value;
			this.UpdateLiveCount();
		}
	}
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this._baseEnemy = (KinematicBody2D)this.FindNode("BaseEnemy");
		this._baseTower = (StaticBody2D)this.FindNode("BaseTower");
		this._chest = (StaticBody2D)this.FindNode("TreasureChest");
		this.baseBullet = (KinematicBody2D)this.FindNode("BaseBullet");
		this.coin = (KinematicBody2D)this.FindNode("BaseCoin");
		this.oldMan = (KinematicBody2D)this.FindNode("OldMan");
		this._buildableArea = (Area2D)this.FindNode("BuildableArea1");
		this.spawnParticles = (CPUParticles2D)this.FindNode("SpawnParticle");
		this.voidParticles = (CPUParticles2D)this.FindNode("VoidParticle");
		this._buildDoneParticle = (CPUParticles2D)this.FindNode("BuildDoneParticle");
		this._buildFailedParticle = (CPUParticles2D)this.FindNode("BuildFailedParticle");
		this._basePopupSprite = (AnimatedSprite)this.FindNode("BasePopupSprite");
		this._pathMinecart1 = (Path2D)this.FindNode("MinecartPath1");
		this._pathMinecart2 = (Path2D)this.FindNode("MinecartPath2");
		this._pathLand = (Path2D)this.FindNode("LandPath");
		this._pathWater = (Path2D)this.FindNode("WaterPath");
		this._minecartSprite = (AnimatedSprite)this.FindNode("Minecart");

		this._damageSplash = (ColorRect)this.FindNode("Canvas").FindNode("ColorRect");
		this._camera = (Camera2D)this.FindNode("Camera2D");
		this._hpContainer = new AnimatedSprite[] {
			(AnimatedSprite)this.FindNode("HP0"),
			(AnimatedSprite)this.FindNode("HP1"),
			(AnimatedSprite)this.FindNode("HP2"),
			(AnimatedSprite)this.FindNode("HP3"),
			(AnimatedSprite)this.FindNode("HP4"),
		};

		this._timeSpeedSprites[0] = (AnimatedSprite)this.FindNode("Canvas").FindNode("TimeContainer").FindNode("HBoxContainer").FindNode("Forward1");
		this._timeSpeedSprites[1] = (AnimatedSprite)this.FindNode("Canvas").FindNode("TimeContainer").FindNode("HBoxContainer").FindNode("Forward2");
		this._timeSpeedSprites[2] = (AnimatedSprite)this.FindNode("Canvas").FindNode("TimeContainer").FindNode("HBoxContainer").FindNode("Forward3");
		this._timeSpeedSprites[3] = (AnimatedSprite)this.FindNode("Canvas").FindNode("TimeContainer").FindNode("HBoxContainer").FindNode("Forward4");

		this._towerDescription = (RichTextLabel)this.FindNode("Canvas").FindNode("MarginContainer3").FindNode("RichTextLabel");
		this._waveDescription = (RichTextLabel)this.FindNode("Canvas").FindNode("MarginContainer").FindNode("RichTextLabel");
		this._luckDescription = (RichTextLabel)this.FindNode("Canvas").FindNode("MarginContainer2").FindNode("RichTextLabel");
		this._timeText = (RichTextLabel)this.FindNode("Canvas").FindNode("TimeContainer").FindNode("RichTextLabel");
		this._victoryScreen = (SplitContainer)this.FindNode("Canvas").FindNode("VictoryScreen");
		this._defeatScreen = (SplitContainer)this.FindNode("Canvas").FindNode("DefeatScreen");
		foreach (SoundName sn in Enum.GetValues(typeof(SoundName)))
		{
			this.Sounds.Add(sn, (AudioStreamPlayer)this.FindNode("SoundPlayers").FindNode(Enum.GetName(typeof(SoundName), sn)));
		}

		this._victoryScreen.FindNode("MarginContainer4").FindNode("Button").Connect("pressed", this, "ExitToMainMenu");
		this._defeatScreen.FindNode("MarginContainer4").FindNode("Button").Connect("pressed", this, "ExitToMainMenu");
	}

	public void SetMainMenu(Node2D mm) => this._mainMenuNode = mm;

	public void ExitToMainMenu()
	{
		this.GetParent().AddChild(this._mainMenuNode);
		this.QueueFree();
	}

	public void RedoLuckText()
	{
		this._luckDescription.BbcodeText = string.Empty;
		this._luckDescription.AppendBbcode($"[right]");
		this._luckDescription.AppendBbcode($"[color=red]RED[/color] CHANCE: { (int)Mathf.Round(this.RedChance * 100) }\n");
		this._luckDescription.AppendBbcode($"[color=purple]PURPLE[/color] CHANCE: { (int)Mathf.Round(this.PurpleChance * 100) }\n");
		this._luckDescription.AppendBbcode($"\n");
		this._luckDescription.AppendBbcode($"{ (this.IsWaveActive ? "ACTIVE" : "AVAILABLE") } EFFECTS: " + string.Join(", ", this._currentEffects.Select(e => e.Name)));
	}

	public void GenerateEffects()
	{
		this._currentEffects = new EnemyEffect[0];
		List<EnemyEffect> potentialEffects = new List<EnemyEffect>();
		int passes = this.waveIndex / 5;
		uint mask = 0;
		while (--passes > 0)
		{
			foreach (Tuple<float, float, uint, EnemyEffect> eff in EnemyEffect.Effects.Where(eff => !potentialEffects.Contains(eff.Item4)))
			{
				if (eff.Item3 == 0 || (mask & eff.Item3) == 0u && this.Rand.NextDouble() < eff.Item1 + eff.Item2 * this.waveIndex)
				{
					mask |= eff.Item3;
					potentialEffects.Add(eff.Item4);
					break;
				}
			}
		}

		this._currentEffects = potentialEffects.ToArray();
	}

	public void AdvanceWave()
	{
		if (this.waveIndex + 1 >= EnemyWave.AllWaves.Length)
		{
			this._pauseWaves = true;
			return;
		}

		this.waveTimeDelay = 15 + this.waveIndex;
		this.currentWave = EnemyWave.AllWaves[++this.waveIndex];
		this.isMinecartWave = this.currentWave.WaveType != WaveType.Sea && ((this.waveIndex > 3 && this.Rand.NextDouble() < 0.3f) || this.currentWave.WaveType == WaveType.Minecart);
		this._waveDescription.BbcodeText = string.Empty;
		this.GenerateEffects();
		this._areEffectsActive = false;
		this.RedoLuckText();
		this._waveDescription.AppendBbcode($"[color=red]WAVE INCOMING IN APPROXIMATELY { (int)this.waveTimeDelay } SECONDS![/color]\n");
		foreach (Tuple<EnemyDataBase, int> wDat in this.currentWave.Spawns)
		{
			this._waveDescription.AppendBbcode($"{ wDat.Item2 }x { wDat.Item1.Name.ToUpper() }\n");
		}

		if (this.isMinecartWave)
		{
			this._waveDescription.AppendBbcode($"LOOKS LIKE SOME OF THEM WILL BE ARRIVING ON RAILS.\n");
		}
	}

	public void CreatePopupSprite(Node2D owner, int frame, float lifespan, string animationNode)
	{
		PopupSpriteScript asp = (PopupSpriteScript)this._basePopupSprite.Duplicate();
		asp.Frame = frame;
		asp.lifespan = lifespan;
		asp.Visible = true;
		((AnimationPlayer)asp.GetChildren()[0]).PlaybackSpeed = ((AnimationPlayer)asp.GetChildren()[0]).GetAnimation(animationNode).Length / lifespan;
		((AnimationPlayer)asp.GetChildren()[0]).Play(animationNode);
		owner.AddChild(asp);
	}

	public EnemyScript SpawnEnemy(Path2D pathToFollow, EnemyDataBase data, bool doEffects = true)
	{
		KinematicBody2D n = (KinematicBody2D)this._baseEnemy.Duplicate();
		n.Set("baseObject", false);
		n.Set("path", pathToFollow);
		EnemyScript es = (EnemyScript)n;
		es.data = data;
		if (this._areEffectsActive)
		{
			foreach (EnemyEffect eff in this._currentEffects)
			{
				es.AddEffect(eff);
			}
		}

		n.Position = this.spawnParticles.Position = pathToFollow.Curve.GetBakedPoints()[0];
		if (pathToFollow == this._pathMinecart1 || pathToFollow == this._pathMinecart2)
		{
			AnimatedSprite asp = (AnimatedSprite)this._minecartSprite.Duplicate();
			asp.Visible = true;
			es.SpriteToAttach = asp;
			asp.Position = new Vector2(0, 5);
			es.speed *= 1.5f;
		}

		foreach (EnemyEffect eff in es.data.Effect)
		{
			es.AddEffect(eff);
		}

		if (doEffects)
		{
			this.spawnParticles.Emitting = true;
			this._emissionTime = 0.25F;
		}

		this.AddChild(n);
		this.numEnemies += 1;
		return es;
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		while (this.Todo.Count > 0)
		{
			this.Todo.Dequeue()();
		}

		this.playTime += delta;
		if (this.numEnemies <= 0)
		{
			this.numEnemies = this.GetChildren().Cast<Node>().Where(c => c is EnemyScript).Cast<EnemyScript>().Where(s => s.IsInsideTree() && !s.baseObject && !s.IsQueuedForDeletion()).Sum(n => 1);
		}

		if (this.numEnemies <= 0 && this._waveSpawnsLeft <= 0 && this.canSpawnNextWave && !this._pauseWaves)
		{
			this.numEnemies = 0;
			this.canSpawnNextWave = false;
			if (!this.treasureStolen)
			{
				float mod = 0.5f;
				do
				{
					mod /= 2;
					ChestScript chest = (ChestScript)this._chest.Duplicate();
					chest.Visible = true;
					chest.Position = this.oldMan.Position + new Vector2(0, 1).Rotated((float)(this.Rand.NextDouble() * Math.PI)) * (16 + (float)this.Rand.NextDouble() * 32);
					chest.Value = this.coinValue;
					this.AddChild(chest);
					this.Sounds[SoundName.WaveComplete].Play();
				} while (this.Rand.NextDouble() <= mod);
			}
			else
			{
				this.coinValue = 0;
				this.treasureStolen = false;
			}

			this.AdvanceWave();
		}

		if (Engine.TimeScale <= 1 - float.Epsilon)
		{
			if (Engine.TimeScale >= 0.5F - float.Epsilon && Engine.TimeScale < 1 - float.Epsilon && this.numEnemies <= 0 && this._waveSpawnsLeft <= 0)
			{
				Engine.TimeScale = 0;
				this.UpdateTimeText();
			}

			if (Engine.TimeScale <= 0.5F - float.Epsilon && (this.numEnemies > 0 || this._waveSpawnsLeft > 0))
			{
				Engine.TimeScale = 0.44f;
				this.UpdateTimeText();
			}
		}

		this.oldMan.Scale = Vector2.One * (1 + (0.5f * Mathf.Max(0, this.oldManAnimationTimer -= delta)));
		this._emissionTime -= delta;
		this.voidEmissionTime -= delta;
		this._damageTime -= delta;
		if (this._damageTime > -0.1f && !this.isFadeTutorialLocked)
		{
			this._damageSplash.Color = new Color(1, 0, 0, Mathf.Max(0, this._damageTime * 5));
		}

		if (this._emissionTime < 0)
		{
			this.spawnParticles.Emitting = false;
		}

		if (this.voidEmissionTime < 0)
		{
			this.voidParticles.Emitting = false;
		}

		if (!this.IsWaveActive && !this._pauseWaves)
		{
			if (this.waveTimeDelay > 0)
			{
				this.waveTimeDelay -= delta;
			}
			else
			{
				if (this._waveSpawnsLeft == 0 && this.waveIndex != -1 && !this.canSpawnNextWave)
				{
					this.Sounds[SoundName.WaveStart].Play();
					this._waveSpawnsLeft = this.currentWave.SpawnIndices.Length;
					this.waveSpawnIndex = 0;
					this._spawns = this.currentWave.WaveType == WaveType.Sea ? new Path2D[] { this._pathWater } : this.isMinecartWave ? new Path2D[] { this._pathLand, this._pathMinecart1, this._pathMinecart2 } : new Path2D[] { this._pathLand };
					this._waveDescription.BbcodeText = string.Empty;
				}
			}
		}

		if (this._waveSpawnsLeft > 0)
		{
			this._waveTimer -= delta;
			if (this._waveTimer <= 0)
			{
				this._waveTimer = this.currentWave.SpawnDelay;
				this._waveSpawnsLeft -= 1;
				if (this._waveSpawnsLeft == 0)
				{
					this.canSpawnNextWave = true;
				}

				this.SpawnEnemy(this._spawns[this._spawns.Length > 1 ? this.Rand.Next(this._spawns.Length) : 0], this.currentWave.Spawns[this.currentWave.SpawnIndices[this.waveSpawnIndex++]].Item1);
			}
		}

		if (!this.isCameraTutorialLocked)
		{

			Transform2D canvasTransform = this.GetCanvasTransform();

			if (this._mousePosition.x > this.GetViewportRect().Size.x - 64)
			{
				this._camera.Position += new Vector2(60, 0) * delta;
			}

			if (this._mousePosition.x < 64)
			{
				this._camera.Position -= new Vector2(60, 0) * delta;
			}

			if (this._mousePosition.y > this.GetViewportRect().Size.y - 64)
			{
				this._camera.Position += new Vector2(0, 60) * delta;
			}

			if (this._mousePosition.y < 64)
			{
				this._camera.Position -= new Vector2(0, 60) * delta;
			}

			Vector2 min = -canvasTransform.origin / canvasTransform.Scale;
			Vector2 viewSize = this.GetViewportRect().Size / canvasTransform.Scale;
			Vector2 max = min + viewSize;
			if (max.x > this.GetViewportRect().Size.x)
			{
				this._camera.Position -= new Vector2((max.x - this.GetViewportRect().Size.x), 0);
			}

			if (min.x < 0)
			{
				this._camera.Position += new Vector2(Mathf.Abs(min.x), 0);
			}

			if (max.y > this.GetViewportRect().Size.y)
			{
				this._camera.Position -= new Vector2(0, (max.y - this.GetViewportRect().Size.y));
			}

			if (min.y < 0)
			{
				this._camera.Position += new Vector2(0, Mathf.Abs(min.y));
			}
		}
	}

	private Vector2 _mousePosition = default;
	private Vector2 _mouseDragPos = default;
	private bool _draggingCamera = false;

	public void UpdateTimeText()
	{
		int s = (int)Mathf.Round(Engine.TimeScale);
		switch (s)
		{
			case 0:
			{
				this._timeSpeedSprites[0].Frame = 604;
				this._timeSpeedSprites[1].Frame = 0;
				this._timeSpeedSprites[2].Frame = 0;
				this._timeSpeedSprites[3].Frame = 0;
				this._timeText.Text = "PAUSED";
				break;
			}

			case 1:
			{
				this._timeSpeedSprites[0].Frame = 964;
				this._timeSpeedSprites[1].Frame = 0;
				this._timeSpeedSprites[2].Frame = 0;
				this._timeSpeedSprites[3].Frame = 0;
				this._timeText.Text = "X1";
				break;
			}

			case 2:
			{
				this._timeSpeedSprites[0].Frame = 964;
				this._timeSpeedSprites[1].Frame = 964;
				this._timeSpeedSprites[2].Frame = 0;
				this._timeSpeedSprites[3].Frame = 0;
				this._timeText.Text = "X2";
				break;
			}

			case 3:
			{
				this._timeSpeedSprites[0].Frame = 964;
				this._timeSpeedSprites[1].Frame = 964;
				this._timeSpeedSprites[2].Frame = 964;
				this._timeSpeedSprites[3].Frame = 0;
				this._timeText.Text = "X3";
				break;
			}

			case 4:
			{
				this._timeSpeedSprites[0].Frame = 964;
				this._timeSpeedSprites[1].Frame = 964;
				this._timeSpeedSprites[2].Frame = 964;
				this._timeSpeedSprites[3].Frame = 964;
				this._timeText.Text = "X4";
				break;
			}

			default:
			{
				break;
			}
		}
		if (Engine.TimeScale > 0 + float.Epsilon && Engine.TimeScale < 1 - float.Epsilon)
		{
			this._timeSpeedSprites[0].Frame = 605;
			this._timeSpeedSprites[1].Frame = 0;
			this._timeSpeedSprites[2].Frame = 0;
			this._timeSpeedSprites[3].Frame = 0;
			this._timeText.Text = "SLOW";
		}
	}

	private float _lastTimeScale;
	public override void _Input(InputEvent @event)
	{
		if (this._victoryScreen.Visible || this._defeatScreen.Visible)
		{
			return;
		}

		if (@event is InputEventMouseMotion mouseMotion)
		{
			if (this._draggingCamera && !this.isCameraTutorialLocked)
			{
				this._camera.Position -= (mouseMotion.Position - this._mousePosition);
			}

			this._mousePosition = mouseMotion.Position;
			Vector2 mousePosition = this.GetGlobalMousePosition();
			RID space = this.GetWorld2d().Space;
			Physics2DDirectSpaceState state = Physics2DServer.SpaceGetDirectState(space);
			Godot.Collections.Array collision = state.IntersectPoint(mousePosition, collideWithAreas: true);
			TowerComponent toPickup = null;
			this.isMouseInBuildableArea = false;
			if (collision.Count > 0)
			{
				foreach (object o in collision.Cast<Dictionary>().Where(d => d.Contains("collider")).Select(d => d["collider"]))
				{
					if (o is TowerComponent towerComponent && !towerComponent.baseObject)
					{
						if (toPickup == null || towerComponent.Position.DistanceTo(mousePosition) < toPickup.Position.DistanceTo(mousePosition))
						{
							toPickup = towerComponent;
						}
					}

					if (o is BuildableArea ba)
					{
						this.isMouseInBuildableArea = true;
					}
				}
			}

			if (this.ComponentMouseOver != toPickup)
			{
				this.ComponentMouseOver = toPickup;
			}
		}

		if (@event is InputEventKey key)
		{
			if (key.Scancode == (uint)KeyList.Backspace)
			{
				this.waveTimeDelay = 0;
			}

			if (key.Scancode == (uint)KeyList.Space && key.IsPressed())
			{
				if (Engine.TimeScale < 1 - float.Epsilon)
				{
					Engine.TimeScale = this._lastTimeScale;
				}
				else
				{
					this._lastTimeScale = Engine.TimeScale;
					Engine.TimeScale = 0;
				}

				this.UpdateTimeText();
			}

			if (key.Scancode == (uint)KeyList.Braceright && Engine.TimeScale < 4 && key.IsPressed())
			{
				Engine.TimeScale += 1F;
				this.UpdateTimeText();
			}

			if (key.Scancode == (uint)KeyList.Braceleft && Engine.TimeScale > 0 && key.IsPressed())
			{
				Engine.TimeScale -= 1F;
				if (Engine.TimeScale < 0)
				{
					Engine.TimeScale = 0;
				}

				this.UpdateTimeText();
			}

			if (key.Scancode == (uint)KeyList.Enter && this.waveTimeDelay > 0 && !this._areEffectsActive && this._currentEffects.Length > 0 && key.IsPressed())
			{
				this._areEffectsActive = true;
				this.Sounds[SoundName.ChestSacrifice].Play();
				foreach (EnemyEffect eff in this._currentEffects)
				{
					this.RedChance += eff.RedValue;
					this.PurpleChance += eff.PurpleValue;
				}

				this.RedoLuckText();
			}
		}

		if (@event is InputEventMouseButton middle)
		{
			if (middle.IsPressed() && middle.ButtonIndex == (int)ButtonList.Middle)
			{
				this._mouseDragPos = middle.Position;
				this._draggingCamera = true;
			}

			if (!middle.IsPressed() && middle.ButtonIndex == (int)ButtonList.Middle)
			{
				this._draggingCamera = false;
			}
		}

		if (@event is InputEventMouseButton mouseBtn && mouseBtn.Pressed)
		{
			if (mouseBtn.ButtonIndex == (int)ButtonList.WheelDown)
			{
				this._camera.Zoom *= 1.1f;
				if (this._camera.Zoom > Vector2.One)
				{
					this._camera.Zoom = Vector2.One;
				}
			}

			if (mouseBtn.ButtonIndex == (int)ButtonList.WheelUp)
			{
				this._camera.Zoom *= 0.9f;
			}

			if (mouseBtn.ButtonIndex == (int)ButtonList.Right && !this.isPlacementTutorialLocked)
			{
				Vector2 mousePos = this.GetGlobalMousePosition();
				RID space = this.GetWorld2d().Space;
				Physics2DDirectSpaceState state = Physics2DServer.SpaceGetDirectState(space);
				Godot.Collections.Array collision = state.IntersectPoint(mousePos);
				if (collision.Count > 0)
				{
					foreach (object o in collision.Cast<Dictionary>().Where(d => d.Contains("collider")).Select(d => d["collider"]))
					{
						if (o is BaseTower tower && !tower.baseObject)
						{
							if (tower.Components.Count <= 0 || !tower.Components.Values.Any(c => c != null))
							{
								++this.towersDestroyed;
								tower.QueueFree();
								CPUParticles2D effect = (CPUParticles2D)this._buildDoneParticle.Duplicate();
								effect.Position = mousePos;
								effect.Emitting = true;
								effect.OneShot = true;
								effect.Visible = true;
								effect.Set("baseObject", false);
								this.Sounds[SoundName.BuildDestroyed].Play();
								this.AddChild(effect);
								break;
							}
						}

						if (o is ChestScript cs && cs.Visible)
						{
							cs.Sacrifice();
						}
					}
				}
			}

			if (mouseBtn.ButtonIndex == (int)ButtonList.Left && !this.isPlacementTutorialLocked)
			{
				Vector2 mousePos = this.GetGlobalMousePosition();
				if (this.CurrentlyDraggedTower == null)
				{
					RID space = this.GetWorld2d().Space;
					Physics2DDirectSpaceState state = Physics2DServer.SpaceGetDirectState(space);
					Godot.Collections.Array collision = state.IntersectPoint(mousePos);
					if (collision.Count > 0)
					{
						bool hasTower = false;
						TowerComponent toPickup = null;
						foreach (object o in collision.Cast<Dictionary>().Where(d => d.Contains("collider")).Select(d => d["collider"]))
						{
							if (o is ChestScript cs && cs.Visible)
							{
								cs.Open();
								this.Sounds[SoundName.ChestOpen].Play();
								return;
							}

							if (o is TowerComponent towerComponent && !towerComponent.baseObject)
							{
								if (toPickup == null || towerComponent.Position.DistanceTo(mousePos) < toPickup.Position.DistanceTo(mousePos))
								{
									toPickup = towerComponent;
								}
							}
						}

						if (toPickup != null)
						{
							hasTower = true;
							this.CurrentlyDraggedTower = toPickup;
							toPickup.dragged = true;
							this.Sounds[SoundName.PickupPowerup].Play();
							if (toPickup.HasOwner)
							{
								toPickup.DetachFromTower(toPickup.OwningTower);
							}
						}

						if (!hasTower && this.isMouseInBuildableArea)
						{
							CPUParticles2D effect = (CPUParticles2D)this._buildFailedParticle.Duplicate();
							effect.Position = mousePos;
							effect.Emitting = true;
							effect.OneShot = true;
							effect.Visible = true;
							this.Sounds[SoundName.BuildFailed].Play();
							effect.Set("baseObject", false);
							this.AddChild(effect);
						}
					}
					else
					{
						if (this.isMouseInBuildableArea)
						{
							CPUParticles2D effect = (CPUParticles2D)this._buildDoneParticle.Duplicate();
							effect.Position = mousePos;
							effect.Emitting = true;
							effect.OneShot = true;
							effect.Visible = true;
							effect.Set("baseObject", false);
							this.AddChild(effect);
							BaseTower bt = (BaseTower)this._baseTower.Duplicate();
							bt.Visible = true;
							bt.Position = mousePos;
							bt.baseObject = false;
							this.AddChild(bt);
							++this.towersBuilt;
							this.Sounds[SoundName.DialogSkip].Play();
						}
						else
						{
							CPUParticles2D effect = (CPUParticles2D)this._buildFailedParticle.Duplicate();
							effect.Position = mousePos;
							effect.Emitting = true;
							effect.OneShot = true;
							effect.Visible = true;
							effect.Set("baseObject", false);
							this.AddChild(effect);
							this.Sounds[SoundName.BuildFailed].Play();
						}
					}
				}
				else
				{
					RID space = this.GetWorld2d().Space;
					Physics2DDirectSpaceState state = Physics2DServer.SpaceGetDirectState(space);
					Godot.Collections.Array collision = state.IntersectPoint(mousePos);
					if (collision.Count > 0)
					{
						foreach (object o in collision.Cast<Dictionary>().Where(d => d.Contains("collider")).Select(d => d["collider"]))
						{
							if (o is BaseTower tower && !tower.baseObject)
							{
								TowerComponent toHandle = null;
								if (tower.Components.ContainsKey(this.CurrentlyDraggedTower.componentType))
								{
									toHandle = tower.Components[this.CurrentlyDraggedTower.componentType];
									toHandle.dragged = true;
									toHandle.DetachFromTower(tower);
									tower.Components.Remove(this.CurrentlyDraggedTower.componentType);
								}

								tower.Components.Add(this.CurrentlyDraggedTower.componentType, this.CurrentlyDraggedTower);
								this.CurrentlyDraggedTower.Attach2Tower(tower);
								this.CurrentlyDraggedTower.dragged = false;
								this.CurrentlyDraggedTower = toHandle;
								this.Sounds[SoundName.PickupPowerup].Play();
								break;
							}
						}
					}
					else
					{
						if (this.isMouseInBuildableArea)
						{
							this.CurrentlyDraggedTower.dragged = false;
							this.CurrentlyDraggedTower = null;
						}
						else
						{
							CPUParticles2D effect = (CPUParticles2D)this._buildFailedParticle.Duplicate();
							effect.Position = mousePos;
							effect.Emitting = true;
							effect.OneShot = true;
							effect.Visible = true;
							effect.Set("baseObject", false);
							this.AddChild(effect);
							this.Sounds[SoundName.BuildFailed].Play();
						}
					}
				}
			}
		}
	}

	public void Win()
	{
		this._victoryScreen.Visible = true;
		this.FillStats((RichTextLabel)this._victoryScreen.FindNode("RichTextLabel"), false);
	}

	public void HandleDeathAreaEntered(Node2D body)
	{
		if (body != null && body is EnemyScript es)
		{
			if (!es.HandleDeathAreaEntry())
			{
				this.TakeLife();
				body.QueueFree();
				this.numEnemies -= 1;
				++this.kills;
			}
		}
	}

	public void TakeLife()
	{
		this.Lives -= 1;
		this.Sounds[SoundName.LifeLost].Play();
		this._damageTime = 0.2f;
		if (this.Lives <= 0)
		{
			this._defeatScreen.Visible = true;
			this.FillStats((RichTextLabel)this._defeatScreen.FindNode("RichTextLabel"), true);
		}
	}

	public void FillStats(RichTextLabel rtl, bool defeat)
	{
		rtl.BbcodeText = string.Empty;
		if (defeat)
		{
			rtl.AppendBbcode("[center]DEFEAT[/center] THE EVIL WIZARD MANAGED TO OVERCOME YOUR DEFENCES, AGAINST ALL ODDS.YOU HAVE BEEN FORCED TO ESCAPE.THE WIZARD NOW RULES THE LAND.MAYBE THE GODS WEREN'T ON YOUR SIDE, OR MAYBE YOU JUST GOT UNLUCKY. BUT YOU CAN ALWAYS TRY AGAIN.\n");
		}
		else
		{
			rtl.AppendBbcode("[center]VICTORY![/center]YOU HAVE SUCCESSFULLY DEFEATED THE EVIL WIZARD AND AVENGED YOUR PARENTS! YOU HAVE PROVEN YOURSELF A WORTHY KING! LET THE LAND PROSPER UNDER YOUR RULE.\n");
		}

		DateTimeOffset dto = new DateTimeOffset(0L, TimeSpan.Zero).AddSeconds(this.playTime);
		rtl.AppendBbcode("PLAYTIME: " + dto.ToString("HH:mm:ss") + '\n');
		rtl.AppendBbcode("ENEMIES KILLED: " + this.kills + '\n');
		rtl.AppendBbcode("TOWERS BUILT: " + this.towersBuilt + '\n');
		rtl.AppendBbcode("TOWERS DESTROYED: " + this.towersDestroyed + '\n');
		rtl.AppendBbcode("DAMAGE DEALT: " + this.totalDamage.ToString("0.00") + '\n');
		rtl.AppendBbcode("MAX DAMAGE IN ONE SHOT: " + this.maxDamage.ToString("0.00") + '\n');
		rtl.AppendBbcode("WAVES SURVIVED: " + this.waveIndex + '\n');
		rtl.AppendBbcode("SECRETS FOUND: " + this.secrets + '\n');
	}

	public void HandleBACursorEnter() => this.isMouseInBuildableArea = true;
	public void HandleBACursorExit() => this.isMouseInBuildableArea = false;

	private AnimatedSprite[] _hpContainer;
	private int lives = 10;
	private TowerComponent componentMouseOver;

	public void UpdateLiveCount()
	{
		this._hpContainer[0].Frame = this.Lives >= 2 ? 512 : this.Lives == 1 ? 511 : 510;
		this._hpContainer[1].Frame = this.Lives >= 4 ? 512 : this.Lives == 3 ? 511 : 510;
		this._hpContainer[2].Frame = this.Lives >= 6 ? 512 : this.Lives == 5 ? 511 : 510;
		this._hpContainer[3].Frame = this.Lives >= 8 ? 512 : this.Lives == 7 ? 511 : 510;
		this._hpContainer[4].Frame = this.Lives >= 10 ? 512 : this.Lives == 9 ? 511 : 510;
	}
}

public enum SoundName
{
	BuildDestroyed,
	BuildFailed,
	ChestOpen,
	Coin,
	DialogSkip,
	EnemyDead,
	EnemyHurt,
	InstallPowerup,
	LifeLost,
	PickupPowerup,
	PurpleDrop,
	RedDrop,
	ShootCold,
	ShootDark,
	ShootFlame,
	ShootPhys,
	ShootLaser,
	ShootLight,
	WaveComplete,
	WaveStart,
	ChestSacrifice,
	NecromancerRaise,
	UndeadLordExplode,
	Mother,
	EnemyResurrect
}
