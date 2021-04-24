using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimonController : MonoBehaviour
{
    [SerializeField]
    UICollection ui;
    
    private bool listenForPlayer = false;
    private int sequencePosition = 0;
    private int layerMask;
    private int score = 0;
    private List<int> gameSequence = new List<int>();
    private GameObject selectedButton;
    private AudioSource playingAudio;
    private Coroutine gameCoroutine;
    private Coroutine playerInputCoroutine;
    private Coroutine blinkCoroutine;
    private Dictionary<string, List<int[]>> Patterns = new Dictionary<string, List<int[]>>();

    public bool openSequence = false;
    public List<GameObject> gameButtons;
    public List<GameObject> allGreenLights;
    public List<GameObject> allRedLights;
    public Text scoreText;

    Ray ray;
    RaycastHit hitData;

    private string SequenceToString(List<int> sequence)
    {
        string output = "";
        for (int i = 0; i < sequence.Count; i++)
        {
            if (i == 0)
            {
                output = sequence[i].ToString();
            }
            else if (i < sequence.Count)
            {
                output += $", {sequence[i]}";
            }
        }

        return output;
    }

    private void Awake()
    {
        print("AWAKE");
        try
        {
            StopCoroutine(gameCoroutine);
            print("Game Coroutine stopped");
        }
        catch(System.Exception err)
        {
            print(err);
        }

        StartCoroutine(PowerOn());
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

        //gameCoroutine = StartCoroutine(PowerOn());
        //playerInputCoroutine = StartCoroutine(ListenerCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = $"Score: {score}";

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toggleUI();
        }

        //Raycast
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitData = Physics2D.Raycast(new Vector2(worldPosition.x, worldPosition.y), Vector2.zero, 0, layerMask);


        if (hitData && Input.GetMouseButtonDown(0)) //  layerMask   && Input.GetMouseButtonDown(0)
        {
            selectedButton = hitData.transform.gameObject;

            if (selectedButton != gameButtons[4])
            {
                if (listenForPlayer)
                {                    
                    StartCoroutine(BlinkOnce(selectedButton, 0.1f, false));

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
                            score++;
                            StartCoroutine(ProgressGame(true));
                        }
                    }
                    else
                    {
                        listenForPlayer = false;
                        print($"Incorrect - Ray Component - {selectedButton.name}");
                        StartCoroutine(ProgressGame(false));
                    }
                }
            }
            else
            {
                StartCoroutine(BlinkOnce(selectedButton, 0.5f, true));
                toggleUI();
            }
        }
    }

    private IEnumerator ProgressGame(bool doProgress)
    {
        listenForPlayer = false;
        yield return new WaitForSeconds(0.5f);

        if (doProgress)
        {
            //foreach (GameObject greenLight in allGreenLights)
            //{
            //    StartCoroutine(BlinkOnce(greenLight, true));
            //}

            yield return new WaitForSeconds(0.5f);

            sequencePosition = 0;
            BuildSequence();
            yield return PlaySequence();
        }
        else
        {
            foreach (GameObject redLight in allRedLights)
            {
                StartCoroutine(BlinkOnce(redLight, 0.5f, true));
                score = 0;
            }

            yield return new WaitForSeconds(0.5f);
            GameReset();
        }

        yield return new WaitForSeconds(0.5f);

        listenForPlayer = true;
    }

    private IEnumerator BlinkOnce(GameObject button, float length, bool silent)
    {
        button.GetComponent<ButtonHelper>().FlashPiece(silent);
        yield return new WaitForSeconds(length);
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
        if (openSequence)
        {
            print("Testing Button LED's...");
            //Start up
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(SetLights(false, 1, Patterns["Clockwise"], 0.1f));

            //Lightshow
            //yield return StartCoroutine(SetLights(true, 2, Patterns["CrissCross"]));
            yield return StartCoroutine(SetLights(true, 1, Patterns["FullOnOff"], 0.2f));
        }

        print("Game Powered Up");

        StartCoroutine(RunGame());
    }

    private void BuildSequence()
    {
        print("Building sequence");

        //for (int i = 0; i < 30; i++)
        //{
        //    gameSequence.Add(Random.Range(0, 4));
        //}

        gameSequence.Add(Random.Range(0, 4));

        //gameSequence = new List<int>() { 0, 1, 2, 3 };

        print($"Game Sequence={SequenceToString(gameSequence)}");
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < gameSequence.Count; i++)
        {
            List<int[]> lightSequence = new List<int[]>() { new int[] { 0, 0, 0, 0 } };
            lightSequence[0][gameSequence[i]] = 1;
            yield return StartCoroutine(SetLights(false, 1, lightSequence, 0.25f));
        }

        listenForPlayer = true;
    }

    private IEnumerator RunGame()
    {
        print("Game Started");
        //yield return StartCoroutine(PowerOn());
        BuildSequence();
        yield return StartCoroutine(PlaySequence());
    }

    private void GameReset()
    {
        StopCoroutine(RunGame());
        print("Game reset");
        gameSequence.Clear();
        sequencePosition = 0;
        StartCoroutine(RunGame());
    }

    private void toggleUI()
    {
        if (ui.Visible())
        {
            if (playingAudio)
            {
                playingAudio.Play();
                playingAudio = null;
            }
            Time.timeScale = 1;
            ui.Hide();
            listenForPlayer = true;
            //print("UI visible, hiding");
        }
        else
        {
            foreach (GameObject button in gameButtons)
            {
                if (button.GetComponent<AudioSource>().isPlaying)
                {
                    playingAudio = button.GetComponent<AudioSource>();
                    playingAudio.Pause();
                }
            }
            Time.timeScale = 0;
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
