namespace tasks;

class UITextbox
{
    const string avaliableCharaters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890-=!@#$%^&*()_+[]{};':|\\\",./<>?`~ ";
    const int beamWidth = 1;
    string text;
    int maxTextLength;
    int lastCharIndex => text.Length-1;
    int beamMaxIndex => lastCharIndex+1;
    Rectangle rect;
    Color bodyColor;
    Color textColor;
    SpriteFont font;

    public TasksProgram program;

    int showingFromIndex;
    int showingToIndex;

    int _beamIndex;
    int BeamIndex {
        get => _beamIndex;
        set {
            _beamIndex = Utils.clamp(value, 0, beamMaxIndex);
            //UpdateShowingIndexes();
        }
    }

    //int CappedBeamIndex => Utils.clamp(BeamIndex, 0, lastCharIndex);

    //Emulating key repeating
    const float keyRepeatWaitTime = 0.5f;
    const float keyRefreshWaitTime = 0.02f;
    float keyRepeatTimer;
    float keyRefreshTimer;
    int arrowKeyDir;

    public Rectangle Rect { get => rect; set => rect = value; }
    public Color BodyColor { get => bodyColor; set => bodyColor = value; }
    public Color TextColor { get => textColor; set => textColor = value; }
    public EventHandler<TextInputEventArgs> TextInputHandler => TextInput;
    public string Text => text;

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

        showingFromIndex = 0;
        showingToIndex = 0;

