namespace tasks;

class UITextbox
{
    const string avaliableCharaters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890-=!@#$%^&*()_+[]{};':|\\\",./<>?`~ ";
    string text;
    int maxTextLength;
    Rectangle rect;
    Color bodyColor;
    Color textColor;
    SpriteFont font;

    public Rectangle Rect { get => rect; set => rect = value; }
    public Color BodyColor { get => bodyColor; set => bodyColor = value; }
    public Color TextColor { get => textColor; set => textColor = value; }
    public string Text => text;
    public EventHandler<TextInputEventArgs> TextInputHandler => TextInput;

    public UITextbox(GameWindow windowToConnect, Point pos, int width, int maxTextLength, 
        Color bodyColor, Color textColor, SpriteFont font, string startText)
    {
        this.bodyColor = bodyColor;
        this.textColor = textColor;
        this.text = startText;
        this.font = font;
        this.maxTextLength = maxTextLength;
       
        rect.Location = pos;
        rect.Width = width;
        rect.Height = (int)font.MeasureString(text).Y;

        windowToConnect.TextInput += TextInputHandler;
    }

    public void TextInput(object? sender, TextInputEventArgs args)
    {
        char character = args.Character;

        if (character == '\b')
        {
            if (text.Length > 0)
                text = text.Remove(text.Length-1);
        }

        bool canAddCharacter = (
            avaliableCharaters.Contains(character) &&
            text.Length < maxTextLength
        );

        if(canAddCharacter)
            text = string.Concat(text, character);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bodyColor);
        Vector2 textPos = Utils.CenteredTextPosInRect(rect, font, text);
        spriteBatch.DrawString(font, text, textPos, textColor);
    }
}