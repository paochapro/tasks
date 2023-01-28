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
    public static Texture2D plusTexture;
    public static Texture2D colorWheelTexture;
    public const int minRectHeight = 64;
    public const int rectWidth = 256;
    public const int bannerHeight = 32; //24;
    public const int cardButtonsWidth = 32;

    //Properties
    public Card Card => card;
    public Rectangle Rectangle => rectangle;
    public bool IsBeingDragged => isBeingDragged;
    public bool IsBeingRenamed => titleRenameTextbox != null; //isBeingRenamed;
    public bool IsQueuedForRemoval => isQueuedForRemoval;
    public UITaskBox? DragTask { get => dragTask; set => dragTask = value; }

    //Fields
    private Color bannerColor;
    private string cardTitle;
    private List<UITaskBox> uiTaskBoxes;

    private Rectangle rectangle;
    private Card card;
    private TasksProgram program;
    private UITaskBox? dragTask;
    private UITextbox? titleRenameTextbox;
    private int placeTaskIndex;

    private bool isBeingDragged;
    private bool isQueuedForRemoval;

    //Style
    private SpriteFont font;

    private Color colorWheelButtonClr;
    private Color addTaskButtonClr;
    private Color bannerTitleColor;

    private readonly Color buttonsDefaultColor;
    private readonly Color buttonsHoverColor;
    private readonly Color bannerTitleDefaultColor;
    private readonly Color bannerTitleHoverColor;

    private int cardTitleMaxWidth;
    private int textMarginX;

    public UICard(TasksProgram program, Card card)
    {
        //Init stuff
        rectangle = new Rectangle(0, 0, rectWidth, 0);

        uiTaskBoxes = new();
        this.card = card;
        this.program = program;
        this.font = program.TextFont;

        Rectangle absoluteBanner = rectangle with { Location = Point.Zero, Height = bannerHeight };
        int textY = (int)Utils.CenteredTextPosInRect(absoluteBanner, font, "A").Y;
        textMarginX = textY;
        cardTitleMaxWidth = rectWidth - cardButtonsWidth * 2 - textMarginX*2;

        //Copying data from card
        this.bannerColor = card.BannerColor;
        this.cardTitle = card.Title;

        foreach (KeyValuePair<string, bool> task in card.Tasks)
        {
            AddTask(new UITaskBox(program, task.Key, task.Value, this));
        }

        //Colors and stuff
        const int buttonsDarkenValue = 60;
        const int buttonsHoverLightenValue = 30;
        const int bannerTitleDarkenValue = 140;
        const int bannerTitleLightenValue = 70;

        buttonsDefaultColor         = bannerColor.DarkenBy(buttonsDarkenValue);
        buttonsHoverColor           = buttonsDefaultColor.LightenBy(buttonsHoverLightenValue);
        bannerTitleDefaultColor     = bannerColor.DarkenBy(bannerTitleDarkenValue);
        bannerTitleHoverColor       = bannerTitleDefaultColor.LightenBy(bannerTitleLightenValue);

        colorWheelButtonClr = buttonsDefaultColor;
        addTaskButtonClr = buttonsDefaultColor;
        bannerTitleColor = bannerTitleDefaultColor;
    }

    public void Update(float dt)
    {
        colorWheelButtonClr = buttonsDefaultColor;
        addTaskButtonClr = buttonsDefaultColor;
        bannerTitleColor = bannerTitleDefaultColor;

        //If we are being dragged:
        //1. Check if user released mouse1 to stop dragging this card
        //2. Or tries to send it to bin
        //And do not update anything else
        if(isBeingDragged)
        {
            if(Input.LBReleased())
                isBeingDragged = false;

            if(program.CardBinRect.Contains(Input.Mouse.Position) && !isBeingDragged)
                isQueuedForRemoval = true;

            return;
        }

        if(titleRenameTextbox != null)
        {
            titleRenameTextbox.Update(dt);

            if(Input.IsKeyDown(Keys.Enter))
            {
                cardTitle = titleRenameTextbox.Text;
                titleRenameTextbox = null;
            }

            if(Input.IsKeyDown(Keys.Escape))
                titleRenameTextbox = null;

                
            return;
        }

        UpdateBannerActions(dt);
        UpdateTaskBoxes(dt);
    }

    private void UpdateBannerActions(float dt)
    {
        Rectangle bannerRect = rectangle with { Height = bannerHeight };
        Point mousePos = Input.Mouse.Position;

        //Check if user clicked on + sign
        Rectangle taskButtonRect = bannerRect with { Width = cardButtonsWidth };
        taskButtonRect.X = bannerRect.Right - taskButtonRect.Width;

        if(taskButtonRect.Contains(mousePos))
        {
            addTaskButtonClr = buttonsHoverColor;

            if(Input.LBPressed())
                AddTask(new UITaskBox(program, "empty", false, this));

            UpdateTaskBoxes(dt);
            return;
        }

        //Check if user clicked on color wheel
        Rectangle colorWheelRect = taskButtonRect;
        colorWheelRect.X = taskButtonRect.Left - colorWheelRect.Width;

        if(colorWheelRect.Contains(mousePos))
        {
            //Popup the color changing window
            colorWheelButtonClr = buttonsHoverColor;

            if(Input.LBPressed())
                print("Color wheel button");

            return;
        }

        //Check if user: 
        //1. Wants to start dragging this card
        //2. Wants to delete it
        //3. Wants to rename it
        if(bannerRect.Contains(mousePos))
        {
            isBeingDragged = Input.LBPressed();

            if(Input.MBPressed())
                isQueuedForRemoval = true;

            if(Input.KeyPressed(Keys.F2))
            {
                Point pos = Utils.CenteredTextPosInRect(bannerRect, font, cardTitle).ToPoint();
                Color bodyColor = bannerColor.DarkenBy(40);
                Color textColor = Color.White;

                titleRenameTextbox = new UITextbox(program.Window, pos, cardTitleMaxWidth, 9999, bodyColor, textColor, font, cardTitle);
            }
        }
    }

    public void UpdateTaskBoxes(float dt)
    {
        //Update taskboxes if we arent dragging any task
        if(dragTask == null)
        {
            UpdateTaskBoxesPosition();

            foreach(UITaskBox taskbox in uiTaskBoxes)
            {
                taskbox.Update(dt);

                if(taskbox.IsBeingDragged)
                {
                    dragTask = taskbox;
                    uiTaskBoxes.Remove(taskbox);
                    break;
                }
            }
        }

        //If a dragged task is in our place, update our tasks positions
        if(dragTask != null)
        {
            //Calculating the index in task list at which we are hovering when dragging a task
            Rectangle body = rectangle with { 
                Y = rectangle.Y + bannerHeight,
                Height = rectangle.Height - bannerHeight 
            };

            float mouseY = Input.Mouse.Position.ToVector2().Y;
            int dragPos = (int)mouseY - (body.Y + UITaskBox.taskMargin);
            int taskCellSpace = UITaskBox.taskHeight + UITaskBox.taskMargin;
            placeTaskIndex = dragPos / taskCellSpace;
            placeTaskIndex = clamp(placeTaskIndex, 0, uiTaskBoxes.Count);
            program.Label_placeTaskIndex.text = "placeTaskIndex: " + placeTaskIndex;

            dragTask.Owner = this;
            dragTask.Update(dt);

            if(!dragTask.IsBeingDragged)
            {
                uiTaskBoxes.Insert(placeTaskIndex, dragTask);
                dragTask = null;
            }
        }

        //Remove tasks that are in removal queue 
        uiTaskBoxes.RemoveAll(tb => tb.IsQueuedForRemoval);
        UpdateTaskBoxesPosition();
    }

    public void UpdatePosition(Point position)
    {
        //Update our location
        rectangle.Location = position;
        UpdateTaskBoxesPosition();
        UpdateRectHeight();
    }

    private void UpdateTaskBoxesPosition()
    {
        //Update taskboxes location
        int totalY = UICard.bannerHeight + UITaskBox.taskMargin;
        Point taskboxPos = rectangle.Location;
        taskboxPos.X += UITaskBox.taskMargin;
        taskboxPos.Y += totalY;

        foreach(UITaskBox taskbox in uiTaskBoxes) 
        {
            bool draggedTaskInThisCard = dragTask != null;
            bool userWantsToInsertHere = taskbox == uiTaskBoxes.ElementAtOrDefault(placeTaskIndex);

            if(draggedTaskInThisCard && userWantsToInsertHere)
                taskboxPos.Y += totalY;

            taskbox.UpdatePosition(taskboxPos);
            taskboxPos.Y += totalY;
        }
    }

    private void UpdateRectHeight()
    {
        //Calculating the rect height
        int dragging = dragTask != null ? 1 : 0;
        int rectHeight = bannerHeight + (uiTaskBoxes.Count + dragging) * (UITaskBox.taskHeight + UITaskBox.tasksOffset) + UITaskBox.taskMargin * 2;
        rectHeight = Math.Max(rectHeight, minRectHeight);
        rectangle.Height = rectHeight;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //Main body
        spriteBatch.FillRectangle(rectangle, bodyColor);
        
        //Banner
        Rectangle banner = rectangle with { Height = bannerHeight };
        spriteBatch.FillRectangle(banner, bannerColor);

        //Draw banner title
        if(titleRenameTextbox != null)
            titleRenameTextbox.Draw(spriteBatch);
        else
        {
            float scale = 1.0f;

            float textWidth = font.MeasureString(cardTitle).X;
            float widthSurpassValue = Utils.inverseLerp(0, cardTitleMaxWidth, textWidth);

            if(widthSurpassValue > 1.0f)
                scale = 1.0f / widthSurpassValue;

            Vector2 measure = font.MeasureString(cardTitle) * scale;
            Vector2 rectPos = banner.Location.ToVector2();
            float y = (banner.Height / 2 - measure.Y / 2);
            Vector2 titlePos = rectPos + new Vector2(textMarginX, y);

            spriteBatch.DrawString(font, cardTitle, titlePos, bannerTitleColor, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        if(program.DebugMode)
            return;

        //Add task and color wheel buttons
        Color hoverRectColorAddTask = Color.Transparent;
        Color hoverRectColorColorWheel = Color.Transparent;
        const int rectOnHoverLightenValue = 10;

        if(addTaskButtonClr == buttonsHoverColor)
            hoverRectColorAddTask = bannerColor.LightenBy(rectOnHoverLightenValue);

        if(colorWheelButtonClr == buttonsHoverColor)
            hoverRectColorColorWheel = bannerColor.LightenBy(rectOnHoverLightenValue);

        Rectangle taskButtonRect = banner with { Width = cardButtonsWidth };
        taskButtonRect.X = banner.Right - taskButtonRect.Width;

        spriteBatch.FillRectangle(taskButtonRect, hoverRectColorAddTask);
        spriteBatch.Draw(plusTexture, taskButtonRect, addTaskButtonClr);

        Rectangle colorWheelRect = taskButtonRect;
        colorWheelRect.X = taskButtonRect.Left - colorWheelRect.Width;

        spriteBatch.FillRectangle(colorWheelRect, hoverRectColorColorWheel);
        spriteBatch.Draw(colorWheelTexture, colorWheelRect, colorWheelButtonClr);

        //Task boxes
        uiTaskBoxes.ForEach(tb => tb.Draw(spriteBatch));

        //Draw place taskbox rect
        if(dragTask != null)
        {
            Rectangle body = new(rectangle.X, banner.Bottom, rectWidth, rectangle.Height - bannerHeight);
            
            int y = UITaskBox.taskMargin + (placeTaskIndex * (UITaskBox.taskHeight + UITaskBox.taskMargin));
            Point placeTaskBoxPos = body.Location + new Point(UITaskBox.taskMargin, y);
            Point placeTaskSize = new(UITaskBox.taskWidth, UITaskBox.taskHeight);
            Rectangle placeTaskBox = new(placeTaskBoxPos, placeTaskSize);

            spriteBatch.DrawRectangle(placeTaskBox, Color.White, 2);
            //spriteBatch.DrawRectangle(body, new Color(255,0,0,255), 1);
        }
    }

    private void AddTask(UITaskBox taskBox) { uiTaskBoxes.Add(taskBox); UpdateRectHeight(); }
}