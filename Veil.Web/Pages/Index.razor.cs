using System;
using System.Threading.Tasks;
using GameLoop;
using Microsoft.JSInterop;
using Microsoft.Xna.Framework;
using Veil.Web.PlatformServices;

namespace Veil.Web.Pages;

public partial class Index
{
    private Game? _game;
    private bool _crashed;

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender)
            JsRuntime.InvokeAsync<object>("initRenderJS", DotNetObjectReference.Create(this));
    }

    [JSInvokable]
    public async Task TickDotNet()
    {
        if (_crashed)
            return;

        try
        {
            // init game
            if (_game == null)
            {
                _game = new CoreGame(b => ServiceConfigurator.Configure(b, JsRuntime));
                _game.Run();
            }

            // run gameloop
            _game.Tick();
        }
        catch (Exception ex)
        {
            _crashed = true;

            var details = ex.ToString();

            // Shows full exception + inner exception + managed stack in DevTools console
            await JsRuntime.InvokeVoidAsync("console.error", details);

            // Also print to stdout/stderr stream
            Console.Error.WriteLine(details);

            // Stop the loop by rethrowing (you'll still see ManagedError, but now you have details above)
            throw;
        }
    }
}