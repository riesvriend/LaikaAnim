using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Power Pets Rescue gameflow / workflow
/// </summary>
[AddComponentMenu("PPR/Flow/PPR Flow"), DisallowMultipleComponent]
public class PPRFlow : MonoBehaviour
{
    public List<PPRStartTransition> StartTransitions;
    public int GameDurationSeconds;

    public PPRTransition[] Transitions
    {
        get => GetComponentsInChildren<PPRTransition>();
    }

    public bool IsScored
    {
        get { return true; }
    }
}
