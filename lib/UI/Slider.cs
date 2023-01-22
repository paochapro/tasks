using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using static Lib.Utils;

namespace Lib;

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
        spriteBatch.DrawString(UI.Font, text, textPos.ToVector2(), Color.Black);

        Rectangle bar = new Rectangle(rect.X, center(rect.Y, rect.Y + size.Y, barSizeY), size.X, barSizeY);
        spriteBatch.FillRectangle(bar, Color.Gray);

        Rectangle slider = new Rectangle(sliderX, rect.Y, sliderSize.X, sliderSize.Y);
        spriteBatch.FillRectangle(slider, Color.White);
        spriteBatch.DrawRectangle(slider, Color.Black, 3);
    }
}