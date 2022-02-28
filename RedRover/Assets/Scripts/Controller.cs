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
            MoveSelectedUnit(mousePosOnGrid);
            ClearUnitMenu();
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
        return tilemap.layoutGrid.WorldToCell(mouseWorldPos);
    }

    private Vector3Int GetUnitGridPosition(GenericUnit unit)
    {
        return tilemap.layoutGrid.WorldToCell(unit.transform.position);
    }

    private void MoveSelectedUnit(Vector3Int destGridPos)
    {
        if (selectedUnit.TileInRange(destGridPos))
        {
            Debug.Log("In Range");
        }
        else
        {
            Debug.Log("Not in range");
        }
        Vector3 clickedWorldPos = tilemap.layoutGrid.GetCellCenterWorld(destGridPos);
        selectedUnit.transform.position = new Vector3(clickedWorldPos.x, clickedWorldPos.y, 1);
    }

    private void ShowTilesInRange(GenericUnit unit)
    {
        Debug.Log("range: " + unit.Reach);
        // Vector3 unitGridPos = GetUnitGridPosition(selectedUnit);
        // TODO: Show nearby tiles differently
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
