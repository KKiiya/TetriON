using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace TetriON.Client.UI;

public class Composers {

    #region Singleton Pattern
    /// <summary>
    /// Calculate scale factor based on original and target screen dimensions
    /// </summary>
    public static float GetScale(int originalWidth, int originalHeight, int targetWidth, int targetHeight, ScaleMode mode) {
        if (originalWidth <= 0 || originalHeight <= 0 || targetWidth <= 0 || targetHeight <= 0) {
            return 1.0f;
        }

        float scaleX = (float)targetWidth / originalWidth;
        float scaleY = (float)targetHeight / originalHeight;

        return mode switch {
            ScaleMode.None => 1.0f,
            ScaleMode.Stretch => scaleX, // Note: May need separate X and Y handling
            ScaleMode.Proportional => Math.Min(scaleX, scaleY),
            ScaleMode.Fill => Math.Max(scaleX, scaleY),
            ScaleMode.FitToScreen => Math.Min(scaleX, scaleY),
            _ => 1.0f
        };
    }

    /// <summary>
    /// Calculate scale factor based on original and target sizes
    /// </summary>
    public static float GetScale(Size originalSize, Size targetSize, ScaleMode mode) {
        return GetScale(originalSize.Width, originalSize.Height, targetSize.Width, targetSize.Height, mode);
    }

    /// <summary>
    /// Calculate scaled point from original position based on scale factor
    /// </summary>
    public static Point GetScaledPoint(int originalX, int originalY, float scale) {
        return new Point((int)(originalX * scale), (int)(originalY * scale));
    }

    /// <summary>
    /// Calculate scaled point from original position
    /// </summary>
    public static Point GetScaledPoint(Point originalPoint, float scale) {
        return GetScaledPoint(originalPoint.X, originalPoint.Y, scale);
    }

    /// <summary>
    /// Calculate positioned point based on anchor preset
    /// </summary>
    public static Point GetAnchoredPoint(int x, int y, int width, int height, int containerWidth, int containerHeight, AnchorPreset anchor) {
        int baseX = x;
        int baseY = y;

        switch (anchor) {
            case AnchorPreset.TopLeft:
                baseX = x;
                baseY = y;
                break;
            case AnchorPreset.TopCenter:
                baseX = (containerWidth - width) / 2 + x;
                baseY = y;
                break;
            case AnchorPreset.TopRight:
                baseX = containerWidth - width - x;
                baseY = y;
                break;
            case AnchorPreset.MiddleLeft:
                baseX = x;
                baseY = (containerHeight - height) / 2 + y;
                break;
            case AnchorPreset.Center:
                baseX = (containerWidth - width) / 2 + x;
                baseY = (containerHeight - height) / 2 + y;
                break;
            case AnchorPreset.MiddleRight:
                baseX = containerWidth - width - x;
                baseY = (containerHeight - height) / 2 + y;
                break;
            case AnchorPreset.BottomLeft:
                baseX = x;
                baseY = containerHeight - height - y;
                break;
            case AnchorPreset.BottomCenter:
                baseX = (containerWidth - width) / 2 + x;
                baseY = containerHeight - height - y;
                break;
            case AnchorPreset.BottomRight:
                baseX = containerWidth - width - x;
                baseY = containerHeight - height - y;
                break;
        }

        return new Point(baseX, baseY);
    }

    /// <summary>
    /// Calculate positioned point based on anchor preset
    /// </summary>
    public static Point GetAnchoredPoint(Point position, Size elementSize, Size containerSize, AnchorPreset anchor) {
        return GetAnchoredPoint(position.X, position.Y, elementSize.Width, elementSize.Height,
            containerSize.Width, containerSize.Height, anchor);
    }

    /// <summary>
    /// Calculate scaled size based on original size and scale factor
    /// </summary>
    public static Size GetScaledSize(int originalWidth, int originalHeight, float scale) {
        return new Size((int)(originalWidth * scale), (int)(originalHeight * scale));
    }

    /// <summary>
    /// Calculate scaled size based on original size
    /// </summary>
    public static Size GetScaledSize(Size originalSize, float scale) {
        return GetScaledSize(originalSize.Width, originalSize.Height, scale);
    }

    /// <summary>
    /// Calculate scaled and positioned point in one operation
    /// </summary>
    public static Point GetScaledAndAnchoredPoint(Point originalPosition, Size originalElementSize,
        Size originalContainerSize, Size targetContainerSize, AnchorPreset anchor, ScaleMode scaleMode) {

        float scale = GetScale(originalContainerSize, targetContainerSize, scaleMode);
        Point scaledPosition = GetScaledPoint(originalPosition, scale);
        Size scaledElementSize = GetScaledSize(originalElementSize, scale);

        return GetAnchoredPoint(scaledPosition, scaledElementSize, targetContainerSize, anchor);
    }

    /// <summary>
    /// Calculate aspect ratio from dimensions
    /// </summary>
    public static float GetAspectRatio(int width, int height) {
        return height > 0 ? (float)width / height : 1.0f;
    }

    /// <summary>
    /// Calculate aspect ratio from size
    /// </summary>
    public static float GetAspectRatio(Size size) {
        return GetAspectRatio(size.Width, size.Height);
    }

    /// <summary>
    /// Calculate aligned position within a container
    /// </summary>
    public static int GetAlignedPosition(int containerSize, int elementSize, Alignment alignment) {
        return alignment switch {
            Alignment.Start => 0,
            Alignment.Center => (containerSize - elementSize) / 2,
            Alignment.End => containerSize - elementSize,
            _ => 0
        };
    }

    /// <summary>
    /// Calculate aligned point within a container
    /// </summary>
    public static Point GetAlignedPoint(Size containerSize, Size elementSize, Alignment horizontalAlignment, Alignment verticalAlignment) {
        return new Point(
            GetAlignedPosition(containerSize.Width, elementSize.Width, horizontalAlignment),
            GetAlignedPosition(containerSize.Height, elementSize.Height, verticalAlignment)
        );
    }
    #endregion


    #region Enums
    public enum AnchorPreset {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum ScaleMode {
        None,           // No automatic scaling
        Stretch,        // Stretch to fit target size (may distort aspect ratio)
        Proportional,   // Scale proportionally to fit within target size
        Fill,           // Scale proportionally to fill target size (may crop)
        FitToScreen     // Scale based on screen resolution percentage
    }

    public enum Alignment {
        Start,
        Center,
        End
    }
    #endregion
}

