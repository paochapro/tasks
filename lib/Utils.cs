using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Lib;

internal static class Utils
{
    public static void print(params object[] args)
    {
        foreach (object var in args)
            Console.Write(var + " ");
        Console.WriteLine();
    }
    public static int clamp(int value, int min, int max)
    {
        if (value > max) value = max;
        if (value < min) value = min;
        return value;
    }
    public static float clamp(float value, float min, float max)
    {
        if (value > max) value = max;
        if (value < min) value = min;
        return value;
    }
    public static string EndingAbcense(string str, string end) => str + (str.EndsWith(end) ? "" : end);
    
    //Extensions
    public static string ReadUntil(this StreamReader reader, char value, bool skipLast = false)
    {
        string result = "";
        int peek = reader.Peek();

        while (Convert.ToChar(peek) != value && peek != -1)
        {
            result += Convert.ToChar(reader.Read());
            peek = reader.Peek();
        }

        if (skipLast) reader.Read();

        return result;
    }

    public static T[][] ToJaggedArray<T>(this T[,] array)
    {
        T[][] jaggedArray = new T[array.GetLength(0)][];

        for(int row = 0; row < array.GetLength(0); ++row)
        {
            jaggedArray[row] = new T[array.GetLength(1)];

            for(int column = 0; column < array.GetLength(1); ++column)
                jaggedArray[row][column] = array[row,column];
        }

        return jaggedArray;
    }

    public static void Iterate<T>(this IEnumerable<T> enumerable, Action<T> func)
    {
        foreach (T value in enumerable)
            func(value);
    }
    
    public static void Iterate<T>(this IEnumerable<T> enumerable, Func<T, bool> func)
    {
        foreach (T value in enumerable)
        {
            if (!func(value)) return;
        }
    }

    //Math
    public static float center(float x, float x2, float size)   => (x + x2) / 2 - size / 2;
    public static float center(float x, float size)             => x / 2 - size / 2;
    public static int center(int x, int x2, int size)           => (x + x2) / 2 - size / 2;
    public static int center(int x, int size)                   => x / 2 - size / 2;
    public static Point2 center(Point2 p, RectangleF r)         => new Point2(p.X / 2 - r.Width / 2, p.Y / 2 - r.Height / 2); 
    public static Point2 center(Point2 p, Point2 s)             => new Point2(p.X / 2 - s.X / 2, p.Y / 2 - s.Y / 2); 

    public static int percent(double value, double percent) => (int)Math.Round(value / 100 * percent);
    public static double avg(params double[] values) => values.Average();
    public static int Round(double value) => (int)Math.Round(value);
    public static int Round(float value) => (int)Math.Round(value);

