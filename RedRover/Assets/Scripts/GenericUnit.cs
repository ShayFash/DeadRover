using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class GenericUnit : MonoBehaviour
{
    public int Reach;
    public int Attack;
    public int Health;
    [HideInInspector]
    public int MaxHealth;

    protected Controller Controller;
    protected Tilemap Tilemap;

    protected void Init()
    {
        MaxHealth = Health;

        Controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Controller>();

        Tilemap = FindObjectOfType<Tilemap>();
    }


    public void AttackUnit(GenericUnit unit)
    {
        Debug.Log("Aaaaattack!");
        unit.TakeDamage(Attack);
    }

    public void TakeDamage(int value)
    {
        Debug.Log("I'm hurt");
        Health = Mathf.Max(0, Health - value);
    }

    public bool UnitInRange(GenericUnit unit)
    {
        // TODO: Use tiles
        Vector3Int myTilePosittion = Tilemap.layoutGrid.WorldToCell(transform.position);
        Vector3Int theirTilePosition = Tilemap.layoutGrid.WorldToCell(unit.transform.position);

        Debug.Log("My tile position: " + myTilePosittion.ToString());
        Debug.Log("Their tile position: " + theirTilePosition.ToString());

        int tileDistance = 0;
        for (int i = 0; i <= 2; i++) {
            tileDistance += Mathf.Abs(myTilePosittion[i] - theirTilePosition[i]);
        }

        return tileDistance <= Reach;
    }

    public IEnumerable<GenericUnit> UnitsInRange(IEnumerable<GenericUnit> units)
    {
        IEnumerable<GenericUnit> inReach = from unit in units where UnitInRange(unit) select unit;

        return  inReach;
    }

    private void OnMouseDown()
    {
        Controller.SelectUnit(this);
    }
}
