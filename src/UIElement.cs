namespace tasks;

interface TasksUIElement
{
    bool IsBeingDragged { get; }
    bool IsBeingRenamed { get; }

    public void Update(float dt);
}