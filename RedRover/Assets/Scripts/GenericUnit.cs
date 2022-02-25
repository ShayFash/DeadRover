using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class GenericUnit : MonoBehaviour
{
    public int Reach;
    public int Attack;
    public int Health;
    public int MaxHealth { get; protected set; }

    [SerializeField]
    protected int SwitchSidesCountdown;
    protected int NumTurnsToSwitchSides;
    protected int NumTimesSwitched = 0;
    protected bool SwitchingSides;


    protected Controller Controller;

    protected Tilemap Tilemap;
    protected TextMeshProUGUI CountdownText;

    protected void Init()
    {
        MaxHealth = Health;
        NumTurnsToSwitchSides = SwitchSidesCountdown;

        Controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Controller>();

        CountdownText = gameObject.GetComponentInChildren<TextMeshProUGUI>();

        Tilemap = FindObjectOfType<Tilemap>();

        Vector3Int myTilePosittion = Tilemap.layoutGrid.WorldToCell(transform.position);
        Vector3 alignedPosition = Tilemap.layoutGrid.GetCellCenterWorld(myTilePosittion);
        alignedPosition.y -= 0.1f;
        transform.position = alignedPosition;
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
        if (Health == 0)
        {
            // TODO: update visually
            SwitchingSides = true;
            SwitchSidesCountdown = NumTurnsToSwitchSides;
            Debug.Log("I'm starting to switch sides!");

            CountdownText.text = SwitchSidesCountdown.ToString();
            CountdownText.enabled = true;
        }
    }

    public bool CanBeAttacked()
    {
        return !SwitchingSides;
    }

    public void DecrementTurnTimers()
    {
        if (!SwitchingSides)
        {
            return;
        }

        SwitchSidesCountdown--;
        CountdownText.text = SwitchSidesCountdown.ToString();

        if (SwitchSidesCountdown <= 0)
        {
            // TODO: update visually
            NumTimesSwitched++;
            tag = CompareTag("Living") ? "Dead" : "Living";

            MaxHealth = Mathf.RoundToInt(MaxHealth * (1 - (0.25f * NumTimesSwitched)));
            Health = MaxHealth;

            CountdownText.enabled = false;

            Debug.Log("I've switched sides");

            if (MaxHealth == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public bool UnitInRange(GenericUnit unit)
    {
        Vector3Int myTilePosittion = Tilemap.layoutGrid.WorldToCell(transform.position);
        Vector3Int theirTilePosition = Tilemap.layoutGrid.WorldToCell(unit.transform.position);

        int tileDistance = 0;
        for (int i = 0; i <= 1; i++) {
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
