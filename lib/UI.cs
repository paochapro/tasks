using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Text;

using static Lib.Utils;

namespace Lib;

//Static
public class UI
{
    private List<UIElement> elements;
    private bool clicking;
    private Game game;

    //Properties
    public bool Clicking => clicking;
    public int CurrentLayer { get; set; }
    public SpriteFont Font { get; set; }
    public Game Game => game;
    
    //Main colors
    public Color MainDefaultColor { get; set; } = Color.Black;
    public Color MainSelectedColor { get; set; } = Color.Gold;
    public Color MainLockedColor { get; set; } = Color.Gray;

    //Bg colors
    public Color BgDefaultColor { get; set; } = Color.White;
    public Color BgSelectedColor { get; set; } = new Color(Color.Yellow, 50);
    public Color BgLockedColor { get; set; } = Color.DarkGray;

    public MouseCursor MouseCursor { get; set; } 

    public UI(Game game)
    {
        this.game = game;
        elements = new();
        CurrentLayer = 0;
        MouseCursor = MouseCursor.Arrow;
        Font = game.Content.Load<SpriteFont>("bahnschrift");
    }
    
    public void UpdateElements(KeyboardState keys, MouseState mouse)
    {
        MouseCursor prevMouseCursor = MouseCursor;
        MouseCursor = MouseCursor.Arrow;

        foreach(UIElement element in elements)
            if (element.Layer == CurrentLayer && !element.Locked)
                element._Update(keys, mouse);

        clicking = (mouse.LeftButton == ButtonState.Pressed);

        //Mouse cursor
        if(MouseCursor == prevMouseCursor) return;

        print("Update mouse cursor");

        Mouse.SetCursor(MouseCursor);
    }
    public void DrawElements(SpriteBatch spriteBatch)
    {
        foreach (UIElement element in elements)
            if (element.Layer == CurrentLayer && !element.Hidden)
                element._Draw(spriteBatch);
    }
    public UIElement Add(UIElement elem)
    {
        elements.Add(elem);
        return elem;
    }
    public void Clear()
    {
        CurrentLayer = 0;
        elements.Clear();
    }
}

//Element
public abstract class UIElement
{
    //Fiels
    protected UI ui;
    public Rectangle rect = Rectangle.Empty;
    protected readonly int layer = 0;
    protected bool allowHold;
    public string text;
    private bool locked = false;

    protected Color mainColor = Color.Purple;
    protected Color bgColor = Color.Purple;

    //Properties
    public bool Hidden { get; set; }
    public int Layer => layer;
    public bool Locked
    {
        get => locked;
        set
        {
            locked = value;
            if (locked)
            {
                mainColor = ui.MainLockedColor;
                bgColor = ui.BgLockedColor;
            };
        }
    }

    public Point Position { get => rect.Location; set => rect.Location = value; }
    public Point Size { get => rect.Size; set => rect.Size = value; }

    protected UIElement(UI ui)
    {
        this.ui = ui;
        ui.Add(this);
    }

    protected UIElement(UI ui, Rectangle rect, string text)
    {
        this.ui = ui;
        ui.Add(this);

        this.rect = rect;
        this.text = text;
    }

    public virtual void _Update(KeyboardState keys, MouseState mouse)
    {
        mainColor = ui.MainDefaultColor;
        bgColor = ui.BgDefaultColor;

        if (rect.Contains(mouse.Position) && !ui.Clicking)
        {
            ui.MouseCursor = MouseCursor.Hand;

            mainColor = ui.MainSelectedColor;
            bgColor = ui.BgSelectedColor;

            if (mouse.LeftButton == ButtonState.Pressed)
                Activate();
        }
    }

    public virtual void _Draw(SpriteBatch spriteBatch) => throw new NotImplementedException("draw isnt implemented on this UI element (UIElement:_Draw)");
    public virtual void Activate() => throw new NotImplementedException("activation isnt implemented on this UI element (UIElement:Activate)");
}

//Button
class Button : UIElement
{
    event Action func;

