namespace tasks;

class UITextbox
{
    public Rectangle Rect { get => rect; set => rect = value; }
    public Color BodyColor { get => bodyColor; set => bodyColor = value; }
    public Color TextColor { get => textColor; set => textColor = value; }
    public string TextboxText => Text;

    const int beamWidth = 1;
    int showingFromIndex;
    int showingToIndex;
    TextboxInput tbInput;
    Rectangle rect;
    Color bodyColor;
    Color textColor;
    SpriteFont font;

    string Text => tbInput.Text;
    int BeamIndex => tbInput.BeamIndex;
    int LastCharIndex => Text.Length - 1;
    bool IsTextEmpty => Text.Length == 0;

    public UITextbox(GameWindow window, Point pos, int width, int maxTextLength, 
        Color bodyColor, Color textColor, SpriteFont font, string startText)
    {
        this.bodyColor = bodyColor;
        this.textColor = textColor;
        this.font = font;

        tbInput = new(window, startText, maxTextLength);
        tbInput.OnAddingCharacter += (char ch) => UpdateShowingToIndex();
        tbInput.OnDeletingCharacter += UpdateShowingToIndex;

        rect.Location = pos;
        rect.Width = width;
        rect.Height = (int)font.MeasureString(Text).Y;

        showingFromIndex = 0;
        showingToIndex = 0;
    }

    public void Update(float dt)
    {
        tbInput.Update(dt);

        UpdateShowingIndexes();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bodyColor);
        Vector2 textPos = Utils.CenteredTextPosInRect(rect, font, Text);

        string visibleText = "";

        if(showingFromIndex <= LastCharIndex)
        {
            int toIndex = Utils.clamp(showingToIndex, 0, LastCharIndex);
            int visibleTextLength = toIndex - showingFromIndex + 1; 
            visibleText = Text.Substring(showingFromIndex, visibleTextLength);
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
            textSubstring = Text.Substring(0, showingFromIndex);
            substringWidth = (int)font.MeasureString(textSubstring).X;

            int leftXOffset = substringWidth - beamWidth/2;

            textSubstring = Text.Substring(0, showingToIndex);
            substringWidth = (int)font.MeasureString(textSubstring).X;

            int rightXOffset = substringWidth - beamWidth/2;

            Rectangle leftRect = beamRect with { X = rect.X + leftXOffset, Height = 3, Width = 5 };
            Rectangle rightRect = beamRect with { X = rect.X + rightXOffset, Height = 3, Width = 5 };
            spriteBatch.FillRectangle(leftRect, Color.Red);
            spriteBatch.FillRectangle(rightRect, Color.Blue);
        };
    }

    void UpdateShowingIndexes()
    {
        if(IsTextEmpty)
        {
            showingFromIndex = 0;
            showingToIndex = 0;
        }

        //Calculate new from and to indexes
        int lastVisibleCharacterIndex = Utils.clamp(BeamIndex, 0, LastCharIndex);
        int firstVisibleCharacterIndex = Utils.clamp(BeamIndex, 0, LastCharIndex);

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

    void UpdateShowingFromIndex()
    {
        if(IsTextEmpty)
        {
            showingFromIndex = 0;
            return;
        }

        int result = showingToIndex - GetFitCharacters(showingToIndex, -1) + 1;
        showingFromIndex = result;
    }

    void UpdateShowingToIndex()
    {
        if(IsTextEmpty)
        {
            showingToIndex = 0;
            return;
        }

        int result = showingFromIndex + GetFitCharacters(showingFromIndex, 1) - 1;
        showingToIndex = result;
    }

    int GetFitCharacters(int index, int dir)
    {
        int fitCharacters = 0;
        int end = (dir == -1) ? -1 : LastCharIndex+1;
        float totalWidth = 0;

        if(index > LastCharIndex || index < 0)
            return 0;

        for(int i = index; i != end; i += dir)
        {
            totalWidth += font.MeasureString(Text[i].ToString()).X;

            if(totalWidth > rect.Width)
                break;

            fitCharacters++;
        }

        //string strDir = dir == -1 ? "left" : "right";
        //Utils.print($"(from {index} to {strDir}) fit characters: {fitCharacters}");

        return fitCharacters;
    }
}