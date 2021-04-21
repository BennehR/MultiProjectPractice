using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonController : MonoBehaviour
{
    [SerializeField]
    UICollection ui;

    // Start is called before the first frame update
    void Start()
    {
        ui.Hide();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toggleUI();
        }
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
