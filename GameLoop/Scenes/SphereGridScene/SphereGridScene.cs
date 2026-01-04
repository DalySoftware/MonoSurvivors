using Autofac;
using GameLoop.Input;
using GameLoop.Scenes.SphereGridScene.UI;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Levelling.SphereGrid.UI;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridScene(
    SpriteBatch spriteBatch,
    SphereGridUi sphereGridUi,
    SphereGridInputManager inputManager,
    InputGate inputGate)
    : IScene
{
    public void Update(GameTime gameTime)
    {
        if (inputGate.ShouldProcessInput())
            inputManager.Update(gameTime);
        sphereGridUi.Update(gameTime);
    }

    public void Draw(GameTime gameTime) => sphereGridUi.Draw(spriteBatch);
    public void Dispose() { }

    public static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<SphereGridPositioner>()
            .WithParameter((pi, _) => pi.Name == "root", (_, ctx) => ctx.Resolve<SphereGrid>().Root)
            .SingleInstance();
        builder.Register<NodePositionMap>(ctx => ctx.Resolve<SphereGridPositioner>().NodePositions())
            .SingleInstance();
        builder.RegisterType<ChaseCamera>()
            .WithParameter((pi, _) => pi.Name == "decayRate", (_, _) => 0.005f)
            .WithParameter((pi, _) => pi.Name == "target", (_, ctx) =>
            {
                var root = ctx.Resolve<SphereGrid>().Root;
                var positions = ctx.Resolve<NodePositionMap>().Positions;
                return new NodePositionWrapper(root, positions);
            })
            .As<ISphereGridCamera>()
            .SingleInstance();

        builder.RegisterType<FogOfWarMask>()
            .WithParameter(new NamedParameter("baseVisionRadius", (int)SphereGridPositioner.HexRadius * 1.5f));

        builder.RegisterType<SphereGridContent>();
        builder.RegisterType<SphereGridInputManager>().SingleInstance();
        builder.RegisterType<SphereGridUi>().SingleInstance();

        // Register the scene itself
        builder.RegisterType<SphereGridScene>();
    }
}