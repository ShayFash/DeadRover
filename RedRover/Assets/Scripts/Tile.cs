using UnityEngine;

public class Tile : MonoBehaviour
{
    private Controller controller;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        controller = FindObjectOfType<Controller>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    private void OnMouseDown()
    {
        controller.TileClicked(transform.position);
    }
}
