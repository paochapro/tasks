using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Myra;
using Myra.Graphics2D.UI;
using Lib;

namespace tasks;

public class Tasks : Game
{
    //Graphics
    public GraphicsDeviceManager Graphics => graphics;
    public SpriteBatch SpriteBatch => spriteBatch;
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    //Something
    private Point screen = new(800,600);
    public Point Screen => screen;
    private readonly Color clearColor = new(100,100,100,255);
    private const string gameName = "Tasks";
    
    //Main fields
    private Desktop desktop;
    private List<DrawCard> cards = new();

    protected override void Update(GameTime gameTime)
    {
        cards.ForEach(c => c.Update());
        Input.CycleEnd();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(clearColor);
        
        spriteBatch.Begin();
        {
            cards.ForEach(c => c.Draw(spriteBatch));
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
 
        
        cards.Add(new DrawCard(card));
        
        base.LoadContent();
    }

    protected override void Initialize()
    {
        Window.Title = gameName;
        Window.AllowUserResizing = false;
        IsMouseVisible = true;
        base.Initialize();
    }
    
    public void ChangeScreenSize(Point size)
    {
        screen = size;
        graphics.PreferredBackBufferWidth = size.X;
        graphics.PreferredBackBufferHeight = size.Y;
        graphics.ApplyChanges();
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