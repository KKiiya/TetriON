using System.Drawing;
using System.Numerics;

namespace TetriON.Client.Content.UI.Utils;

public class UDim2(Vector2 position, Point offset, Vector2 objectSize, Size parentSize) : IDisposable {

    public static UDim2 Zero => new(new Vector2(0, 0), new Point(0, 0), new Vector2(0, 0), new Size(0, 0));

    public Vector2 PositionPercentil { get; set; } = position;
    public Point Offset { get; set; } = offset;
    public Vector2 ObjectSize { get; set; } = objectSize;
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
