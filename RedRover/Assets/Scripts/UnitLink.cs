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

        Material material = new Material(Shader.Find("Shader Graphs/NewLink"));
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
            lineRenderer.material.SetColor("_Color", Color.yellow);
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
        Vector3 unit1TilePosition = unit1.GetTilePosition();
        Vector3 unit2TilePosition = unit2.GetTilePosition();

        float tileDistance = 0;
        tileDistance += Mathf.Abs(unit1TilePosition[0] - unit2TilePosition[0]);
        tileDistance += Mathf.Abs(unit1TilePosition[2] - unit2TilePosition[2]);

        bool unitsActive = unit1.IsActive() && unit2.IsActive();
        return lineRenderer.enabled && unit1.CompareTag(unit2.tag) && unitsActive && tileDistance <= MaxTileDistance;
    }

    private IEnumerator FollowTransform()
    {
        unit1.linked = true;
        unit2.linked = true;
        if (LinkConditionsMet())
        {
            unit1.Buff(unit2.BaseAttack, unit2.BaseMovement, Math.Max(unit2.BaseReach, unit1.BaseReach));
            unit2.Buff(unit1.BaseAttack, unit1.BaseMovement, Math.Max(unit2.BaseReach, unit1.BaseReach));
        }

        while (LinkConditionsMet())
        {
            lineRenderer.SetPosition(0, unit1.transform.position);
            lineRenderer.SetPosition(1, unit2.transform.position);

            yield return new WaitForEndOfFrame();
        }
        unit1.RemoveBuff();
        unit2.RemoveBuff();
        unit1.linked = false;
        unit2.linked = false;


        HideLink();
    }
}
