namespace Lib;

public abstract class BaseGame : Game
{
    public GraphicsDeviceManager Graphics => graphics;
    public Assets Assets => assets;
    public bool DebugMode => debugMode;
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

    public BaseGame() : base()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected sealed override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Process(dt);
        Input.CycleEnd();
        base.Update(gameTime);
    }

    protected sealed override void Draw(GameTime gameTime) => Render(spriteBatch);

    protected sealed override void LoadContent()
    {
        spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
        assets = new Assets(Content);
        LoadAssets();
        base.LoadContent();
    }

    protected sealed override void Initialize()
    {
        Screen = new(1400,800);
        Window.AllowUserResizing = true;
        IsMouseVisible = true;
        Init();
        base.Initialize();
    }

    //Yes, I have to use synonyms and stuff to make this work :P
    protected abstract void Process(float dt);
    protected abstract void Render(SpriteBatch spriteBatch);
    protected abstract void LoadAssets();
    protected abstract void Init();
}