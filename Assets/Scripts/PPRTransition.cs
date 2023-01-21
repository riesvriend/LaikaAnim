using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[AddComponentMenu("PPR/Flow/PPR Transition"), DisallowMultipleComponent]
public class PPRTransition : MonoBehaviour
{
    public List<AnimalDef> AnimalsToAdd;
    public PPRState PreviousState;
    public PPRState NextState;

    [UnityEngine.Header("Conditions")]
    public bool IsTimeout;

    public PPRFlow Flow
    {
        get => GetComponentInParent<PPRFlow>();
    }
}
