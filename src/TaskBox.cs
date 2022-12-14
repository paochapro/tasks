using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Lib;

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

    private bool IsChecked => isChecked;
    private bool IsHovered => hover;
    private Rectangle Rectangle => rectangle;
    
    private bool isChecked = false;
    private bool hover = false;
    private string taskDescription = "";
    private Rectangle rectangle;
    private UICard owner;

    public UITaskBox(string taskDescription, bool isChecked, UICard owner)
    {
        this.taskDescription = taskDescription;
        this.isChecked = isChecked;
        this.owner = owner;
        rectangle = new Rectangle(0, 0, taskWidth, taskHeight);
    }
    
    public void Update(Point position)
    {
        rectangle.Location = position;

        hover = rectangle.Contains(Input.Mouse.Position);

        if (Input.LBPressed() && hover)
        {
            isChecked = !isChecked;
            owner.Card.Tasks[taskDescription] = isChecked;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Point position)
    {
        rectangle.Location = position;
        
        Point checkboxPos = new(rectangle.Right - checkboxSize - checkboxMargin, rectangle.Bottom - checkboxSize - checkboxMargin);
        Rectangle checkbox = new Rectangle(checkboxPos, new Point(checkboxSize,checkboxSize));

        if (hover) {
            spriteBatch.FillRectangle(rectangle, bodyColor);
            spriteBatch.FillRectangle(checkbox, checkboxColor);
        }
        else {
            spriteBatch.FillRectangle(rectangle, new Color(bodyColor.ToVector3() - hoverColorAddition.ToVector3()));
            spriteBatch.FillRectangle(checkbox, new Color(checkboxColor.ToVector3() - hoverColorAddition.ToVector3()));
        }

        if (isChecked)
        {
            Rectangle check = checkbox;
            check.Location += new Point(checkMargin);
            check.Size -= new Point(checkMargin * 2);
            spriteBatch.FillRectangle(check, owner.Card.BannerColor);
        }
    }
}