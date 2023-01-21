using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using static Lib.Utils;

namespace Lib;

class Assets
{
    private ContentManager content;
    private Texture2D errorTexture;

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

    public T? Load<T>(string asset) where T: class
    {
        ContentAvaliable();

        if (!AssetExists(asset))
        {
            print("\""+asset+"\" wasn't found in MonoGame:Load");

            T assetObj = default(T);
            Type assetType = typeof(T);

            if(defaultAssets.ContainsKey(assetType))
                assetObj = defaultAssets[assetType] as T;
            
            return assetObj;
        }
        return content.Load<T>(asset);
    }
    public Texture2D LoadTexture(string asset)
    {
        ContentAvaliable();

        if (!AssetExists(asset))
        {
            print("\""+asset+"\" texture wasn't found in MonoGame:Load");
            return errorTexture;
        }

        return content.Load<Texture2D>(asset);
    }
    public T GetDefault<T>() where T: class 
    {
        Type assetType = typeof(T);
        if(defaultAssets.ContainsKey(assetType))
            return defaultAssets[assetType] as T;
        else
            throw new Exception("No default asset for " + assetType.Name + " asset type (Asset:GetDefault)");
    } 
}
