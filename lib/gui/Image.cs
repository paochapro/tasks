using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib.Gui;

class Image : LibGuiElement
{
    public Texture2D Texture { get; private set; }
    public Angle Rotation { get; set; }
    
    public Image(LibGuiManager ui, Texture2D image, Rectangle box) : base(ui, box, "")
    {
        Texture = image;
    }
    
    public Image(LibGuiManager ui, Texture2D image, Point position)
        : this(ui, image, new Rectangle(position, image.Bounds.Size))
    {
    }
    
    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, new Rectangle(rect.Center, rect.Size), null, Color.White, -Rotation.Radians, Texture.Bounds.Size.ToVector2() / 2, SpriteEffects.None, 0);
    }

    public override void Activate() {}
}