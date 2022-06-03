using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace MalbersAnimations
{
    [CustomEditor(typeof(MInput))]
    public class MInputEditor : Editor
    {
        protected ReorderableList list;
        protected SerializedProperty inputs, showInputEvents, IgnoreOnPause, OnInputEnabled, OnInputDisableds, OnInputDisabled;
        private MInput MInp;


        protected virtual void OnEnable()
        {
            MInp = ((MInput)target);

            inputs = serializedObject.FindProperty("inputs");
            OnInputEnabled = serializedObject.FindProperty("OnInputEnabled");
            OnInputDisabled = serializedObject.FindProperty("OnInputDisabled");
            showInputEvents = serializedObject.FindProperty("showInputEvents");
            IgnoreOnPause = serializedObject.FindProperty("IgnoreOnPause");

            list = new ReorderableList(serializedObject, inputs, true, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

           MalbersEditor.DrawDescription("Inputs to connect to components via UnityEvents");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(IgnoreOnPause);
                    DrawRewired();
                    DrawListAnEvents();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "MInput Inspector");
                }
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();
        }

        protected void DrawRewired()
        {
#if REWIRED
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayerID"), new GUIContent("Player ID", "Rewired Player ID"));
                EditorGUILayout.EndVertical();
#endif
        }

        protected void DrawListAnEvents()
        {
            list.DoLayoutList();

            var Index = list.index;

            if (Index != -1)
            {
                SerializedProperty Element = inputs.GetArrayElementAtIndex(Index);
                DrawInputEvents(Element, Index);
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (MalbersEditor.Foldout( showInputEvents, "[Enable/Disable] Events"))
                {
                    EditorGUILayout.PropertyField(OnInputEnabled);
                    EditorGUILayout.PropertyField(OnInputDisabled);
                }
            }
            EditorGUILayout.EndVertical();
        }

        protected void DrawInputEvents(SerializedProperty Element, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                var inputname =  Element.FindPropertyRelative("name").stringValue;
                Element.isExpanded = MalbersEditor.Foldout(Element.isExpanded, $" [{inputname}] Input");
                
                if (Element.isExpanded)
                {

                    var active = Element.FindPropertyRelative("active");
                    var OnInputChanged = Element.FindPropertyRelative("OnInputChanged");
                    var OnInputPressed = Element.FindPropertyRelative("OnInputPressed");
                    var OnInputDown = Element.FindPropertyRelative("OnInputDown");
                    var OnInputUp = Element.FindPropertyRelative("OnInputUp");
                    var OnInputEnable = Element.FindPropertyRelative("OnInputEnable");
                    var OnInputDisable = Element.FindPropertyRelative("OnInputDisable");
                    var ResetOnDisable = Element.FindPropertyRelative("ResetOnDisable");

                    EditorGUILayout.PropertyField(active);
                    EditorGUILayout.PropertyField(ResetOnDisable);
                    EditorGUILayout.Space();
                   // EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

                    InputButton GetPressed = (InputButton)Element.FindPropertyRelative("GetPressed").enumValueIndex;


                    switch (GetPressed)
                    {
                        case InputButton.Press:
                            EditorGUILayout.PropertyField(OnInputChanged);
                            EditorGUILayout.PropertyField(OnInputPressed);
                            EditorGUILayout.PropertyField(OnInputDown);
                            EditorGUILayout.PropertyField(OnInputUp);
                            break;
                        case InputButton.Down:
                            EditorGUILayout.PropertyField(OnInputDown);
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        case InputButton.Up:
                            EditorGUILayout.PropertyField(OnInputUp);
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        case InputButton.LongPress:
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("LongPressTime"), new GUIContent("Long Press Time", "Time the Input Should be Pressed"));
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnLongPress"), new GUIContent("On Long Press Completed"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnPressedNormalized"), new GUIContent("On Pressed Time Normalized"));
                            EditorGUILayout.PropertyField(OnInputDown, new GUIContent("On Input Down"));
                            EditorGUILayout.PropertyField(OnInputUp, new GUIContent("On Pressed Interrupted"));
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        case InputButton.DoubleTap:
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("DoubleTapTime"));
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(OnInputDown, new GUIContent("On First Tap"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnDoubleTap"));
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        case InputButton.Toggle:
                            EditorGUILayout.PropertyField(OnInputChanged,  new GUIContent("On Input Toggle"));
                            EditorGUILayout.PropertyField(OnInputDown,  new GUIContent("On Toggle On"));
                            EditorGUILayout.PropertyField(OnInputUp,  new GUIContent("On Toggle Off"));
                            break;  
                        case InputButton.Axis:
                            EditorGUILayout.PropertyField(OnInputChanged);
                            EditorGUILayout.PropertyField(OnInputPressed);
                            EditorGUILayout.PropertyField(OnInputDown);
                            EditorGUILayout.PropertyField(OnInputUp);
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnPressedNormalized"), new GUIContent("On Axis Value Changed"));
                            break;
                        default:
                            break;
                    }

                    OnInputEnable.isExpanded = MalbersEditor.Foldout(OnInputEnable.isExpanded, $"[{inputname}] Enable/Disable");

                    if (OnInputEnable.isExpanded)
                    {
                        EditorGUILayout.PropertyField(OnInputEnable, new GUIContent("On [" + inputname + "] Enabled"));
                        EditorGUILayout.PropertyField(OnInputDisable, new GUIContent("On [" + inputname + "] Disabled"));
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        protected void HeaderCallbackDelegate(Rect rect)
        {

            Rect R_1 = new Rect(rect.x + 20, rect.y, (rect.width - 20) / 4 + 12, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + (rect.width - 20) / 4 + 35, rect.y, (rect.width - 20) / 4 - 20, EditorGUIUtility.singleLineHeight);
            Rect R_3 = new Rect(rect.x + ((rect.width - 20) / 4) * 2 + 18, rect.y, ((rect.width - 30) / 4) + 11, EditorGUIUtility.singleLineHeight);
            Rect R_4 = new Rect(rect.x + ((rect.width) / 4) * 3 + 15, rect.y, ((rect.width) / 4) - 15, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_1, "   Name", EditorStyles.boldLabel);
            EditorGUI.LabelField(R_2, "   Type", EditorStyles.boldLabel);
            EditorGUI.LabelField(R_3, "  Value", EditorStyles.boldLabel);
            EditorGUI.LabelField(R_4, "Button", EditorStyles.boldLabel);
        }

        protected void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MInp.inputs[index];

            var elementSer = inputs.GetArrayElementAtIndex(index);

            rect.y += 2;
            element.active.Value = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight), element.active.Value);

            Rect R_1 = new Rect(rect.x + 20, rect.y, (rect.width - 20) / 4 + 12, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + (rect.width - 20) / 4 + 35, rect.y, (rect.width - 20) / 4 - 20, EditorGUIUtility.singleLineHeight);
            Rect R_3 = new Rect(rect.x + ((rect.width - 20) / 4) * 2 + 18, rect.y, ((rect.width - 30) / 4) + 11, EditorGUIUtility.singleLineHeight);
            Rect R_4 = new Rect(rect.x + ((rect.width) / 4) * 3 + 15, rect.y, ((rect.width) / 4) - 15, EditorGUIUtility.singleLineHeight);

            //GUIStyle a = new GUIStyle(EditorStyles.label);

            ////This make the name a editable label
            //a.fontStyle = FontStyle.Normal;


            var name = elementSer.FindPropertyRelative("name");
            var type = elementSer.FindPropertyRelative("type");
            var input = elementSer.FindPropertyRelative("input");
            var key = elementSer.FindPropertyRelative("key");
            var GetPressed = elementSer.FindPropertyRelative("GetPressed");

            EditorGUI.PropertyField(R_1, name, GUIContent.none);
            //name.stringValue = EditorGUI.TextField(R_1, name.stringValue, EditorStyles.textField);

            var lockType = false;

            if (GetPressed.enumValueIndex == (int)InputButton.Axis)
            {
                type.enumValueIndex = (int)InputType.Input;
                type.serializedObject.ApplyModifiedProperties();
                lockType = true;
            }


            using (new EditorGUI.DisabledGroupScope(lockType))
            {
                EditorGUI.PropertyField(R_2, type, GUIContent.none);
            }

            if (type.intValue != 1)
                EditorGUI.PropertyField(R_3, input, GUIContent.none);
            else
                EditorGUI.PropertyField(R_3, key, GUIContent.none);

            EditorGUI.PropertyField(R_4, GetPressed, GUIContent.none);
        }

        protected void OnAddCallBack(ReorderableList list)
        {
            if (MInp.inputs == null)
            {
                MInp.inputs = new System.Collections.Generic.List<InputRow>();
            }
            MInp.inputs.Add(new InputRow("New", "InputValue", KeyCode.Alpha0, InputButton.Press, InputType.Input));
        }
    }
}