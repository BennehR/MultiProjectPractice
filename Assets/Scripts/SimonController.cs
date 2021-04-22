using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonController : MonoBehaviour
{
    [SerializeField]
    UICollection ui;
    
    private bool isGameRunning = true;
    private List<int> gameSequence = new List<int>();
    private int sequencePosition = 0;
    private Coroutine gameCoroutine;
    private Dictionary<string, List<int[]>> Patterns = new Dictionary<string, List<int[]>>();

    public int score = 0;
    public List<GameObject> gameButtons;
    
    

    // Start is called before the first frame update
    void Start()
    {
        ui.Hide();

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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toggleUI();
        }

        if (!isGameRunning)
        {
            StopCoroutine(gameCoroutine);
            isGameRunning = true;
        }
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
                        print($"Flash {gameButtons[pos].name.ToString()}");
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
        for(int i = 0; i < gameSequence.Count; i++)
        {
            List<int[]> lightSequence = new List<int[]>() { new int[] { 0, 0, 0, 0 } };
            lightSequence[0][gameSequence[i]] = 1;
            yield return StartCoroutine(SetLights(false, 1, lightSequence, 0.05f));
        }
    }                        

    private IEnumerator RunGame()
    {
        print("Game Started");
        yield return StartCoroutine(PowerOn());
        BuildSequence();
        yield return StartCoroutine(PlaySequence());

        yield return new WaitForEndOfFrame();
    }

    private void GameReset()
    {
        gameSequence.Clear();
        sequencePosition = 0;
    }

    private void toggleUI()
    {
        if (ui.Visible())
        {
            ui.Hide();
            Time.timeScale = 0;
            //print("UI visible, hiding");
        }
        else
        {
            Time.timeScale = 0;
            ui.Show();
            //print("UI hidden, showing");
        }
    }
    public void ButtonResume()
    {
        toggleUI();
    }
}
