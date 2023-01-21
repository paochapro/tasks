using MonoGame.Extended;

static class Rnd
{
    public static int GetSeed() => (int)DateTime.Now.Ticks;

    public static int Between(int a, int b, bool inclusive = true)
    {
        int seed = (int)DateTime.Now.Ticks;

        if (a > b)
        {
            (a, b) = (b, a);
        }

        b += inclusive ? 1 : 0;
        return new Random(seed).Next(a, b);
    }
    public static int Int(int min, int max, bool inclusive = true)
    {
        max += inclusive ? 1 : 0;
        return new Random(GetSeed()).Next(min, max);
    }
    public static float Float(float min = 0.0f, float max = 1.0f) => new Random(GetSeed()).NextSingle(min, max);
    
    public static int Range(Range<int> range, bool inclusive = true) => Int(range.Min, range.Max, inclusive);
    public static float Range(Range<float> range) => Float(range.Min, range.Max);

    public static bool Chance(int percent)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(GetSeed()).Next(100) < percent;
    }
    public static int Chance(params int[] chances)
    {
        if (chances.Sum() != 100)
            return -1;

        int seed = (int)DateTime.Now.Ticks;
        int randomNumber = new Random(seed).Next(100) + 1;

        int previousSum = 0;
        int index = 0;
        foreach (int chance in chances)
        {
            if (randomNumber <= previousSum + chance &&
                randomNumber > previousSum)
            {
                return index;
            }

            index++;
            previousSum += chance;
        }

        //Error, impossible
        return -2;
    }
}