using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib.Gui;

public class Label : LibGuiElement
{
    public Color Color { get; set; }

    public Label(LibGuiManager ui, Point pos, string text, Color color) : base(ui, new Rectangle(pos, Point.Zero), text)
    {
        Color = color;
    }
    
    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(LibGuiManager.Font, text, rect.Location.ToVector2(), Color);
    }
}