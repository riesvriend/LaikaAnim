/// <summary>
/// Timeline control via UI Slider.
/// </summary>


using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace sergeyiwanski
{
    public class UI_TimelineController : MonoBehaviour
    {
        public Slider slider;
        public PlayableDirector playableDirector;


        private void Start()
        {
            slider.maxValue = (float)playableDirector.duration;
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                playableDirector.time = slider.value;
            }
            else
            {
                slider.value = (float)playableDirector.time;
            }
        }
    }
}
