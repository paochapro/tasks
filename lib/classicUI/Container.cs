using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib;

class Container : ClassicUIElement
{
    private List<ClassicUIElement> elements = new();
    public int ElementOffset { get; set; }

    public Container(ClassicUIManager ui, Rectangle box, int elementOffset) : base(ui)
    {
        rect = box;
        ElementOffset = elementOffset;
    }
    
    public void Add(ClassicUIElement element)
    {
        elements.Add(element);
        Rearrange();
    }
    public void Remove(ClassicUIElement element)
    {
        elements.Remove(element);
        Rearrange();
    }

    private void Rearrange()
    {
        ClassicUIElement? previousElement = null;
        
        foreach (ClassicUIElement element in elements)
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