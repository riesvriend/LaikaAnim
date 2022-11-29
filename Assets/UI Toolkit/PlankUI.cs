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
        public string Title { get; set; } = "Brushing";
        public bool IsVisible { get; set; } = false;
        public float Percentage { get; set; } = 0.0f;
    }

    public class PlankUI : MonoBehaviour
    {
        public ProgressModel ProgressModel { get; set; } =
            new ProgressModel { IsVisible = true, Title = "Progress" };

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
        }

        // Update is called once per frame
        void Update()
        {
            if (!ProgressModel.IsVisible)
            {
                progressContainer.style.display = DisplayStyle.None;
            }
            else
            {
                progressContainer.style.display = DisplayStyle.Flex;
                progressLabel.text = ProgressModel.Title;
                progressBar.value = ProgressModel.Percentage;
            }
        }
    }
}
