using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;

namespace TetriON.Client.Rendering;

public class RendererManager(IController controller) : IRendererManager {

    private readonly List<IRenderer> _renderers = [];
    private readonly List<IRenderer> _specialRenderers = [];
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

    public void DrawSpecialRenderers() {
        foreach (var renderer in _specialRenderers) {
            if (renderer.IsActive) renderer.Draw();
        }
    }

    public List<IRenderer> GetActiveRenderers() {
        return [.. _renderers.Where(r => r.IsActive)];
    }

    public List<Action> GetPostDrawActions() {
        return _postDrawActions;
    }

    public List<IRenderer> GetSpecialRenderers() {
        return [.. _specialRenderers.Where(r => r.IsActive)];
    }

    public void RegisterRenderers(IRenderer[] renderers, bool reorder = true) {
        _renderers.AddRange(renderers);
        if (reorder) _renderers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
    }

    public void RegisterSpecialRenderers(IRenderer[] renderer, bool reorder = true) {
        _specialRenderers.AddRange(renderer);
        if (reorder) _specialRenderers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
    }

    public void UnregisterRenderers(IRenderer[] renderers, bool reorder = true) {
        foreach (var renderer in renderers) _renderers.Remove(renderer);
        if (reorder) _renderers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
    }

    public void UnregisterSpecialRenderers(IRenderer[] renderers, bool reorder = true) {
        foreach (var renderer in renderers) _specialRenderers.Remove(renderer);
        if (reorder) _specialRenderers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
    }

    public void UpdateRenderers(GameTime gameTime) {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var renderer in _renderers) {
            if (renderer.IsActive) renderer.Update(deltaTime);
        }
    }
}
