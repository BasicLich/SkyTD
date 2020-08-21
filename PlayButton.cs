using Godot;

public class PlayButton : Button
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.

    private PackedScene _sceneRes;

    public override void _Ready()
    {
        this.Connect("pressed", this, "HandlePressed");
        this._sceneRes = GD.Load<PackedScene>("res://Node2D.tscn");
    }

    public void HandlePressed()
    {
        if (this.Name.ToLower().Contains("play"))
        {
            Node newRoot = this._sceneRes.Instance();
            Node sceneRoot = this.GetParent().GetParent().GetParent().GetParent().GetParent();
            Node uiRoot = this.GetParent().GetParent().GetParent().GetParent();
            GD.Print(uiRoot.Name);
            if (newRoot is GameScript gs)
            {
                gs.SetMainMenu((Node2D)uiRoot);
            }

            sceneRoot.RemoveChild(uiRoot);
            sceneRoot.AddChild(newRoot);
        }
        else
        {
            if (this.Name.ToLower().Contains("bestiary"))
            {
                Node uiRoot = this.GetParent().GetParent().GetParent().GetParent();
                Node2D selfContainer = (Node2D)uiRoot.GetNode("MainMenuProper");
                Node2D bestiaryContainer = (Node2D)uiRoot.GetNode("Bestiary");
                bestiaryContainer.Visible = true;
                selfContainer.Visible = false;
                ((UIRoot)uiRoot).PrepareBestiary();
            }
            else
            {
                this.GetTree().Quit();
            }
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
