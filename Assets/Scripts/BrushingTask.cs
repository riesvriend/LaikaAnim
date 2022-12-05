﻿using PowerPetsRescue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class StrokingStatus
    {
        public const int StrokesPerGoldMedal = 20;

        public AnimalInstance animal;
        public int strokeCount;

        public float Percentage
        {
            get { return ((float)strokeCount / StrokesPerGoldMedal) * 100f; }
        }
    }

    public class BrushingTask : MonoBehaviour
    {
        // Parent game that uses this task
        public GameInstance game;

        public GameObject Comb { get; internal set; }
        private CombTouchHaptics combTouchHaptics = null;

        // Track bushing progress
        private Dictionary<AnimalInstance, StrokingStatus> animalStrokingStatus = new();

        public AnimalInstance ActiveAnimal
        {
            get => game.activeAnimal;
            private set { game.SetActiveAnimal(value); }
        }

        private void Start()
        {
            if (Comb != null)
            {
                combTouchHaptics = Comb.GetComponentInChildren<CombTouchHaptics>();
                combTouchHaptics.OnStrokingStarted.AddListener(OnStrokingStarted);
                combTouchHaptics.OnStrokingStopped.AddListener(OnStrokingStopped);
            }
        }

        /// <summary>
        /// We dont rely on Destroy to prevent that we can access all the
        /// related objects
        /// </summary>
        public void Stop()
        {
            // needed to prevent Update() from re-activating the progress model again
            ActiveAnimal = null;
            if (combTouchHaptics != null)
            {
                combTouchHaptics.OnStrokingStarted.RemoveListener(OnStrokingStarted);
                combTouchHaptics.OnStrokingStopped.RemoveListener(OnStrokingStopped);
            }
            ProgressModel.IsVisible = false;
        }

        ProgressModel ProgressModel
        {
            get => game.playground.plankUI.ProgressModel;
        }

        private void Update()
        {
            var progress = ProgressModel;
            var status = ActiveStatus();
            if (status == null)
                progress.IsVisible = false;
            else
            {
                // Show stroke progress and target on menu
                progress.IsVisible = true;
                progress.TaskTitle = "Voortgang";
                progress.Percentage = status.Percentage;
                progress.ProgressBarTitle = "Borstelen";
            }
        }

        AnimalInstance FindAnimalInstance(GameObject animal)
        {
            return animalStrokingStatus.Keys.SingleOrDefault(a => a.gameObject == animal);
        }

        StrokingStatus ActiveStatus()
        {
            if (ActiveAnimal == null)
                return null;

            animalStrokingStatus.TryGetValue(ActiveAnimal, out var status);
            return status;
        }

        private void OnStrokingStarted(CombTouchHaptics.StrokeEvent e)
        {
            ActiveAnimal = FindAnimalInstance(e.Animal);

            var status = ActiveStatus();
            status.strokeCount++;

            if (status.strokeCount >= StrokingStatus.StrokesPerGoldMedal)
            {
                status.strokeCount = 0;
                game.TaskCompleted(this);
            }
        }

        private void OnStrokingStopped(CombTouchHaptics.StrokeEvent e) { }

        internal void AddAnimal(AnimalInstance animal)
        {
            animalStrokingStatus.Add(
                animal,
                new StrokingStatus { animal = animal, strokeCount = 0 }
            );
        }
    }
}
