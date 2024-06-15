using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class Crossword : MonoBehaviour {
    public bool useDictionary;                      // Should we use the dictionary?
    public TextAsset dictionary;                    // If useDictionary is true, this is the dictionary that will be used.
    public string[] words;                          // Define words that should show up in crossword. It's overwritten with dictionary words id useDictionary is true.
    public int maxWordsNumber;                      // Maximum number of words that will shop up in crossword. Ignored if useDictionary is false.
    public int maxWordLength;                       // Maximum length of the single word that will shop up in crossword. Ignored if useDictionary is false.
    public bool hint, allowInverse;                 // If hint is true, letters of the words listed will be in upper case. if allowInverse is true, wors can be selected in reverse order.
    public int horizontal, vertical;                // defines the size of the grid.
    public float sensibility;                       // How much is the tile sensible on the mouse over.
    public float spacing;                           // Spacing between the tiles
    public GameObject tile, background, current;             
    public Color defaultColor, selectedColor, correctColor;
    [HideInInspector]
    public bool ready = false, correct = false;
    public string selectedWord = "";
    
    public List<GameObject> selected = new List<GameObject>();

    private List<GameObject> tiles = new List<GameObject>();
    private GameObject temp, backgroundObj;
    private int solved = 0;
    private float startTime, endTime;
    private string[,] wordMatrix;
    private Dictionary<string, bool> _words = new Dictionary<string, bool>();
    public Dictionary<string, bool> placedWords = new Dictionary<string, bool>();
    private string[] letters = new string[26] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "z", "x", "y", "w" };
    private Ray ray;
    private RaycastHit hit;
    private int marker = 0;

    public TextMeshProUGUI hint1, hint2, hint3, answer;
    public GameObject solvedGO;
    
    private static Crossword instance;
    public static Crossword Instance
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

    void Start()
    {
        
        
       
       
        List<string> fetchLength = new List<string>();
        int counter = 0;

        if (useDictionary)
        {
            words = dictionary.text.Split(';');
            
        }
        else
        {
            /*for (int i = 0; i < 4; i++)
            {
                words[i] = PlayerInput.Instance.acceptedWords[i]; 
            }*/

            words[0] = "Apple";
            words[1] = "Banana";
            words[2] = "Orange";
            words[3] = "Fruits";
            words[4] = "Fruits";
            words[5] = "Fruitsss";
            words[6] = "Fruits";
            words[7] = "Fruits";
            words[8] = "Fruits";

            maxWordsNumber = 4;
        }

        maxWordsNumber = 4;
        
        if (maxWordsNumber <= 0)
        {
            maxWordsNumber = 1;
        }

        Shuffle(words);

        for (int i = 0; i < 4; i++)
        {
            words[i] = PlayerInput.Instance.acceptedWords[i]; 
        }
        
        Mathf.Clamp(maxWordLength, 0, vertical < horizontal ? horizontal : vertical);
       
        while (fetchLength.Count < maxWordsNumber + 1)
        {
            if (words[counter].Length <= maxWordLength)
            {
                fetchLength.Add(words[counter]);
            }
            counter++;
        }

        for (int i = 0; i < maxWordsNumber; i++)
        {
            if (!_words.ContainsKey(fetchLength[i].ToUpper()) && !_words.ContainsKey(fetchLength[i]))
            {
                if (hint)
                    _words.Add(fetchLength[i].ToUpper(), false);
                else
                    _words.Add(fetchLength[i], false);
            }
        }

        Mathf.Clamp01(sensibility);

        wordMatrix = new string[horizontal, vertical];

        // Instantiate background
        InstantiateBackground();

        //Instantiate tiles
        for (int i = 0; i < horizontal; i++)
        {
            for (int j = 0; j < vertical; j++)
            {
                temp = Instantiate(tile, new Vector3(i * 1 * tile.transform.localScale.x * spacing, 10, j * 1 * tile.transform.localScale.z * spacing), Quaternion.identity) as GameObject;
                temp.name = "tile_" + i.ToString() + "_" + j.ToString();
                temp.transform.eulerAngles = new Vector3(180, 0, 0);
                temp.transform.parent = backgroundObj.transform;
                
                BoxCollider boxCollider = temp.GetComponent<BoxCollider>() as BoxCollider;
                boxCollider.size = new Vector3(sensibility, 1, sensibility);
                temp.GetComponent<Letter>().letter.text = "";
                temp.GetComponent<Letter>().horizontal = i;
                temp.GetComponent<Letter>().vertical = j;
                tiles.Add(tile);
                wordMatrix[i, j] = "";
            }
        }

        // Center the background and tiles to the middle of the screen
        CenterBackground();

        PlaceWords();
        FillRest();

        startTime = Time.time;
    }

    // Center the whole game
    private void CenterBackground()
    {
        backgroundObj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2-40, (Screen.height / 2) -230, 200));
    }

    private void InstantiateBackground()
    {
        if (horizontal % 2 != 0 && vertical % 2 == 0)
            backgroundObj = Instantiate(background, new Vector3((tile.transform.localScale.x * spacing) * (horizontal / 2), 1, (tile.transform.localScale.z * spacing) * (vertical / 2) - (tile.transform.localScale.z * spacing)), Quaternion.identity) as GameObject;
        else if (horizontal % 2 == 0 && vertical % 2 != 0)
            backgroundObj = Instantiate(background, new Vector3((tile.transform.localScale.x * spacing) * (horizontal / 2) - (tile.transform.localScale.x * spacing), 1, (tile.transform.localScale.z * spacing) * (vertical / 2)), Quaternion.identity) as GameObject;
        else
            backgroundObj = Instantiate(background, new Vector3((tile.transform.localScale.x * spacing) * (horizontal / 2) - (tile.transform.localScale.x * spacing), 1, (tile.transform.localScale.z * spacing) * (vertical / 2) - (tile.transform.localScale.z * spacing)), Quaternion.identity) as GameObject;

        backgroundObj.transform.eulerAngles = new Vector3(180, 0, 0);
        backgroundObj.transform.localScale = new Vector3(((tile.transform.localScale.x * spacing) * horizontal), 1, ((tile.transform.localScale.x * spacing) * vertical));
   }

    void Update()
    {
        // We have started with selecting
        if (Input.GetMouseButton(0) )
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // try to raycast an image...
            if (Physics.Raycast(ray, out hit))
            {
                // ...and select it if it's hit
                current = hit.transform.gameObject;
            }
            ready = true;
        }

        // Check the selected word
        if (Input.GetMouseButtonUp(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // try to raycast an image...
            if (Physics.Raycast(ray, out hit))
            {
                // ...and select it if it's hit
                current = hit.transform.gameObject;
            }
            Check();
        }

        // same thing,but for mobile devices
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    ready = true;
                }
            }

            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Ended)
                {
                    Check();
                }
            }
        }
    }

    // Check the selected word
    private void Check()
    {
        if (!correct)
        {
            foreach (KeyValuePair<string, bool> p in placedWords)
            {
                if (selectedWord.ToLower() == p.Key.Trim().ToLower())
                {
                    foreach (GameObject g in selected)
                    {
                        g.GetComponent<Letter>().solved = true;
                    }
                    correct = true;
                }
                if (allowInverse)
                {
                    if (Inverse(selectedWord.ToLower()) == p.Key.Trim().ToLower())
                    {
                        foreach (GameObject g in selected)
                        {
                            g.GetComponent<Letter>().solved = true;
                        }
                        correct = true;
                    }
                }
            }
        }

        if (correct)
        {
            placedWords.Remove(selectedWord);
            placedWords.Remove(Inverse(selectedWord));

            if (_words.ContainsKey(selectedWord))
                placedWords.Add(selectedWord, true);
            else if (_words.ContainsKey(Inverse(selectedWord)))
                placedWords.Add(Inverse(selectedWord), true);

            solved++;
        }

        if (solved == placedWords.Count)
            endTime = Time.time;

        ready = false;
        selected.Clear();
        selectedWord = "";
        correct = false;
    }

    private void PlaceWords()
    {
        System.Random rn = new System.Random();
        foreach (KeyValuePair<string, bool> p in _words)
        {
            string s = p.Key.Trim();
            bool placed = false;
            // continue trying to place the word in
            // the matrix until it fits
            while (placed == false)
            {
                // generate a new random row and column
                int row = rn.Next(horizontal);
                int column = rn.Next(vertical);
                // generate a new random x & y direction vector
                // x direction: -1, 0, or 1
                // y direction -1, 0, or 1
                // (although direction can never be 0, 0, this is null)
                int dirX = 0;
                int dirY = 0;
                while (dirX == 0 && dirY == 0)
                {
                    dirX = rn.Next(3) - 1;
                    dirY = rn.Next(3) - 1;
                }
                // try to place the word in the random row, column and x, y direction
                if (hint)
                {
                    placed = PlaceWord(s.ToUpper(), row, column, dirX, dirY);
                }
                else
                {
                    placed = PlaceWord(s.ToLower(), row, column, dirX, dirY);
                }
                marker++;
                if (marker > 500)
                {
                    break;
                }
            }
        }
    }

    private bool PlaceWord(string word, int row, int col, int dirx, int diry)
    {
        // check if spot is taken or too big
        if (dirx > 0)
        {
            if (row + word.Length >= horizontal)
            {
                return false;
            }
        }
        if (dirx < 0)
        {
            if (row - word.Length < 0)
            {
                return false;
            }
        }
        if (diry > 0)
        {
            if (col + word.Length >= vertical)
            {
                return false;
            }
        }
        if (diry < 0)
        {
            if (col - word.Length < 0)
            {
                return false;
            }
        }

        if (((0 * diry) + col) == vertical - 1)
            return false;

        // now check if overlap
        for (int i = 0; i < word.Length; i++)
        {
            if (!string.IsNullOrEmpty(wordMatrix[(i * dirx) + row, (i * diry) + col]))
                return false;
        }

        // place word
        placedWords.Add(word, false);
        char[] _w = word.ToCharArray();
        for (int i = 0; i < _w.Length; i++)
        {
            wordMatrix[(i * dirx) + row, (i * diry) + col] = _w[i].ToString();
            GameObject.Find("tile_" + ((i * dirx) + row).ToString() + "_" + ((i * diry) + col).ToString()).GetComponent<Letter>().letter.text = _w[i].ToString();
        }

        return true;
    }

    // When all words are placed, fill the rest with random letters;
    private void FillRest()
    {
        for (int i = 0; i < horizontal; i++)
        {
            for (int j = 0; j < vertical; j++)
            {
                if (wordMatrix[i, j] == "")
                {
                    wordMatrix[i, j] = letters[UnityEngine.Random.Range(0, letters.Length)];
                    GameObject.Find("tile_" + i.ToString() + "_" + j.ToString()).GetComponent<Letter>().letter.text = wordMatrix[i, j];
                }
            }
        }
    }

    // Shuffles elements in string array
    private void Shuffle(string[] words)
    {
        for (int t = 0; t < words.Length; t++)
        {
            string tmp = words[t];
            int r = UnityEngine.Random.Range(t, words.Length);
            words[t] = words[r];
            words[r] = tmp;
        }
    }

    // Returns time since game started
    private string CurrentTime()
    {
        TimeSpan t = TimeSpan.FromSeconds(Mathf.RoundToInt(Time.time - startTime));
        return String.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

    // Returns game finish time
    private string FinalTime()
    {
        TimeSpan t = TimeSpan.FromSeconds(Mathf.RoundToInt(endTime - startTime));
        return String.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

    // Inverts the selected word
    private string Inverse(string word)
    {
        string result = "";
        char[] letters = word.ToCharArray();
        for (int i = letters.Length - 1; i >= 0; i--)
        {
            result += letters[i];
        }
        return result;
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        // Show message the the memory game is solved and time 
        if (solved == placedWords.Count)
        {
            solvedGO.SetActive(true);
            //GUI.Box(new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 15, 200, 30), "Solved!");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Time ");
            GUILayout.Label(FinalTime());
            GUILayout.EndHorizontal();
        }
        else
        {
            // Show time since game started
            GUILayout.BeginHorizontal();
            GUILayout.Label("Time ");
            GUILayout.Label(CurrentTime());
            GUILayout.EndHorizontal();
        }

        // Show each word. If it's found, it'll be marked with plus.
        foreach (KeyValuePair<string, bool> p in placedWords)
        {
            GUILayout.BeginHorizontal();
           // GUILayout.Label(p.Key);
           
           if (p.Value && p.Key.ToLower() == PlayerInput.Instance.hintsFields[0].text.ToLower())
           {
               hint1.text ="1. " + p.Key;
               //GUILayout.Label("+");
           }
           if (p.Value && p.Key.ToLower() == PlayerInput.Instance.hintsFields[1].text.ToLower())
           {
               hint2.text ="2. " + p.Key;
               //GUILayout.Label("+");
           }
           if (p.Value && p.Key.ToLower() == PlayerInput.Instance.hintsFields[2].text.ToLower())
           {
               hint3.text ="3. " + p.Key;
               //GUILayout.Label("+");
           }
           if (p.Value && p.Key.ToLower() == PlayerInput.Instance.answerField.text.ToLower())
           {
               answer.text ="Ans. " + p.Key;
               //GUILayout.Label("+");
           }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    public void Solved()
    {
        SceneManager.LoadScene(0);
    }
}