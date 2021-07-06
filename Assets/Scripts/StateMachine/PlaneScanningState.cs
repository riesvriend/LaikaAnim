using UnityEngine;

// TODO: Inherit from PlaneScanningState so that we place the carpet on the floor
public class CarpetPlayState : IState
{
    private readonly GameObject canvas;

    public CarpetPlayState(GameObject canvas)
    {
        this.canvas = canvas;
    }

    public void EnterState()
    {
        canvas.SetActive(true);
    }

    public void ExecuteState()
    {
    }

    public void ExitState()
    {
        canvas.SetActive(false);
    }
}