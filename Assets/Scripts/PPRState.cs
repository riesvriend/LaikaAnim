using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[AddComponentMenu("PPR/Flow/PPR State"), DisallowMultipleComponent]
public class PPRState : MonoBehaviour
{
    public List<PPRTransition> Transitions;
}
