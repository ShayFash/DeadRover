using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class TurnStateManager : MonoBehaviour
{
    protected string SelectingUnit = "Selecting Enemy Unit";
    protected string Waiting = "Waiting";
    protected string Moving = "Moving";
    protected string Attacking = "Attacking";

    protected string ChoosingAction = "Choosing Action";

    public GameObject QuestionMessage;
    public GameObject AnswerMessage;

    public GameObject AlertMessage1;
    public GameObject AlertMessage2;
    public GameObject AlertMessage3;

    private TextMeshProUGUI LivingTurnState;
    private TextMeshProUGUI DeadTurnState;

    private TextMeshProUGUI AnswerText;

    private TextMeshProUGUI AlertText1;
    private TextMeshProUGUI AlertText2;
    private TextMeshProUGUI AlertText3;

    private GenericUnit SelectedUnit;

    private bool GameStarted;

    private int AlertCount;


    //----------Setup the appropiate gameObjects----------
    void Start()
    {
        TextMeshProUGUI[] childTexts = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        LivingTurnState = Array.Find(childTexts, delegate (TextMeshProUGUI t) { return t.CompareTag("LivingTurnState"); });
        DeadTurnState = Array.Find(childTexts, delegate (TextMeshProUGUI t) { return t.CompareTag("DeadTurnState"); });

        AnswerText = AnswerMessage.GetComponentInChildren<TextMeshProUGUI>();

        AlertText1 = AlertMessage1.GetComponentInChildren<TextMeshProUGUI>();
        AlertText2 = AlertMessage2.GetComponentInChildren<TextMeshProUGUI>();
        AlertText3 = AlertMessage3.GetComponentInChildren<TextMeshProUGUI>();

        AlertCount = 0;
    }
    //----------------------------------------------------


    //----------Show the current states between the two teams (turns)----------
    public void UpdateTurnStates(string currentPlayer, string currentState, GenericUnit selected) 
    {
        SelectedUnit = selected;

        if (currentPlayer.Equals("Living") && currentState.Equals("SelectingUnit")) 
        {
            SetState(LivingTurnState, "Waiting");
            SetState(DeadTurnState, "SelectingUnit");
            if (Time.timeScale != 0f) StartCoroutine(ShowSelectionQuestion());
        }
        else if (currentPlayer.Equals("Living") && currentState.Equals("Waiting")) 
        {
            SetState(LivingTurnState, "ChoosingAction");
            SetState(DeadTurnState, "Waiting");
            QuestionMessage.SetActive(false);
        }
        else if (currentPlayer.Equals("Living") && currentState.Equals("Moving")) 
        {
            SetState(LivingTurnState, "Moving");
            SetState(DeadTurnState, "Waiting");
            QuestionMessage.SetActive(false);
        }
        else if (currentPlayer.Equals("Living") && currentState.Equals("Attacking")) 
        {
            SetState(LivingTurnState, "Attacking");
            SetState(DeadTurnState, "Waiting");
            QuestionMessage.SetActive(false);
        }
        else if (currentPlayer.Equals("Dead") && currentState.Equals("SelectingUnit")) 
        {
            SetState(LivingTurnState, "SelectingUnit");
            SetState(DeadTurnState, "Waiting");
            if (Time.timeScale != 0f) StartCoroutine(ShowSelectionQuestion());
        }
        else if (currentPlayer.Equals("Dead") && currentState.Equals("Waiting")) 
        {
            SetState(LivingTurnState, "Waiting");
            SetState(DeadTurnState, "ChoosingAction");
            QuestionMessage.SetActive(false);
        }
        else if (currentPlayer.Equals("Dead") && currentState.Equals("Moving")) 
        {
            SetState(LivingTurnState, "Waiting");
            SetState(DeadTurnState, "Moving");
            QuestionMessage.SetActive(false);
        }
        else if (currentPlayer.Equals("Dead") && currentState.Equals("Attacking")) 
        {
            SetState(LivingTurnState, "Waiting");
            SetState(DeadTurnState, "Attacking");
            QuestionMessage.SetActive(false);
        }
    }
    
    private IEnumerator ShowSelectionQuestion()
    {
        GameStarted = true;
        QuestionMessage.SetActive(true);
        yield return new WaitForSeconds(3f);
        if (SelectedUnit != null) StartCoroutine(ShowSelectionAnswer(SelectedUnit.unitName));
    }

    private IEnumerator ShowSelectionAnswer(string unit)
    {
        AnswerText.text = "Dead rover...dead rover...that " + unit + " must come over!";
        AnswerMessage.SetActive(true);
        yield return new WaitForSeconds(3f);
        AnswerMessage.SetActive(false);
    }

    private void SetState(TextMeshProUGUI textPro, string state) 
    {
        if (state.Equals("SelectingUnit")) 
        {
            textPro.text = SelectingUnit;
        }
        else if (state.Equals("Waiting")) 
        {
            textPro.text = Waiting;
        }
        else if (state.Equals("Moving")) 
        {
            textPro.text = Moving;
        }
        else if (state.Equals("Attacking")) 
        {
            textPro.text = Attacking;
        }
        else if (state.Equals("ChoosingAction")) 
        {
            textPro.text = ChoosingAction;
        }
    }
    //---------------------------------------------------------------


    //----------Show alert message for individual units (animals)----------
    public void ShowAlert(string tag, string name, string state) 
    {
        string animal = tag + " " + name;

        string message = "";
        if (state == "Moved") 
        {
            message = "The " + animal + " is on the move!";
        }
        else if (state == "WasAttacked") 
        {
            message = "The " + animal + " has been injured!";
        }
        else if (state == "ChangingSides") 
        {
            if (tag == "Living") 
            {
                message = "The " + animal + " has been turned dead and will switch sides!";
            }
            else if (tag == "Dead")
            {
                message = "The " + animal + " has been revived and will switch sides!";
            }
        }
        else if (state == "WasKilled") 
        {
            message = "The " + animal + " has been killed, never to return!";
        }

        if (GameStarted) StartCoroutine(ShowAlertMessage(message));
    }

    private IEnumerator ShowAlertMessage(string aInfo)
    {
        AlertCount++;
        GameObject alert = new GameObject();
        if (AlertCount == 1) 
        {
            AlertText1.text = aInfo;
            alert = AlertMessage1;
        }
        else if (AlertCount == 2) 
        {
            AlertText2.text = aInfo;
            alert = AlertMessage2;
        }
        else if (AlertCount == 3) 
        {
            AlertText3.text = aInfo;
            alert = AlertMessage3;
        }

        alert.SetActive(true);
        yield return new WaitForSeconds(4f);
        alert.SetActive(false);
        AlertCount--;
    }
    //---------------------------------------------------------------
}