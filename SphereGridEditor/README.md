# Sphere Grid Editor

Interactive MonoGame-based editor for the sphere grid definition.

## Usage

```powershell
dotnet run --project .\SphereGridEditor\
```

## Controls

### Navigation

- **Left-click** - Select node
- **Middle-click drag** - Pan camera
- **Hover** - View node tooltip (cost, powerup, connection directions)
- **Ctrl + P** - Exit

### Editing

- **N** - Create new empty node at mouse position
- **Delete** - Delete selected node (cannot delete root)
- **1-6** - Start connection from selected node or prepare edge for deletion
    - 1 = TopLeft
    - 2 = TopRight
    - 3 = MiddleLeft
    - 4 = MiddleRight
    - 5 = BottomLeft
    - 6 = BottomRight
- **Left-click target node** - Complete connection (after pressing 1-6)
- **X** - Delete edge (after pressing 1-6 on existing connection)
- **Right-click** - Cancel connection mode

### Export

- **C** - Copy generated C# code to clipboard

## Workflow

1. **Create nodes**: Press **N** to create empty nodes
2. **Connect nodes**: Select a node → press **1-6** for direction → click target node
3. **Delete edges**: Select a node → press **1-6** on existing connection → press **X**
4. **Delete nodes**: Select a node → press **Delete**
5. **Export**: Press **C** to copy the full C# code to paste into `SphereGrid.Create()`

## Notes

- All edges are bidirectional - creating an edge in one direction automatically creates the reverse
- Tooltips show existing connection directions to help avoid conflicts
- The generated code includes all edge directions explicitly for clarity
