using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Interaction/Pickable")]

    public class Pickable : MonoBehaviour, ICollectable
    {
        //  public enum CollectType { Collectable, Hold, OneUse } //For different types of collectable items? FOR ANOTHER UPDATE

        public bool Align = true;
        public bool AlignPos = true;
        public float AlignTime = 0.15f;
        public float AlignDistance = 1f;

        public FloatReference PickDelay = new FloatReference(0);
        public FloatReference DropDelay = new FloatReference(0);
        public FloatReference coolDown = new FloatReference(0f);
        public BoolReference m_Collectable = new BoolReference(false);
        public BoolReference m_ByAnimation = new BoolReference(false);
        public BoolReference m_DestroyOnPick = new BoolReference(false);
        [Tooltip("Unparent the Pickable, so it does not have any Transform parents.")]
        public BoolReference SceneRoot = new BoolReference(true);

        public FloatReference m_Value = new FloatReference(1f); //Not done yet
        public BoolReference m_AutoPick = new BoolReference(false); //Not done yet
        public IntReference m_ID = new IntReference();         //Not done yet

        /// <summary>Who Did the Picking </summary>
        public GameObject Picker { get; set; }


        public BoolEvent OnFocused = new BoolEvent();
        public GameObjectEvent OnPicked = new GameObjectEvent();
        public GameObjectEvent OnPrePicked = new GameObjectEvent();
        public GameObjectEvent OnDropped = new GameObjectEvent();
        public GameObjectEvent OnPreDropped = new GameObjectEvent();

        [SerializeField] private Rigidbody rb;
        [RequiredField] public Collider m_collider;

        private float currentPickTime;

        /// <summary>Is this Object being picked </summary>
        public bool IsPicked { get; set; }

        /// <summary>Current value of the Item</summary>
        public float Value { get => m_Value.Value; set => m_Value.Value = value; }

        /// <summary>The Item will be autopicked if the Picker is focusing it</summary>
        public bool AutoPick { get => m_AutoPick.Value; set => m_AutoPick.Value = value; }
        public bool Collectable { get => m_Collectable.Value; set => m_Collectable.Value = value; }
        public Rigidbody RigidBody => rb;

        /// <summary>The Pick Up Drop Logic will be called via animator events/messages</summary>
        public bool ByAnimation { get => m_ByAnimation.Value; set => m_ByAnimation.Value = value; }
        public bool DestroyOnPick { get => m_DestroyOnPick.Value; set => m_DestroyOnPick.Value = value; }
        public bool InCoolDown => !MTools.ElapsedTime(CurrentPickTime, coolDown);
        public int ID { get => m_ID.Value; set => m_ID.Value = value; }


        private bool focused;
        public bool Focused 
        {
            get => focused;
            set => OnFocused.Invoke(focused = value);
        }

        /// <summary>Game Time the Pickable was Picked</summary>
        public float CurrentPickTime { get => currentPickTime; set => currentPickTime = value; }

        private void OnDisable()
        {
            Focused = false;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            CurrentPickTime = -coolDown; 
            if (SceneRoot.Value) transform.parent = null;
        }

        public virtual void Pick()
        {
            if (RigidBody)
            {
                RigidBody.useGravity = false;
                RigidBody.velocity = Vector3.zero;
                RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                RigidBody.isKinematic = true;
            }

            m_collider.enabled = false;
            IsPicked = true;
            OnPicked.Invoke(Picker);
            CurrentPickTime = Time.time;
        }

        public virtual void Drop()
        {
            IsPicked = false;

            if (RigidBody)
            {
                RigidBody.useGravity = true;
                RigidBody.isKinematic = false;
                RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            m_collider.enabled = true;

            var localScale = transform.localScale;
            transform.parent = null;
            transform.localScale = localScale;

            OnDropped.Invoke(Picker);

            Picker = null; //Reset who did the picking

            CurrentPickTime = Time.time;
        }

        [HideInInspector] public int EditorTabs = 0;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, AlignDistance);
        }

        private void Reset()
        {
            m_collider = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
            var EInteract = MTools.GetInstance<MEvent>("Interact UI");

            if (EInteract)
            {
                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<Transform>(OnFocused, EInteract.Invoke, transform);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnFocused, EInteract.Invoke);
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnPicked, EInteract.Invoke, false);
            }
        }
