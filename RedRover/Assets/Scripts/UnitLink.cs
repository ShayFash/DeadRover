using System.Collections;
using UnityEngine;

public class UnitLink : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
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
