using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;

namespace tasks;

public class Card
{
    //Main fields
    public Dictionary<string, bool> Tasks { get; private set; }
    public Color BannerColor { get; set; }
    public string Title { get; set; }

    public Card(string title, Color bannerColor)
    {
        BannerColor = bannerColor;
        Title = title;
        Tasks = new();
    }
}

public class DrawCard
{
    //Static
    public static readonly Color bodyColor = new(90, 90, 90);
    public const int minRectHeight = 64;
    public const int rectWidth = 256;
    public const int bannerHeight = 24;

    //Draw
    public Rectangle Rectangle => rectangle;
    private Rectangle rectangle;
    
    //Main card
    public Card Card => card;
    private Card card;

    private List<TaskBox> tasks;
    
    public DrawCard(Card card)
    {
        tasks = new();

        const int span = 4;
        int y = bannerHeight + span;
        
        foreach (KeyValuePair<string, bool> task in card.Tasks)
        {
            tasks.Add(new TaskBox(task.Key, task.Value, this, y));
            y += TaskBox.taskHeight + span;
        }
        
        rectangle = new Rectangle(0, 0, rectWidth, minRectHeight * 6);
        this.card = card;
    }

    public void Update() => tasks.ForEach(t => t.Update());

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(rectangle, bodyColor);
        
        Rectangle banner = rectangle with { Height = bannerHeight };
        spriteBatch.FillRectangle(banner, card.BannerColor);
        
        tasks.ForEach(t => t.Draw(spriteBatch));
    }
}