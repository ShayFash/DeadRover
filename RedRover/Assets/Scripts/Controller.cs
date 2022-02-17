using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{
    // Option: Use tags instead?!
    public List<GenericUnit> team1 = new List<GenericUnit>();
    public List<GenericUnit> team2 = new List<GenericUnit>();

    public Grid grid;
    public Tilemap tilemap;

    private GameObject selectedUnit;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key was pressed.");
            List<GenericUnit> enemiesInRange = team1[0].GetEnemiesInRange(team2);
            foreach (GenericUnit enemy in enemiesInRange)
            {
                team1[0].Attack(enemy);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S key was pressed.");
            Debug.Log("There are " + team1[0].GetEnemiesInRange(team2).Count + " enemies in range.");
        }


        //Tilemap interaction

        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int mousePos = GetGridPosition();
            Debug.Log("Clicked grid cell: " + mousePos);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Unit")) {
                if(selectedUnit != hit.collider.gameObject)
                {
                    selectedUnit = hit.collider.gameObject;
                    //Tile clickedTile = tilemap.GetTile<Tile>(mousePos);
                    //Debug.Log(clickedTile);
                    //clickedTile.color = Color.blue;
                }
                else
                {
                    selectedUnit = null;
                }
            }

        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if(selectedUnit != null)
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

    Vector3Int GetGridPosition()
    {

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return grid.WorldToCell(mouseWorldPos);

    }

}
