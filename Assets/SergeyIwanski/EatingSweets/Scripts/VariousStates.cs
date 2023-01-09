/// <summary>
/// Toggles child objects based on percentages. 
/// Equally divides from 100 percent between objects.
/// </summary>

using System.Collections.Generic;
using UnityEngine;


namespace sergeyiwanski
{
    [ExecuteInEditMode]
    public class VariousStates : MonoBehaviour
    {
        [Tooltip("The current percentage of the state of the object.")]
        [Range(0, 100)]
        public int percentage = 100;

        //for automatic calculation...
        List<Transform> list; //set of child objects
        int divisor; //amount of percentage on an object
        int index;
        //AudioSource sound;


        // Use this for initialization
        void Start()
        {
            list = new List<Transform>();
            //sound = GetComponent<AudioSource>();
                      
            foreach(Transform item in GetComponentsInChildren<Transform>(true))
            {
                if (item.parent == transform)
                {
                    item.gameObject.SetActive(false);
                    list.Add(item);
                }
            }
            list[0].gameObject.SetActive(true);

            divisor = 100 / list.Count;
        }


        private void Update()
        {
            OnChange();
        }


        //This method displays the percentage change
        void OnChange()
        {
            //for optimize
            if (index == (100 - percentage) / divisor) return; 

            //hide previous object
            list[index].gameObject.SetActive(false);

            //calculate new index
            index = ((100-percentage) / divisor);

            //show current object
            if (index < list.Count)
            {
                list[index].gameObject.SetActive(true);
                //if (sound && index > 0) sound.Play();
            }
            else
            {
                index--;
            }
        }
    }
}
