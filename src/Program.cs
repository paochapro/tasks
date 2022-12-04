using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Myra;
using Myra.Graphics2D.UI;
using Lib;

using static Lib.Utils;

namespace tasks;

public class Tasks : Game
{
    //Graphics
    public GraphicsDeviceManager Graphics => graphics;
    public SpriteBatch SpriteBatch => spriteBatch;
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    //Window
    private Point screen;
    public Point Screen
    {
        get => screen;
        set  {
            screen = value;
            graphics.PreferredBackBufferWidth = screen.X;
            graphics.PreferredBackBufferHeight = screen.Y;
            graphics.ApplyChanges();
        }
    }

    private readonly Color clearColor = new(100,100,100,255);
    private const string gameName = "Tasks";
    
    //Main fields
    private Desktop desktop;
    private UICard? dragCard = null;
    private List<UICard> UICards = new();

    protected override void Update(GameTime gameTime)
    {
        Point cardPos = Point.Zero;
        cardPos.X += 64;
        cardPos.Y += 16 / 2;

        if(Input.LBReleased())
        {
            dragCard = null;
        } 


        double dragPos = Input.Mouse.Position.ToVector2().X - 64 + 16/2;
        int placeCardIndex = (int)Math.Floor(dragPos / (UICard.rectWidth + 16));
        placeCardIndex = clamp(placeCardIndex, 0, UICards.Count);

        if(dragCard == null) {
            foreach(UICard card in UICards) 
            {
                bool isDragging = card.Update(cardPos);

                if(isDragging)
                {
                    dragCard = card;
                    break;
                }

                cardPos.X += UICard.rectWidth + 16;
            }
        }

        Input.CycleEnd();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(clearColor);
        
        spriteBatch.Begin();
        {
            Point cardPos = Point.Zero;
            cardPos.X += 64;
            cardPos.Y += 16 / 2;

            foreach(UICard card in UICards.Except(new[] {dragCard}))
            {
                card.Draw(spriteBatch, cardPos);
                cardPos.X += UICard.rectWidth + 16;
            }

            Point dragCardPos = Input.Mouse.Position - new Point(UICard.rectWidth, UICard.bannerHeight) / new Point(2, 2);
            dragCard?.Draw(spriteBatch, dragCardPos);
        }
        spriteBatch.End();
    }

    protected override void LoadContent()
    {
        spriteBatch = new(GraphicsDevice);
        Assets.Content = Content;
        MyraEnvironment.Game = this;

        Card card = new Card("simple task", Color.Lime);
        
        card.Tasks.Add("lol", true);
        card.Tasks.Add("whata", false);
        card.Tasks.Add("wh", false);
        card.Tasks.Add("rth", true);
        card.Tasks.Add("wata", true);
        card.Tasks.Add("ata", false);
 
        UICards.Add(new UICard(card));
        UICards.Add(new UICard(card));
        UICards.Add(new UICard(card));
        
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

    public Tasks()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "content";
    }
}

class Program {
    static void Main() {
        using(Tasks tasks = new Tasks())
            tasks.Run();
    }
}