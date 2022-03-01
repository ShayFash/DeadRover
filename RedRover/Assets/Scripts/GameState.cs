using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    private Dictionary<Vector2Int, GenericUnit> units;

    public GameState()
    {
        units = new Dictionary<Vector2Int, GenericUnit>();
    }

    public void SetUnitAtPosition(Vector2Int position, GenericUnit unit)
    {
        units.TryAdd(position, unit);
    }

    public void RemoveUnitAtPosition(Vector2Int position)
    {
        units.Remove(position);
    }
}
