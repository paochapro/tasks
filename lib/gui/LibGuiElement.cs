using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Lib.Gui;

public abstract class LibGuiElement
{
    //Fiels
    LibGuiManager ui;
    bool locked = false;
    protected int layer = 0;
    protected bool allowHold;

    public string text;
    public Rectangle rect = Rectangle.Empty;

    protected Color borderColor = Color.Purple;
    protected Color bodyColor = Color.Purple;
    protected Color textColor = Color.Purple;

    //Properties
    public LibGuiManager LibGuiManager => ui;
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
                borderColor = ui.BorderLockedColor;
                bodyColor = ui.BodyLockedColor;
                textColor = ui.TextLockedColor;
            };
        }
    }

    public Point Position { get => rect.Location; set => rect.Location = value; }
    public Point Size { get => rect.Size; set => rect.Size = value; }

    protected LibGuiElement(LibGuiManager ui)
    {
        this.ui = ui;
        ui.Add(this);
        text = "";
    }

    protected LibGuiElement(LibGuiManager ui, Rectangle rect, string text) : this(ui)
    {
        borderColor = ui.BorderDefaultColor;
        bodyColor = ui.BodyDefaultColor;
        textColor = ui.TextDefaultColor;

        this.rect = rect;
        this.text = text;
    }

    public virtual void Activate() => throw new NotImplementedException("activation isnt implemented on this LibGuiManager element (LibGuiElement:Activate)");

    public virtual void _Update(KeyboardState keys, MouseState mouse)
    {
        borderColor = ui.BorderDefaultColor;
        bodyColor = ui.BodyDefaultColor;
        textColor = ui.TextDefaultColor;

        if (rect.Contains(mouse.Position) && !ui.Clicking)
        {
            ui.MouseCursor = MouseCursor.Hand;

            borderColor = ui.BorderHoverColor;
            bodyColor = ui.BodyHoverColor;
            textColor = ui.TextHoverColor;

            if (mouse.LeftButton == ButtonState.Pressed)
                Activate();
        }
    }

    public virtual void _Draw(SpriteBatch spriteBatch) => throw new NotImplementedException("draw isnt implemented on this LibGuiManager element (LibGuiElement:_Draw)");
}