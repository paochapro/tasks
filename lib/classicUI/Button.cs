using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib;

class Button : ClassicUIElement
{
    private const float defaultBorderThickness = 2;

    public event Action func;
    public float borderThickness;

    public Button(ClassicUIManager ui, Rectangle rect, Action func, string text) : base(ui, rect, text)
    {
        this.func = func;
        allowHold = false;
        borderThickness = defaultBorderThickness;
    }

    public override void Activate() => func.Invoke();

    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bodyColor);
        spriteBatch.DrawRectangle(rect, borderColor, borderThickness);
        
        float textScale = 1f;
        
        Vector2 measure = ClassicUIManager.Font.MeasureString(text) * textScale;
        Vector2 position = new Vector2(rect.Center.X - measure.X / 2, rect.Center.Y - measure.Y / 2);
        
        spriteBatch.DrawString(ClassicUIManager.Font, text, position, textColor, 0, new Vector2(0, 0), textScale, SpriteEffects.None, 0);
    }
}

class TextureButton : Button
{
    static int instance = 0;
    Texture2D texture;

    public TextureButton(ClassicUIManager ui, Texture2D texture, Rectangle rect, Action func) 
        : base(ui, rect, func, "TEXTURE_BUTTON" + (instance++))
    {
        this.texture = texture;
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, rect, Color.White);
    }
}