using Newtonsoft.Json;

using static Lib.Utils;

namespace tasks;

class TableManager
{
    public Card[]? LoadFile(string filepath)
    {
        if(!File.Exists(filepath))
        {
            print($"file \"{filepath}\" doesnt exists! TableManager:LoadFile");
            return null;
        }

        string json = "";

        using(StreamReader reader = new StreamReader(filepath))
            json = reader.ReadToEnd();

        return Load(json);
    }

    public bool SaveFile(UICard[] uiCards, string filepath)
    {
        Card[] savingCards = uiCards.Select(uiCard => uiCard.GeneratedCard).ToArray();
        Save(savingCards, filepath);
        return true;
    }

    void Save(Card[] cards, string filepath)
    {
        string json = JsonConvert.SerializeObject(cards);

        using(FileStream fs = new(filepath, FileMode.OpenOrCreate))
        using(StreamWriter writer = new StreamWriter(fs))
            writer.Write(json);
    }

    Card[]? Load(string json)
    {   
        Card[]? result = JsonConvert.DeserializeObject<Card[]>(json);

        if(result != null)
        {
            if(result is Card[] table)
                return table;
            else
                Console.WriteLine("json isnt a Card[]! TableManager:Load");
        }
        else
            Console.WriteLine("loading json failed! TableManager:Load");

        return null;
    }
}