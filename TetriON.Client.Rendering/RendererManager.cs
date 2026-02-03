using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;

namespace TetriON.Client.Rendering;

public class RendererManager(IController controller) : IRendererManager {

    private readonly List<IRenderer> _renderers = [];
    private readonly List<Action> _postDrawActions = [];
    public IController Controller => controller;

    public void DoPostDrawActions() {
        if (_postDrawActions.Count > 0) {
            foreach (var action in _postDrawActions) action();
            _postDrawActions.Clear();
        }
    }

    public void DrawRenderers() {
        foreach (var renderer in _renderers) {
            if (renderer.IsActive) renderer.Draw();
        }
    }

    public List<IRenderer> GetActiveRenderers() {
        return _renderers.Where(r => r.IsActive).ToList();
    }

    public List<Action> GetPostDrawActions() {
        return _postDrawActions;
    }

    public void RegisterRenderers(IRenderer[] renderers, bool reorder = true) {
        _renderers.AddRange(renderers);
        if (reorder) _renderers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
    }

    public void UnregisterRenderers(IRenderer[] renderers, bool reorder = true) {
        foreach (var renderer in renderers) _renderers.Remove(renderer);
        if (reorder) _renderers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
    }

    public void UpdateRenderers(GameTime gameTime) {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var renderer in _renderers) {
            if (renderer.IsActive) renderer.Update(deltaTime);
        }
    }
}
