using System;
using System.Collections;
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

    private State state = State.Normal;
    
    private GenericUnit selectedUnit;
    private GenericUnit[] units;

    public Tilemap tilemap;

    private Canvas unitMenu;
    private TextMeshProUGUI attackText;

    private Coroutine cancelCoroutine;

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("Decrementing Turn Timers");
            Array.ForEach(units, delegate (GenericUnit u) { u.DecrementTurnTimers(); });
        }

        if (Input.GetMouseButtonDown(0) && state == State.Moving)
        {
            Vector3Int mousePosOnGrid = GetClickedGridPosition();
            if (MoveSelectedUnit(mousePosOnGrid))
            {
                ClearUnitMenu();
            }
        }
    }

    private void Start()
    {
        units = FindObjectsOfType<GenericUnit>();

        unitMenu = GameObject.FindGameObjectWithTag("UnitMenu").GetComponent<Canvas>();
        TextMeshProUGUI[] unitMenuChildren = unitMenu.GetComponentsInChildren<TextMeshProUGUI>();

        attackText = Array.Find(unitMenuChildren, delegate (TextMeshProUGUI t) {
            return t.gameObject.CompareTag("AttackStatDisplay");
        });
    }

    public void SelectUnit(GenericUnit unit)
    {
        switch (state)
        {
            case State.Normal:
                selectedUnit = unit;

                unitMenu.enabled = true;
                attackText.text = unit.Attack.ToString() + " Attack";
                break;

            case State.Attacking:
                if (unit.CanBeAttacked() && !selectedUnit.CompareTag(unit.tag) && selectedUnit.UnitInRange(unit))
                {
                    selectedUnit.AttackUnit(unit);
                    ClearUnitMenu();
                }
                break;

            default:
                break;
        }
    }

    private void ClearUnitMenu()
    {
        state = State.Normal;
        unitMenu.enabled = false;
        StopCoroutine(cancelCoroutine);
    }

    private Vector3Int GetClickedGridPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        return tilemap.layoutGrid.WorldToCell(mouseWorldPos);
    }

    private Vector3Int GetUnitGridPosition(GenericUnit unit)
    {
        return tilemap.layoutGrid.WorldToCell(unit.transform.position);
    }

    private bool MoveSelectedUnit(Vector3Int destGridPos)
    {

        if (selectedUnit.TileInRange(destGridPos))
        {
            RemoveColorFromTilesInRange(selectedUnit);

            Vector3 clickedWorldPos = tilemap.layoutGrid.GetCellCenterWorld(destGridPos);
            selectedUnit.transform.position = clickedWorldPos;
            return true;
        }

        return false;
    }

    private void ShowTilesInRange(GenericUnit unit)
    {
        Vector3Int gridPos = GetUnitGridPosition(unit);

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


    public void Attack()
    {
        state = State.Attacking;
        cancelCoroutine = StartCoroutine(CancelAction());
    }

    public void Move()
    {
        state = State.Moving;
        ShowTilesInRange(selectedUnit);
        cancelCoroutine = StartCoroutine(CancelAction());
    }

    IEnumerator CancelAction()
    {
        while (!Input.GetKeyDown(KeyCode.Escape))
        {
            yield return null;
        }
        Debug.Log("Cancelling action");
        state = State.Normal;
    }

}
