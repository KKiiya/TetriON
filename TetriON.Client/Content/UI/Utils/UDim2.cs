using System.Drawing;
using System.Numerics;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Content.UI.Utils;

public class UDim2(Vector2 position, Point offset, Vector2 objectSize, Size parentSize) : IDisposable {

    public static UDim2 Zero => new(new Vector2(0, 0), new Point(0, 0), new Vector2(0, 0), new Size(0, 0));

    private Vector2 _positionPercentil = position;
    private Point _offset = offset;
    private Vector2 _objectSize = objectSize;
    private Size _parentSize = parentSize;

    // Vector that MUST range from 0 to 1 as float values that act as percentages of a parent container.
    public Vector2 PositionPercentil {
        get => _positionPercentil;
        set {
            if (_positionPercentil == value) return;
            _positionPercentil = value;
            CalculateAbsoluteCache();
        }
    }

    // Absolute offset in pixels that will be added to the position.
    public Point Offset {
        get => _offset;
        set {
            if (_offset == value) return;
            _offset = value;
            CalculateAbsoluteCache();
        }
    }

    // Size of the object this UDim2 is representing, in percentage of the parent size.
    public Vector2 ObjectSize {
        get => _objectSize;
        set {
            if (_objectSize == value) return;
            _objectSize = value;
            CalculateAbsoluteCache();
        }
    }

    // Size of the parent container, used to calculate absolute positions in pixels.
    public Size ParentSize {
        get => _parentSize;
        set {
            if (_parentSize == value) return;
            _parentSize = value;
            CalculateAbsoluteCache();
        }
    }

    private Size _cachedAbsoluteSize = Size.Empty;
    private Point _cachedAbsolutePosition = Point.Empty;


    public Point ToAbsolute() {
        return _cachedAbsolutePosition;
    }

    public Point ToRelative() {
        float objectSizeX = ObjectSize.X * ParentSize.Width;
        float objectSizeY = ObjectSize.Y * ParentSize.Height;
        objectSizeX = Math.Max(objectSizeX, 1);
        objectSizeY = Math.Max(objectSizeY, 1);
        int x = (int)((Offset.X - (PositionPercentil.X * ParentSize.Width)) / objectSizeX);
        int y = (int)((Offset.Y - (PositionPercentil.Y * ParentSize.Height)) / objectSizeY);
        return new Point(x, y);
    }

    public Size GetAbsoluteSize() {
        return _cachedAbsoluteSize;
    }

    public void Dispose() {
        PositionPercentil = Vector2.Zero;
        Offset = Point.Empty;
        ObjectSize = Vector2.Zero;
        ParentSize = Size.Empty;
        GC.SuppressFinalize(this);
    }

    private void CalculateAbsoluteCache() {
        _cachedAbsolutePosition = CalculateAbsolutePosition();
        _cachedAbsoluteSize = CalculateAbsoluteSize();
    }

    private Point CalculateAbsolutePosition() {
        int x = (int)(PositionPercentil.X * ParentSize.Width) + Offset.X;
        int y = (int)(PositionPercentil.Y * ParentSize.Height) + Offset.Y;
        return new Point(x, y);
    }

    private Size CalculateAbsoluteSize() {
        int width = (int)(ObjectSize.X * ParentSize.Width);
        int height = (int)(ObjectSize.Y * ParentSize.Height);
        return new Size(width, height);
    }

    ~UDim2() {
        Dispose();
    }
}
