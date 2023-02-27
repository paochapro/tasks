using static Lib.Utils;
using Myra.Graphics2D.UI;
using System.Text.RegularExpressions;

namespace tasks;

public partial class TasksProgram : BaseGame
{
    public Rectangle CardBinRect => cardBinRect;
    public SpriteFont TextFont => textFont;

    Desktop desktop;
    TableFileManager tableFileManager;

    List<UICard> uiCards = new();
    int placeCardIndex;

    SpriteFont textFont;

    //Other stuff
    const int cardStartOffset = 16;
    const int bottomBarHeight = 200;
    const int cardBinWidth = 300;

    Rectangle cardBinRect;
    Rectangle bottomBarRect;

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

        UpdateCards(dt);
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

    void RenderPlaceholderCard(SpriteBatch spriteBatch, int draggedCardHeight)
    {
        Point pos = new Point(cardStartOffset + placeCardIndex * (UICard.rectWidth + 16), cardStartOffset);

        int cardH = draggedCardHeight;
        const int cardW = UICard.rectWidth;
        const int size = 10;
        const int halfSize = size/2;
        const int thick = 2;

        var drawLinePoints = (Vector2 p1, Vector2 p2) => {
            spriteBatch.DrawLine(p1, p2, Color.White, thick);
        };

        var drawLine = (float x1, float y1, float x2, float y2) => {
            spriteBatch.DrawLine(new(x1,y1), new(x2,y2), Color.White, thick);
        };

        var drawHorizontalLine = (int y) => 
        {
            int startX = pos.X + halfSize + size;
            int endX = cardW + pos.X;
            int offset = size*2;

            drawLine(pos.X, y, pos.X + halfSize, y);
            drawLine(pos.X + cardW - halfSize, y, pos.X + cardW - thick, y);

            for(int x = startX; (x + size) <= endX; x += offset)
            {
                drawLine(x, y, x + size, y);
            }
        };

        var drawVerticalLine = (int x) => 
        {
            int startY = pos.Y + halfSize + size;
            int endY = pos.Y + draggedCardHeight;
            int offset = size*2;

            drawLine(x, pos.Y, x, pos.Y + halfSize);
            drawLine(x, pos.Y + cardH - halfSize, x, pos.Y + cardH - thick);

            for(int y = startY; (y + size) <= endY; y += offset)
            {
                drawLine(x, y, x, y + size);
            }
        };

        drawHorizontalLine(pos.Y);
        drawHorizontalLine(pos.Y + draggedCardHeight - thick);
        drawVerticalLine(pos.X);
        drawVerticalLine(pos.X + UICard.rectWidth - thick);
        
        //Rectangle placeRect = new(pos, new(UICard.rectWidth, draggedCardHeight));
        //spriteBatch.DrawRectangle(placeRect, Color.White, 2);
    }

    bool Save(string filepath)
    {
        return tableFileManager.SaveFile(uiCards.ToArray(), tablesPath + filepath);
    }

    bool Load(string filepath)
    {
        Card[]? cards = tableFileManager.LoadFile(tablesPath + filepath);

        if(cards == null)
            return false;

        uiCards = cards.Select(card => new UICard(this, card)).ToList();

        return true;
    }

    protected override void Render(SpriteBatch spriteBatch)
    {
        Graphics.GraphicsDevice.Clear(clearColor);
        
        spriteBatch.Begin();
        {
            //Drawing bottom bar
            spriteBatch.FillRectangle(bottomBarRect, clearColor.DarkenBy(20));
            spriteBatch.FillRectangle(cardBinRect, clearColor.DarkenBy(40));

            //Drawing cards
            foreach(UICard card in uiCards)
            {
                if(card.ElementState == ElementState.BeingDragged)
                    continue;

                card.Draw(spriteBatch);
            }   

            UICard? draggedCard = uiCards.Find(card => card.ElementState == ElementState.BeingDragged);

            if(draggedCard != null) 
            {   
                RenderPlaceholderCard(spriteBatch, draggedCard.Rectangle.Height); 
                draggedCard.Draw(spriteBatch);
            }

            UITaskBox? draggedTask = uiCards.Find(card => card.DragTask != null)?.DragTask;

            if(draggedTask != null)
                draggedTask.Draw(spriteBatch);
        }
        spriteBatch.End();

        desktop.Render();
    }

    protected override void LoadAssets()
    {
        Myra.MyraEnvironment.Game = this;
        desktop = new();
        tableFileManager = new();
        textFont = Assets.GetDefault<SpriteFont>();

        listOfColors = new() {
            Color.Red,
            Color.Yellow,
            Color.Green,
            Color.Blue,
            Color.Purple,
            Color.Cyan,
        };

        currentListOfColors = listOfColors.ToList();

        //Bottom bar rects
        bottomBarRect = new Rectangle(0, Screen.Y - bottomBarHeight, Screen.X, bottomBarHeight);
        cardBinRect = bottomBarRect with { Width = cardBinWidth, X = bottomBarRect.Right - cardBinWidth };
        
        CreateUI();
    }

    protected override void Init()
    {
        Window.Title = "Tasks";
    }

    void GenerateRandomCards() 
    {
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
    }
}

class Program {
    static void Main() {
        using(TasksProgram program = new TasksProgram())
            program.Run();
    }
}