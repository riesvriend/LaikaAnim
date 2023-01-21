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

    public bool UsesProgressBar
    {
        get => IsAppleVisible || IsCombVisible;
    }

    public PPRFlow Flow
    {
        get => GetComponentInParent<PPRFlow>();
    }

    public List<PPRTransition> NextTransitions
    {
        get => Flow.Transitions.Where(t => t.PreviousState == this).ToList();
    }
}
