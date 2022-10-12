using MalbersAnimations;
using MalbersAnimations.Controller;
using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PPR
{
    public class PettableRabbit : PettableAnimal
    {
        // Properties below from Zone.cs in Malbers
        /// <summary>ID of the Zone regarding the Type of Zone(State,Stance,Mode) </summary> 
       
        public MAction ActionID;
        public ModeID modeID;

        private MAnimal animal;

        private void Awake()
        {
            animal = GetComponent<MAnimal>();
        }

        public override void OnStartPetting()
        { 
            var mode = animal.Mode_Get(modeID);
            mode.ResetMode(); // Needed to ensure we clear the 'sleep' mode playing flag
            animal.Mode_Activate(modeID, ActionID.ID);
        }

        public override void OnStopPetting()
        {
            animal.Mode_Interrupt(); // clears the ability ID ('sleep')
            animal.Mode_Stop(); // clears the 'sleep' mode
        }
    }
}
