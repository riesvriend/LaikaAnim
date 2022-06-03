using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary> Monobehaviour used to call Coroutines in Scriptable Objects </summary> 
    [DefaultExecutionOrder(500000000)]
    public class MScriptableCoroutine : MonoBehaviour
    {
        internal List<ScriptableCoroutine> ScriptableCoroutines;
        public static MScriptableCoroutine Main;
        
        internal void Restart()
        {
            if (Main == null) //if there's a main animal already seted
            {
                Main = this;
                ScriptableCoroutines = new List<ScriptableCoroutine>();
            }
        }

        private void Awake()
        {
            Restart();
            DontDestroyOnLoad(this);
        }


        public static void PlayCoroutine(ScriptableCoroutine SC, IEnumerator Coroutine)
        {
            Initialize(); //In case is not initialized
            
            if (Main != null && Main.enabled && Main.isActiveAndEnabled)
            {
                if (!Main.ScriptableCoroutines.Contains(SC))
                {
                    Main.ScriptableCoroutines.Add(SC); //Add the Fist Time
                }
                Main.StartCoroutine(Coroutine);
            }
        }

        public static void Stop_Coroutine(IEnumerator Coroutine)
        {
            Main.StopCoroutine(Coroutine);
        }

        public static void Initialize()
        {
            if (Main == null && Application.isPlaying)
            {
                var ScriptCoro = new GameObject();
                ScriptCoro.name = "Scriptable Coroutines";
                ScriptCoro.AddComponent<MScriptableCoroutine>();
            }
        }

        protected virtual void OnDestroy()
        {
            if (ScriptableCoroutines != null)
            foreach (var c in ScriptableCoroutines)
                c.CleanCoroutine();

            StopAllCoroutines();
        }
    }
}