using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{
    private enum State
    {
        Normal,
        Attacking,
        Moving
    }

    private enum Player
    {
        Living,
        Dead
    }

    private State state = State.Normal;
    private Player activePlayer = Player.Living;   // Living starts
    
    private GenericUnit selectedUnit;
    private GenericUnit[] units;

    private Tilemap tilemap;
    private GameState gameState;

    private Canvas unitMenu;
    private TextMeshProUGUI attackText;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && state == State.Moving)
        {
            Vector3Int mousePosOnGrid = GetClickedGridPosition();
            if (moveSelectedUnit(mousePosOnGrid))
            {
                // TODO: prevent them from moving again
                state = State.Normal;
            }
        }
    }

    private void Start()
    {
        tilemap = FindObjectOfType<Tilemap>();
        units = FindObjectsOfType<GenericUnit>();

        unitMenu = GameObject.FindGameObjectWithTag("UnitMenu").GetComponent<Canvas>();
        TextMeshProUGUI[] unitMenuChildren = unitMenu.GetComponentsInChildren<TextMeshProUGUI>();

        attackText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("AttackStatDisplay");
        });

        gameState = new GameState();

        Array.ForEach(units, delegate (GenericUnit u) { gameState.SetUnitAtPosition((Vector2Int)u.GetPosition(), u); });
    }

    public void SelectUnit(GenericUnit unit)
    {
        switch (state)
        {
            case State.Normal:
                if (!unit.CompareTag(activePlayer.ToString()) || !unit.CanBeSelected()) 
                {
                    return;
                }
                selectedUnit = unit;

                unitMenu.enabled = true;
                attackText.text = unit.Attack.ToString() + " Attack";
                break;

            case State.Attacking:
                if (unit.CompareTag(activePlayer.ToString())) 
                {
                    return;
                }
                if (unit.CanBeAttacked() && !selectedUnit.CompareTag(unit.tag) && selectedUnit.UnitInRange(unit))

                {
                    selectedUnit.AttackUnit(unit);

                    state = State.Normal;
                    unitMenu.enabled = false;

                    EndTurn();
                }
                break;

            default:
                break;
        }
    }

    private Vector3Int GetClickedGridPosition()
    {
        // TODO: check to see if this is messed up as well
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        return tilemap.layoutGrid.WorldToCell(mouseWorldPos);
    }

    private void ShowTilesInRange(GenericUnit unit)
    {
        foreach(Vector3Int tilePos in unit.TilesInRange(tilemap))
        {
            tilemap.SetTileFlags(tilePos, TileFlags.None);
            tilemap.SetColor(tilePos, Color.red);
        }

        tilemap.SetTileFlags(new Vector3Int(0, 0, 1), TileFlags.None);
        tilemap.SetColor(new Vector3Int(0,0,0), Color.yellow);
    }

    private void RemoveColorFromTilesInRange(GenericUnit unit)
    {
        foreach(Vector3Int tileInRange in unit.TilesInRange(tilemap)){
            tilemap.SetColor(tileInRange, new Color(1.0f, 1.0f, 1.0f, 1.0f));
            tilemap.SetTileFlags(tileInRange, TileFlags.LockColor);
        }
    }


    private bool moveSelectedUnit(Vector3Int cellPosition)
    {
        gameState.RemoveUnitAtPosition((Vector2Int)selectedUnit.GetPosition());

        if (selectedUnit.TileInRange(cellPosition))
        {
            RemoveColorFromTilesInRange(selectedUnit);

            selectedUnit.Move(cellPosition);

            gameState.SetUnitAtPosition((Vector2Int)selectedUnit.GetPosition(), selectedUnit);

            return true;
        }

        return false;
    }

    public void Attack()
    {
        state = State.Attacking;
    }

    public void Move()
    {
        state = State.Moving;
        ShowTilesInRange(selectedUnit);
    }

    public void EndTurn()
    {
        Debug.Log("Turn has ended!");
        state = State.Normal;
        unitMenu.enabled = false;

        ChangeTurns();
    }

    private void ChangeTurns() 
    {
        if (activePlayer == Player.Living) 
        {
            activePlayer = Player.Dead;
        }
        else if (activePlayer == Player.Dead) 
        {
            activePlayer = Player.Living;
        }
        Array.ForEach(units, delegate (GenericUnit u) { u.DecrementTurnTimers(); });
    }
}
