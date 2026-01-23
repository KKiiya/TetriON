using System.Drawing;
using System.Numerics;

namespace TetriON.Client.Content.UI.Utils;

public class UDim2(Vector2 position, Point offset, Vector2 objectSize, Size parentSize) : IDisposable {

    public static UDim2 Zero => new(new Vector2(0, 0), new Point(0, 0), new Vector2(0, 0), new Size(0, 0));

    // Vector that MUST range from 0 to 1 as float values that act as percentages of a parent container.
    public Vector2 PositionPercentil { get; set; } = position;

    // Absolute offset in pixels that will be added to the position.
    public Point Offset { get; set; } = offset;

    // Size of the object this UDim2 is representing, in percentage of the parent size.
    public Vector2 ObjectSize { get; set; } = objectSize;

    // Size of the parent container, used to calculate absolute positions in pixels.
    public Size ParentSize { get; set; } = parentSize;

    public Point ToAbsolute() {
        int x = (int)(PositionPercentil.X * ParentSize.Width) + Offset.X;
        int y = (int)(PositionPercentil.Y * ParentSize.Height) + Offset.Y;
        return new Point(x, y);
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
        int width = (int)(ObjectSize.X * ParentSize.Width);
        int height = (int)(ObjectSize.Y * ParentSize.Height);
        return new Size(width, height);
    }

    public void Dispose() {
        PositionPercentil = Vector2.Zero;
        Offset = Point.Empty;
        ObjectSize = Vector2.Zero;
        ParentSize = Size.Empty;
        GC.SuppressFinalize(this);
    }

    ~UDim2() {
        Dispose();
    }
}
