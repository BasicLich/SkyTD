using Godot;

public class TutorialScript : Node2D
{
    public GameScript game;
    public RichTextLabel rtl;
    public CenterContainer dialogContainer;

    public string[] lines = new string[] {
        "YOU. YOU ARE FINALLY AWAKE.",
        "THE EVIL WIZARD WHO PRETENDED TO BE A COURT WIZARD HAD JUST...",
        "KILLED YOUR MOTHER AND YOUR FATHER!",
        "CONGRATULATIONS ON BECOMING THE NEW KING, I GUESS.",
        "OH, AND THE WIZARD IS COMING TO FINISH THE JOB.",
        "BUT JUST LIKE ANY INCOMPETENT VILLAIN HE LEFT IT TO HIS HENCHMAN.",
        "SO YOU BETTER HURRY UP! I WON'T SAVE YOU, YOU WON'T EITHER",
        "BUT THERE BABIES WILL!",
        string.Empty,
        "GREAT, AREN'T THEY?",
        "WHAT DO YOU MEAN WHAT THESE ARE? THEY ARE TOWERS! WELL, NOT YET, BUT THEY WILL BE! AND THEY WILL HELP YOU!",
        "YOU SEE HERE WE DON'T BUILD A TOWER FROM THE GROUND AS IT IS, WE COMPOSE IT FROM VARIOUS COMPONENTS.",
        "HERE LIE 2 BASES AND 2 GUNS. COMBINED THEY WILL MAKE 2 FINE TOWERS!",
        "WHY TOWERS? WELL, YOU SEE, THE EVIL HENCHMEN ARE TOO STUPID TO UNDERSTAND THAT YOU CAN WALK SOMEWHERE ELSE BUT THE PATHS!",
        "SO IF YOU LAID OUT THE TOWERS ALONG THE PATH...",
        "WELL, YOU GET IT. YOU CAN ORDER A TOWER CONSTRUCTION SPOT ANYWHERE WHERE IT IS POSSIBLE BY DOING A LEFT MOUSE CLICK THERE",
        "WHATEVER THAT MEANS.",
        "YOU CAN ALSO PICK UP TOWER COMPONENTS IN A SIMILAR WAY, AND TO INSTALL THEM ONTO A TOWER JUST MOVE THEM TO THE TOWER AND,",
        "YOU'VE GUESSED IT, LEFT CLICK AGAIN. YOU CAN EVEN PICK UP INSTALLED COMPONENTS! THAT IS NEAT!",
        "ME? OH, I AM YOUR BESTEST FRIEND. I WILL MAKE MORE COMPONENTS FOR YOU. WELL, I DON'T WORK FOR FREE THOUGH.",
        "SO YOU BETTER BEAT THOSE HENCHMEN FOR EVERY OUNCE OF COIN THEY'VE GOT, HUH?",
        "NOW, KEEP IN MIND THAT SOME INGREDIENTS I CAN'T JUST CREATE OUT OF THIN AIR.",
        "THOSE INGREDIENTS ALLOW ME TO CREATE EXTREME COMPONENS, THE LIKES OF WHICH THE WORLD HAD NEVER SEEN!",
        "HOWEVER I CAN'T MAKE THEM. BUT THIS IS WHERE YOU COME INTO PLAY!",
        "YOU CAN SACRIFICE A CHEST TO NYARLATHOTEP BY RIGHT-CLICKING IT. IT WILL DESTROY THE CHEST AND ALL ITS CONTENTS",
        "BUT THAT MIGHT JUST HELP US. ALSO YOU CAN TAUNT THE WIZARD BY PRESSING ENTER BEFORE THE WAVE ARRIVED.",
        "THAT WILL MAKE THE WAVE MORE DIFFICULT, BUT IT MIGHT REWARD YOU IN THE END.",
        "ANYWAY, GET TO WORK, YOU WON'T DEFEND YOURSELF BY YOURSELF. OR SOMETHING."
    };

    public int cLineIndex = 0;

    public TutorialStage CurrentStage { get; set; } = TutorialStage.IntroFade;

    private float _tutorialTimer;

