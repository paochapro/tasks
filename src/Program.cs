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
    TableManager tableManager;

    List<UICard> uiCards = new();

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

    protected override void Process(float dt)
    {
        //Calculating the index in card list at which we are hovering when dragging a card or a task
        int maxIndex = uiCards.Count - 1;
        float mouseX = Input.Mouse.Position.ToVector2().X;
        int dragPos = (int)mouseX - cardStartOffset/2;
        int cardCellSpace = UICard.rectWidth + 16;
        placeCardIndex = dragPos / cardCellSpace;
        placeCardIndex = clamp(placeCardIndex, 0, maxIndex);

        if(!CheckAndUpdateStates(dt))
        {
            UpdateDefault(dt);

            if(!CheckAndUpdateStates(dt))
                UpdateCardsPositions();
        }

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
    }

    bool CheckAndUpdateStates(float dt)
    {
        bool dragging = false;
        UICard? modifiedCard = uiCards.Find(card => card.ElementState != ElementState.Default);

        if(modifiedCard != null)
        {   
            if(modifiedCard.ElementState == ElementState.BeingRenamed)
                UpdateRenaming(modifiedCard, dt);

            if(modifiedCard.ElementState == ElementState.BeingDragged)
            {
                UpdateDraggingCard(modifiedCard, dt);
                UpdateDraggingCardsPosition(modifiedCard);
                dragging = true;
            }
        }

        UITaskBox? renamingTask = uiCards.Find(card => card.RenamingTask != null)?.RenamingTask;
        UITaskBox? draggedTask = uiCards.Find(card => card.DragTask != null)?.DragTask;

        if(renamingTask != null)
            UpdateRenaming(renamingTask, dt);

        if(draggedTask != null)
            UpdateDraggingTask(draggedTask, dt);

        if(!dragging)
            UpdateCardsPositions();

        return modifiedCard != null || renamingTask != null || draggedTask != null;
    }

    void UpdateDefault(float dt)
    {
        if(Input.KeyPressed(Keys.OemTilde))
            debugMode = !debugMode;

        if(Input.IsKeyDown(Keys.LeftControl) && Input.KeyPressed(Keys.A))
            AddCard();

        classicUIManager.UpdateElements(Input.Keys, Input.Mouse);

        UpdateCards(dt);

        // if(draggedTask != null)
        //     UpdateDraggingTask(dt);
        // else
        // {
        //     if(draggedCard != null)
        //         UpdateDraggingCard(dt);
        //     else
        //         UpdateCards(dt);
        // }
    }

    void UpdateRenaming(UIElement renamingElement, float dt)
    {
        renamingElement.Update(dt);
    }

    void UpdateCards(float dt)
    {
        foreach(UICard card in uiCards)
            card.Update(dt);
    }

    void UpdateDraggingCard(UICard draggedCard, float dt)
    {
        Point cardPos = new Point(cardStartOffset);

        draggedCard.Update(dt);

        if(draggedCard.ElementState != ElementState.BeingDragged)
        {
            //Insert drag card at place index
            uiCards.Remove(draggedCard);
            uiCards.Insert(placeCardIndex, draggedCard);
        }
    }
    
    void UpdateDraggingTask(UITaskBox draggedTask, float dt)
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

    void UpdateCardsPositions()
    {
        Point cardPos = new Point(cardStartOffset);

        foreach(UICard card in uiCards)
        {
            card.UpdatePosition(cardPos);
            cardPos.X += UICard.rectWidth + 16;
        }
    }

    void UpdateDraggingCardsPosition(UICard draggedCard)
    {
        Point cardPos = new Point(cardStartOffset);

        IEnumerable<UICard> drawCards = uiCards.Where(card => !card.Equals(draggedCard));
        foreach(UICard card in drawCards)
        {
            if(card == drawCards.ElementAtOrDefault(placeCardIndex))
                cardPos.X += UICard.rectWidth + 16;

            card.UpdatePosition(cardPos);
            cardPos.X += UICard.rectWidth + 16;
        }
    }

    void AddCard()
    {
        Card defaultCard = new Card("New", currentListOfColors.FirstOrDefault(), new Dictionary<string, bool>());
        currentListOfColors.RemoveAt(0);

        if(currentListOfColors.Count == 0)
            currentListOfColors = listOfColors.ToList();

        UICard defaultUICard = new UICard(this, defaultCard);
        uiCards.Add(defaultUICard);
    }

    void Save(string filepath)
    {
        tableManager.SaveFile(uiCards.ToArray(), "tables/" + filepath);
    }

    bool Load(string filepath)
    {
        Card[]? cards = tableManager.LoadFile("tables/" + filepath);

        if(cards == null)
            return false;

        uiCards = cards.Select(card => new UICard(this, card)).ToList();

        return true;
    }

    protected override void Paint(SpriteBatch spriteBatch)
    {
        Graphics.GraphicsDevice.Clear(clearColor);
        
        SpriteBatch.Begin();
        {
            //Drawing bottom bar
            SpriteBatch.FillRectangle(bottomBarRect, clearColor.DarkenBy(20));
            SpriteBatch.FillRectangle(cardBinRect, clearColor.DarkenBy(40));

            //Drawing cards
            foreach(UICard card in uiCards)
            {
                if(card.ElementState == ElementState.BeingDragged)
                    continue;

                card.Draw(SpriteBatch);
            }

            //Drawing lib classicUIManager
            classicUIManager.DrawElements(SpriteBatch);

            UICard? draggedCard = uiCards.Find(card => card.ElementState == ElementState.BeingDragged);

            if(draggedCard != null)
            {
                Point placePos = new Point(cardStartOffset);

                placePos.X += placeCardIndex * (UICard.rectWidth + 16);

                //Place rect
                Rectangle placeRect = new(placePos, new(UICard.rectWidth, draggedCard.Rectangle.Height));

                SpriteBatch.DrawRectangle(placeRect, Color.White, 2);

                draggedCard.Draw(spriteBatch);
            }

            UITaskBox? draggedTask = uiCards.Find(card => card.DragTask != null)?.DragTask;

            if(draggedTask != null)
                draggedTask.Draw(SpriteBatch);
        }
        SpriteBatch.End();
    }

    protected override void LoadAssets()
    {
        textFont = Assets.GetDefault<SpriteFont>();
        classicUIManager = new ClassicUIManager(this);
        tableManager = new TableManager();

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

                List<KeyValuePair<string,bool>> tasks = new();

                for(int t = 0; t < Rnd.Int(3,12); ++t)
                {
                    string taskText = "task " + t;
                    bool isChecked = Convert.ToBoolean(Rnd.Int(0,1));
                    tasks.Add(new KeyValuePair<string,bool>(taskText, isChecked));
                }

                Card card = new Card("card " + c, color, tasks);
                uiCards.Add(new UICard(this, card));
            }
        };

        Textbox tb = new Textbox(classicUIManager, new(200, 750, 400, 25), "saving");

        Button generateRandom = new Button(classicUIManager, new Rectangle(500,700,200,50), addRandomizedCards, "Generate random");
        Button addCard = new Button(classicUIManager, new Rectangle(200,700,200,50), AddCard, "Add card");
        Button saveCards = new Button(classicUIManager, new Rectangle(700,700,200,50), () => Save(tb.WrittenText), "Save");
        Button loadCards = new Button(classicUIManager, new Rectangle(900,700,200,50), () => Load(tb.WrittenText), "Load");
    }

    protected override void Init() 
    {
        Window.Title = "Tasks";
    }
    
    public TasksProgram() {}
}

class Program {
    static void Main() {
        using(TasksProgram program = new TasksProgram())
            program.Run();
    }
}