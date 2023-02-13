namespace tasks;

public enum ElementState { Default, BeingDragged, BeingRenamed }

public interface UIElement
{
    ElementState ElementState { get; }
    void Update(float dt);
}