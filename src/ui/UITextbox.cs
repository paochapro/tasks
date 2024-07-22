using System.Security.Cryptography;

namespace tasks;

class UITextbox
{
    public Rectangle Rect { get => rect; set => rect = value; }
    public Color BodyColor { get => bodyColor; set => bodyColor = value; }
    public Color TextColor { get => textColor; set => textColor = value; }
    public string TextboxText => tbInput.Text;

    const int beamWidth = 1;
    int showingFromIndex;
    int showingToIndex;
    TextboxInput tbInput;
    Rectangle rect;
    Color bodyColor;
    Color textColor;
    SpriteFont font;
    int rightBoundCharacterIndex = -1;
    int leftBoundCharacterIndex = 0;

    //int BeamIndex => tbInput.BeamIndex;
    int LastCharIndex => tbInput.Text.Length - 1;
    bool IsTextEmpty => tbInput.Text.Length == 0;

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
        rect.Height = (int)font.MeasureString("a").Y;

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
        //Draw body and end the spriteBatch
        spriteBatch.FillRectangle(rect, bodyColor);
        spriteBatch.End();

        //Draw text
        float textPosX = GetTextPositionX();

        var beamSubstring = tbInput.Text.Substring(0, tbInput.BeamIndex);
        var beamSubstringWidth = font.MeasureString(beamSubstring).X;
        int beamPosX = (int)(textPosX + beamSubstringWidth - beamWidth/2);

        if(beamPosX < rect.X) {
            leftBoundCharacterIndex = tbInput.BeamIndex;
            rightBoundCharacterIndex = -1;
            textPosX = GetTextPositionX();
        }

        if(beamPosX > rect.X + rect.Width) {
            rightBoundCharacterIndex = tbInput.BeamIndex - 1;
            leftBoundCharacterIndex = -1;
            textPosX = GetTextPositionX();
        }

        RasterizerState rasterizer = new() { ScissorTestEnable = true };

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, rasterizer);
        {
            Rectangle previousScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
            spriteBatch.GraphicsDevice.ScissorRectangle = rect;

            spriteBatch.DrawString(font, tbInput.Text, new Vector2(textPosX, rect.Y), textColor);

            spriteBatch.GraphicsDevice.ScissorRectangle = previousScissorRect;
        }
        spriteBatch.End();

        //Resume the spriteBatch
        spriteBatch.Begin();

        Rectangle beamRect = rect with { Width = beamWidth, X = beamPosX };
        spriteBatch.FillRectangle(beamRect, Color.White);
    }

    public void DrawSubstring(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rect, bodyColor);
        Vector2 textPos = Utils.CenteredTextPosInRect(rect, font, tbInput.Text);

        string visibleText = "";

        if(showingFromIndex <= LastCharIndex)
        {
            int toIndex = Utils.clamp(showingToIndex, 0, LastCharIndex);
            int visibleTextLength = toIndex - showingFromIndex + 1; 
            visibleText = tbInput.Text.Substring(showingFromIndex, visibleTextLength);
            spriteBatch.DrawString(font, visibleText, textPos, textColor);
        }

        int localBeamIndex = tbInput.BeamIndex - showingFromIndex;
        string textSubstring = visibleText.Substring(0, localBeamIndex);
        int substringWidth = (int)font.MeasureString(textSubstring).X;
        int beamXOffset = substringWidth - beamWidth/2;

        Rectangle beamRect = rect with { Width = beamWidth, X = rect.X + beamXOffset };
        spriteBatch.FillRectangle(beamRect, Color.White);

        //Do not use, the function doesnt handle situations where indexes are out of bounds for now
        var drawVisibleTextIndexes = () => {
            textSubstring = tbInput.Text.Substring(0, showingFromIndex);
            substringWidth = (int)font.MeasureString(textSubstring).X;

            int leftXOffset = substringWidth - beamWidth/2;

            textSubstring = tbInput.Text.Substring(0, showingToIndex);
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
        int lastVisibleCharacterIndex = Utils.clamp(tbInput.BeamIndex, 0, LastCharIndex);
        int firstVisibleCharacterIndex = Utils.clamp(tbInput.BeamIndex, 0, LastCharIndex);

        if(tbInput.BeamIndex > showingToIndex)
        {
            showingToIndex = lastVisibleCharacterIndex;
            UpdateShowingFromIndex();
        }

        if(tbInput.BeamIndex <= showingFromIndex)
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
            totalWidth += font.MeasureString(tbInput.Text[i].ToString()).X;

            if(totalWidth > rect.Width)
                break;

            fitCharacters++;
        }

        //string strDir = dir == -1 ? "left" : "right";
        //Utils.print($"(from {index} to {strDir}) fit characters: {fitCharacters}");

        return fitCharacters;
    }

    float GetTextPositionX() 
    {
        float textPosX = rect.Location.X;

        string visibleSubstring;

        if(tbInput.Text.Length > 0)
        {
            if(rightBoundCharacterIndex != -1)
                visibleSubstring = tbInput.Text.Substring(0, rightBoundCharacterIndex + 1);
            else
                visibleSubstring = tbInput.Text.Substring(leftBoundCharacterIndex);

            var wholeMeasure = font.MeasureString(tbInput.Text);
            var substringMeasure = font.MeasureString(visibleSubstring);

            if(rightBoundCharacterIndex != -1)
                textPosX += rect.Width - substringMeasure.X;
            else
                textPosX -= wholeMeasure.X - substringMeasure.X;
        }

        return textPosX;
    }
}