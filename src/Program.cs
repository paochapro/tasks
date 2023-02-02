using static Lib.Utils;

namespace tasks;

public class TasksProgram : BaseGame
{
    public Rectangle CardBinRect => cardBinRect;
    public Label lbl_showingFromIndex;
    public Label lbl_showingToIndex;
    public Label Label_placeTaskIndex => lbl_placeTaskIndex;
    public SpriteFont TextFont => textFont;

    ClassicUIManager classicUIManager;
    SpriteFont textFont;

    List<UICard> uiCards = new();

    UIElement? renamingElement;
    UICard? draggedCard;
    UITaskBox? draggedTask;

    int placeCardIndex;

    //Other stuff
    const int cardStartOffset = 16;
    const int bottomBarHeight = 200;
    const int cardBinWidth = 300;

    Rectangle cardBinRect;
    Rectangle bottomBarRect;

    Label lbl_placeCardIndex;
    Label lbl_placeTaskIndex;
    Label lbl_dt;

    List<Color> listOfColors;
    List<Color> currentListOfColors;

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        placeCardIndex = 0;
        
        //If we arent dragging a card (in this case dragging a task instead) then maxIndex should count-1
        //If we are dragging a card then we should add an additional slot to maxIndex
        int maxIndex = uiCards.Count - 1;
        maxIndex += draggedCard != null ? 1 : 0;

        //Calculating the index in card list at which we are hovering when dragging a card or a task
        float mouseX = Input.Mouse.Position.ToVector2().X;
        int dragPos = (int)mouseX - cardStartOffset/2;
        int cardCellSpace = UICard.rectWidth + 16;
        placeCardIndex = dragPos / cardCellSpace;
        placeCardIndex = clamp(placeCardIndex, 0, maxIndex);
        
        //Update cards
        if(renamingElement != null)
            UpdateRenaming(dt);
        else
            UpdateDefault(dt);

        //Remove all cards in removal queue
        uiCards.RemoveAll(c => c.IsQueuedForRemoval);

        //Debug info (red text)
        lbl_placeCardIndex.text = "placeCardIndex: " + placeCardIndex;
        lbl_dt.text = "dt: " + dt.ToString();

        if(debugMode) {
            lbl_placeCardIndex.Hidden = false;
            lbl_placeTaskIndex.Hidden = false;
            lbl_dt.Hidden = false;
            lbl_showingFromIndex.Hidden = false;
            lbl_showingToIndex.Hidden = false;
        }
        else {
            lbl_placeCardIndex.Hidden = true;
            lbl_placeTaskIndex.Hidden = true;
            lbl_dt.Hidden = true;
            lbl_showingFromIndex.Hidden = true;
            lbl_showingToIndex.Hidden = true;
        }