#endif
    }


    //INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(Pickable)), CanEditMultipleObjects]
    public class PickableEditor : Editor
    {
        private SerializedProperty //   PickAnimations, PickUpMode, PickUpAbility, DropMode, DropAbility,DropAnimations, 
            Align, AlignTime, AlignDistance, AlignPos, EditorTabs,
            m_AutoPick, DropDelay, PickDelay, rb, CoolDown, SceneRoot,
            OnFocused, OnPrePicked, OnPicked, OnDropped, OnPreDropped, /*ShowEvents, */FloatID, IntID, m_collider, m_Collectable, m_ByAnimation, m_DestroyOnPick;

        private Pickable m;

        protected string[] Tabs1 = new string[] { "General", "Events" };

        private void OnEnable()
        {
            m = (Pickable)target;

            EditorTabs = serializedObject.FindProperty("EditorTabs");
            SceneRoot = serializedObject.FindProperty("SceneRoot");
            rb = serializedObject.FindProperty("rb");
            PickDelay = serializedObject.FindProperty("PickDelay");
            DropDelay = serializedObject.FindProperty("DropDelay");
            m_Collectable = serializedObject.FindProperty("m_Collectable");
            m_ByAnimation = serializedObject.FindProperty("m_ByAnimation");
            m_DestroyOnPick = serializedObject.FindProperty("m_DestroyOnPick");


            Align = serializedObject.FindProperty("Align");
            AlignTime = serializedObject.FindProperty("AlignTime");
            AlignDistance = serializedObject.FindProperty("AlignDistance");
            OnFocused = serializedObject.FindProperty("OnFocused");
            OnPicked = serializedObject.FindProperty("OnPicked");
            OnPrePicked = serializedObject.FindProperty("OnPrePicked");
            OnDropped = serializedObject.FindProperty("OnDropped");
            OnPreDropped = serializedObject.FindProperty("OnPreDropped");
            //ShowEvents = serializedObject.FindProperty("ShowEvents");
            FloatID = serializedObject.FindProperty("m_Value");
            IntID = serializedObject.FindProperty("m_ID");
            m_collider = serializedObject.FindProperty("m_collider");
            AlignPos = serializedObject.FindProperty("AlignPos");
            //Collectable = serializedObject.FindProperty("Collectable");
            m_AutoPick = serializedObject.FindProperty("m_AutoPick");
            CoolDown = serializedObject.FindProperty("coolDown");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Pickable - Collectable object");
            EditorGUILayout.BeginVertical(MTools.StyleGray);
            {

                EditorTabs.intValue = GUILayout.Toolbar(EditorTabs.intValue, Tabs1);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    if (Application.isPlaying)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ToggleLeft("Is Picked", m.IsPicked);
                        EditorGUI.EndDisabledGroup();
                    }

                    if (EditorTabs.intValue == 0) DrawGeneral();
                    else DrawEvents();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(OnFocused);
            if (m.PickDelay > 0 || m.m_ByAnimation.Value)
                EditorGUILayout.PropertyField(OnPrePicked, new GUIContent("On Pre-Picked By"));
            EditorGUILayout.PropertyField(OnPicked, new GUIContent("On Picked By"));
            if (m.DropDelay > 0 || m.m_ByAnimation.Value)
                EditorGUILayout.PropertyField(OnPreDropped, new GUIContent("On Pre-Dropped By"));
            EditorGUILayout.PropertyField(OnDropped, new GUIContent("On Dropped By"));

        }

        private void DrawGeneral()
        {
            m_AutoPick.isExpanded = MalbersEditor.Foldout(m_AutoPick.isExpanded, "Pickable Item");

            if (m_AutoPick.isExpanded)
            {
                EditorGUILayout.PropertyField(IntID, new GUIContent("ID", "Int value the Pickable Item can store. This ID is used by the Picker component to Identify each Pickable Object"));
                EditorGUILayout.PropertyField(FloatID, new GUIContent("Float Value", "Float value the Pickable Item can store.. that it can be use for anything"));

                EditorGUILayout.PropertyField(m_AutoPick, new GUIContent("Auto", "The Item will be Picked Automatically"));
                EditorGUILayout.PropertyField(m_ByAnimation,
                    new GUIContent("Use Animation", "The Item will Pre-Picked/Dropped by the Picker Animator." +
                    " Pick-Drop Logic is called by Animation Event or Animator Message Behaviour.\nUse the Methods: TryPickUpDrop(); TryPickUp(); TryDrop();"));

                EditorGUILayout.PropertyField(SceneRoot);


                EditorGUILayout.PropertyField(m_Collectable, new GUIContent("Collectable", "The Item will Picked by the Pickable and it will be stored"));
                if (m.Collectable)
                    EditorGUILayout.PropertyField(m_DestroyOnPick, new GUIContent("Destroy Collectable", "The Item will be destroyed after is picked"));
            }
            //EditorGUILayout.EndVertical();

            // EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                m_collider.isExpanded = MalbersEditor.Foldout(m_collider.isExpanded, "References");
                if (m_collider.isExpanded)
                {
                    EditorGUILayout.PropertyField(m_collider);
                    EditorGUILayout.PropertyField(rb, new GUIContent("Rigid Body"));
                }
            }
            //EditorGUILayout.EndVertical();



            // EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                CoolDown.isExpanded = MalbersEditor.Foldout(CoolDown.isExpanded, "Delays");

                if (CoolDown.isExpanded)
                {
                    EditorGUILayout.PropertyField(CoolDown);
                    EditorGUILayout.PropertyField(PickDelay, new GUIContent("Pick Delay", "Delay time after Calling the Pick Action"));
                    EditorGUILayout.PropertyField(DropDelay, new GUIContent("Drop Delay", "Delay time after Calling the Drop Action"));
                }
            }
            // EditorGUILayout.EndVertical();

            //  EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Align.isExpanded = MalbersEditor.Foldout(Align.isExpanded, "Alignment");

                if (Align.isExpanded)
                {
                    EditorGUILayout.PropertyField(Align, new GUIContent("Align On Pick", "Align the character to the Item"));

                    if (Align.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(AlignPos, new GUIContent("Align Pos", "align the Position"));
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(AlignDistance, new GUIContent("Distance", "Distance to move the Animal towards the Item"), GUILayout.MinWidth(50));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(AlignTime, new GUIContent("Time", "Time required to do the alignment"));
                    }
                }
            }
            // EditorGUILayout.EndVertical();
        }
    }
#endif
}