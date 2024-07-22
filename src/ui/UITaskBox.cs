using static Lib.Utils;

namespace tasks;

public class UITaskBox : UIElement
{
    public static readonly Color hoverColorAddition = new Color(10, 10, 10);
    public static readonly Color defaultBodyColor = new(60, 60, 60);
    public const int checkboxSize = 24;
    public const int taskHeight = 32;
    public const int taskMargin = 4;
    public const int taskWidth = UICard.rectWidth - taskMargin*2;
    public const int checkboxMargin = (taskHeight-checkboxSize) / 2;
    public const int checkMargin = 4;
    public const int tasksOffset = 4;

    public bool IsQueuedForRemoval => isQueuedForRemoval;
    public bool IsChecked => isChecked;
    public string Description => description;
    public ElementState ElementState => elementState;
    public UICard Owner { get => owner; set => owner = value; }

    UICard owner;
    Rectangle rectangle;
    SpriteFont font;
    bool isChecked;
    bool hover;
    string description;
    bool isQueuedForRemoval;
    UITextboxCreator tbCreator;
    ElementState elementState;

    UITextbox? _renameTextbox;
    UITextbox? renameTextbox {
        get => _renameTextbox;
        set {
            _renameTextbox = value;
            elementState = _renameTextbox == null ? ElementState.Default : ElementState.BeingRenamed;
        }
    }
    
    readonly Color tbBodyColor;
    readonly Color tbTextColor;
    readonly int textMarginX;

    //Description
    public UITaskBox(TasksProgram program, string description, bool isChecked, UICard owner)
    {
        this.description = description;
        this.isChecked = isChecked;
        this.owner = owner;
        this.font = program.TextFont;
        rectangle = new Rectangle(0, 0, taskWidth, taskHeight);

        //Textbox creator settings
        Rectangle absoluteRect = rectangle with { Location = Point.Zero };
        int textY = (int)Utils.CenteredTextPosInRect(absoluteRect, font, "A").Y;
        textMarginX = textY;
        int tbWidth = taskWidth - (checkboxSize + checkboxMargin * 2) - textMarginX;

        tbBodyColor = defaultBodyColor.DarkenBy(40);
        tbTextColor = Color.White;
        tbCreator = new(program.Window, tbWidth, 9999, tbBodyColor, tbTextColor, font);
    }

    public void UpdatePosition(Point position)
    {
        rectangle.Location = position;
    }
    
    public void Update(float dt)
    {
        if(elementState == ElementState.BeingDragged)
        {
            Vector2 mousePos = Input.Mouse.Position.ToVector2();
            Vector2 taskSize = new(taskWidth, taskHeight);
            Vector2 dragTaskPos = mousePos - taskSize / 2;
            UpdatePosition(dragTaskPos.ToPoint());

            if(Input.RBReleased())
                elementState = ElementState.Default;

            hover = false;
            return;
        }

        if(renameTextbox != null)
            renameTextbox.Update(dt);

        UpdateActions();
    }

    public void UpdateActions()
    {
        if(renameTextbox != null)
        {
            if(Input.IsKeyDown(Keys.Enter))
            {
                description = renameTextbox.TextboxText;
                renameTextbox = null;
            }

            if (Input.IsKeyDown(Keys.Escape))
                renameTextbox = null;

            hover = false;
            return;
        }

        hover = rectangle.Contains(Input.Mouse.Position);
        if(!hover) return;

        if(Input.MBPressed())
        {
            isQueuedForRemoval = true;
            return;
        }

        if(Input.RBPressed())
        {
            elementState = ElementState.BeingDragged;
            return;
        }

        if(Input.LBPressed())
            isChecked = !isChecked;

        if(Input.KeyPressed(Keys.F2))
        {
            Point pos = Utils.CenteredTextPosInRect(rectangle, font, description).ToPoint();
            renameTextbox = tbCreator.CreateUITextbox(pos, description);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Color bodyColor)
    {
        Point checkboxPos = new(rectangle.Right - checkboxSize - checkboxMargin, rectangle.Bottom - checkboxSize - checkboxMargin);
        Rectangle checkbox = new Rectangle(checkboxPos, new Point(checkboxSize,checkboxSize));

        Color checkBoxColor = bodyColor.DarkenBy(10);

        if (hover) {
            bodyColor = new Color(bodyColor.ToVector3() + hoverColorAddition.ToVector3());
            checkBoxColor = new Color(checkBoxColor.ToVector3() + hoverColorAddition.ToVector3());
        }

        spriteBatch.FillRectangle(rectangle, bodyColor);
        spriteBatch.FillRectangle(checkbox, checkBoxColor);

        if (isChecked)
        {
            Rectangle check = checkbox;
            check.Location += new Point(checkMargin);
            check.Size -= new Point(checkMargin * 2);
            spriteBatch.FillRectangle(check, owner.BannerColor);
        }

        //Description
        if(renameTextbox != null)
            renameTextbox.Draw(spriteBatch);
        else
            DrawDescription(spriteBatch);
    }

    public void Draw(SpriteBatch spriteBatch) => Draw(spriteBatch, defaultBodyColor);


    void DrawDescription(SpriteBatch spriteBatch)
    {
        float descriptionMaxWidth = rectangle.Width - (checkboxSize + checkboxMargin * 2) - textMarginX;
        float scale = GetBoundedTextScale(description, descriptionMaxWidth, font);

        float x = textMarginX;
        float y = CenteredTextPosInRect(rectangle, font, description, scale).Y;
        Vector2 titlePos = new Vector2(x + rectangle.X, y);
        
        spriteBatch.DrawString(font, description, titlePos, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
    }
}