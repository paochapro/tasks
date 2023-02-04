using static Lib.Utils;

namespace tasks;

public class UITaskBox : UIElement
{
    public static readonly Color hoverColorAddition = new Color(10, 10, 10);
    public static readonly Color bodyColor = new(70, 70, 70);
    public static readonly Color checkboxColor = new(60, 60, 60);
    public const int checkboxSize = 24;
    public const int taskHeight = 32;
    public const int taskMargin = 4;
    public const int taskWidth = UICard.rectWidth - taskMargin*2;
    public const int checkboxMargin = (taskHeight-checkboxSize) / 2;
    public const int checkMargin = 4;
    public const int tasksOffset = 4;

    public bool IsQueuedForRemoval => isQueuedForRemoval;
    public bool IsBeingDragged => isBeingDragged;
    public bool IsBeingRenamed => renameTextbox != null;
    public ElementState ElementState => elementState;
    public UICard Owner { get => owner; set => owner = value; }
    

    UICard owner;
    Rectangle rectangle;
    SpriteFont font;
    bool isChecked;
    bool hover;
    string description;
    bool isQueuedForRemoval;
    bool isBeingDragged;
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
        int textMarginX = textY;
        int tbWidth = taskWidth - (checkboxSize + checkboxMargin * 2) - textMarginX*2;

        tbBodyColor = bodyColor.DarkenBy(40);
        tbTextColor = Color.White;
        tbCreator = new(program.Window, tbWidth, 9999, tbBodyColor, tbTextColor, font);
    }

    public void UpdatePosition(Point position)
    {
        rectangle.Location = position;
    }
    
    public void Update(float dt)
    {
        if(isBeingDragged)
        {
            Vector2 mousePos = Input.Mouse.Position.ToVector2();
            Vector2 taskSize = new(taskWidth, taskHeight);
            Vector2 dragTaskPos = mousePos - taskSize / 2;
            UpdatePosition(dragTaskPos.ToPoint());

            isBeingDragged = !Input.RBReleased();
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
            isBeingDragged = true;
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

    public void Draw(SpriteBatch spriteBatch)
    {
        Point checkboxPos = new(rectangle.Right - checkboxSize - checkboxMargin, rectangle.Bottom - checkboxSize - checkboxMargin);
        Rectangle checkbox = new Rectangle(checkboxPos, new Point(checkboxSize,checkboxSize));

        Color rectColor = new Color();
        Color checkBoxColor = new Color();

        if (hover) {
            rectColor = bodyColor;
            checkBoxColor = checkboxColor;
        }
        else {
            rectColor = new Color(bodyColor.ToVector3() - hoverColorAddition.ToVector3());
            checkBoxColor = new Color(checkboxColor.ToVector3() - hoverColorAddition.ToVector3());
        }

        spriteBatch.FillRectangle(rectangle, rectColor);
        spriteBatch.FillRectangle(checkbox, checkBoxColor);

        if (isChecked)
        {
            Rectangle check = checkbox;
            check.Location += new Point(checkMargin);
            check.Size -= new Point(checkMargin * 2);
            spriteBatch.FillRectangle(check, owner.Card.BannerColor);
        }

        //Description
        if(renameTextbox != null)
            renameTextbox.Draw(spriteBatch);
        else
        {
            Vector2 textPos = Utils.CenteredTextPosInRect(rectangle, font, description);
            spriteBatch.DrawString(font, description, textPos, Color.White);
        }

    }
}