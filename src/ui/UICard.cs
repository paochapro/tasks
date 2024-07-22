using static Lib.Utils;

namespace tasks;

public class UICard : UIElement
{
    static readonly Color defaultBodyColor = new(90, 90, 90);

    public const int minRectHeight = 64;
    public const int rectWidth = 256;
    public const int bannerHeight = 32;
    public const int cardButtonsWidth = 32;
    public const int bottomAddition = 2;

    //Generate card
    public Card GeneratedCard {
        get  {
            var createKeyValuePair = (UITaskBox tb) => new KeyValuePair<string,bool>(tb.Description, tb.IsChecked);
            var tasks = uiTaskBoxes.Select(createKeyValuePair);
            Card result = new Card(cardTitle, bannerColor, tasks);
            return result;
        }
    }

    public Color BannerColor => bannerColor; 
    public Rectangle Rectangle => rectangle;
    public UITaskBox? DragTask { get => dragTask; set => dragTask = value; }
    public UITaskBox? RenamingTask => uiTaskBoxes.Find(tb => tb.ElementState == ElementState.BeingRenamed);
    public ElementState ElementState => elementState;
    public bool IsQueuedForRemoval => isQueuedForRemoval;

    bool isCompleted => uiTaskBoxes.All(tb => tb.IsChecked);

    Card card;
    Color bannerColor;
    string cardTitle;
    List<UITaskBox> uiTaskBoxes;
    Rectangle rectangle;
    UITaskBox? dragTask;
    TasksProgram program;
    int placeTaskIndex;
    bool isQueuedForRemoval;
    UITextboxCreator renameTbCreator;
    ElementState elementState;

    UITextbox? _renameTextbox;
    UITextbox? renameTextbox {
        get => _renameTextbox;
        set {
            _renameTextbox = value;
            elementState = _renameTextbox == null ? ElementState.Default : ElementState.BeingRenamed;
        }
    }

    SpriteFont font;
    Color colorWheelButtonClr;
    Color addTaskButtonClr;
    Color bodyColor;
    Texture2D plusTexture;
    Texture2D colorWheelTexture;
    Color buttonsDefaultColor;
    Color buttonsHoverColor;
    Color bannerTitleColor;
    Color tbBodyColor;
    Color tbTextColor;
    Color completedBodyColor;
    readonly int cardTitleMaxWidth;
    readonly int textMarginX;

    public UICard(TasksProgram program, Card card)
    {
        //Init stuff
        rectangle = new Rectangle(0, 0, rectWidth, 0);

        uiTaskBoxes = new();
        this.card = card;
        this.program = program;
        this.font = program.TextFont;

        plusTexture = program.Content.Load<Texture2D>("plus");
        colorWheelTexture = program.Content.Load<Texture2D>("color_wheel");

        Rectangle absoluteBanner = rectangle with { Location = Point.Zero, Height = bannerHeight };
        int textY = (int)Utils.CenteredTextPosInRect(absoluteBanner, font, "A").Y;
        textMarginX = textY;
        cardTitleMaxWidth = rectWidth - cardButtonsWidth * 2 - textMarginX*2;

        //Copying data from card
        this.cardTitle = card.Title;

        foreach (KeyValuePair<string, bool> task in card.Tasks)
        {
            AddTask(new UITaskBox(program, task.Key, task.Value, this));
        }

        UpdateColor(card.BannerColor);
    }

    public void Update(float dt)
    {
        bodyColor = defaultBodyColor;
        colorWheelButtonClr = buttonsDefaultColor;
        addTaskButtonClr = buttonsDefaultColor;

        if(isCompleted && uiTaskBoxes.Count != 0)
            bodyColor = completedBodyColor;

        if(DragTask != null)
            UpdateDraggingTask(dt);
        else
        {
            switch(elementState)
            {
                case ElementState.Default:
                    UpdateDefault(dt);
                    break;
                case ElementState.BeingDragged:
                    UpdateDragging(dt);
                    break;
                case ElementState.BeingRenamed:
                    UpdateRenaming(dt);
                    break;
            }
        }
    }

