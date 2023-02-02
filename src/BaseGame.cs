namespace tasks;

public abstract class BaseGame : Game
{
    public GraphicsDeviceManager Graphics => graphics;
    public SpriteBatch SpriteBatch => spriteBatch;
    public Assets Assets => assets;
    public bool DebugMode => debugMode;
    public const string GameName = "Tasks";
    public readonly Color clearColor = new(100,100,100,255);

    protected bool debugMode;
    protected Point Screen
    {
        get => _screen;
        set {
            _screen = value;
            graphics.PreferredBackBufferWidth = value.X;
            graphics.PreferredBackBufferHeight = value.Y;
            graphics.ApplyChanges();
        }
    }

    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    Assets assets;
    Point _screen;

    public BaseGame()
    {
        graphics = new GraphicsDeviceManager(this);
    }

    protected override void LoadContent()
    {
        Content.RootDirectory = "Content";
        spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
        assets = new Assets(Content);
    }
}