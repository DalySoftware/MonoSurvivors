using System;
using System.Collections.Generic;
using Characters;
using Characters.Enemy;
using ContentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay;

public class CharacterManager(ContentManager content)
{
    private readonly List<Character> _characters = [];
    private readonly Texture2D _enemyTexture = content.Load<Texture2D>(Paths.Images.Enemy);
    private readonly Texture2D _playerTexture = content.Load<Texture2D>(Paths.Images.Player);

    public void Add(Func<Character> characterFactory) => _characters.Add(characterFactory());

    public void Add(Character character) => _characters.Add(character);

    public void Update(GameTime gameTime) => _characters.ForEach(c => c.UpdatePosition(gameTime));

    public void Draw(SpriteBatch spriteBatch) => _characters.ForEach(c => Draw(spriteBatch, c));

    private void Draw(SpriteBatch spriteBatch, Character character)
    {
        var texture = Texture(character);
        spriteBatch.Draw(texture, character.Position, origin: texture.Centre);
    }

    private Texture2D Texture(Character character) => character switch
    {
        BasicEnemy => _enemyTexture,
        PlayerCharacter => _playerTexture,
        _ => throw new Exception("Unmapped character texture")
    };
}