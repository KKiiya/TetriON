using MonoGameGum.GueDeriving;

namespace TetriON.Client.UI;

public class UISpriteHandler {

    private readonly Dictionary<string, SpriteRuntime> _sprites = [];
    private readonly Dictionary<string, DateTime> _spriteStartTimes = [];
    private readonly Dictionary<string, float> _spriteMaxSeconds = [];

    public void LoopSprite(SpriteRuntime sprite, int loops) {
        var animationChain = sprite.AnimationChains;
        if (animationChain == null || animationChain.Count == 0) return;

        sprite.Animate = true;
        _sprites[sprite.Name] = sprite;
        _spriteStartTimes[sprite.Name] = DateTime.Now;

        float totalLength = 0;
        foreach (var frame in animationChain[0]) totalLength += frame.FrameLength;

        _spriteMaxSeconds[sprite.Name] = totalLength * loops;
    }

    public void Update(float delta) {
        var spritesToRemove = new List<string>();

        foreach (var sprite in _sprites.Values) {
            var name = sprite.Name;
            if (!_spriteStartTimes.ContainsKey(name) || !_spriteMaxSeconds.ContainsKey(name))
                continue;

            var elapsedSeconds = (DateTime.Now - _spriteStartTimes[name]).TotalSeconds;
            var maxSeconds = _spriteMaxSeconds[name];

            if (elapsedSeconds >= maxSeconds) {
                sprite.Animate = false;
                spritesToRemove.Add(name);
            }
        }

        // Remove completed sprites
        foreach (var name in spritesToRemove) {
            _sprites.Remove(name);
            _spriteStartTimes.Remove(name);
            _spriteMaxSeconds.Remove(name);
        }
    }
}
