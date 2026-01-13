using System;

namespace TetriON.Client.Animations;

/// <summary>
/// Central resolver for easing function calculations.
/// Converts linear progress (0-1) to eased progress using various easing functions.
/// </summary>
public static class EasingResolver {

    /// <summary>
    /// Apply easing function to normalized progress
    /// </summary>
    /// <param name="type">Easing function type</param>
    /// <param name="t">Linear progress (0.0 to 1.0)</param>
    /// <returns>Eased progress (0.0 to 1.0)</returns>
    public static float Ease(EasingType type, float t) {
        t = Math.Clamp(t, 0.0f, 1.0f);

        return type switch {
            EasingType.Linear => t,

            // Quadratic
            EasingType.EaseInQuad => EaseInQuad(t),
            EasingType.EaseOutQuad => EaseOutQuad(t),
            EasingType.EaseInOutQuad => EaseInOutQuad(t),

            // Cubic
            EasingType.EaseInCubic => EaseInCubic(t),
            EasingType.EaseOutCubic => EaseOutCubic(t),
            EasingType.EaseInOutCubic => EaseInOutCubic(t),

            // Quartic
            EasingType.EaseInQuart => EaseInQuart(t),
            EasingType.EaseOutQuart => EaseOutQuart(t),
            EasingType.EaseInOutQuart => EaseInOutQuart(t),

            // Quintic
            EasingType.EaseInQuint => EaseInQuint(t),
            EasingType.EaseOutQuint => EaseOutQuint(t),
            EasingType.EaseInOutQuint => EaseInOutQuint(t),

            // Sine
            EasingType.EaseInSine => EaseInSine(t),
            EasingType.EaseOutSine => EaseOutSine(t),
            EasingType.EaseInOutSine => EaseInOutSine(t),

            // Exponential
            EasingType.EaseInExpo => EaseInExpo(t),
            EasingType.EaseOutExpo => EaseOutExpo(t),
            EasingType.EaseInOutExpo => EaseInOutExpo(t),

            // Circular
            EasingType.EaseInCirc => EaseInCirc(t),
            EasingType.EaseOutCirc => EaseOutCirc(t),
            EasingType.EaseInOutCirc => EaseInOutCirc(t),

            // Elastic
            EasingType.EaseInElastic => EaseInElastic(t),
            EasingType.EaseOutElastic => EaseOutElastic(t),
            EasingType.EaseInOutElastic => EaseInOutElastic(t),

            // Back
            EasingType.EaseInBack => EaseInBack(t),
            EasingType.EaseOutBack => EaseOutBack(t),
            EasingType.EaseInOutBack => EaseInOutBack(t),

            // Bounce
            EasingType.EaseInBounce => EaseInBounce(t),
            EasingType.EaseOutBounce => EaseOutBounce(t),
            EasingType.EaseInOutBounce => EaseInOutBounce(t),

            _ => t
        };
    }

    #region Quadratic
    private static float EaseInQuad(float t) => t * t;
    private static float EaseOutQuad(float t) => t * (2 - t);
    private static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    #endregion

    #region Cubic
    private static float EaseInCubic(float t) => t * t * t;
    private static float EaseOutCubic(float t) => (--t) * t * t + 1;
    private static float EaseInOutCubic(float t) => t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
    #endregion

    #region Quartic
    private static float EaseInQuart(float t) => t * t * t * t;
    private static float EaseOutQuart(float t) => 1 - (--t) * t * t * t;
    private static float EaseInOutQuart(float t) => t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
    #endregion

    #region Quintic
    private static float EaseInQuint(float t) => t * t * t * t * t;
    private static float EaseOutQuint(float t) => 1 + (--t) * t * t * t * t;
    private static float EaseInOutQuint(float t) => t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
    #endregion

    #region Sine
    private static float EaseInSine(float t) => 1 - MathF.Cos(t * MathF.PI / 2);
    private static float EaseOutSine(float t) => MathF.Sin(t * MathF.PI / 2);
    private static float EaseInOutSine(float t) => -(MathF.Cos(MathF.PI * t) - 1) / 2;
    #endregion

    #region Exponential
    private static float EaseInExpo(float t) => t == 0 ? 0 : MathF.Pow(2, 10 * (t - 1));
    private static float EaseOutExpo(float t) => t == 1 ? 1 : 1 - MathF.Pow(2, -10 * t);
    private static float EaseInOutExpo(float t) {
        if (t == 0) return 0;
        if (t == 1) return 1;
        return t < 0.5f ? MathF.Pow(2, 20 * t - 10) / 2 : (2 - MathF.Pow(2, -20 * t + 10)) / 2;
    }
    #endregion

    #region Circular
    private static float EaseInCirc(float t) => 1 - MathF.Sqrt(1 - t * t);
    private static float EaseOutCirc(float t) => MathF.Sqrt(1 - (--t) * t);
    private static float EaseInOutCirc(float t) => t < 0.5f
        ? (1 - MathF.Sqrt(1 - 4 * t * t)) / 2
        : (MathF.Sqrt(1 - (-2 * t + 2) * (-2 * t + 2)) + 1) / 2;
    #endregion

    #region Elastic
    private static float EaseInElastic(float t) {
        const float c4 = 2 * MathF.PI / 3;
        return t == 0 ? 0 : t == 1 ? 1 : -MathF.Pow(2, 10 * t - 10) * MathF.Sin((t * 10 - 10.75f) * c4);
    }

    private static float EaseOutElastic(float t) {
        const float c4 = 2 * MathF.PI / 3;
        return t == 0 ? 0 : t == 1 ? 1 : MathF.Pow(2, -10 * t) * MathF.Sin((t * 10 - 0.75f) * c4) + 1;
    }

    private static float EaseInOutElastic(float t) {
        const float c5 = 2 * MathF.PI / 4.5f;
        return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f
            ? -(MathF.Pow(2, 20 * t - 10) * MathF.Sin((20 * t - 11.125f) * c5)) / 2
            : MathF.Pow(2, -20 * t + 10) * MathF.Sin((20 * t - 11.125f) * c5) / 2 + 1;
    }
    #endregion

    #region Back
    private static float EaseInBack(float t) {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return c3 * t * t * t - c1 * t * t;
    }

    private static float EaseOutBack(float t) {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(t - 1, 3) + c1 * MathF.Pow(t - 1, 2);
    }

    private static float EaseInOutBack(float t) {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;
        return t < 0.5f
            ? MathF.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2) / 2
            : (MathF.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }
    #endregion

    #region Bounce
    private static float EaseOutBounce(float t) {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1 / d1) return n1 * t * t;
        else if (t < 2 / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
        else if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        else return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }

    private static float EaseInBounce(float t) => 1 - EaseOutBounce(1 - t);

    private static float EaseInOutBounce(float t) => t < 0.5f
        ? (1 - EaseOutBounce(1 - 2 * t)) / 2
        : (1 + EaseOutBounce(2 * t - 1)) / 2;
    #endregion
}