        Input.CycleEnd();
        base.Update(gameTime);
    }

    void UpdateDefault(float dt)
    {
        if(Input.KeyPressed(Keys.OemTilde))
            debugMode = !debugMode;

        if(Input.IsKeyDown(Keys.LeftControl) && Input.KeyPressed(Keys.A))
            AddCard();

        classicUIManager.UpdateElements(Input.Keys, Input.Mouse);

        if(draggedTask != null)
            UpdateDraggingTask(dt);
        else
        {
            if(draggedCard != null)
                UpdateDraggingCard(dt);
            else
                UpdateCards(dt);
        }

    }

    void UpdateRenaming(float dt)
    {
        renamingElement.Update(dt);
            
        if(renamingElement.ElementState != ElementState.BeingRenamed)
            renamingElement = null;
    }

    void UpdateCards(float dt)
    {
        foreach(UICard card in uiCards)
        {
            card.Update(dt);
            
            //Check if card state have changed
            if(card.DragTask != null)
            {
                draggedTask = card.DragTask;
                break;
            }

            if(card.ElementState == ElementState.BeingRenamed)
            {
                renamingElement = card;
                break;    
            }

            if(card.ElementState == ElementState.BeingDragged)
            {
                draggedCard = card;
                uiCards.Remove(card);
                break;
            }
        }
    }

    void UpdateDraggingCard(float dt)
    {
        Point cardPos = new Point(cardStartOffset);

        //If we are dragging, update only drag card
        if(draggedCard != null)
        {
            draggedCard.Update(dt);

            if(draggedCard.ElementState != ElementState.BeingDragged)
            {
                //Insert drag card at place index
                uiCards.Insert(placeCardIndex, draggedCard);

                //Not dragging card anymore
                draggedCard = null;
            }
        }
    }
    
    void UpdateDraggingTask(float dt)
    {
        //If we are dragging a task, tell the card under which we are dragging our task, that its in their place
        foreach(UICard card in uiCards)
        {
            card.DragTask = null;

            if(card == uiCards.ElementAtOrDefault(placeCardIndex))
            {
                card.DragTask = draggedTask;
                card.Update(dt);
            }
        }
    }

    void AddCard()
    {
        Card defaultCard = new Card("New", currentListOfColors.FirstOrDefault());
        currentListOfColors.RemoveAt(0);

        if(currentListOfColors.Count == 0)
            currentListOfColors = listOfColors.ToList();

        UICard defaultUICard = new UICard(this, defaultCard);
        uiCards.Add(defaultUICard);
    }

    // void UpdateCardPositions()
    // {
    //     Point cardPos = new Point(cardStartOffset);

    //     if(dragCard != null)
    //     {
    //         foreach(UICard card in uiCards)
    //         {
    //             if(card == uiCards.ElementAtOrDefault(placeCardIndex))
    //                 cardPos.X += UICard.rectWidth + 16;

    //             card.UpdatePosition(cardPos);
    //             cardPos.X += UICard.rectWidth + 16;
    //         }
    //     }
    //     else
    //     {
    //         foreach(UICard card in uiCards)
    //         {
    //             card.UpdatePosition(cardPos);
    //             cardPos.X += UICard.rectWidth + 16;
    //         }
    //     }
    // }

    protected override void Draw(GameTime gameTime)
    {
        Graphics.GraphicsDevice.Clear(clearColor);
        
        SpriteBatch.Begin();
        {
            //Drawing bottom bar
            SpriteBatch.FillRectangle(bottomBarRect, clearColor.DarkenBy(20));
            SpriteBatch.FillRectangle(cardBinRect, clearColor.DarkenBy(40));

            //Drawing place rect
            if(draggedCard != null)
            {
                //Calculating place rect pos
                Point placePos = new Point(cardStartOffset);

                placePos.X += placeCardIndex * (UICard.rectWidth + 16);

                //Place rect
                Rectangle placeRect = new(placePos, new(UICard.rectWidth, draggedCard.Rectangle.Height));

                SpriteBatch.DrawRectangle(placeRect, Color.White, 2);
            }

            //Drawing cards
            foreach(UICard card in uiCards)
                card.Draw(SpriteBatch);

            //Drawing lib classicUIManager
            classicUIManager.DrawElements(SpriteBatch);

            draggedCard?.Draw(SpriteBatch);
            draggedTask?.Draw(SpriteBatch);
        }
        SpriteBatch.End();
    }

    protected override void LoadContent()
    {
        textFont = Assets.GetDefault<SpriteFont>();
        UICard.plusTexture = Content.Load<Texture2D>("plus");
        UICard.colorWheelTexture = Content.Load<Texture2D>("color_wheel");

        listOfColors = new() {
            Color.Red,
            Color.Yellow,
            Color.Green,
            Color.Blue,
            Color.Purple,
            Color.Cyan,
        };

        currentListOfColors = listOfColors.ToList();

        //UI colors
        classicUIManager.BodyDefaultColor = new Color(40,40,40);
        classicUIManager.BodyLockedColor = classicUIManager.BodyDefaultColor.DarkenBy(60);
        classicUIManager.BodyHoverColor = classicUIManager.BodyDefaultColor.LightenBy(10);

        classicUIManager.BorderDefaultColor = classicUIManager.BodyDefaultColor.DarkenBy(20);
        classicUIManager.BorderLockedColor = classicUIManager.BorderDefaultColor.DarkenBy(60);
        classicUIManager.BorderHoverColor = classicUIManager.BorderDefaultColor.LightenBy(10);

        classicUIManager.TextDefaultColor = Color.White;
        classicUIManager.TextHoverColor = Color.White;
        classicUIManager.TextLockedColor = Color.White.DarkenBy(60);

        //Bottom bar rects
        bottomBarRect = new Rectangle(0, Screen.Y - bottomBarHeight, Screen.X, bottomBarHeight);
        cardBinRect = bottomBarRect with { Width = cardBinWidth, X = bottomBarRect.Right - cardBinWidth };

        //Debug labels
        lbl_placeCardIndex = new(classicUIManager, new(0, 500), "placeCardIndex: #", Color.Red);
        lbl_placeTaskIndex = new(classicUIManager, new(0, 500-30), "placeTaskIndex: #", Color.Red);
        lbl_dt = new(classicUIManager, new(0, 500-60), "dt: #", Color.Red);

        lbl_showingFromIndex = new(classicUIManager, new(0, 500-90), "showingFromIndex: #", Color.Red);
        lbl_showingToIndex = new(classicUIManager, new(0, 500-120), "showingToIndex: #", Color.Red);

        //Randomized cards
        var addRandomizedCards = () => {
            uiCards.Clear();

            for(int c = 0; c < Rnd.Int(2,6); ++c)
            {
                Color color = Color.White;
                color.R = (byte)Rnd.Int(0,255);
                color.G = (byte)Rnd.Int(0,255);
                color.B = (byte)Rnd.Int(0,255);

                Card card = new Card("card " + c, color);
                
                for(int t = 0; t < Rnd.Int(3,12); ++t)
                {
                    string taskText = "task " + t;
                    bool isChecked = Convert.ToBoolean(Rnd.Int(0,1));
                    card.Tasks.Add(taskText, isChecked);
                }
                
                uiCards.Add(new UICard(this, card));
            }
        };

        Button generateRandom = new Button(classicUIManager, new Rectangle(500,700,200,50), addRandomizedCards, "Generate random");
        Button addCard = new Button(classicUIManager, new Rectangle(200,700,200,50), AddCard, "Add card");

        addRandomizedCards();
        
        base.LoadContent();
    }

    protected override void Initialize()
    {
        Screen = new(1400,800);
        Window.Title = GameName;
        Window.AllowUserResizing = false;
        IsMouseVisible = true;
        base.Initialize();
    }
    
    public TasksProgram()
    {
        classicUIManager = new ClassicUIManager(this);
    }
}

class Program {
    static void Main() {
        using(TasksProgram program = new TasksProgram())
            program.Run();
    }
}