    public override void _Ready()
    {
        this.game = (GameScript)this.GetParent();
        this.rtl = (RichTextLabel)this.game.FindNode("CenterContainer").FindNode("PanelContainer").FindNode("RichTextLabel");
        this.dialogContainer = (CenterContainer)this.game.FindNode("CenterContainer");
        this.game.isTutorial = true;
        this.game.isCameraTutorialLocked = true;
        this.game.isPlacementTutorialLocked = true;
    }

    public float cameraMovementTime;
    public float cameraMovementTimeDesired;
    public Vector2 cameraDesiredPosition;
    public Vector2 cameraStartPosition;
    public Vector2 cameraDesiredZoom;
    public Vector2 cameraStartZoom;
    public bool cameraDone = true;

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (this.CurrentStage == TutorialStage.IntroFade)
        {
            this.game._camera.Position = new Vector2(0, 0);
            this.game._camera.Zoom = Vector2.One * 0.25f;
            if ((this._tutorialTimer += delta) > 3)
            {
                this.CurrentStage = TutorialStage.FirstChat;
                this.game.isFadeTutorialLocked = false;
                this.rtl.AppendBbcode(this.lines[0]);
                this.dialogContainer.Visible = true;
            }
            else
            {
                this.game.isFadeTutorialLocked = true;
                this.game._damageSplash.Color = new Color(0, 0, 0, 1 - (this._tutorialTimer / 3f));
            }
        }

        if (!this.cameraDone)
        {
            this.cameraMovementTime += delta;
            float cmi = (this.cameraMovementTime / this.cameraMovementTimeDesired);
            float cmtI = 1 - cmi;
            this.game._camera.Position = new Vector2(this.cameraStartPosition.x * cmtI + this.cameraDesiredPosition.x * cmi, this.cameraStartPosition.y * cmtI + this.cameraDesiredPosition.y * cmi);
            this.game._camera.Zoom = new Vector2(this.cameraStartZoom.x * cmtI + this.cameraDesiredZoom.x * cmi, this.cameraStartZoom.y * cmtI + this.cameraDesiredZoom.y * cmi);
            if (this.cameraMovementTime >= this.cameraMovementTimeDesired)
            {
                this.cameraDone = true;
                if (this.CurrentStage == TutorialStage.End)
                {
                    this.game.isFadeTutorialLocked = this.game.isTutorial = this.game.isCameraTutorialLocked = this.game.isPlacementTutorialLocked = false;
                    this.dialogContainer.Visible = false;
                    this.game.AdvanceWave();
                    this.QueueFree();
                }
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn && @event.IsPressed() && btn.ButtonIndex == (int)ButtonList.Left)
        {
            if (this.CurrentStage == TutorialStage.FirstChat)
            {
                this.game.Sounds[SoundName.DialogSkip].Play();
                ++this.cLineIndex;
                if (this.cLineIndex == 7)
                {
                    this.cameraMovementTime = 0;
                    this.cameraMovementTimeDesired = 1;
                    this.cameraDesiredPosition = new Vector2(200, 260);
                    this.cameraStartPosition = this.game._camera.Position;
                    this.cameraDesiredZoom = this.game._camera.Zoom;
                    this.cameraStartZoom = this.game._camera.Zoom;
                    this.cameraDone = false;
                }

                if (this.cLineIndex >= this.lines.Length)
                {
                    this.CurrentStage = TutorialStage.End;
                    this.cameraMovementTime = 0;
                    this.cameraMovementTimeDesired = 1;
                    this.cameraDesiredPosition = new Vector2(0, 0);
                    this.cameraStartPosition = this.game._camera.Position;
                    this.cameraDesiredZoom = new Vector2(0.9f, 0.9f);
                    this.cameraStartZoom = this.game._camera.Zoom;
                    this.cameraDone = false;
                    return;
                }

                this.rtl.BbcodeText = string.Empty;
                this.rtl.AppendBbcode(this.lines[this.cLineIndex]);
            }
        }
    }
}

public enum TutorialStage
{
    IntroFade,
    FirstChat,
    TowerCreate,
    ComponentPick,
    ComponentAttach,
    TowerAssemble,
    EnemyDefeat,
    ChestLoot,
    End
}
