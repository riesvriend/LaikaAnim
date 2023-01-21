using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Power Pets Rescue gameflow / workflow
/// </summary>
[AddComponentMenu("PPR/Flow/PPR Flow"), DisallowMultipleComponent]
public class PPRFlow : MonoBehaviour
{
    public List<PPRStartTransition> StartTransitions;

    public PPRTransition[] Transitions
    {
        get => GetComponentsInChildren<PPRTransition>();
    }
}
