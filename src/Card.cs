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
    //Static style vars
    public static readonly Color bodyColor = new(90, 90, 90);
    public const int minRectHeight = bannerHeight + 4 * (UITaskBox.taskHeight + tasksOffset) + tasksMargin * 2;
    public const int rectWidth = 256;
    public const int bannerHeight = 32;
    public const int tasksOffset = 4;
    public const int tasksMargin = 6;

    //Main
    public Rectangle Rectangle => rectangle;
    public bool IsBeingDragged => isBeingDragged;

    private Rectangle rectangle;
    private List<UITaskBox> uiTaskBoxes;
    private bool isBeingDragged;

    //Data card    
    public Card Card => card;
    private Card card;
    
    public UICard(Card card)
    {
        //Calculating the rect height based on how many tasks given card has
        int rectHeight = bannerHeight + card.Tasks.Count * (UITaskBox.taskHeight + tasksOffset) + tasksMargin * 2;
        rectHeight = Math.Max(rectHeight, minRectHeight);

        //Init stuff
        rectangle = new Rectangle(0, 0, rectWidth, rectHeight);
        uiTaskBoxes = new();
        this.card = card;

        //Adding ui taskboxes
        foreach (KeyValuePair<string, bool> task in card.Tasks)
        {
            uiTaskBoxes.Add(new UITaskBox(task.Key, task.Value, this));
        }
    }

    public void UpdatePosition(Point position)
    {
        //Update our location
        rectangle.Location = position;

        //Update taskboxes location
        Point taskboxPos = position;
        taskboxPos.X += UITaskBox.taskMargin;
        taskboxPos.Y += UICard.bannerHeight;

        foreach(UITaskBox taskbox in uiTaskBoxes) 
        {
            taskboxPos.Y += UITaskBox.taskMargin;
            taskbox.UpdatePosition(taskboxPos);
            taskboxPos.Y += UITaskBox.taskHeight;
        }
    }

    public void Update(float dt)
    {
        //If we are being dragged - check if user released mouse1 to stop dragging this card
        //And do not update anything else
        if(isBeingDragged)
        {
            isBeingDragged = !Input.LBReleased();
            return;
        }

        foreach(UITaskBox taskbox in uiTaskBoxes)
            taskbox.Update(dt);

        //Check if user clicked on the banner to start dragging this card
        Rectangle bannerRect = rectangle with { Height = bannerHeight };
        isBeingDragged = bannerRect.Contains(Input.Mouse.Position) && Input.LBPressed();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //Drawing main body
        spriteBatch.FillRectangle(rectangle, bodyColor);
        
        //Drawing banner
        Rectangle banner = rectangle with { Height = bannerHeight };
        spriteBatch.FillRectangle(banner, card.BannerColor);

        //Drawing task boxes
        uiTaskBoxes.ForEach(tb => tb.Draw(spriteBatch));
    }
}