    public Button(UI ui, Rectangle rect, Action func, string text) : base(ui, rect, text)
    {
        this.func = func;
        allowHold = false;
    }
    public override void Activate() => func.Invoke();

    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bgColor);
        spriteBatch.DrawRectangle(rect, mainColor, 4);
        
        float scale = 1f;
        
        Vector2 measure = ui.Font.MeasureString(text) * scale;
        Vector2 position = new Vector2(rect.Center.X - measure.X / 2, rect.Center.Y - measure.Y / 2);
        
        spriteBatch.DrawString(ui.Font, text, position, mainColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
    }
}

class TextureButton : Button
{
    static int instance = 0;
    Texture2D texture;

    public TextureButton(UI ui, Texture2D texture, Rectangle rect, Action func) 
        : base(ui, rect, func, "TEXTURE_BUTTON" + (instance++))
    {
        this.texture = texture;
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, rect, Color.White);
    }
}

class CheckBox : UIElement
{
    static readonly Point size = new Point(24, 24);
    const int textOffset = 5;

    bool isChecked = false;
    bool IsChecked => isChecked;
    event Action act1;
    event Action act2;

    public CheckBox(UI ui, Point pos, Action act1, Action act2, string text, int layer) : base(ui, new Rectangle(pos, Point.Zero), text)
    {
        this.act1 = act1;
        this.act2 = act2;
        allowHold = false;
        rect.Width = (int)ui.Font.MeasureString(text).X;
    }
    public override void Activate()
    {
        isChecked = !isChecked;

        if (isChecked)
        {
            act1.Invoke();
        }
        else
        {
            act2.Invoke();
        }
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        Rectangle box = new(rect.Location, size);
        spriteBatch.FillRectangle(box, bgColor);
        spriteBatch.DrawRectangle(box, mainColor, 3);

        float scale = 0.7f;

        Vector2 measure = ui.Font.MeasureString(text) * scale;
        Vector2 position = new Vector2(box.Right + textOffset, center(box.Y, box.Bottom, measure.Y));

        spriteBatch.DrawString(ui.Font, text, position, mainColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);

        if(isChecked)
        {
            spriteBatch.DrawCircle(new CircleF(box.Center, 7f), 32, mainColor, 7);
        }
    }
}

class Slider : UIElement
{
    public const int sizeY = 50;
    const int sliderSizeX = 20;
    const int sliderOffset = 15;

    static readonly Point size = new(300 + sliderSizeX, sizeY);
    static readonly Point sliderSize = new(sliderSizeX, size.Y);
    static readonly int barSizeY = 10;

    int min, max;
    int sliderX;
    event Action<int> func;

    Point textSize;
    Point textPos;

    public Slider(UI ui, Point pos, string text, Action<int> func, int defaultValue, int layer) : base(ui, new Rectangle(pos, Point.Zero), text)
    {
        textSize = ui.Font.MeasureString(text).ToPoint();
        textPos = new Point(pos.X, center(pos.Y, pos.Y + size.Y, textSize.Y) - 3);

        int offset = textSize.X + sliderOffset;

        rect.X += offset;
        min = rect.X;
        max = rect.X + size.X - sliderSize.X;
        sliderX = rect.X + defaultValue;
        allowHold = true;

        this.func = func;
    }

    public override void Activate()
    {
        sliderX = Input.Mouse.Position.X;
        sliderX = clamp(sliderX, min, max);

        //Getting range from 0 to 100
        int denominator = (size.X - sliderSizeX) / 100;
        float value = (float)(sliderX - rect.X) / denominator;

        func.Invoke((int)Math.Round(value));
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(ui.Font, text, textPos.ToVector2(), Color.Black);

        Rectangle bar = new Rectangle(rect.X, center(rect.Y, rect.Y + size.Y, barSizeY), size.X, barSizeY);
        spriteBatch.FillRectangle(bar, Color.Gray);

        Rectangle slider = new Rectangle(sliderX, rect.Y, sliderSize.X, sliderSize.Y);
        spriteBatch.FillRectangle(slider, Color.White);
        spriteBatch.DrawRectangle(slider, Color.Black, 3);
    }
}

