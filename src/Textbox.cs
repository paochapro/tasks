namespace tasks;

class UITextbox
{
    const string avaliableCharaters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890-=!@#$%^&*()_+[]{};':|\\\",./<>?`~ ";
    const int beamWidth = 1;
    string text;
    int maxTextLength;
    int lastCharIndex => text.Length-1;
    Rectangle rect;
    Color bodyColor;
    Color textColor;
    SpriteFont font;

    int _beamIndex;
    int BeamIndex {
        get => _beamIndex;
        set {
            _beamIndex = Utils.clamp(value, 0, lastCharIndex + 1);
        }
    }

    //Emulating key repeating
    const float keyRepeatWaitTime = 0.5f;
    const float keyRefreshWaitTime = 0.02f;
    float keyRepeatTimer;
    float keyRefreshTimer;
    int arrowKeyDir;

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

    public void Update(float dt)
    {
        const int left = -1;
        const int right = 1;

        var justPressed = (int dir) => {
            keyRepeatTimer = keyRepeatWaitTime;
            arrowKeyDir = dir;
            BeamIndex += dir;
        };

        var keepPressing = (int dir) => {
            keyRepeatTimer -= dt;

            if(keyRepeatTimer <= 0)
            {
                keyRefreshTimer -= dt;

                if(keyRefreshTimer <= 0)
                {
                    BeamIndex += dir;
                    keyRefreshTimer = keyRefreshWaitTime;
                }
            }
        };
        
        if(Input.KeyPressed(Keys.Left)) justPressed(left);
        if(Input.KeyPressed(Keys.Right)) justPressed(right);
    
        if(Input.IsKeyDown(Keys.Left) && arrowKeyDir == left)
            keepPressing(left);

        if(Input.IsKeyDown(Keys.Right) && arrowKeyDir == right) 
            keepPressing(right);
    }

    public void TextInput(object? sender, TextInputEventArgs args)
    {
        char character = args.Character;

        if (character == '\b')
        {
            if (text.Length > 0)
            {
                int removeIndex = BeamIndex - 1;
                if(removeIndex < 0) return;

                text = text.Remove(removeIndex, 1);
                BeamIndex -= 1;
            }
            return;
        }

        bool canAddCharacter = (
            avaliableCharaters.Contains(character) &&
            text.Length < maxTextLength
        );

        if(canAddCharacter)
        {
            if(BeamIndex > lastCharIndex)
                text = string.Concat(text, character);
            else
                text = text.Insert(BeamIndex, character.ToString());

            BeamIndex += 1;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bodyColor);
        Vector2 textPos = Utils.CenteredTextPosInRect(rect, font, text);
        spriteBatch.DrawString(font, text, textPos, textColor);

        string textSubstring = text.Substring(0, BeamIndex);
        int substringWidth = (int)font.MeasureString(textSubstring).X;
        int beamXOffset = substringWidth - beamWidth/2;

        Rectangle beamRect = rect with { Width = beamWidth, X = rect.X + beamXOffset };
        spriteBatch.FillRectangle(beamRect, Color.White);
    }
}