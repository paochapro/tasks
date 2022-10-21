using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Lib;

class Tasks : Game
{
    public GraphicsDeviceManager Graphics => graphics;
    public SpriteBatch SpriteBatch => spriteBatch;
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    
    private const string gameName = "Tasks";
    private Point screen = new(800,600);
    public Point Screen => screen;
    
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }

    protected override void LoadContent()
    {
        spriteBatch = new(GraphicsDevice);
        Assets.Content = Content;
        MyraEnvironment.Game = this;
        
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