//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEngine;

//namespace Assets.Scripts.Synchrony
//{
//    public class GuiStyles
//    {
//        // See WitStyles() for sample code

//        public static GUIStyle Label;
//        public static GUIStyle Button;
//        public static GUIStyle TextField;
//        public static Texture2D DarkTexture;
//        public static Texture2D TextFieldTexture;

//        static GuiStyles()
//        {
//            Label = new GUIStyle(EditorStyles.label);
//            Label.richText = true;
//            Label.wordWrap = true;
//            Label.fontSize = 14;

//            TextFieldTexture = new Texture2D(1, 1);
//            TextFieldTexture.SetPixel(0, 0, new Color(.85f, .85f, .95f));
//            TextFieldTexture.Apply();

//            TextField = new GUIStyle(EditorStyles.textField);
//            TextField.normal.background = TextFieldTexture;
//            TextField.normal.textColor = Color.black;

//            DarkTexture = new Texture2D(1, 1);
//            DarkTexture.SetPixel(0, 0, new Color(0.267f, 0.286f, 0.31f));
//            DarkTexture.Apply();

//            Button = new GUIStyle(EditorStyles.miniButton);
//            //Button.normal.background = DarkTexture;
//        }
//    }
//}