        windowToConnect.TextInput += TextInputHandler;
    }

    public void Update(float dt)
    {
        Controls(dt);
        UpdateShowingIndexes();
        program.lbl_showingFromIndex.text = "showingFromIndex: " + showingFromIndex; 
        program.lbl_showingToIndex.text = "showingToIndex: " + showingToIndex;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bodyColor);
        Vector2 textPos = Utils.CenteredTextPosInRect(rect, font, text);

        string visibleText = "";

        if(showingFromIndex <= lastCharIndex)
        {
            int toIndex = Utils.clamp(showingToIndex, 0, lastCharIndex);
            int visibleTextLength = toIndex - showingFromIndex + 1; 
            visibleText = text.Substring(showingFromIndex, visibleTextLength);
            spriteBatch.DrawString(font, visibleText, textPos, textColor);
        }

        int localBeamIndex = BeamIndex - showingFromIndex;
        string textSubstring = visibleText.Substring(0, localBeamIndex);
        int substringWidth = (int)font.MeasureString(textSubstring).X;
        int beamXOffset = substringWidth - beamWidth/2;

        Rectangle beamRect = rect with { Width = beamWidth, X = rect.X + beamXOffset };
        spriteBatch.FillRectangle(beamRect, Color.White);

        //Do not use, the function doesnt handle situations where indexes are out of bounds for now
        var drawVisibleTextIndexes = () => {
            textSubstring = text.Substring(0, showingFromIndex);
            substringWidth = (int)font.MeasureString(textSubstring).X;

            int leftXOffset = substringWidth - beamWidth/2;

            textSubstring = text.Substring(0, showingToIndex);
            substringWidth = (int)font.MeasureString(textSubstring).X;

            int rightXOffset = substringWidth - beamWidth/2;

            Rectangle leftRect = beamRect with { X = rect.X + leftXOffset, Height = 3, Width = 5 };
            Rectangle rightRect = beamRect with { X = rect.X + rightXOffset, Height = 3, Width = 5 };
            spriteBatch.FillRectangle(leftRect, Color.Red);
            spriteBatch.FillRectangle(rightRect, Color.Blue);
        };
    }

    private void UpdateShowingIndexes()
    {
        if(text.Length == 0)
        {
            showingFromIndex = 0;
            showingToIndex = 0;
        }

        //Calculate new from and to indexes
        int lastVisibleCharacterIndex = Utils.clamp(BeamIndex, 0, lastCharIndex);
        int firstVisibleCharacterIndex = Utils.clamp(BeamIndex, 0, lastCharIndex);

        if(BeamIndex > showingToIndex)
        {
            showingToIndex = lastVisibleCharacterIndex;
            UpdateShowingFromIndex();
        }

        if(BeamIndex <= showingFromIndex)
        {
            showingFromIndex = firstVisibleCharacterIndex;
            UpdateShowingToIndex();
        }
    }

    private void UpdateShowingFromIndex()
    {
        if(text.Length == 0)
        {
            showingFromIndex = 0;
            return;
        }

        int result = showingToIndex - GetFitCharacters(showingToIndex, -1) + 1;
        showingFromIndex = result;
    }

    private void UpdateShowingToIndex()
    {
        if(text.Length == 0)
        {
            showingToIndex = 0;
            return;
        }

        int result = showingFromIndex + GetFitCharacters(showingFromIndex, 1) - 1;
        showingToIndex = result;
    }

    private int GetFitCharacters(int index, int dir)
    {
        int fitCharacters = 0;
        int end = (dir == -1) ? -1 : lastCharIndex+1;
        float totalWidth = 0;

        if(index > lastCharIndex || index < 0)
            return 0;

        for(int i = index; i != end; i += dir)
        {
            totalWidth += font.MeasureString(text[i].ToString()).X;

            if(totalWidth > rect.Width)
                break;

            fitCharacters++;
        }

        //string strDir = dir == -1 ? "left" : "right";
        //Utils.print($"(from {index} to {strDir}) fit characters: {fitCharacters}");

        return fitCharacters;
    }

    private void Controls(float dt)
    {
        const int left = -1;
        const int right = 1;

        var justPressed = (int dir) => 
        {
            if(Input.IsKeyDown(Keys.LeftControl))
                MoveBeamToWordSide(dir);
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
                        MoveBeamToWordSide(dir);
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

    private void TextInput(object? sender, TextInputEventArgs args)
    {
        char character = args.Character;
        const char backspace = '\b';
        const char delete = (char)127;

        if (text.Length > 0)
        {
            if (character == backspace)
            {
                if(Input.IsKeyDown(Keys.LeftControl))
                    EraseWord();
                else 
                {
                    int removeIndex = BeamIndex - 1;
                    if(removeIndex < 0) return;

                    text = text.Remove(removeIndex, 1);
                    BeamIndex -= 1;
                }

                UpdateShowingToIndex();

                return;
            }

            if (character == delete)
            {
                if(BeamIndex <= lastCharIndex)
                    text = text.Remove(BeamIndex, 1);

                UpdateShowingToIndex();

                return;
            }
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

            UpdateShowingToIndex();
        }
    }

    private int IndexClamp(int index) => Utils.clamp(index, 0, lastCharIndex);

    //Control functionality
    private void EraseWord()
    {
        const char space = ' ';
        const int left = -1;

        int endBeamIndex = BeamIndex;

        int checkIndex = BeamIndex - 1;
        checkIndex = IndexClamp(checkIndex);

        if(text[checkIndex] == space)
            MoveBeamWhile(left, space);
        else
            MoveBeamUntil(left, space);

        text = text.Remove(BeamIndex, endBeamIndex - BeamIndex);
    }

    private void MoveBeamToWordSide(int dir)
    {
        if(text.Length == 0) return;

        const int left = -1;
        const int right = 1;
        const char space = ' ';

        //Check if character at beam (or previous from beam in case of going left) is space
        //If it is them move beam to word 
        int checkIndex = BeamIndex;

        if(dir == left) 
            checkIndex -= 1;
        
        checkIndex = IndexClamp(checkIndex);

        if(text[checkIndex] == space)
        {
            MoveBeamWhile(dir, space);

            if(BeamIndex == 0)
                return;

            if(BeamIndex == beamMaxIndex)
                return;
        }

        //When beam is at word then move it to word side
        MoveBeamUntil(dir, space);
    }

    private void MoveBeam(int dir, char ch, bool whileCharacter)
    {
        if(text.Length == 0) return;

        const int left = -1;
        const int right = 1;

        int end = dir == left ? -1 : beamMaxIndex;

        for(int i = BeamIndex; i != end; i += dir)
        {
            int checkIndex = i;

            if(dir == left)
            {
                checkIndex -= 1;
                checkIndex = IndexClamp(checkIndex);
            }

            bool breakCondition;

            if(whileCharacter)
                breakCondition = text[checkIndex] != ch;
            else
                breakCondition = text[checkIndex] == ch;

            BeamIndex = i;
            
            if(breakCondition)
                return;
        }

        if(dir == left) BeamIndex = 0;
        if(dir == right) BeamIndex = beamMaxIndex;
    }

    private void MoveBeamUntil(int dir, char ch) => MoveBeam(dir, ch, false);
    private void MoveBeamWhile(int dir, char ch) => MoveBeam(dir, ch, true);
}