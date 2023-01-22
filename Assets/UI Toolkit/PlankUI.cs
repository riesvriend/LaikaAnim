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
        public string TaskTitle { get; internal set; } = "Pak kam of appel";
        public string ProgressBarTitle { get; set; } = "";
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; }
        }
        public float Percentage { get; set; } = 100f;

        public int Medals { get; set; }

        internal void AddMedal()
        {
            Medals++;
        }
    }

    public class PlankUI : MonoBehaviour
    {
        public ProgressModel ProgressModel { get; private set; } =
            new ProgressModel { IsVisible = true };

        private VisualElement root;

        private VisualElement progressContainer;
        private Label progressLabel;
        private ProgressBar progressBar;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            root = doc.rootVisualElement;

            progressContainer = root.Q<VisualElement>("ProgressContainer");
            progressLabel = progressContainer.Q<Label>("ProgressLabel");
            progressBar = progressContainer.Q<ProgressBar>("ProgressBar");
            progressBar.lowValue = 0f;
            progressBar.highValue = 100f;
        }

        // Update is called once per frame
        // TODO: apply updates to the UI elements in an event handler that fires when the ProgressModel is updated,
        // or in the next frame after that update, so that we batch the updates
        void Update()
        {
            if (!ProgressModel.IsVisible)
            {
                progressContainer.style.display = DisplayStyle.None;
            }
            else
            {
                progressContainer.style.display = DisplayStyle.Flex;

                progressLabel.text = ProgressModel.TaskTitle;
                progressBar.title = ProgressModel.ProgressBarTitle;
                progressBar.value = ProgressModel.Percentage;
            }
        }
    }
}
