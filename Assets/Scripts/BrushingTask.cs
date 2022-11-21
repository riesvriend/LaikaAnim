using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class BrushingTask : MonoBehaviour
    {
        public const int StrokesPerGoldMedal = 5;
        
        // Track bushing progress
        public Dictionary<AnimalInstance, StrokingStatus> animalStrokingStatus = new();

        // Show stroke progress and target on menu

        // Show strok progress on animal (how?)

        // Play a brushing sound when brushing
    }

    public class StrokingStatus
    {
        public AnimalInstance animal;
        public int strokeCount;
    }
}
