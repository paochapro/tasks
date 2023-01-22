using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Text;

namespace Lib;

class Textbox : UIElement
{
    private const string avaliableCharaters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890-=!@#$%^&*()_+[]{};':|\\\",./<>?`~ ";
    private bool focus = false;
    private StringBuilder writtenText = new();

    public string WrittenText => writtenText.ToString();

    public Textbox(UI ui, Rectangle rect, string text) : base(ui, rect, text)
    {
        borderColor = Color.Black;
        bodyColor = Color.White;
    }

    public override void Activate()
    {
        if (focus) return;

        UI.Game.Window.TextInput += TextInput;
        focus = true;        
    }

    public void Deactivate()
    {
        UI.Game.Window.TextInput -= TextInput;
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
            UI.MouseCursor = MouseCursor.IBeam;

            if (mouse.LeftButton == ButtonState.Pressed && !UI.Clicking)
            {
                Activate();
                return;
            }
        }
        
        bool unfocusInput = (
            keys.IsKeyDown(Keys.Escape) ||
            (mouse.LeftButton == ButtonState.Pressed && !UI.Clicking)
        ); 
        
        if (focus && unfocusInput)
            Deactivate();
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bodyColor);

        if(writtenText.Length == 0 && !focus)
            spriteBatch.DrawString(UI.Font, text, rect.Location.ToVector2() + new Vector2(10, 10), Color.Gray);
        else
            spriteBatch.DrawString(UI.Font, writtenText, rect.Location.ToVector2() + new Vector2(10, 10), borderColor);

        spriteBatch.DrawRectangle(rect, focus ? Color.Blue : borderColor, 3);
    }
}