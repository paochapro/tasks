using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib.Gui;

abstract class Container : LibGuiElement
{
    List<LibGuiElement> elements;
    int elementsOffset;

    public int ElementOffset { get => elementsOffset; set => elementsOffset = value; }
    public IEnumerable<LibGuiElement> Elements => elements;

    public Container(LibGuiManager ui, Rectangle box) : base(ui)
    {
        rect = box;
        elements = new();
    }
    
    public void Add(LibGuiElement element)
    {
        elements.Add(element);
        Rearrange();
    }

    public void Remove(LibGuiElement element)
    {
        elements.Remove(element);
        Rearrange();
    }

    protected abstract void Rearrange();
}