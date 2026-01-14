using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.UI;
using static TetriON.Client.UI.Composers;

namespace TetriON.Client.Animations;

/// <summary>
/// Base class for UI elements that support smooth resizing, animations and adjustments.
/// Uses normalized values (0.0 to 1.0) to interpolate between original and target states.
/// </summary>
public abstract class Adjustable(ClientController controller) {

    public ClientController Controller { get; } = controller ?? throw new ArgumentNullException(nameof(controller));
    public SpriteBatch spriteBatch => Controller.SpriteBatch;


    #region Original State Properties
    // Store original position and dimensions
    protected Point OriginalPosition { get; set; }
    protected Size OriginalSize { get; set; }
    protected Size OriginalContainerSize { get; set; }

    // Store original visual properties
    protected float OriginalOpacity { get; set; } = 1.0f;
    protected float OriginalRotation { get; set; } = 0.0f;
    protected float OriginalScale { get; set; } = 1.0f;
    #endregion


    #region Target State Properties
    // Store target position and dimensions
    protected Point TargetPosition { get; set; }
    protected Size TargetSize { get; set; }
    protected Size TargetContainerSize { get; set; }

    // Store target visual properties
    protected float TargetOpacity { get; set; } = 1.0f;
    protected float TargetRotation { get; set; } = 0.0f;
    protected float TargetScale { get; set; } = 1.0f;
    #endregion


    #region Current State Properties
    // Current interpolated values
    protected Point CurrentPosition { get; set; }
    protected Size CurrentSize { get; set; }
    protected float CurrentOpacity { get; set; } = 1.0f;
    protected float CurrentRotation { get; set; } = 0.0f;
    protected float CurrentScale { get; set; } = 1.0f;
    #endregion

    #region Configuration Properties
    // Adjustment behavior settings
    protected AnchorPreset Anchor { get; set; } = AnchorPreset.TopLeft;
    protected ScaleMode ScalingMode { get; set; } = ScaleMode.Proportional;
    protected bool IsAdjusting { get; set; } = false;
    #endregion


    #region Initialization Methods
    /// <summary>
    /// Initialize with original position and size
    /// </summary>
    public virtual void Initialize(Point position, Size size, Size containerSize) {
        OriginalPosition = position;
        OriginalSize = size;
        OriginalContainerSize = containerSize;

        CurrentPosition = position;
        CurrentSize = size;

        TargetPosition = position;
        TargetSize = size;
        TargetContainerSize = containerSize;
    }

    /// <summary>
    /// Set the target state for adjustment
    /// </summary>
    public virtual void SetTarget(Point targetPosition, Size targetSize, Size targetContainerSize) {
        TargetPosition = targetPosition;
        TargetSize = targetSize;
        TargetContainerSize = targetContainerSize;
    }

    /// <summary>
    /// Set original state from current values
    /// </summary>
    public virtual void StoreOriginalState() {
        OriginalPosition = CurrentPosition;
        OriginalSize = CurrentSize;
        OriginalOpacity = CurrentOpacity;
        OriginalRotation = CurrentRotation;
        OriginalScale = CurrentScale;
    }
    #endregion


