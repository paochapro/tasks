using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

using static Lib.Utils;

namespace Lib;

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
    
    public Color BorderDefaultColor { get; set; } = Color.Black;
    public Color BorderHoverColor { get; set; } = Color.Gold;
    public Color BorderLockedColor { get; set; } = Color.Gray;

    public Color TextDefaultColor { get; set; } = Color.Black;
    public Color TextHoverColor { get; set; } = Color.Gold;
    public Color TextLockedColor { get; set; } = Color.Gray;

    public Color BodyDefaultColor { get; set; } = Color.White;
    public Color BodyHoverColor { get; set; } = new Color(Color.Yellow, 50);
    public Color BodyLockedColor { get; set; } = Color.DarkGray;

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