class Label : UIElement
{
    public Color Color { get; set; }

    public Label(UI ui, Point pos, string text, Color color) : base(ui, new Rectangle(pos, Point.Zero), text)
    {
        Color = color;
    }
    
    public override void _Update(KeyboardState keys, MouseState mouse) {}

    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(ui.Font, text, rect.Location.ToVector2(), Color);
    }
}

class TextBox : UIElement
{
    private const string avaliableCharaters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890-=!@#$%^&*()_+[]{};':|\\\",./<>?`~ ";
    private bool focus = false;
    private StringBuilder writtenText = new();

    public string WrittenText => writtenText.ToString();

    public TextBox(UI ui, Rectangle rect, string text) : base(ui, rect, text)
    {
        mainColor = Color.Black;
        bgColor = Color.White;
    }

    public override void Activate()
    {
        if (focus) return;

        ui.Game.Window.TextInput += TextInput;
        focus = true;        
    }

    public void Deactivate()
    {
        ui.Game.Window.TextInput -= TextInput;
        focus = false;
    }

    private void TextInput(object? sender, TextInputEventArgs e)
    {
        if (e.Character == '\b')
        {
            if (writtenText.Length > 0)
                writtenText.Remove(writtenText.Length - 1, 1);
        }

        if(avaliableCharaters.Contains(e.Character))
            writtenText.Append(e.Character);
    }

    public override void _Update(KeyboardState keys, MouseState mouse)
    {
        if (rect.Contains(mouse.Position))
        {
            ui.MouseCursor = MouseCursor.IBeam;

            if (mouse.LeftButton == ButtonState.Pressed && !ui.Clicking)
            {
                Activate();
                return;
            }
        }
        
        bool unfocusInput = (
            keys.IsKeyDown(Keys.Escape) ||
            (mouse.LeftButton == ButtonState.Pressed && !ui.Clicking)
        ); 
        
        if (focus && unfocusInput)
            Deactivate();
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bgColor);

        if(writtenText.Length == 0 && !focus)
            spriteBatch.DrawString(ui.Font, text, rect.Location.ToVector2() + new Vector2(10, 10), Color.Gray);
        else
            spriteBatch.DrawString(ui.Font, writtenText, rect.Location.ToVector2() + new Vector2(10, 10), mainColor);

        spriteBatch.DrawRectangle(rect, focus ? Color.Blue : mainColor, 3);
    }
}

class Image : UIElement
{
    public Texture2D Texture { get; private set; }
    public Angle Rotation { get; set; }
    
    public Image(UI ui, Texture2D image, Rectangle box) : base(ui, box, "")
    {
        Texture = image;
    }
    
    public Image(UI ui, Texture2D image, Point position)
        : this(ui, image, new Rectangle(position, image.Bounds.Size))
    {
    }
    
    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, new Rectangle(rect.Center, rect.Size), null, Color.White, -Rotation.Radians, Texture.Bounds.Size.ToVector2() / 2, SpriteEffects.None, 0);
    }

    public override void _Update(KeyboardState keys, MouseState mouse) {}

    public override void Activate() {}
}

//Containers
class Container : UIElement
{
    private List<UIElement> elements = new();
    public int ElementOffset { get; set; }

    public Container(UI ui, Rectangle box, int elementOffset) : base(ui)
    {
        rect = box;
        ElementOffset = elementOffset;
    }
    
    public void Add(UIElement element)
    {
        elements.Add(element);
        Rearrange();
    }
    public void Remove(UIElement element)
    {
        elements.Remove(element);
        Rearrange();
    }

    private void Rearrange()
    {
        UIElement? previousElement = null;
        
        foreach (UIElement element in elements)
        {
            element.rect.X = (previousElement?.rect.Right + ElementOffset ?? rect.X);
            element.rect.Y = rect.Y;
            previousElement = element;
        }
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        throw new NotImplementedException();
    }

    public override void _Update(KeyboardState keys, MouseState mouse) {}

    public override void Activate() {}
}