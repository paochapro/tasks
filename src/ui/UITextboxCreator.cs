namespace tasks;

class UITextboxCreator
{
    GameWindow gameWindow;
    int width;
    int maxTextLength;
    Color bodyColor;
    Color textColor;
    SpriteFont font;

    public UITextboxCreator(GameWindow gameWindow, int width, int maxTextLength, Color bodyColor, Color textColor, SpriteFont font) {
        this.gameWindow = gameWindow;
        this.width = width;
        this.maxTextLength = maxTextLength;
        this.bodyColor = bodyColor;
        this.textColor = textColor;
        this.font = font;
    }

    public UITextbox CreateUITextbox(Point pos, string startText) 
    {
        return new(gameWindow, pos, width, maxTextLength, bodyColor, textColor, font, startText);
    }
}