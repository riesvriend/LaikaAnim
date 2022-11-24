using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchrony
{
    public static class SynchronyUtils
    {
        public static string FullName(this GameObject gameObject)
        {
            if (gameObject.transform != gameObject.transform.root)
                return $"{gameObject.transform.parent.gameObject.FullName()}/{gameObject.name}";
            else
                return gameObject.name;
        }

        // https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object
        public static bool IsClassOrSubclass<TPotentialBase>(this Type potentialDescendant)
        {
            var potentialBase = typeof(TPotentialBase);
            return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
        }

        public static void Log(this string text)
        {
            Debug.Log(text); // Output to Unity console
            System.Diagnostics.Debug.WriteLine(text); // Output to VS.NET

            var logTextBoxes = GameObject.FindGameObjectsWithTag("LogTextBox");
            foreach (var textbox in logTextBoxes)
            {
                var tmProTextBox = textbox.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmProTextBox != null)
                {
                    var newText = (text + Environment.NewLine + tmProTextBox.text);
                    if (newText.Length > 2000)
                        newText = newText.Substring(startIndex: 0, length: 2000);

                    tmProTextBox.text = newText;
                }

                // Input field text area will scroll the text but has side issues such as showing the popup 
                // keyboard and claiming focus
                var tmProTextInput = textbox.GetComponent<TMPro.TMP_InputField>();
                if (tmProTextInput != null)
                {
                    tmProTextInput.text += text + Environment.NewLine;
                    // https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/api/TMPro.TMP_InputField.html
                    var pos = tmProTextInput.text.Length;
                    // Move caret to bottom of log and scroll into focus
                    // https://forum.unity.com/threads/textmeshpro-caret-position-change.526114/
                    tmProTextInput.selectionAnchorPosition = pos;
                    tmProTextInput.selectionFocusPosition = pos;
                    tmProTextInput.Select();
                    tmProTextInput.ForceLabelUpdate();
                }
            }
        }

        public static bool IsPlayingInEditor()
        {
            return Application.isEditor && Application.isPlaying;
        }

        public static bool IsAltOrOptionPressed()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        public static bool IsMouseWheelTurned()
        {
            return Input.mouseScrollDelta.y != 0;
        }

        //// When playing a scene other than _preLoadScene in the editor, we jump to the preload scene with the App instead
        //// so that we have our app context setup
        //public static App PreLoadAppScene()
        //{
        //    var app = GameObject.FindObjectOfType<App>(); // Lives in the _preloadScene with don't destroy on load
        //    if (app != null)
        //        return app;
            
        //    // Prevent the objects in the current scene from activating and immediately destroying
        //    // due to the preload scene being loaded. This saves a lot of null-checks in each of the game objects we write
        //    var gameObjectsInCurrentScene = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject));
        //    foreach (GameObject obj in gameObjectsInCurrentScene)
        //        obj.SetActive(false);
            
        //    UnityEngine.SceneManagement.SceneManager.LoadScene("_preloadScene");
        //    return null;
        //}
    }
}