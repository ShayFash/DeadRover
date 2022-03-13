using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLink : MonoBehaviour
{
    public GameObject link;
    public LineRenderer lineRenderer;

    private GenericUnit thisUnit;
    private GenericUnit linkedUnit;

    private bool currentlyLinked;

    // Start is called before the first frame update
    void Awake()
    {
        thisUnit = this.GetComponent<GenericUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlyLinked) 
        {
            link.SetActive(true);

            Vector3 startPosition = thisUnit.transform.position;
            Vector3 endPosition = linkedUnit.transform.position;

            //For creating line renderer object
            lineRenderer.startColor = thisUnit.CompareTag("Living") ? Color.green : Color.red;
            lineRenderer.endColor = thisUnit.CompareTag("Living") ? Color.green : Color.red;
            lineRenderer.startWidth = 0.10f;
            lineRenderer.endWidth = 0.10f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true; 

            //For drawing line in the world space, provide the x,y,z values
            lineRenderer.SetPosition(0, startPosition); 
            lineRenderer.SetPosition(1, endPosition); 
        } 
        else 
        {
            link.SetActive(false);
        }
    }

    public void SetLink(GenericUnit unit) 
    {
        if (unit == null) currentlyLinked = false;
        else currentlyLinked = true;
        linkedUnit = unit;
    }

    public bool IsLinked() 
    {
        return currentlyLinked;
    }

}