    public void UpdatePosition(Point pos)
    {
        //Update our location
        rectangle.Location = pos;
        UpdateTaskBoxesPosition();
        UpdateRectHeight();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //Main body
        spriteBatch.FillRectangle(rectangle, bodyColor);
        
        //Banner
        Rectangle banner = rectangle with { Height = bannerHeight };
        spriteBatch.FillRectangle(banner, bannerColor);

        //Draw banner title
        if(renameTextbox != null)
            renameTextbox.Draw(spriteBatch);
        else
            DrawTitle(spriteBatch);

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
        if(isCompleted)
            uiTaskBoxes.ForEach(tb => tb.Draw(spriteBatch, bannerColor));
        else
            uiTaskBoxes.ForEach(tb => tb.Draw(spriteBatch));

        //Draw place taskbox rect
        if(dragTask != null)
        {
            Rectangle body = new(rectangle.X, banner.Bottom, rectWidth, rectangle.Height - bannerHeight);
            
            int y = UITaskBox.taskMargin + (placeTaskIndex * (UITaskBox.taskHeight + UITaskBox.tasksOffset));
            Point placeTaskBoxPos = body.Location + new Point(UITaskBox.taskMargin, y);
            Point placeTaskSize = new(UITaskBox.taskWidth, UITaskBox.taskHeight);
            Rectangle placeTaskBox = new(placeTaskBoxPos, placeTaskSize);

            spriteBatch.DrawRectangle(placeTaskBox, Color.White, 2);
            //spriteBatch.DrawRectangle(body, new Color(255,0,0,255), 1);
        }
    }

    void DrawTitle(SpriteBatch spriteBatch)
    {
        Rectangle banner = rectangle with { Height = bannerHeight };
        spriteBatch.FillRectangle(banner, bannerColor);

        float scale = GetBoundedTextScale(cardTitle, cardTitleMaxWidth, font);

        float x = textMarginX;
        float y = CenteredTextPosInRect(banner, font, cardTitle, scale).Y;
        Vector2 titlePos = new Vector2(x + rectangle.X, y);

        spriteBatch.DrawString(font, cardTitle, titlePos, bannerTitleColor, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
    }

    //State updates
    void UpdateDragging(float dt)
    {
        Vector2 mousePos = Input.Mouse.Position.ToVector2();
        Vector2 bannerSize = new Vector2(UICard.rectWidth, UICard.bannerHeight);
        Vector2 dragCardPos = mousePos - bannerSize / 2;

        UpdatePosition(dragCardPos.ToPoint());

        if(Input.LBReleased())
            elementState = ElementState.Default;
    }

    void UpdateRenaming(float dt)
    {
        renameTextbox.Update(dt);

        if(Input.IsKeyDown(Keys.Enter))
        {
            cardTitle = renameTextbox.TextboxText;
            renameTextbox = null;
        }

        if(Input.IsKeyDown(Keys.Escape))
        {
            renameTextbox = null;
        }
    }

    void UpdateDefault(float dt)
    {
        UpdateBannerActions(dt);
        UpdateTaskBoxes(dt);
    }

    void UpdateDraggingTask(float dt)
    {
        //Calculating the index in task list at which we are hovering when dragging a task
        Rectangle body = rectangle with { 
            Y = rectangle.Y + bannerHeight,
            Height = rectangle.Height - bannerHeight 
        };

        int maxIndex = uiTaskBoxes.Count;
        float mouseY = Input.Mouse.Position.ToVector2().Y;
        int dragPos = (int)mouseY - (body.Y + UITaskBox.taskMargin);
        int taskCellSpace = UITaskBox.taskHeight + UITaskBox.tasksOffset;
        placeTaskIndex = dragPos / taskCellSpace;
        placeTaskIndex = clamp(placeTaskIndex, 0, maxIndex);

        dragTask.Owner = this;
        dragTask.Update(dt);

        if(dragTask.ElementState != ElementState.BeingDragged)
        {
            uiTaskBoxes.Remove(dragTask);
            uiTaskBoxes.Insert(placeTaskIndex, dragTask);
            dragTask = null;
        }

        UpdateTaskBoxesPosition();
    }

    //Update certain parts
    void UpdateBannerActions(float dt)
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
                program.CreateColorWheelDialog(this);

            return;
        }

