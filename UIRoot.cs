using Godot;
using System;
using System.Collections.Generic;

public class UIRoot : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.

	private Dictionary<Button, Guid> _hack { get; } = new Dictionary<Button, Guid>();
	private VBoxContainer _iconScrollList;

	private AnimatedSprite _bigIcon;
	private AnimatedSprite _lockIcon;
	private RichTextLabel _enemyName;
	private RichTextLabel _enemyDescription;

	public override void _Ready()
	{
		PersistentDataStorage.Load();
		if (this._iconScrollList == null)
		{
			this._iconScrollList = (VBoxContainer)this.GetNode("Bestiary").GetNode("Container").GetNode("MarginContainer").GetNode("ScrollListEnemyIcons").GetNode("ScrollBoxEnemyIcons");

			this._bigIcon = (AnimatedSprite)this.GetNode("Bestiary").GetNode("Container2").GetNode("MarginContainer").GetNode("BestiaryLog").GetNode("EnemySprite");
			this._lockIcon = (AnimatedSprite)this.GetNode("Bestiary").GetNode("Container2").GetNode("MarginContainer").GetNode("BestiaryLog").GetNode("LockIcon");
			this._enemyDescription = (RichTextLabel)this.GetNode("Bestiary").GetNode("Container2").GetNode("MarginContainer").GetNode("BestiaryLog").GetNode("Container").GetNode("MarginContainer").GetNode("EnemyDescription");
			this._enemyName = (RichTextLabel)this.GetNode("Bestiary").GetNode("Container2").GetNode("MarginContainer").GetNode("BestiaryLog").GetNode("Container2").GetNode("MarginContainer").GetNode("EnemyName");
			this.PopulateBestiary();
			this.PrepareBestiary();
			((Button)this.GetNode("Bestiary").GetNode("MarginContainer").GetNode("Container").GetNode("MarginContainer").GetNode("Button")).Connect("pressed", this, "CloseBestiary");
		}
	}

	public void CloseBestiary()
	{
		Node2D mainmenu = (Node2D)this.GetNode("MainMenuProper");
		Node2D bestiaryContainer = (Node2D)this.GetNode("Bestiary");
		bestiaryContainer.Visible = false;
		mainmenu.Visible = true;
	}

	public void PrepareBestiary()
	{
		this._bigIcon.Frame = 0;
		this._lockIcon.Visible = false;
		this._enemyDescription.BbcodeText = string.Empty;
		this._enemyName.BbcodeText = string.Empty;
		foreach (Node n in this._iconScrollList.GetChildren())
		{
			if (n is Button btn && this._hack.ContainsKey(btn))
			{
				Guid enemyID = this._hack[btn];
				if (PersistentDataStorage.PersistentEnemyDatas.ContainsKey(enemyID))
				{
					AnimatedSprite asp = (AnimatedSprite)btn.GetChildren()[0];
					asp.Modulate = PersistentDataStorage.PersistentEnemyDatas[enemyID].IsUnlocked ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1);
				}
			}
		}
	}

	public void PopulateBestiary()
	{
		Button firstButton = (Button)this._iconScrollList.GetChildren()[0];
		foreach (KeyValuePair<Guid, EnemyDataBase> datas in PersistentDataStorage.PersistentEnemyDatas)
		{
			Button clone = (Button)firstButton.Duplicate();
			((AnimatedSprite)clone.GetChildren()[0]).Frame = datas.Value.Frame;
			this._hack[clone] = datas.Key;
			this._iconScrollList.AddChild(clone);
			clone.Connect("pressed", this, "EnemyNodeClick", new Godot.Collections.Array(new[] { clone }));
		}

		firstButton.QueueFree();
	}

	public void EnemyNodeClick(Button btn)
	{
		Guid enemyID = this._hack[btn];
		if (PersistentDataStorage.PersistentEnemyDatas.ContainsKey(enemyID))
		{
			EnemyDataBase enemyData = PersistentDataStorage.PersistentEnemyDatas[enemyID];
			this._bigIcon.Frame = enemyData.Frame;
			this._bigIcon.Modulate = enemyData.IsUnlocked ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1);
			this._lockIcon.Visible = !enemyData.IsUnlocked;
			this._enemyName.BbcodeText = string.Empty;
			this._enemyName.AppendBbcode(enemyData.IsUnlocked ? enemyData.BestiaryName.Replace(" ", "  ") : "LOCKED!");
			this._enemyDescription.BbcodeText = string.Empty;
			this._enemyDescription.AppendBbcode(enemyData.IsUnlocked ? enemyData.BestiaryDescription.Replace(" ", "  ") : "DEFEAT  THIS  CREATURE  IN  THE  GAME  TO  UNLOCK  ITS  INFORMATION");
		}
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}
