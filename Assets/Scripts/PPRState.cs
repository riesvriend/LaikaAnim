using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[AddComponentMenu("PPR/Flow/PPR State")]
public class PPRState : MonoBehaviour
{
    public bool IsTableVisible;
    public bool IsAppleVisible;
    public bool IsCombVisible;

    private bool? isScored;
    private bool? hasFlow;
    private PPRFlow flow;

    private void Start()
    {
        flow = GetComponentInParent<PPRFlow>();
    }

    public bool UsesProgressBar
    {
        get => IsAppleVisible || IsCombVisible;
    }

    public bool IsScored
    {
        get
        {
            if (!isScored.HasValue)
            {
                if (Flow == null)
                    isScored = false;
                else
                    isScored = Flow.IsScored;
            }
            return isScored.Value;
        }
    }

    public PPRFlow Flow
    {
        get => flow;
    }

    public List<PPRTransition> NextTransitions
    {
        get => flow?.Transitions.Where(t => t.PreviousState == this).ToList();
    }
}
