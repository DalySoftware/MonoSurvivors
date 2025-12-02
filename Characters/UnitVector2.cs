namespace Characters;

public readonly record struct UnitVector2
{
    public float X { get; }
    public float Y { get; }
    
    /// <summary>
    /// Keeps the direction of the input but ignores scale, normalizing to 1
    /// </summary>
    public UnitVector2(float x, float y)
    {
        var v = new Vector2(x, y);
        v.Normalize();
        X = float.IsNaN(v.X) ? 0f : v.X;
        Y = float.IsNaN(v.Y) ? 0f : v.Y;
    }
    
    public static explicit operator Vector2(UnitVector2 uv) => new(uv.X, uv.Y);
}