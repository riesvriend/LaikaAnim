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
        public string TaskTitle { get; internal set; } = "Klik hier";
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
            progressLabel = root.Q<Label>("ProgressLabel");
            progressBar = root.Q<ProgressBar>("ProgressBar");
            progressBar.lowValue = 0f;
            progressBar.highValue = 100f;
        }

        // Update is called once per frame
        void Update()
        {
            if (!ProgressModel.IsVisible)
            {
                "progressContainer hidden".Log();

                // Hide the progressbar and label
                progressContainer.style.display = DisplayStyle.None;
                progressLabel.text = " ";
                progressBar.value = 0;
                progressBar.title = " ";

                //root.visible = false;

                //progressContainer.style.display = DisplayStyle.None;
                //progressContainer.visible = false;
                //progressBar.style.display = DisplayStyle.None;
                //progressBar.visible = false;
                //progressLabel.text = "";
                //progressBar.value = 0f;
                //progressBar.title = "";
            }
            else
            {
                "progressContainer visible".Log();

                progressContainer.style.display = DisplayStyle.Flex;
                progressContainer.visible = true;
                progressBar.style.display = DisplayStyle.Flex;
                progressBar.visible = true;
                progressLabel.text = ProgressModel.TaskTitle;
                progressBar.value = ProgressModel.Percentage;
                progressBar.title = ProgressModel.ProgressBarTitle;
            }
        }

        private void OnGUI() { }
    }
}
