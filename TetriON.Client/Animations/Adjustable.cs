using System.Drawing;
using System.Numerics;
using TetriON.Client.Content.UI;
using TetriON.Client.Content.UI.Utils;
using static TetriON.Client.Content.UI.Composers;

namespace TetriON.Client.Animations;

/// <summary>
/// Base class for UI elements that support smooth resizing, animations and adjustments.
/// Uses normalized values (0.0 to 1.0) to interpolate between original and target states.
/// </summary>
public abstract class Adjustable(ClientController? controller) {

    public ClientController Controller { get; } = controller ?? throw new ArgumentNullException(nameof(controller));


    #region Original State Properties
    // Store original position and dimensions
    protected UDim2 OriginalUDim2 { get; set; } = UDim2.Zero;
    protected Vector2 OriginalPosition {
        get => OriginalUDim2.PositionPercentil;
        set => OriginalUDim2.PositionPercentil = value;
    }
    protected Point OriginalOffset {
        get => OriginalUDim2.Offset;
        set => OriginalUDim2.Offset = new(value.X, value.Y);
    }
    protected Vector2 OriginalSize {
        get => OriginalUDim2.ObjectSize;
        set => OriginalUDim2.ObjectSize = value;
    }
    protected Size OriginalContainerSize {
        get => OriginalUDim2.ParentSize;
        set => OriginalUDim2.ParentSize = value;
    }

    // Store original visual properties
    protected float OriginalOpacity { get; set; } = 1.0f;
    protected float OriginalRotation { get; set; } = 0.0f;
    protected float OriginalScale { get; set; } = 1.0f;
    #endregion


    #region Target State Properties
    // Store target position and dimensions
    protected UDim2 TargetUDim2 { get; set; } = UDim2.Zero;
    protected Vector2 TargetPosition {
        get => TargetUDim2.PositionPercentil;
        set => TargetUDim2.PositionPercentil = value;
    }
    protected Point TargetOffset {
        get => TargetUDim2.Offset;
        set => TargetUDim2.Offset = new(value.X, value.Y);
    }
    protected Vector2 TargetSize {
        get => TargetUDim2.ObjectSize;
        set => TargetUDim2.ObjectSize = value;
    }
    protected Size TargetContainerSize {
        get => TargetUDim2.ParentSize;
        set => TargetUDim2.ParentSize = value;
    }

    // Store target visual properties
    protected float TargetOpacity { get; set; } = 1.0f;
    protected float TargetRotation { get; set; } = 0.0f;
    protected float TargetScale { get; set; } = 1.0f;
    #endregion


    #region Current State Properties
    // Current interpolated values
    protected UDim2 CurrentUDim2 { get; set; } = UDim2.Zero;
    protected Vector2 CurrentPosition {
        get => CurrentUDim2.PositionPercentil;
        set => CurrentUDim2.PositionPercentil = value;
    }
    protected Point CurrentOffset {
        get => CurrentUDim2.Offset;
        set => CurrentUDim2.Offset = new(value.X, value.Y);
    }
    protected Vector2 CurrentSize {
        get => CurrentUDim2.ObjectSize;
        set => CurrentUDim2.ObjectSize = value;
    }
    protected Size CurrentContainerSize {
        get => CurrentUDim2.ParentSize;
        set => CurrentUDim2.ParentSize = value;
    }

    /// <summary>Gets the absolute pixel position from the UDim2 (Scale * ParentSize + Offset)</summary>
    protected Point CurrentAbsolutePosition => CurrentUDim2.ToAbsolute();

    /// <summary>Gets the absolute pixel size from the UDim2 (ObjectSize * ParentSize)</summary>
    protected Size CurrentAbsoluteSize { get => CurrentUDim2.GetAbsoluteSize(); }

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
    /// Initialize with original position and size using scale values (0-1)
    /// </summary>
    public virtual void Initialize(Vector2 position, Point offset, Vector2 size, Size containerSize) {
        OriginalUDim2 = new UDim2(position, offset, size, containerSize);
        CurrentUDim2 = new UDim2(position, offset, size, containerSize);
        TargetUDim2 = new UDim2(position, offset, size, containerSize);
    }

