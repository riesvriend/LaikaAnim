using Synchrony;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerPetsRescue
{
    // The UI-model for the status that is displayed on the wooden plank on the side
    // of the playground
    // TODO:
    // Display a list of tasks, for each animal that needs brushing, so how much brushing is needed
    public class ProgressModel
    {
        public class ProgressBarModel
        {
            public bool IsVisible;
            public string Title;
            public string ProgressBarText;
            public float Percentage;
        }

        public ProgressBarModel ScoreProgress = new ProgressBarModel { };
        public ProgressBarModel TimeProgress = new ProgressBarModel { };
        public ProgressBarModel TaskProgress = new ProgressBarModel { };
        //public string TaskTitle { get; internal set; } = "Pak kam of appel";
        //public string TaskProgressBarTitle { get; set; } = "";
        //private bool _isVisible;
        //public bool IsVisible
        //{
        //    get => _isVisible;
        //    set { _isVisible = value; }
        //}
        //public float Percentage { get; set; } = 100f;

        //public int Medals { get; set; }

        //internal void AddMedal()
        //{
        //    Medals++;
        //}
    }

    public class PlankUI : MonoBehaviour
    {
        public class ProgressUI
        {
            public VisualElement Container;
            public Label Label;
            public ProgressBar ProgressBar;
        }

        public ProgressModel ProgressModel { get; private set; } = new ProgressModel { };

        private VisualElement root;

        private ProgressUI scoreProgressUI;
        private ProgressUI timeProgressUI;
        private ProgressUI taskProgressUI;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            root = doc.rootVisualElement;

            scoreProgressUI = GetProgressUI("ScoreContainer");
            timeProgressUI = GetProgressUI("TimeContainer");
            taskProgressUI = GetProgressUI("TaskContainer");
        }

        private ProgressUI GetProgressUI(string containerName)
        {
            var ui = new ProgressUI { Container = root.Q<VisualElement>(containerName) };
            ui.Label = ui.Container.Q<Label>("ProgressLabel");
            ui.ProgressBar = ui.Container.Q<ProgressBar>("ProgressBar");
            ui.ProgressBar.lowValue = 0f;
            ui.ProgressBar.highValue = 100f;
            return ui;
        }

        // Update is called once per frame
        // TODO: apply updates to the UI elements in an event handler that fires when the ProgressModel is updated,
        // or in the next frame after that update, so that we batch the updates
        void Update()
        {
            UpdateUI(scoreProgressUI, ProgressModel.ScoreProgress);
            UpdateUI(timeProgressUI, ProgressModel.TimeProgress);
            UpdateUI(taskProgressUI, ProgressModel.TaskProgress);
        }

        private void UpdateUI(ProgressUI ui, ProgressModel.ProgressBarModel progress)
        {
            if (!progress.IsVisible)
            {
                ui.Container.style.display = DisplayStyle.None;
            }
            else
            {
                ui.Container.style.display = DisplayStyle.Flex;

                ui.Label.text = progress.Title;
                ui.ProgressBar.title = progress.ProgressBarText;
                ui.ProgressBar.value = progress.Percentage;
            }
        }
    }
}
