using PowerPetsRescue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class BrushingStatus : BaseStatus
    {
        public const int StrokesPerGoldMedal = 20;
        public int strokeCount;

        public override float Percentage
        {
            get { return ((float)strokeCount / StrokesPerGoldMedal) * 100f; }
        }

        public override void UpdateProgress(ProgressModel progress)
        {
            base.UpdateProgress(progress);

            progress.ProgressBarTitle = "Borstelen";
        }

        public override void IncrementProgress()
        {
            strokeCount++;
        }

        public override void ResetProgress()
        {
            strokeCount = 0;
        }

        public override bool IsCompleted()
        {
            return strokeCount >= StrokesPerGoldMedal;
        }
    }

    public class BrushingTask : BaseTask<BrushingStatus>
    {
        protected override void OnTaskCompleted()
        {
            game.TaskCompleted(this);
        }

        protected override void OnAddAnimal(AnimalInstance animal)
        {
            taskStatus.Add(animal, new BrushingStatus { animal = animal, strokeCount = 0 });
        }
    }
}
