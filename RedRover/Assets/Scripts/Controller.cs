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

    public Grid grid;
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
            Vector3Int mousePos = GetClickedGridPosition();
            MoveSelectedUnit(mousePos);
        }

        // DEBUG MOVEMENT
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (selectedUnit != null && state == State.Moving)
            {
                selectedUnit.transform.position += new Vector3(1, 0, 0);
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow) && state == State.Moving)
        {
            if (selectedUnit != null)
            {
                selectedUnit.transform.position -= new Vector3(1, 0, 0);
            }
        }

        if (Input.GetKey(KeyCode.UpArrow) && state == State.Moving)
        {
            if (selectedUnit != null)
            {
                selectedUnit.transform.position += new Vector3(0, 1, 0);
            }
        }

        if (Input.GetKey(KeyCode.DownArrow) && state == State.Moving)
        {
            if (selectedUnit != null)
            {
                selectedUnit.transform.position -= new Vector3(0, 1, 0);
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
                    state = State.Normal;
                    unitMenu.enabled = false;
                    StopCoroutine(cancelCoroutine);
                }
                break;

            default:
                break;
        }
    }

    private Vector3Int GetClickedGridPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return grid.WorldToCell(mouseWorldPos);
    }

    private Vector3Int GetUnitGridPosition(GenericUnit unit)
    {
        return grid.WorldToCell(unit.transform.position);
    }

    private void MoveSelectedUnit(Vector3Int destGridPos)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(destGridPos);
        selectedUnit.transform.position = new Vector3(worldPos.x, worldPos.y, 1);
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
