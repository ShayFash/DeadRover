using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
 
public class Slideshow : MonoBehaviour
{
 
    public Texture2D[] imageArray;
    public RawImage image;
    private int currentImage;

    void Start()
    {
        currentImage = 0;
        image.texture = imageArray[currentImage];
    }

    public void clickedNext() {
        currentImage++;

        if(currentImage >= imageArray.Length)
            currentImage = 0;
        image.texture = imageArray[currentImage];
    }

    public void clickedBack() {
        currentImage--;

        if(currentImage < 0)
            currentImage = imageArray.Length-1;
        image.texture = imageArray[currentImage];
    }
}
