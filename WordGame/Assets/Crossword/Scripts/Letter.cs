using UnityEngine;
using System.Collections;
using TMPro;

public class Letter : MonoBehaviour
{
    public TextMeshPro letter;
    public bool used = false;
    public bool solved = false;
    [HideInInspector]
    public int horizontal, vertical;

    void Start()
    {
        GetComponent<Renderer>().materials[0].color = Crossword.Instance.defaultColor;
    }
    /*
    void OnMouseOver()
    {
        if (Crossword.Instance.ready)
        {
            if (!used)
            {
                Crossword.Instance.selected.Add(this.gameObject);
                renderer.materials[0].color = Crossword.Instance.selectedColor;
                Crossword.Instance.selectedWord += letter.text;
                used = true;
            }
        }
    }
    */
    void Update()
    {
        if (Crossword.Instance.ready)
        {
            if (!used && Crossword.Instance.current==gameObject)
            {
                Crossword.Instance.selected.Add(this.gameObject);
                GetComponent<Renderer>().materials[0].color = Crossword.Instance.selectedColor;
                Crossword.Instance.selectedWord += letter.text;
                used = true;
            }
        }


        if (solved)
        {
            if (GetComponent<Renderer>().materials[0].color != Crossword.Instance.correctColor)
                GetComponent<Renderer>().materials[0].color = Crossword.Instance.correctColor;

            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            used = false;
            if (GetComponent<Renderer>().materials[0].color != Crossword.Instance.defaultColor)
                GetComponent<Renderer>().materials[0].color = Crossword.Instance.defaultColor;
        }
    }
}
