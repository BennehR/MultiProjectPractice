using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonController : MonoBehaviour
{
    [SerializeField]
    UICollection ui;
    
    private bool isGameRunning = true;
    private bool listenForPlayer = false;
    private int sequencePosition = 0;
    private int layerMask;
    private List<int> gameSequence = new List<int>();
    private Coroutine gameCoroutine;
    private Coroutine playerInputCoroutine;
    private Coroutine blinkCoroutine;
    private Dictionary<string, List<int[]>> Patterns = new Dictionary<string, List<int[]>>();

    public int score = 0;
    public List<GameObject> gameButtons;

    public GameObject selectedButton;
    Ray ray;
    RaycastHit hitData;

    private string SequenceToString(List<int> sequence)
    {
        string output = "";
        for (int i = 0; i < sequence.Count; i++)
        {
            if (i < sequence.Count - 1)
            {
                output = $"{output}, {sequence[i]}, ";
            }
            else if (i == sequence.Count - 1)
            {
                output = $"{output}, {sequence[i]}";
            }
        }

        return output;
    }

    // Start is called before the first frame update
    void Start()
    {
        ui.Hide();

        layerMask = LayerMask.GetMask("GamePieces");

        Patterns.Add("Clockwise", new List<int[]>
        {
            new int[] { 1, 0, 0, 0 },
            new int[] { 0, 1, 0, 0 },
            new int[] { 0, 0, 1, 0 },
            new int[] { 0, 0, 0, 1 }
        });

        Patterns.Add("CrissCross", new List<int[]>
        {
            new int[] { 1, 0, 1, 0 },
            new int[] { 0, 1, 0, 1 },
            new int[] { 1, 0, 1, 0 },
            new int[] { 0, 1, 0, 1 }
        });

        Patterns.Add("FullOnOff", new List<int[]>
        {
            new int[] { 1, 1, 1, 1 },
            new int[] { 0, 0, 0, 0 },
            new int[] { 1, 1, 1, 1 },
            new int[] { 0, 0, 0, 0 }
        });

        gameCoroutine = StartCoroutine(RunGame());
        //playerInputCoroutine = StartCoroutine(ListenerCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toggleUI();
        }

        //Raycast
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitData = Physics2D.Raycast(new Vector2(worldPosition.x, worldPosition.y), Vector2.zero, 0);

        
        if (hitData && Input.GetMouseButtonDown(0)) //  layerMask   && Input.GetMouseButtonDown(0)
        {
            selectedButton = hitData.transform.gameObject;

            

            if (selectedButton != gameButtons[4])
            {
                if (listenForPlayer)
                {
                    StartCoroutine(BlinkOnce(selectedButton));

                    if (selectedButton == gameButtons[gameSequence[sequencePosition]])
                    {
                        if (sequencePosition < gameSequence.Count)
                        {
                            print($"Correct - Ray Component - {selectedButton.name}");
                            print($"Sequence Position: {sequencePosition + 1} / {gameSequence.Count}");
                            sequencePosition++;
                        }

                        if (sequencePosition == gameSequence.Count)
                        {
                            print("This was the last piece in the sequence");
                            StartCoroutine(ProgressGame());
                        }
                    }
                    else
                    {
                        print($"Incorrect - Ray Component - {selectedButton.name}");
                        GameReset();
                    }
                }
            }
            else
            {
                StartCoroutine(BlinkOnce(selectedButton));
                toggleUI();
            }
        }
    }
    //TODO work the update into a coroutine?



    private IEnumerator ProgressGame()
    {
        listenForPlayer = false;

        //TODO Make something to flash all green maybe?

        sequencePosition = 0;
        print($"Game Sequence={SequenceToString(gameSequence)}");
        BuildSequence();
        print($"Game Sequence={SequenceToString(gameSequence)}");
        yield return PlaySequence();

        listenForPlayer = true;
    }

    private IEnumerator BlinkOnce(GameObject button)
    {
        button.GetComponent<ButtonHelper>().FlashPiece(false);
        yield return new WaitForSeconds(0.25f);
        button.GetComponent<ButtonHelper>().ResetPiece();
    }

    private IEnumerator SetLights(bool silent, int loops, List<int[]> pattern, float length)
    {
        //print($"Setting lights - {pattern}");
        for (int L = 0; L < loops; L++)
        {
            //print($"Loop {L}");
            foreach (int[] lightSequence in pattern)
            {
                //print($"Sequence {lightSequence[0]} {lightSequence[1]} {lightSequence[2]} {lightSequence[3]} ");
                for (int pos = 0; pos < 4; pos++)
                {
                    //print(pos);
                    if (lightSequence[pos] == 1)
                    {
                        gameButtons[pos].GetComponent<ButtonHelper>().FlashPiece(silent);
                        //print($"Flash {gameButtons[pos].name.ToString()}");
                    }
                }

                //print("Lights on");
                yield return new WaitForSeconds(length);

                foreach (GameObject button in gameButtons)
                {
                    button.GetComponent<ButtonHelper>().ResetPiece();
                }

                yield return new WaitForSeconds(length);
                //print("Lights off");
            }
        }
    }

    private IEnumerator PowerOn()
    {
        print("Testing Button LED's...");

        //Start up
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(SetLights(false, 1, Patterns["Clockwise"], 0.1f));

        //Lightshow
        //yield return StartCoroutine(SetLights(true, 2, Patterns["CrissCross"]));
        yield return StartCoroutine(SetLights(true, 1, Patterns["FullOnOff"], 0.2f));

        yield return new WaitForSeconds(1);
        print("Game Powered Up");
    }

    private void BuildSequence()
    {
        print("Building sequence");

        //for(int i = 0; i < 30; i++)
        //{
        //    gameSequence.Add(Random.Range(0, 4));
        //}

        gameSequence = new List<int>() { 0, 1, 2, 3 };

        string output = "";

        foreach(int i in gameSequence)
        {
            output += $"{i} ";
        }
        print($"Finished building sequence {output}");
    }

    private IEnumerator PlaySequence()
    {
        for (int i = 0; i < gameSequence.Count; i++)
        {
            List<int[]> lightSequence = new List<int[]>() { new int[] { 0, 0, 0, 0 } };
            lightSequence[0][gameSequence[i]] = 1;
            yield return StartCoroutine(SetLights(false, 1, lightSequence, 0.25f));
        }

        listenForPlayer = true;
    }
    
    //private IEnumerator WaitForPlayerSequence()
    //{
    //    sequencePosition = 0;

    //    while (isGameRunning)
    //    {
    //        while (listenForPlayer)
    //        {

    //        }
    //    }
    //}

    private IEnumerator RunGame()
    {
        print("Game Started");
        //yield return StartCoroutine(PowerOn());
        BuildSequence();
        yield return StartCoroutine(PlaySequence());

        yield return new WaitForEndOfFrame();
    }

    private void GameReset()
    {
        print("Game reset");
        gameSequence.Clear();
        StartCoroutine(ProgressGame());
    }

    private void toggleUI()
    {
        if (ui.Visible())
        {
            ui.Hide();
            listenForPlayer = true;
            //Time.timeScale = 0;
            //print("UI visible, hiding");
        }
        else
        {
            //Time.timeScale = 0;
            listenForPlayer = false;
            ui.Show();
            //print("UI hidden, showing");
        }
    }
    public void ButtonResume()
    {
        toggleUI();
    }
}
