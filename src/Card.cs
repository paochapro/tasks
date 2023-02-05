namespace tasks;

public readonly record struct Card(string Title, Color BannerColor, IEnumerable<KeyValuePair<string,bool>> Tasks);