namespace Lib.Gui;

class HContainer : Container
{
    public HContainer(LibGuiManager ui) : base(ui, Rectangle.Empty)
    {
        
    }

    protected override void Rearrange()
    {
        LibGuiElement previousElement = Elements.First();
        previousElement.rect.Location = this.rect.Location;

        foreach (LibGuiElement element in Elements.Skip(1))
        {
            element.rect.X = previousElement.rect.Right + ElementOffset;
            element.rect.Y = rect.Y;
            previousElement = element;
        }
    }
}