    public static Point2 Center(this Point2 p1, Point2 p2) => new Point2( (p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

    public static float lerp(float a, float b, float t) => (1-t) * a + t * b;
    public static float inverseLerp(float a, float b, float v) => (v-a) / (b-a);

    //Text
    public static Vector2 CenteredTextPosInRect(Rectangle rect, SpriteFont font, string text, float measureScale = 1.0f)
    {
        Vector2 rectPos = rect.Location.ToVector2();
        Vector2 measure = font.MeasureString(text) * measureScale;
        float y = rectPos.Y + (rect.Height / 2 - measure.Y / 2);
        float x = rectPos.X + (y - rectPos.Y);
        return new Vector2(x,y);
    }

    public static float GetBoundedTextScale(string text, float maxWidth, SpriteFont font)
    {
        float scale = 1.0f;

        float textWidth = font.MeasureString(text).X;
        float widthSurpassValue = Utils.inverseLerp(0, maxWidth, textWidth);

        if(widthSurpassValue > 1.0f)
            scale = 1.0f / widthSurpassValue;

        return scale;
    }
}

class ReadOnly2DArray<T>
{
    private T[,] array;
    
    public T this[int y, int x] => array[y,x];

    public int Height => array.GetLength(0);
    public int Width => array.GetLength(1);

    public void Iterate(Action<int, int> func)
    {
        for(int y = 0; y < Height; ++y)
        for(int x = 0; x < Width; ++x)
        {
            func(y,x);
        }
    }
    
    public void Iterate(Func<int, int, bool> func)
    {
        for(int y = 0; y < Height; ++y)
        for(int x = 0; x < Width; ++x)
        {
            if (!func(y, x)) 
                return;
        }
    }
    
    public void Iterate(Action<T> func)
    {
        for(int y = 0; y < Height; ++y)
        for(int x = 0; x < Width; ++x)
        {
            func(array[y,x]);
        }
    }
    
    public void Iterate(Func<T, bool> func)
    {
        for(int y = 0; y < Height; ++y)
        for(int x = 0; x < Width; ++x)
        {
            if (!func(array[y, x]))
                return;
        }
    }
    
    public ReadOnly2DArray(T[,] array)
    {
        this.array = array;
    }
}

static class Directions
{
    public static Vector2 Up            => -Vector2.UnitY;
    public static Vector2 Down          => Vector2.UnitY;
    public static Vector2 Left          => -Vector2.UnitX;
    public static Vector2 Right         => Vector2.UnitX;
    public static Vector2 UpLeft        => Up + Left;
    public static Vector2 UpRight       => Up + Right;
    public static Vector2 DownLeft      => Down + Left;
    public static Vector2 DownRight     => Down + Right;
}

static class ColorExtensions
{
    public static Color LightenBy(this Color color, int value)
    {
        return color.AddColor(new Color(value, value, value));
    }

    public static Color DarkenBy(this Color color, int value)
    {
        return color.SubtractColor(new Color(value, value, value));
    }

    public static Color AddColor(this Color color, Color other)
    {
        Vector3 vec1 = new(color.R, color.G, color.B);
        Vector3 vec2 = new(other.R, other.G, other.B);
        return ColorAddition(vec1, vec2);
    }

    public static Color SubtractColor(this Color color, Color other)
    {
        Vector3 vec1 = new(color.R, color.G, color.B);
        Vector3 vec2 = new(other.R, other.G, other.B);
        return ColorAddition(vec1, -vec2);
    }

    private static Color ColorAddition(Vector3 clr1, Vector3 clr2)
    {
        Color resultClr = Color.White;
        resultClr.R = (byte)Utils.clamp(clr1.X + clr2.X, 0, 255);
        resultClr.G = (byte)Utils.clamp(clr1.Y + clr2.Y, 0, 255);
        resultClr.B = (byte)Utils.clamp(clr1.Z + clr2.Z, 0, 255);
        return resultClr;
    }
}

static class TextureExtensions
{
    private static Texture2D ColorAddition(this Texture2D texture, Vector3 color)
    {
        int w = texture.Width;
        int h = texture.Height;
        Texture2D result = new Texture2D(texture.GraphicsDevice, w, h);

        Color[] colorData = new Color[w * h];
        texture.GetData(colorData);

        for(int i = 0; i < colorData.Length; ++i)
        {
            colorData[i].R = (byte)Utils.clamp(colorData[i].R + color.X, 0, 255); 
            colorData[i].G = (byte)Utils.clamp(colorData[i].G + color.Y, 0, 255);
            colorData[i].B = (byte)Utils.clamp(colorData[i].B + color.Z, 0, 255);
        }

        result.SetData(colorData);

        return result;
    }

    public static Texture2D AddColor(this Texture2D texture, Color color)
    {
        return texture.ColorAddition(color.ToVector3());
    }

    public static Texture2D SubtractColor(this Texture2D texture, Color color)
    {
        return texture.ColorAddition(-color.ToVector3());
    }

    public static Texture2D LightenBy(this Texture2D texture, byte value)
    {
        return texture.AddColor(new Color(value,value,value));
    }

    public static Texture2D DarkenBy(this Texture2D texture, byte value)
    {
        return texture.SubtractColor(new Color(value,value,value));
    }
}

static class IEnumerableExtensions
{
    public static IEnumerable<T> Swap<T>(this IEnumerable<T> enumerable, T obj1, T obj2) where T: notnull
    {
        IEnumerable<T> result = Enumerable.Empty<T>();
        result = enumerable.Select(x => x.Equals(obj1) ? obj2 : x.Equals(obj2) ? obj1 : x);
        return result;
    }
}