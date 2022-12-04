using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Lib;

using static Lib.Utils;

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

public class UICard
{
    //Static
    public static readonly Color bodyColor = new(90, 90, 90);
    public const int minRectHeight = bannerHeight + 4 * (UITaskBox.taskHeight + tasksOffset) + tasksMargin * 2;
    public const int rectWidth = 256;
    public const int bannerHeight = 32;
    public const int tasksOffset = 4;
    public const int tasksMargin = 6;

    //Main
    public Rectangle Rectangle => rectangle;
    private Rectangle rectangle;
    private List<UITaskBox> UITaskBoxes;

    //Card    
    public Card Card => card;
    private Card card;
    
    public UICard(Card card)
    {
        this.card = card;
        int rectHeight = bannerHeight + card.Tasks.Count * (UITaskBox.taskHeight + tasksOffset) + tasksMargin * 2;
        rectHeight = Math.Max(rectHeight, minRectHeight);

        UITaskBoxes = new();
        rectangle = new Rectangle(0, 0, rectWidth, rectHeight);

        foreach (KeyValuePair<string, bool> task in card.Tasks)
        {
            UITaskBoxes.Add(new UITaskBox(task.Key, task.Value, this));
        }
    }

    public bool Update(Point position)
    {
        rectangle.Location = position;

        Point taskboxPos = position;
        taskboxPos.X += UITaskBox.taskMargin;
        taskboxPos.Y += UICard.bannerHeight;

        foreach(UITaskBox taskbox in UITaskBoxes) 
        {
            taskboxPos.Y += UITaskBox.taskMargin;
            taskbox.Update(taskboxPos);
            taskboxPos.Y += UITaskBox.taskHeight;
        }
        
        Rectangle bannerRect = rectangle with { Height = bannerHeight };
        return bannerRect.Contains(Input.Mouse.Position) && Input.LBPressed();
    }

    public void Draw(SpriteBatch spriteBatch, Point position)
    {
        rectangle.Location = position;

        spriteBatch.FillRectangle(rectangle, bodyColor);
        
        Rectangle banner = rectangle with { Height = bannerHeight };
        spriteBatch.FillRectangle(banner, card.BannerColor);
        
        Point taskboxPos = position;
        taskboxPos.X += UITaskBox.taskMargin;
        taskboxPos.Y += UICard.bannerHeight;

        foreach(UITaskBox taskbox in UITaskBoxes) 
        {
            taskboxPos.Y += UITaskBox.taskMargin;
            taskbox.Draw(spriteBatch, taskboxPos);
            taskboxPos.Y += UITaskBox.taskHeight;
        }
    }
}