    #region Adjustment Methods
    /// <summary>
    /// Adjust position based on normalized progress (0.0 to 1.0)
    /// </summary>
    /// <param name="progress">Normalized value from 0.0 (original) to 1.0 (target)</param>
    public virtual void AdjustPosition(float progress) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);

        CurrentPosition = new Point(
            (int)Lerp(OriginalPosition.X, TargetPosition.X, progress),
            (int)Lerp(OriginalPosition.Y, TargetPosition.Y, progress)
        );
    }

    /// <summary>
    /// Adjust size based on normalized progress (0.0 to 1.0)
    /// </summary>
    /// <param name="progress">Normalized value from 0.0 (original) to 1.0 (target)</param>
    public virtual void AdjustSize(float progress) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);

        CurrentSize = new Size(
            (int)Lerp(OriginalSize.Width, TargetSize.Width, progress),
            (int)Lerp(OriginalSize.Height, TargetSize.Height, progress)
        );
    }

    /// <summary>
    /// Adjust opacity based on normalized progress (0.0 to 1.0)
    /// </summary>
    /// <param name="progress">Normalized value from 0.0 (original) to 1.0 (target)</param>
    public virtual void AdjustOpacity(float progress) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);
        CurrentOpacity = Lerp(OriginalOpacity, TargetOpacity, progress);
    }

    /// <summary>
    /// Adjust rotation based on normalized progress (0.0 to 1.0)
    /// </summary>
    /// <param name="progress">Normalized value from 0.0 (original) to 1.0 (target)</param>
    public virtual void AdjustRotation(float progress) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);
        CurrentRotation = Lerp(OriginalRotation, TargetRotation, progress);
    }

    /// <summary>
    /// Adjust scale based on normalized progress (0.0 to 1.0)
    /// </summary>
    /// <param name="progress">Normalized value from 0.0 (original) to 1.0 (target)</param>
    public virtual void AdjustScale(float progress) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);
        CurrentScale = Lerp(OriginalScale, TargetScale, progress);
    }

    /// <summary>
    /// Adjust all properties based on normalized progress (0.0 to 1.0)
    /// </summary>
    /// <param name="progress">Normalized value from 0.0 (original) to 1.0 (target)</param>
    public virtual void AdjustAll(float progress) {
        AdjustPosition(progress);
        AdjustSize(progress);
        AdjustOpacity(progress);
        AdjustRotation(progress);
        AdjustScale(progress);
    }
    #endregion


    #region Screen Adjustment Methods
    /// <summary>
    /// Adjust for screen resize using Composers utility methods
    /// </summary>
    /// <param name="newContainerSize">New container/screen size</param>
    /// <param name="progress">Normalized transition progress (0.0 to 1.0)</param>
    public virtual void AdjustForScreenResize(Size newContainerSize, float progress = 1.0f) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);

        // Calculate scale using Composers utility
        float scale = Composers.GetScale(OriginalContainerSize, newContainerSize, ScalingMode);

        // Calculate new position using Composers utility
        Point newPosition = GetScaledAndAnchoredPoint(
            OriginalPosition,
            OriginalSize,
            OriginalContainerSize,
            newContainerSize,
            Anchor,
            ScalingMode
        );

        // Calculate new size using Composers utility
        Size newSize = GetScaledSize(OriginalSize, scale);

        // Interpolate to new values based on progress
        CurrentPosition = new Point(
            (int)Lerp(CurrentPosition.X, newPosition.X, progress),
            (int)Lerp(CurrentPosition.Y, newPosition.Y, progress)
        );

        CurrentSize = new Size(
            (int)Lerp(CurrentSize.Width, newSize.Width, progress),
            (int)Lerp(CurrentSize.Height, newSize.Height, progress)
        );

        CurrentScale = Lerp(CurrentScale, scale, progress);
    }

    /// <summary>
    /// Adjust position with anchor using Composers utility
    /// </summary>
    /// <param name="offset">Position offset</param>
    /// <param name="containerSize">Container size</param>
    /// <param name="anchor">Anchor preset</param>
    /// <param name="progress">Normalized transition progress (0.0 to 1.0)</param>
    public virtual void AdjustWithAnchor(Point offset, Size containerSize, AnchorPreset anchor, float progress = 1.0f) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);

        Point anchoredPosition = GetAnchoredPoint(
            offset,
            CurrentSize,
            containerSize,
            anchor
        );

        CurrentPosition = new Point(
            (int)Lerp(CurrentPosition.X, anchoredPosition.X, progress),
            (int)Lerp(CurrentPosition.Y, anchoredPosition.Y, progress)
        );
    }

    /// <summary>
    /// Adjust position with alignment using Composers utility
    /// </summary>
    /// <param name="containerSize">Container size</param>
    /// <param name="horizontalAlignment">Horizontal alignment</param>
    /// <param name="verticalAlignment">Vertical alignment</param>
    /// <param name="progress">Normalized transition progress (0.0 to 1.0)</param>
    public virtual void AdjustWithAlignment(Size containerSize, Alignment horizontalAlignment,
        Alignment verticalAlignment, float progress = 1.0f) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);

        Point alignedPosition = GetAlignedPoint(
            containerSize,
            CurrentSize,
            horizontalAlignment,
            verticalAlignment
        );

        CurrentPosition = new Point(
            (int)Lerp(CurrentPosition.X, alignedPosition.X, progress),
            (int)Lerp(CurrentPosition.Y, alignedPosition.Y, progress)
        );
    }
    #endregion


    #region Getter Methods
    /// <summary>
    /// Get current position
    /// </summary>
    public virtual Point GetPosition() => CurrentPosition;

    /// <summary>
    /// Get current size
    /// </summary>
    public virtual Size GetSize() => CurrentSize;

    /// <summary>
    /// Get current opacity (0.0 to 1.0)
    /// </summary>
    public virtual float GetOpacity() => CurrentOpacity;

    /// <summary>
    /// Get current rotation in degrees
    /// </summary>
    public virtual float GetRotation() => CurrentRotation;

    /// <summary>
    /// Get current scale factor
    /// </summary>
    public virtual float GetScale() => CurrentScale;

    /// <summary>
    /// Get original position
    /// </summary>
    public virtual Point GetOriginalPosition() => OriginalPosition;

    /// <summary>
    /// Get original size
    /// </summary>
    public virtual Size GetOriginalSize() => OriginalSize;

    /// <summary>
    /// Get target position
    /// </summary>
    public virtual Point GetTargetPosition() => TargetPosition;

    /// <summary>
    /// Get target size
    /// </summary>
    public virtual Size GetTargetSize() => TargetSize;

    /// <summary>
    /// Check if currently adjusting
    /// </summary>
    public virtual bool IsCurrentlyAdjusting() => IsAdjusting;
    #endregion


    #region Setter Methods
    /// <summary>
    /// Set current position
    /// </summary>
    public virtual void SetPosition(Point position) => CurrentPosition = position;

    /// <summary>
    /// Set current size
    /// </summary>
    public virtual void SetSize(Size size) => CurrentSize = size;

    /// <summary>
    /// Set current opacity (0.0 to 1.0)
    /// </summary>
    public virtual void SetOpacity(float opacity) => CurrentOpacity = Math.Clamp(opacity, 0.0f, 1.0f);

    /// <summary>
    /// Set current rotation in degrees
    /// </summary>
    public virtual void SetRotation(float rotation) => CurrentRotation = rotation;

    /// <summary>
    /// Set current scale factor
    /// </summary>
    public virtual void SetScale(float scale) => CurrentScale = Math.Max(0.0f, scale);

    /// <summary>
    /// Set target opacity (0.0 to 1.0)
    /// </summary>
    public virtual void SetTargetOpacity(float opacity) => TargetOpacity = Math.Clamp(opacity, 0.0f, 1.0f);

    /// <summary>
    /// Set target rotation in degrees
    /// </summary>
    public virtual void SetTargetRotation(float rotation) => TargetRotation = rotation;

    /// <summary>
    /// Set target scale factor
    /// </summary>
    public virtual void SetTargetScale(float scale) => TargetScale = Math.Max(0.0f, scale);

    /// <summary>
    /// Set anchor preset
    /// </summary>
    public virtual void SetAnchor(AnchorPreset anchor) => Anchor = anchor;

    /// <summary>
    /// Set scale mode
    /// </summary>
    public virtual void SetScaleMode(ScaleMode mode) => ScalingMode = mode;

    /// <summary>
    /// Set adjusting state
    /// </summary>
    public virtual void SetAdjusting(bool adjusting) => IsAdjusting = adjusting;
    #endregion


    #region Reset Methods
    /// <summary>
    /// Reset to original state
    /// </summary>
    public virtual void ResetToOriginal() {
        CurrentPosition = OriginalPosition;
        CurrentSize = OriginalSize;
        CurrentOpacity = OriginalOpacity;
        CurrentRotation = OriginalRotation;
        CurrentScale = OriginalScale;
    }

    /// <summary>
    /// Reset to target state
    /// </summary>
    public virtual void ResetToTarget() {
        CurrentPosition = TargetPosition;
        CurrentSize = TargetSize;
        CurrentOpacity = TargetOpacity;
        CurrentRotation = TargetRotation;
        CurrentScale = TargetScale;
    }

    /// <summary>
    /// Snap current state to target (instant transition)
    /// </summary>
    public virtual void SnapToTarget() {
        AdjustAll(1.0f);
    }
    #endregion


    #region Utility Methods
    /// <summary>
    /// Linear interpolation between two values
    /// </summary>
    /// <param name="start">Start value (at progress = 0.0)</param>
    /// <param name="end">End value (at progress = 1.0)</param>
    /// <param name="progress">Normalized progress (0.0 to 1.0)</param>
    /// <returns>Interpolated value</returns>
    protected static float Lerp(float start, float end, float progress) {
        return start + (end - start) * progress;
    }

    /// <summary>
    /// Get normalized progress from current to target position
    /// </summary>
    /// <returns>Normalized value (0.0 to 1.0)</returns>
    public virtual float GetProgressToTarget() {
        if (OriginalPosition == TargetPosition) return 1.0f;

        float distanceTotal = Distance(OriginalPosition, TargetPosition);
        if (distanceTotal == 0) return 1.0f;

        float distanceCurrent = Distance(OriginalPosition, CurrentPosition);
        return Math.Clamp(distanceCurrent / distanceTotal, 0.0f, 1.0f);
    }

    /// <summary>
    /// Calculate distance between two points
    /// </summary>
    protected static float Distance(Point a, Point b) {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
    #endregion
}
