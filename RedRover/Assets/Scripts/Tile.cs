using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Controller controller;

    private void Start()
    {
        controller = FindObjectOfType<Controller>();
    }

    private void OnMouseDown()
    {
        controller.TileClicked(transform.position);
    }
}
