namespace tasks;

public class Card
{
    public Dictionary<string, bool> Tasks { get; private set; }
    public Color BannerColor { get; set; }
    public string Title { get; set; }

    public Card(string title, Color bannerColor)
    {
        BannerColor = bannerColor;
        Title = title;
        Tasks = new();
    }
}