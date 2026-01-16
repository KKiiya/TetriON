using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace TetriON.Core.Board;

public class Cell {

    public bool IsOccupied { get; private set; }
    public Color CellColor { get; private set; }
    public CellType Type { get; set; } = CellType.Normal;
    public byte Identifier { get; set; } = 0;

    public Cell() {
        IsOccupied = false;
        CellColor = Color.Empty;
    }

    public void Occupy(Color color, CellType type = CellType.Normal, byte identifier = 0) {
        IsOccupied = true;
        CellColor = color;
        Type = type;
        Identifier = identifier;
    }

    public void Vacate() {
        IsOccupied = false;
        CellColor = Color.Empty;
        Type = CellType.Normal;
        Identifier = 0;
    }

    public enum CellType {
        Normal,
        Garbage,
        Locked
    }
}
