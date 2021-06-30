using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synchrony;

public class MainMenuState : IState
{
    private GameObject mainMenuCanvas;
    public GameObject MainMenuCanvas { get => mainMenuCanvas; set => mainMenuCanvas = value; }
    public MainMenuState(GameObject mainMenuCanvas)
    {
        this.mainMenuCanvas = mainMenuCanvas;
    }
    public void EnterState()
    {
        "<<< MainMenuState EnterState".Log();
        mainMenuCanvas.SetActive(true);
    }

    public void ExecuteState()
    {
        "<<< MainMenuState ExecuteState".Log();
    }

    public void ExitState()
    {
        "<<< MainMenuState ExitState".Log();
        mainMenuCanvas.SetActive(false);
    }
}