        //Check if user: 
        //1. Wants to start dragging this card
        //2. Wants to delete it
        //3. Wants to rename it
        if(bannerRect.Contains(mousePos))
        {
            if(Input.LBPressed())
                elementState = ElementState.BeingDragged;

            if(Input.MBPressed())
                isQueuedForRemoval = true;

            if(Input.KeyPressed(Keys.F2))
            {
                Point pos = Utils.CenteredTextPosInRect(bannerRect, font, cardTitle).ToPoint();
                renameTextbox = renameTbCreator.CreateUITextbox(pos, cardTitle);
            }
        }
    }

    void UpdateTaskBoxes(float dt)
    {
        foreach(UITaskBox taskbox in uiTaskBoxes)
        {
            taskbox.Update(dt);

            if(taskbox.ElementState == ElementState.BeingDragged)
            {
                dragTask = taskbox;
                uiTaskBoxes.Remove(dragTask);
                UpdateDraggingTask(dt);
                return;
            }
        }

        //Remove tasks that are in removal queue 
        uiTaskBoxes.RemoveAll(tb => tb.IsQueuedForRemoval);
        UpdateTaskBoxesPosition();
    }

    void UpdateTaskBoxesPosition()
    {
        //Update taskboxes location
        int totalY = UITaskBox.taskHeight + UITaskBox.tasksOffset;
        Point taskboxPos = rectangle.Location;
        taskboxPos.X += UITaskBox.taskMargin;
        taskboxPos.Y += UICard.bannerHeight + UITaskBox.taskMargin;

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

    void UpdateRectHeight()
    {
        //Calculating the rect height
        int count = uiTaskBoxes.Count + (dragTask != null ? 1 : 0);
        int needsBottomAddition = count >= 2 ? bottomAddition : 0;
        int rectHeight = bannerHeight + (count * UITaskBox.taskHeight) + ( (count-1) * UITaskBox.tasksOffset) + UITaskBox.taskMargin * 2 + needsBottomAddition;
        rectHeight = Math.Max(rectHeight, minRectHeight);
        rectangle.Height = rectHeight;
    }

    public void UpdateColor(Color newBannerColor)
    {
        bannerColor = newBannerColor;

        //Colors and stuff
        const int buttonsDarkenValue = 60;
        const int buttonsHoverLightenValue = 30;
        const int bannerTitleDarkenValue = 140;

        completedBodyColor = bannerColor.DarkenBy(60);

        buttonsDefaultColor         = bannerColor.DarkenBy(buttonsDarkenValue);
        buttonsHoverColor           = buttonsDefaultColor.LightenBy(buttonsHoverLightenValue);
        bannerTitleColor            = bannerColor.DarkenBy(bannerTitleDarkenValue);

        colorWheelButtonClr = buttonsDefaultColor;
        addTaskButtonClr = buttonsDefaultColor;

        tbBodyColor = bannerColor.DarkenBy(40);
        tbTextColor = Color.White;
        renameTbCreator = new UITextboxCreator(program.Window, cardTitleMaxWidth, -1, tbBodyColor, tbTextColor, font);
    }

    void AddTask(UITaskBox taskBox)
    { 
        uiTaskBoxes.Add(taskBox); 
        UpdateRectHeight(); 
    }
}