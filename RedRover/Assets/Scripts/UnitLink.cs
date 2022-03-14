using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLink : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private GenericUnit unit;
    private GenericUnit linkedUnit;

    private bool currentlyLinked;

    // Start is called before the first frame update
    void Awake()
    {
        unit = GetComponent<GenericUnit>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.10f;
        lineRenderer.endWidth = 0.10f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }

    public void SetLink(Transform other) 
    {
        lineRenderer.enabled = true;
        StartCoroutine(FollowTransform(other));
    }

    public bool AlreadyConnected()
    {
        return lineRenderer.enabled;
    }

    public void HideLink()
    {
        lineRenderer.enabled = false;
    }

    private IEnumerator FollowTransform(Transform other)
    {
        while (lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, other.position);

            yield return new WaitForEndOfFrame();
        }
    }

}