    /// <summary>
    /// Initialize with absolute pixel position and size (converts to scale)
    /// </summary>
    public virtual void Initialize(Point absolutePosition, Size absoluteSize, Size containerSize) {
        Vector2 positionScale = new(0, 0);
        Vector2 sizeScale = new(0, 0);

        if (containerSize.Width > 0 && containerSize.Height > 0) {
            positionScale = new Vector2(
                (float)absolutePosition.X / containerSize.Width,
                (float)absolutePosition.Y / containerSize.Height
            );
            sizeScale = new Vector2(
                (float)absoluteSize.Width / containerSize.Width,
                (float)absoluteSize.Height / containerSize.Height
            );
        }

        Initialize(positionScale, new Point(0, 0), sizeScale, containerSize);
    }

    /// <summary>
    /// Set the target state for adjustment
    /// </summary>
    public virtual void SetTarget(Vector2 targetPosition, Vector2 targetSize, Size targetContainerSize) {
        TargetPosition = targetPosition;
        TargetSize = targetSize;
        TargetContainerSize = targetContainerSize;
    }

    /// <summary>
    /// Set the target state for adjustment with offset
    /// </summary>
    public virtual void SetTarget(Vector2 targetPosition, Point targetOffset, Vector2 targetSize, Size targetContainerSize) {
        TargetPosition = targetPosition;
        TargetOffset = targetOffset;
        TargetSize = targetSize;
        TargetContainerSize = targetContainerSize;
    }

    /// <summary>
    /// Set original state from current values
    /// </summary>
    public virtual void StoreOriginalState() {
        OriginalPosition = CurrentPosition;
        OriginalOffset = CurrentOffset;
        OriginalSize = CurrentSize;
        OriginalContainerSize = CurrentContainerSize;
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

        CurrentPosition = new Vector2(
            Lerp(OriginalPosition.X, TargetPosition.X, progress),
            Lerp(OriginalPosition.Y, TargetPosition.Y, progress)
        );
    }

