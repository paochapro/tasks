﻿using static Lib.Utils;

namespace tasks;

public class UITaskBox
{
    //Static
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

    //public bool IsChecked => isChecked;
    //public bool IsHovered => hover;
    //public Rectangle Rectangle => rectangle;
    
    private UICard owner;
    private Rectangle rectangle;
    private SpriteFont textFont;
    private bool isChecked;
    private bool hover;
    private string description;
    private bool isQueuedForRemoval;
    private bool isBeingDragged;

    public bool IsQueuedForRemoval => isQueuedForRemoval;
    public bool IsBeingDragged => isBeingDragged;
    public UICard Owner { get => owner; set => owner = value; }

    //Description
    public UITaskBox(TasksProgram program, string description, bool isChecked, UICard owner)
    {
        this.description = description;
        this.isChecked = isChecked;
        this.owner = owner;
        this.textFont = program.TextFont;
        rectangle = new Rectangle(0, 0, taskWidth, taskHeight);
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
        Vector2 textPos = Utils.CenteredTextPosInRect(rectangle, textFont, description);
        spriteBatch.DrawString(textFont, description, textPos, Color.White);
    }
}