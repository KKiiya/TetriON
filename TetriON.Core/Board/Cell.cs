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

    public Cell() {
        IsOccupied = false;
        CellColor = Color.Empty;
    }

    public void Occupy(Color color) {
        IsOccupied = true;
        CellColor = color;
    }

    public void Vacate() {
        IsOccupied = false;
        CellColor = Color.Empty;
    }

    public enum CellType {
        Normal,
        Garbage,
        Locked
    }
}
