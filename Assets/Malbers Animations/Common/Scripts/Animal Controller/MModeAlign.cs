using MalbersAnimations.Controller;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Animal Controller/Mode Align")]
    public class MModeAlign : MonoBehaviour
    {
        [RequiredField] public MAnimal animal;

        [ContextMenuItem("Attack Mode","AddAttackMode")]
        public List<ModeID> modes = new List<ModeID>();

        [Tooltip("It will search only for Animals on the Radius. If is set to false then it will search for all colliders using the LayerMask")]
        public bool AnimalsOnly = true;
        public LayerReference Layer = new LayerReference(-1);
        [Tooltip("Radius used for the Search")
            ,UnityEngine.Serialization.FormerlySerializedAs("LookRadius")]
        [Min(0)] public float SearchRadius = 2f;
        [Tooltip("Radius used push closer/farther the Target when playing the Mode"), UnityEngine.Serialization.FormerlySerializedAs("DistanceRadius")]
        [Min(0)] public float Distance = 0;
        [Tooltip("Time needed to complete the Position aligment")]
        [Min(0)] public float AlignTime = 0.3f;
        [Tooltip("Time needed to complete the Rotation aligment")]
        [Min(0)] public float LookAtTime = 0.15f;
        public Color debugColor = new Color(1, 0.5f, 0, 0.2f);

        void Awake()
        {
            if (animal == null)
                animal = this.FindComponent<MAnimal>();

            if (modes == null || modes.Count == 0)
            {
                Debug.LogWarning("Please Add Modes to the Mode Align. ", this);
                enabled = false;
            }
        }

        void OnEnable()
        { animal.OnModeStart.AddListener(StartingMode); }

        void OnDisable()
        { animal.OnModeStart.RemoveListener(StartingMode); }

        void StartingMode(int ModeID, int ability)
        {
            if (!isActiveAndEnabled) return;

            if (modes == null || modes.Count == 0 || modes.Exists(x => x.ID == ModeID))
            {
                if (AnimalsOnly)
                    AlignAnimalsOnly();
                else
                    Align();
            }
        }

        private void AlignAnimalsOnly()
        {
            MAnimal ClosestAnimal = null;
            float ClosestDistance = float.MaxValue;

            foreach (var a in MAnimal.Animals)
            {
                if (a == animal ||                                              //We are the same animal
                    a.ActiveStateID.ID == StateEnum.Death                       //The Animal is death
                    || a.Sleep                                                  //The Animal is sleep (Not working)
                    || !MTools.Layer_in_LayerMask(a.gameObject.layer, Layer)    //Is not using the correct layer
                    ) continue; //Don't Find yourself or don't find death animals

                var animalsDistance = Vector3.Distance(transform.position, a.Center);

                if (SearchRadius >= animalsDistance && ClosestDistance >= animalsDistance)
                {
                    ClosestDistance = animalsDistance;
                    ClosestAnimal = a;
                }
            }

            if (ClosestAnimal)
                StartAligning(ClosestAnimal.Center);
        }

        private void Align()
        {
            var pos = animal.Center;

            var AllColliders = Physics.OverlapSphere(pos, SearchRadius, Layer.Value);

            Collider ClosestCollider = null;
            float ClosestDistance = float.MaxValue;

            foreach (var col in AllColliders)
            {
                if (col.transform.root == animal.transform.root) continue; //Don't Find yourself

                var DistCol = Vector3.Distance(transform.position, col.bounds.center);

                if (ClosestDistance > DistCol)
                {
                    ClosestDistance = DistCol;
                    ClosestCollider = col;
                }
            }
            if (ClosestCollider) StartAligning(ClosestCollider.bounds.center);
        }

        private void StartAligning(Vector3 TargetCenter)
        {
            TargetCenter.y = animal.transform.position.y;
            Debug.DrawLine(transform.position, TargetCenter, Color.red, 3f);
            StartCoroutine(MTools.AlignLookAtTransform(animal.transform, TargetCenter, LookAtTime));
            if (Distance > 0) StartCoroutine(MTools.AlignTransformRadius(animal.transform, TargetCenter, AlignTime, Distance * animal.ScaleFactor));  //Align Look At the Zone
        }



#if UNITY_EDITOR

        [ContextMenu("Add Attack Mode")]
        private void AddAttackMode()
        {
            Reset();
        }


        void Reset()
        {
            ModeID modeID = MTools.GetResource<ModeID>("Attack1");
            animal = gameObject.FindComponent<MAnimal>();
            modes = new List<ModeID>();
            modes.Add(modeID);
            MTools.SetDirty(this);
        }


        void OnDrawGizmosSelected()
        {
            if (animal)
            {
                UnityEditor.Handles.color = debugColor;
                UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, SearchRadius);
                var c = debugColor; c.a = 1;
                UnityEditor.Handles.color = c;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, SearchRadius);

                UnityEditor.Handles.color = (c + Color.white) / 2;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, Distance);

            }
        }
#endif
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MModeAlign)),CanEditMultipleObjects]
    public class MModeAlignEditor : Editor
    {
        SerializedProperty animal, modes, AnimalsOnly, Layer, LookRadius, DistanceRadius, AlignTime, LookAtTime, debugColor;
        private void OnEnable()
        { 
            animal = serializedObject.FindProperty("animal");
            modes = serializedObject.FindProperty("modes");
            AnimalsOnly = serializedObject.FindProperty("AnimalsOnly");
            Layer = serializedObject.FindProperty("Layer");
            LookRadius = serializedObject.FindProperty("SearchRadius");
            DistanceRadius = serializedObject.FindProperty("Distance");
            AlignTime = serializedObject.FindProperty("AlignTime");
            LookAtTime = serializedObject.FindProperty("LookAtTime");
            debugColor = serializedObject.FindProperty("debugColor"); 
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription($"Execute a LookAt towards the closest Animal or GameObject  when is playing a Mode on the list");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(animal);
                EditorGUILayout.PropertyField(debugColor, GUIContent.none, GUILayout.Width(36));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(modes,true);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(AnimalsOnly);
            EditorGUILayout.PropertyField(Layer);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(LookRadius);
            if (LookRadius.floatValue > 0)
                EditorGUILayout.PropertyField(LookAtTime);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(DistanceRadius);
            if (DistanceRadius.floatValue>0)
                EditorGUILayout.PropertyField(AlignTime);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif

}