using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using static Lib.Utils;

namespace Lib;

public class Assets
{
    private ContentManager content;

    private Dictionary<Type, object> defaultAssets;

    public Assets(ContentManager content)
    {
        this.content = content;

        defaultAssets = new() {
            [typeof(Texture2D)] = content.Load<Texture2D>("error"),
            [typeof(SpriteFont)] = content.Load<SpriteFont>("bahnschrift"),
        };
    }

    private void ContentAvaliable()
    {
        if (content == null)
            throw new Exception("content is null in MonoGame");
    }
    private bool AssetExists(string asset)
    {
        ContentAvaliable();
        return File.Exists(content.RootDirectory + @"\" + asset + ".xnb");
    }

    public T Load<T>(string asset) where T: class
    {
        ContentAvaliable();

        if (!AssetExists(asset))
        {
            print("\""+asset+"\" wasn't found in MonoGame:Load");

            Type assetType = typeof(T);

            if(defaultAssets.ContainsKey(assetType))
                return (T)defaultAssets[assetType];
            else
                throw new Exception($"No default asset for {assetType} was found in MonoGame:Load");            
        }
        return content.Load<T>(asset);
    }

    public Texture2D LoadTexture(string asset) => Load<Texture2D>(asset);
    
    public T GetDefault<T>() where T: class 
    {
        Type assetType = typeof(T);
        if(defaultAssets.ContainsKey(assetType))
        {
            T defaultAsset = (T)defaultAssets[assetType];
            return defaultAsset;
        }
        else
            throw new Exception("No default asset for " + assetType.Name + " asset type (Asset:GetDefault)");
    } 
}
