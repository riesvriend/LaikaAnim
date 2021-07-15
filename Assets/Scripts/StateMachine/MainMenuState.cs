using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synchrony;
using System;

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

    public void ExitState()
    {
        mainMenuCanvas.SetActive(false);
    }

    public void ExecuteState()
    {
        "<<< MainMenuState ExecuteState".Log();
    }
}
