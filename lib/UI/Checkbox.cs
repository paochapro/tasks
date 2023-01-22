using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using static Lib.Utils;

namespace Lib;

class Checkbox : UIElement
{
    static readonly Point size = new Point(24, 24);
    const int textOffset = 5;

    bool isChecked = false;
    bool IsChecked => isChecked;
    event Action onCheck;
    event Action onUncheck;

    public Checkbox(UI ui, Point pos, Action onCheck, Action onUncheck, string text, int layer) 
        : base(ui, new Rectangle(pos, Point.Zero), text)
    {
        this.onCheck = onCheck;
        this.onUncheck = onUncheck;
        allowHold = false;
        rect.Width = (int)ui.Font.MeasureString(text).X;
    }
    
    public override void Activate()
    {
        isChecked = !isChecked;

        if (isChecked)
        {
            onCheck.Invoke();
        }
        else
        {
            onUncheck.Invoke();
        }
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        Rectangle box = new(rect.Location, size);
        spriteBatch.FillRectangle(box, bodyColor);
        spriteBatch.DrawRectangle(box, borderColor, 3);

        float scale = 0.7f;

        Vector2 measure = UI.Font.MeasureString(text) * scale;
        Vector2 position = new Vector2(box.Right + textOffset, center(box.Y, box.Bottom, measure.Y));

        spriteBatch.DrawString(UI.Font, text, position, borderColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);

        if(isChecked)
        {
            spriteBatch.DrawCircle(new CircleF(box.Center, 7f), 32, borderColor, 7);
        }
    }
}