    /// <summary>
    /// Adjust offset based on normalized progress (0.0 to 1.0)
    /// </summary>
    /// <param name="progress">Normalized value from 0.0 (original) to 1.0 (target)</param>
    public virtual void AdjustOffset(float progress) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);

        CurrentOffset = new Point(
            (int)Lerp(OriginalOffset.X, TargetOffset.X, progress),
            (int)Lerp(OriginalOffset.Y, TargetOffset.Y, progress)
        );
    }

    /// <summary>
    /// Adjust size based on normalized progress (0.0 to 1.0)
    /// </summary>
    /// <param name="progress">Normalized value from 0.0 (original) to 1.0 (target)</param>
    public virtual void AdjustSize(float progress) {
        progress = Math.Clamp(progress, 0.0f, 1.0f);

        CurrentSize = new Vector2(
            Lerp(OriginalSize.X, TargetSize.X, progress),
            Lerp(OriginalSize.Y, TargetSize.Y, progress)
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
        AdjustOffset(progress);
        AdjustSize(progress);
        AdjustOpacity(progress);
        AdjustRotation(progress);
        AdjustScale(progress);
    }
    #endregion


    #region Getter Methods
    /// <summary>
    /// Get current position (scale, 0-1)
    /// </summary>
    public virtual Vector2 GetPosition() => CurrentPosition;

    /// <summary>
    /// Get current absolute position in pixels (Scale * ParentSize + Offset)
    /// </summary>
    public virtual Point GetAbsolutePosition() => CurrentAbsolutePosition;

    /// <summary>
    /// Get current size (scale, 0-1)
    /// </summary>
    public virtual Vector2 GetSize() => CurrentSize;

    /// <summary>
    /// Get current absolute size in pixels (ObjectSize * ParentSize)
    /// </summary>
    public virtual Size GetAbsoluteSize() => CurrentAbsoluteSize;

    /// <summary>
    /// Get current center point in absolute pixels
    /// </summary>
    public virtual Point GetCenter() {
        var absPos = CurrentAbsolutePosition;
        var absSize = CurrentAbsoluteSize;
        return new Point(
            absPos.X + absSize.Width / 2,
            absPos.Y + absSize.Height / 2
        );
    }

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
    public virtual Vector2 GetOriginalPosition() => OriginalPosition;

    /// <summary>
    /// Get original offset
    /// </summary>
    public virtual Point GetOriginalOffset() => OriginalOffset;

    /// <summary>
    /// Get original size
    /// </summary>
    public virtual Vector2 GetOriginalSize() => OriginalSize;

    /// <summary>
    /// Get original container size
    /// </summary>
    public virtual Size GetOriginalContainerSize() => OriginalContainerSize;

    /// <summary>
    /// Get target position
    /// </summary>
    public virtual Vector2 GetTargetPosition() => TargetPosition;

    /// <summary>
    /// Get target offset
    /// </summary>
    public virtual Point GetTargetOffset() => TargetOffset;

    /// <summary>
    /// Get target size
    /// </summary>
    public virtual Vector2 GetTargetSize() => TargetSize;

    /// <summary>
    /// Get target container size
    /// </summary>
    public virtual Size GetTargetContainerSize() => TargetContainerSize;

    /// <summary>
    /// Check if currently adjusting
    /// </summary>
    public virtual bool IsCurrentlyAdjusting() => IsAdjusting;
    #endregion


    #region Setter Methods
    /// <summary>
    /// Set current udim2
    /// </summary>
    public virtual void SetUDim2(UDim2 udim2) => CurrentUDim2 = udim2;


    /// <summary>
    /// Set current position (scale, 0-1)
    /// </summary>
    public virtual void SetPosition(Vector2 position) => CurrentPosition = position;

    /// <summary>
    /// Set current position from absolute pixels (converts to scale based on parent size)
    /// </summary>
    public virtual void SetPosition(Point absolutePosition) {
        if (CurrentContainerSize.Width > 0 && CurrentContainerSize.Height > 0) {
            CurrentPosition = new Vector2(
                (float)absolutePosition.X / CurrentContainerSize.Width,
                (float)absolutePosition.Y / CurrentContainerSize.Height
            );
        }
    }

    /// <summary
    /// Set current offset
    /// </summary>
    public virtual void SetOffset(Point offset) => CurrentOffset = offset;

    /// <summary>
    /// Set current size (scale, 0-1)
    /// </summary>
    public virtual void SetSize(Vector2 size) => CurrentSize = size;

    /// <summary>
    /// Set current size from absolute pixels (converts to scale based on parent size)
    /// </summary>
    public virtual void SetSize(Size absoluteSize) {
        if (CurrentContainerSize.Width > 0 && CurrentContainerSize.Height > 0) {
            CurrentSize = new Vector2(
                (float)absoluteSize.Width / CurrentContainerSize.Width,
                (float)absoluteSize.Height / CurrentContainerSize.Height
            );
        }
    }

    /// <summary>
    /// Set current parent size
    /// </summary>
    public virtual void SetContainerSize(Size containerSize) => CurrentContainerSize = containerSize;

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
        CurrentOffset = OriginalOffset;
        CurrentSize = OriginalSize;
        CurrentContainerSize = OriginalContainerSize;
        CurrentOpacity = OriginalOpacity;
        CurrentRotation = OriginalRotation;
        CurrentScale = OriginalScale;
    }

    /// <summary>
    /// Reset to target state
    /// </summary>
    public virtual void ResetToTarget() {
        CurrentPosition = TargetPosition;
        CurrentOffset = TargetOffset;
        CurrentSize = TargetSize;
        CurrentContainerSize = TargetContainerSize;
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
    protected static float Distance(Vector2 a, Vector2 b) {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
    #endregion
}
