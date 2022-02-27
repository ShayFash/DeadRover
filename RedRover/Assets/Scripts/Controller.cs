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
        Attacking
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

        //Tilemap interaction

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector3Int mousePos = GetGridPosition();
        //    Debug.Log("Clicked grid cell: " + mousePos);

        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        //    if (hit.collider != null && hit.collider.gameObject.CompareTag("Unit"))
        //    {
        //        if (selectedUnit != hit.collider.gameObject)
        //        {
        //            selectedUnit = hit.collider.gameObject;
        //            //not used by now
        //            Tile clickedTile = tilemap.GetTile<Tile>(mousePos);
        //        }
        //        else
        //        {
        //            selectedUnit = null;
        //        }
        //    }
        //    else
        //    {
        //        if (this.selectedUnit != null)
        //        {
        //            MoveSelectedUnit(mousePos);
        //        }

        //    }

        //}

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (selectedUnit != null)
            {
                selectedUnit.transform.position += new Vector3(1, 0, 0);
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (selectedUnit != null)
            {
                selectedUnit.transform.position -= new Vector3(1, 0, 0);
            }
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (selectedUnit != null)
            {
                selectedUnit.transform.position += new Vector3(0, 1, 0);
            }
        }

        if (Input.GetKey(KeyCode.DownArrow))
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

    Vector3Int GetGridPosition()
    {

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return grid.WorldToCell(mouseWorldPos);

    }

    void MoveSelectedUnit(Vector3Int destGridPos)
    {
        destGridPos = new Vector3Int(destGridPos.x, destGridPos.y, 5);
        selectedUnit.transform.position = grid.CellToWorld(destGridPos);
        Debug.Log("move from: " + selectedUnit.transform.position + " to: " + grid.CellToWorld(destGridPos));
    }

    public void Attack()
    {
        state = State.Attacking;
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
