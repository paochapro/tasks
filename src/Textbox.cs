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

    int showingFromIndex;
    int showingToIndex;

    int _beamIndex;
    int BeamIndex {
        get => _beamIndex;
        set {
            _beamIndex = Utils.clamp(value, 0, lastCharIndex + 1);
        }
    }

    int CappedBeamIndex => Utils.clamp(BeamIndex, 0, lastCharIndex);

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
        Controls(dt);    
        UpdateShowingIndexes(dt);
    }

    public void UpdateShowingIndexes(float dt)
    {
        const int fitCharacters = 16;

        //Based on how many characters would fit in textbox rect, calculate end index
        // int fitCharacters = 0;
        // float totalWidth = 0;

        // foreach(char ch in text.Skip(showingFromIndex))
        // {
        //     float characterWidth = font.MeasureString(ch.ToString()).X;
        //     totalWidth += characterWidth;

        //     if(totalWidth >= rect.Width)
        //         break;

        //     fitCharacters++;
        // }

        showingToIndex = showingFromIndex + fitCharacters;

        if(BeamIndex > showingToIndex)
            showingFromIndex = BeamIndex - fitCharacters;

        if(BeamIndex < showingFromIndex)
            showingFromIndex = BeamIndex;

        showingToIndex = showingFromIndex + fitCharacters;
    }

    public void Controls(float dt)
    {
        const int left = -1;
        const int right = 1;

        var ctrlSkip = (int dir) => 
        {
            const char space = ' ';
            int end = dir == left ? -1 : lastCharIndex + 1;

            int addLeft = dir == left ? 1 : 0;
            int addRight = dir == right ? 1 : 0;

            int movedOffset = Utils.clamp(BeamIndex - 1, 0, lastCharIndex);

            if(dir == left && text[movedOffset] == space)
                BeamIndex -= 1;

            if(text[CappedBeamIndex] == space)
            {
                for(int i = CappedBeamIndex; i != end; i += dir)
                {
                    if(text[i] != space)
                    {
                        BeamIndex = i;
                        break;
                    }
                    BeamIndex = i + addRight;
                }
            }

            for(int i = CappedBeamIndex; i != end; i += dir)
            {
                if(text[i] == space)
                {
                    BeamIndex = i + addLeft;
                    break;
                }
                BeamIndex = i + addRight;
            }
        };

        var justPressed = (int dir) => 
        {
            if(Input.IsKeyDown(Keys.LeftControl))
                ctrlSkip(dir);
            else
                BeamIndex += dir;

            arrowKeyDir = dir;
            keyRepeatTimer = keyRepeatWaitTime;
        };

        var keepPressing = (int dir) => 
        {
            keyRepeatTimer -= dt;

            if(keyRepeatTimer <= 0)
            {
                keyRefreshTimer -= dt;

                if(keyRefreshTimer <= 0)
                {
                    if(Input.IsKeyDown(Keys.LeftControl))
                        ctrlSkip(dir);
                    else
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

        string visibleText = "";

        if(showingFromIndex <= lastCharIndex)
        {
            int visibleTextLength = Utils.clamp((showingToIndex - showingFromIndex) + 1, 0, text.Length - showingFromIndex); 
            visibleText = text.Substring(showingFromIndex, visibleTextLength);
            spriteBatch.DrawString(font, visibleText, textPos, textColor);
        }

        int localBeamIndex = BeamIndex - showingFromIndex;
        string textSubstring = visibleText.Substring(0, localBeamIndex);
        int substringWidth = (int)font.MeasureString(textSubstring).X;
        int beamXOffset = substringWidth - beamWidth/2;

        Rectangle beamRect = rect with { Width = beamWidth, X = rect.X + beamXOffset };
        spriteBatch.FillRectangle(beamRect, Color.White);
    }
}