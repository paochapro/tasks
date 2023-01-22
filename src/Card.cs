using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    public const int minRectHeight = 64;
    public const int rectWidth = 256;
    public const int bannerHeight = 32; //24;
    public const int cardButtonsWidth = bannerHeight;

    //Properties
    public Card Card => card;
    public Rectangle Rectangle => rectangle;
    public bool IsBeingDragged => isBeingDragged;
    public bool QueuedForRemoval => queuedForRemoval;

    //Fields
    private Rectangle rectangle;
    private List<UITaskBox> uiTaskBoxes;
    private Card card;
    private TasksProgram program;
    private bool isBeingDragged;
    private bool queuedForRemoval;

    private Color colorWheelButtonClr;
    private Color addTaskButtonClr;
    
    public UICard(TasksProgram program, Card card)
    {
        //Init stuff
        rectangle = new Rectangle(0, 0, rectWidth, 0);

        uiTaskBoxes = new();
        this.card = card;
        this.program = program;

        colorWheelButtonClr = Color.White;
        addTaskButtonClr = Color.White;

        //Adding ui taskboxes
        foreach (KeyValuePair<string, bool> task in card.Tasks)
        {
            AddTask(new UITaskBox(program, task.Key, task.Value, this));
        }
    }

    public void UpdatePosition(Point position)
    {
        //Update our location
        rectangle.Location = position;
        UpdateTaskBoxesPosition();
    }

    private void UpdateTaskBoxesPosition()
    {
        //Update taskboxes location
        Point taskboxPos = rectangle.Location;
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
        addTaskButtonClr = Color.White;
        colorWheelButtonClr = Color.White;

        //If we are being dragged - check if user released mouse1 to stop dragging this card
        //And do not update anything else
        if(isBeingDragged)
        {
            isBeingDragged = !Input.LBReleased();

            if(program.CardBinRect.Contains(Input.Mouse.Position) && !isBeingDragged)
                queuedForRemoval = true;

            return;
        }

        var updateTaskBoxes = () => {
            UpdateTaskBoxesPosition();
            
            //Update taskboxes
            foreach(UITaskBox taskbox in uiTaskBoxes)
                taskbox.Update(dt);

            int removedTaskBoxes = uiTaskBoxes.RemoveAll(tb => tb.QueuedForRemoval);

            if(removedTaskBoxes > 0)
                UpdateRectHeight();
        };


        Rectangle bannerRect = rectangle with { Height = bannerHeight };

        //Check if user clicked on + sign
        Rectangle taskButtonRect = bannerRect with { Width = cardButtonsWidth };
        taskButtonRect.X = bannerRect.Right - taskButtonRect.Width;

        if(taskButtonRect.Contains(Input.Mouse.Position))
        {
            addTaskButtonClr = Color.Black;

            if(Input.LBPressed())
                AddTask(new UITaskBox(program, "empty", false, this));

            updateTaskBoxes();
            return;
        }

        //Check if user clicked on color wheel
        Rectangle colorWheelRect = taskButtonRect;
        colorWheelRect.X = taskButtonRect.Left - colorWheelRect.Width;

        if(colorWheelRect.Contains(Input.Mouse.Position))
        {
            //Popup the color changing window
            colorWheelButtonClr = Color.Black;

            if(Input.LBPressed())
                print("Color wheel button");

            return;
        }
        
        //Check if user clicked on the banner to start dragging this card
        isBeingDragged = bannerRect.Contains(Input.Mouse.Position) && Input.LBPressed();
        updateTaskBoxes();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //Main body
        spriteBatch.FillRectangle(rectangle, bodyColor);
        
        //Banner
        Rectangle banner = rectangle with { Height = bannerHeight };
        spriteBatch.FillRectangle(banner, card.BannerColor);

        //Banner title
        Vector2 measure = program.TextFont.MeasureString(card.Title);
        Vector2 rectPos = banner.Location.ToVector2();
        float offset = (banner.Height / 2 - measure.Y / 2);
        Vector2 titlePos = rectPos + new Vector2(offset);

        //Banner title color
        int darkValue = 120;
        Color bannerTitleColor = card.BannerColor.DarkenBy(darkValue);

        spriteBatch.DrawString(program.TextFont, card.Title, titlePos, bannerTitleColor);

        //Add task and color wheel buttons
        Rectangle taskButtonRect = banner with { Width = cardButtonsWidth };
        taskButtonRect.X = banner.Right - taskButtonRect.Width;
        spriteBatch.FillRectangle(taskButtonRect, addTaskButtonClr);

        Rectangle colorWheelRect = taskButtonRect;
        colorWheelRect.X = taskButtonRect.Left - colorWheelRect.Width;
        spriteBatch.FillRectangle(colorWheelRect, colorWheelButtonClr);

        //Task boxes
        uiTaskBoxes.ForEach(tb => tb.Draw(spriteBatch));
    }

    private void AddTask(UITaskBox taskBox) { uiTaskBoxes.Add(taskBox); UpdateRectHeight(); }

    private void UpdateRectHeight()
    {
        //Calculating the rect height based on how many tasks given card has
        int rectHeight = bannerHeight + uiTaskBoxes.Count * (UITaskBox.taskHeight + UITaskBox.tasksOffset) + UITaskBox.taskMargin * 2;
        rectHeight = Math.Max(rectHeight, minRectHeight);
        rectangle.Height = rectHeight;
    }
}