using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib;

public class Label : ClassicUIElement
{
    public Color Color { get; set; }

    public Label(ClassicUIManager ui, Point pos, string text, Color color) : base(ui, new Rectangle(pos, Point.Zero), text)
    {
        Color = color;
    }
    
    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(ClassicUIManager.Font, text, rect.Location.ToVector2(), Color);
    }
}