using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Lib;

using static Lib.Utils;

namespace tasks;

public class TasksProgram : Game
{
    //Graphics
    public GraphicsDeviceManager Graphics => graphics;
    public SpriteBatch SpriteBatch => spriteBatch;
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    //Other stuff
    private Assets assets;
    private UI ui;
    private SpriteFont textFont;
    private readonly Color clearColor = new(100,100,100,255);

    public SpriteFont TextFont => textFont;

    //Window settings
    private Point screen;
    public Point Screen
    { 
        get => screen; 
        set {
            screen = value;
            graphics.PreferredBackBufferWidth = value.X;
            graphics.PreferredBackBufferHeight = value.Y;
            graphics.ApplyChanges();
        }
    }

    private const string gameName = "Tasks";
    
    //Main fields
    private List<UICard> uiCards = new();
    private UICard? dragCard;
    private int placeCardIndex;

    //Other stuff
    private const int cardStartOffset = 16;
    private const int bottomBarHeight = 200;
    private const int cardBinWidth = 300;

    private Rectangle cardBinRect;
    private Rectangle bottomBarRect;
    public Rectangle CardBinRect => cardBinRect;

    private Label lbl_placeCardIndex;
    private Label lbl_placeTaskIndex;
    private Label lbl_dt;
    public Label Label_placeTaskIndex => lbl_placeTaskIndex;

    private List<UICard> cardsToRemove;

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        placeCardIndex = 0;

        //Calculating the index in card list at which we are hovering when dragging a card or a task
        Vector2 mousePos = Input.Mouse.Position.ToVector2();
        double dragPos = mousePos.X - cardStartOffset/2;
        placeCardIndex = (int)Math.Floor(dragPos / (UICard.rectWidth + 16));
        placeCardIndex = clamp(placeCardIndex, 0, uiCards.Count-1);

        //Update cards
        NoDraggingCardUpdate(dt);
        DraggingCardUpdate(dt);

        //Remove all cards in removal queue
        uiCards.RemoveAll(c => c.QueuedForRemoval);

        lbl_placeCardIndex.text = "placeCardIndex: " + placeCardIndex;
        lbl_dt.text = "dt: " + dt.ToString();

        ui.UpdateElements(Input.Keys, Input.Mouse);
        Input.CycleEnd();
        base.Update(gameTime);
    }

    private void NoDraggingCardUpdate(float dt)
    {
        Point cardPos = new Point(cardStartOffset);

        UITaskBox? dragTask = uiCards.Find(c => c.DragTask != null)?.DragTask;

        //If we are not dragging - update all cards
        if(dragCard == null)
        {
            foreach(UICard card in uiCards) 
            {
                card.UpdatePosition(cardPos);

                //If we arent dragging any tasks update card
                if(dragTask == null)
                {
                    card.Update(dt);

                    //Check if being dragged
                    if(card.IsBeingDragged)
                    {
                        dragCard = card;
                        uiCards.Remove(card);
                        break;
                    }

                    //Check if we are dragging some task
                    dragTask = card.DragTask;
                }
                
                //If we are dragging a task, tell the card under which we are dragging our task, that its in their place 
                if(dragTask != null)
                {
                    card.DragTask = null;

                    if(card == uiCards.ElementAtOrDefault(placeCardIndex))
                    {
                        card.DragTask = dragTask;
                        card.UpdateTaskBoxes(dt);
                    }
                }

                cardPos.X += UICard.rectWidth + 16;
            }
        }
    }

    private void DraggingCardUpdate(float dt)
    {
        Point cardPos = new Point(cardStartOffset);

        //If we are dragging, update only drag card
        if(dragCard != null)
        {
            Vector2 mousePos = Input.Mouse.Position.ToVector2();

            //Update other cards position
            foreach(UICard card in uiCards)
            {
                //If we are hovering on this card move it to left side
                if(card == uiCards.ElementAtOrDefault(placeCardIndex))
                    cardPos.X += UICard.rectWidth + 16;

                card.UpdatePosition(cardPos);

                cardPos.X += UICard.rectWidth + 16;
            }

            //Update drag card position
            Vector2 bannerSize = new Vector2(UICard.rectWidth, UICard.bannerHeight);
            Vector2 dragCardPos = mousePos - bannerSize / 2;

            dragCard.UpdatePosition(dragCardPos.ToPoint());
            dragCard.Update(dt); //should remade this

            if(!dragCard.IsBeingDragged)
            {
                //Insert drag card at place index
                uiCards.Insert(placeCardIndex, dragCard);

                //Not dragging card anymore
                dragCard = null;
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(clearColor);
        
        spriteBatch.Begin();
        {
            //Drawing bottom bar
            spriteBatch.FillRectangle(bottomBarRect, clearColor.DarkenBy(20));
            spriteBatch.FillRectangle(cardBinRect, clearColor.DarkenBy(40));

            //Drawing place rect
            if(dragCard != null)
            {
                //Calculating place rect pos
                Point placePos = new Point(cardStartOffset);

                placePos.X += placeCardIndex * (UICard.rectWidth + 16);

                //Place rect
                Rectangle placeRect = new(placePos, new(UICard.rectWidth, dragCard.Rectangle.Height));

                spriteBatch.DrawRectangle(placeRect, Color.White, 2);
            }

            //Drawing cards
            foreach(UICard card in uiCards)
                card.Draw(spriteBatch);

            //Drawing lib ui
            ui.DrawElements(spriteBatch);

            dragCard?.Draw(spriteBatch);
        }
        spriteBatch.End();
    }

    protected override void LoadContent()
    {
        spriteBatch = new(GraphicsDevice);
        assets = new Assets(Content);
        textFont = assets.GetDefault<SpriteFont>();
        ui = new UI(this);
        //MyraEnvironment.Game = this;

        bottomBarRect = new Rectangle(0, Screen.Y - bottomBarHeight, Screen.X, bottomBarHeight);
        cardBinRect = bottomBarRect with { Width = cardBinWidth, X = bottomBarRect.Right - cardBinWidth };

        lbl_placeCardIndex = new(ui, new(0, 500), "placeCardIndex: #", Color.Red);
        lbl_placeTaskIndex = new(ui, new(0, 500-30), "placeTaskIndex: #", Color.Red);
        lbl_dt = new(ui, new(0, 500-60), "dt: #", Color.Red);

        //Randomized cards
        var addRandomizedCards = () => {
            uiCards.Clear();

            for(int c = 0; c < Rnd.Int(2,6); ++c)
            {
                Color color = Color.White;
                color.R = (byte)Rnd.Int(0,255);
                color.G = (byte)Rnd.Int(0,255);
                color.B = (byte)Rnd.Int(0,255);

                Card card = new Card("card " + c, color);
                
                for(int t = 0; t < Rnd.Int(3,12); ++t)
                {
                    string taskText = "task " + t;
                    bool isChecked = Convert.ToBoolean(Rnd.Int(0,1));
                    card.Tasks.Add(taskText, isChecked);
                }
                
                uiCards.Add(new UICard(this, card));
            }
        };

        Button button = new Button(ui, new Rectangle(500,700,200,50), addRandomizedCards, "Generate random");

        addRandomizedCards();
        
        base.LoadContent();
    }

    protected override void Initialize()
    {
        Screen = new(1400,800);
        Window.Title = gameName;
        Window.AllowUserResizing = false;
        IsMouseVisible = true;
        base.Initialize();
    }
    
    public TasksProgram()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "content";
    }
}

class Program {
    static void Main() {
        using(TasksProgram program = new TasksProgram())
            program.Run();
    }
}