using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAnimal : MonoBehaviour
{
    public Sprite oldImage;
    public Sprite Bear;
    public Sprite Deer;
    public Sprite Rabbit;
    public Sprite Fox;
    public Sprite Owl;
    
    public Sprite DBear;
    public Sprite DDeer;
    public Sprite DRabbit;
    public Sprite DFox;
    public Sprite DOwl;

    public void changeBear()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Bear;
    }
    public void changeDeer()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Deer;
    }
    public void changeRabbit()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Rabbit;
    }
    public void changeFox()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Fox;
    }
    public void changeOwl()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Owl;
    }

    public void changeDBear()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = DBear;
    }
    public void changeDDeer()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = DDeer;
    }
    public void changeDRabbit()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = DRabbit;
    }
    public void changeDFox()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = DFox;
    }
    public void changeDOwl()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = DOwl;
    }
    public void changeNull()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = oldImage;
    }
}



