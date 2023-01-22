using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib;

public class Label : UIElement
{
    public Color Color { get; set; }

    public Label(UI ui, Point pos, string text, Color color) : base(ui, new Rectangle(pos, Point.Zero), text)
    {
        Color = color;
    }
    
    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(UI.Font, text, rect.Location.ToVector2(), Color);
    }
}