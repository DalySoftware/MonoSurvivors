namespace GameLoop.Rendering;

public interface IViewportSync
{
    void ForceRefresh();
    void Update();
}