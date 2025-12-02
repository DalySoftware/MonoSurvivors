namespace Characters;

public readonly record struct UnitVector2
{
    public float X { get; }
    public float Y { get; }
    
    public UnitVector2(float x, float y)
    {
        var v = new Vector2(x, y);
        v.Normalize();
        X = v.X;
        Y = v.Y;
    }
    
    public static explicit operator Vector2(UnitVector2 uv) => new(uv.X, uv.Y);
}