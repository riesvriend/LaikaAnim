using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System;
using System.Collections;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    /// <summary>Class to identify stances on the Animal Controller </summary>
    [System.Serializable]
    public class Stances 
    {
        [Tooltip("ID value for the Stance")]
        public StanceID ID;

        [Tooltip("Enable Disable the Stance")]
        public BoolReference Active = new BoolReference(true);

        [Tooltip("Unique Input to play for each Ability")]
        public StringReference Input;

        [Tooltip("Lock the Stance if its Active. It cannot exit the Stance unless is Unlocked")]
        public BoolReference Locked = new BoolReference();

        [Tooltip("This Stance allows Straffing?")]
        public BoolReference CanStrafe = new BoolReference();

        [Tooltip("When the Stance Exit, it cannot be activated again until the cooldown time elapsed")]
        public FloatReference CoolDown = new FloatReference(0);

        [Tooltip("Is/IsNOT active State on this list")]
        public bool activeState = true;

        /// <summary>Include/Exclude the  States on this list depending the Affect variable</summary>
        [Tooltip("Include/Exclude the  States on this list depending the Affect variable")]
        public List<StateID> affectStates = new List<StateID>();


        public UnityEvent OnStanceEnter = new UnityEvent();
        public UnityEvent OnStanceExit = new UnityEvent();
    }
}
