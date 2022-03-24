using System;
using System.Collections;
using UnityEngine;

public class UnitLink : MonoBehaviour
{
    public int MaxTileDistance = 2;
    private LineRenderer lineRenderer;

    private GenericUnit unit1;
    private GenericUnit unit2;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        Material material = new Material(Shader.Find("Shader Graphs/Link"));
        material.SetFloat("_Tiling", 0.25f);
        material.SetFloat("_Speed", 0.5f);

        lineRenderer.material = material;
    }

    public void SetLink(GenericUnit unit1, GenericUnit unit2) 
    {
        this.unit1 = unit1;
        this.unit2 = unit2;
        if (unit1.CompareTag("Living"))
        {
            lineRenderer.material.SetColor("_Color", Color.white);
        } else if (unit1.CompareTag("Dead"))
        {
            lineRenderer.material.SetColor("_Color", new Color(0.21961f, 0.05490f, 0.27451f));
        }
        lineRenderer.enabled = true;
        StartCoroutine(FollowTransform());
    }

    public bool AlreadyConnected()
    {
        return lineRenderer.enabled;
    }

    public void HideLink()
    {
        lineRenderer.enabled = false;
    }

    private bool LinkConditionsMet()
    {
        Vector3Int unit1TilePosition = unit1.GetTilePosition();
        Vector3Int unit2TilePosition = unit2.GetTilePosition();

        int tileDistance = 0;
        for (int d = 0; d <= 1; d++)
        {
            tileDistance += Mathf.Abs(unit1TilePosition[d] - unit2TilePosition[d]);
        }

        bool unitsActive = unit1.IsActive() && unit2.IsActive();
        return lineRenderer.enabled && unit1.CompareTag(unit2.tag) && unitsActive && tileDistance <= MaxTileDistance;
    }

    private IEnumerator FollowTransform()
    {
        if (LinkConditionsMet())
        {
            unit1.Buff(unit2.Attack, unit2.Movement, Math.Max(unit2.Reach, unit1.Reach));
            unit2.Buff(unit1.Attack, unit1.Movement, Math.Max(unit2.Reach, unit1.Reach));
        }

        while (LinkConditionsMet())
        {
            lineRenderer.SetPosition(0, unit1.transform.position);
            lineRenderer.SetPosition(1, unit2.transform.position);

            yield return new WaitForEndOfFrame();
        }
        unit1.RemoveBuff();
        unit2.RemoveBuff();
        HideLink();
    }
}
