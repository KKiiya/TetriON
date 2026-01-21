using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace TetriON.Client.Content.UI.Utils;

public class UDim2(Vector2 scale, Vector2 offset, Size objectSize, Size parentSize) {

    public static UDim2 Zero => new(new Vector2(0, 0), new Vector2(0, 0), new Size(0, 0), new Size(0, 0));

    public Vector2 Scale { get; set; } = scale;
    public Vector2 Offset { get; set; } = offset;
    public Size ObjectSize { get; set; } = objectSize;
    public Size ParentSize { get; set; } = parentSize;

    public Point ToAbsolute() {
        int x = (int)(Scale.X * ParentSize.Width) + (int)Offset.X;
        int y = (int)(Scale.Y * ParentSize.Height) + (int)Offset.Y;
        return new Point(x, y);
    }

    public Point ToRelative() {
        int x = (int)((Offset.X - (Scale.X * ParentSize.Width)) / ObjectSize.Width);
        int y = (int)((Offset.Y - (Scale.Y * ParentSize.Height)) / ObjectSize.Height);
        return new Point(x, y);
    }
}
