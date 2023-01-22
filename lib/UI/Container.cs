using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib;

class Container : UIElement
{
    private List<UIElement> elements = new();
    public int ElementOffset { get; set; }

    public Container(UI ui, Rectangle box, int elementOffset) : base(ui)
    {
        rect = box;
        ElementOffset = elementOffset;
    }
    
    public void Add(UIElement element)
    {
        elements.Add(element);
        Rearrange();
    }
    public void Remove(UIElement element)
    {
        elements.Remove(element);
        Rearrange();
    }

    private void Rearrange()
    {
        UIElement? previousElement = null;
        
        foreach (UIElement element in elements)
        {
            element.rect.X = (previousElement?.rect.Right + ElementOffset ?? rect.X);
            element.rect.Y = rect.Y;
            previousElement = element;
        }
    }

    public override void _Draw(SpriteBatch spriteBatch)
    {
        throw new NotImplementedException();
    }

    public override void Activate() {}
}