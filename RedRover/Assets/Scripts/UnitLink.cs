using System.Collections;
using UnityEngine;

public class UnitLink : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Coroutine activeCoroutine;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        Material material = new Material(Shader.Find("Shader Graphs/Link"));
        material.SetFloat("_Speed", 0.5f);
        // material.SetColor("_Color", Color.white);

        lineRenderer.material = material;
    }

    public void SetLink(Transform other) 
    {
        lineRenderer.loop = false;
        lineRenderer.positionCount = 2;
        lineRenderer.material.SetFloat("_Tiling", 0.25f);
        lineRenderer.textureMode = LineTextureMode.Tile;

        if (CompareTag("Living"))
        {
            lineRenderer.material.SetColor("_Color", Color.white);
        } else if (CompareTag("Dead"))
        {
            lineRenderer.material.SetColor("_Color", new Color(0.21961f, 0.05490f, 0.27451f));
        }
        lineRenderer.enabled = true;
        activeCoroutine = StartCoroutine(FollowTransform(other));
    }

    public bool AlreadyConnected()
    {
        return lineRenderer.enabled;
    }

    public void HideLink()
    {
        lineRenderer.enabled = false;
    }

    public void SwitchingSides()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        if (CompareTag("Dead"))
        {
            lineRenderer.material.SetColor("_Color", Color.white);
        }
        else if (CompareTag("Living"))
        {
            lineRenderer.material.SetColor("_Color", new Color(0.21961f, 0.05490f, 0.27451f));
        }

        DrawPolygon(128, 0.5f, transform.position);
        lineRenderer.material.SetFloat("_Tiling", 1f);
        lineRenderer.textureMode = LineTextureMode.Stretch;

        lineRenderer.enabled = true;
    }

    private void DrawPolygon(int vertexNumber, float radius, Vector3 centerPos)
    {
        lineRenderer.loop = true;
        float angle = 2 * Mathf.PI / vertexNumber;
        lineRenderer.positionCount = vertexNumber;

        Matrix4x4 rotationMatrix = new Matrix4x4(
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            );

        for (int i = 0; i < vertexNumber; i++)
        {
            rotationMatrix[0, 0] = Mathf.Cos(angle * i);
            rotationMatrix[0, 1] = Mathf.Sin(angle * i);

            rotationMatrix[1, 0] = -Mathf.Sin(angle * i);
            rotationMatrix[1, 1] = Mathf.Cos(angle * i);


            Vector3 initialRelativePosition = new Vector3(0, radius, 0);
            Vector3 point = centerPos + rotationMatrix.MultiplyPoint(initialRelativePosition);
            lineRenderer.SetPosition(i, point);

        }
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
