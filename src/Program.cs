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
    private Assets assets;
    private SpriteFont textFont;
    private UI ui;
    private readonly Color clearColor = new(100,100,100,255);

    public SpriteFont TextFont => textFont;

    //Window settings
    private Point screen;
    public Point Screen { get => screen; set {
        screen = value;
        graphics.PreferredBackBufferWidth = value.X;
        graphics.PreferredBackBufferHeight = value.Y;
        graphics.ApplyChanges();
    }}

    private const string gameName = "Tasks";
    
    //Main fields
    private List<UICard> uiCards = new();
    private UICard? dragCard;
    private int placeCardIndex;

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        //Start card position
        Point cardPos = Point.Zero;
        cardPos.X += 64;
        cardPos.Y += 16 / 2;

        //Reset place card index, because it could stay at some value, when user isnt dragging card
        placeCardIndex = 0;

        //If we are not dragging - update all cards
        if(dragCard == null)
        {
            foreach(UICard card in uiCards) 
            {
                card.UpdatePosition(cardPos);
                card.Update(dt);

                if(card.IsBeingDragged)
                {
                    dragCard = card;
                    uiCards.Remove(card);
                    break;
                }

                cardPos.X += UICard.rectWidth + 16;
            }
        }

        //If we are dragging, update only drag card
        if(dragCard != null)
        {
            Vector2 mousePos = Input.Mouse.Position.ToVector2();

            //Calculating the index in card list at which we are hovering when dragging a card
            double dragPos = mousePos.X - 64 + 16/2;
            placeCardIndex = (int)Math.Floor(dragPos / (UICard.rectWidth + 16));
            placeCardIndex = clamp(placeCardIndex, 0, uiCards.Count);

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

        lbl_placeCardIndex.text = "placeCardIndex: " + placeCardIndex;
        lbl_dt.text = "dt: " + dt.ToString();

        ui.UpdateElements(Input.Keys, Input.Mouse);
        Input.CycleEnd();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(clearColor);
        
        spriteBatch.Begin();
        {
            //Drawing place rect
            if(dragCard != null)
            {
                //Calculating place rect pos
                Point placePos = Point.Zero;
                placePos.X += 64;
                placePos.Y += 16 / 2;

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

    Label lbl_placeCardIndex;
    Label lbl_dt;
    protected override void LoadContent()
    {
        spriteBatch = new(GraphicsDevice);
        assets = new Assets(Content);
        textFont = assets.GetDefault<SpriteFont>();
        ui = new UI(this);
        //MyraEnvironment.Game = this;

        lbl_placeCardIndex = new(ui, Point.Zero, "placeCardIndex: #", Color.Red);
        lbl_dt = new(ui, new Point(0, 30), "dt: #", Color.Red);

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