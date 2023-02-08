using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    [Header("PlayerAInput")] 
    public TMP_InputField[] hintsFields;
    public TMP_InputField answerField;

    [Space(8)] 
    public string[] acceptedWords;

    [Space] public TextMeshProUGUI debugText;

    public GameObject player2Turn, player1Turn;
    private static PlayerInput instance;
    public static PlayerInput Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    public void EnterBtn()
    {
        for (int i = 0; i < 3; i++)
        {
            if (hintsFields[i].text == String.Empty || answerField.text == String.Empty)
            {
                debugText.text = "Make Sure all Field are Filled";
                return;
            }
            else
            {
                debugText.text = "";

                for (int j = 0; j < 4; j++)
                {
                    acceptedWords[i] = hintsFields[i].text;
                    
                    if (j == 3)
                        acceptedWords[j] = answerField.text;
                }
                
                player2Turn.SetActive(true);
                player1Turn.SetActive(false);
            }
                
        }
    }
}
