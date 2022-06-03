using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/malbers-input")]
    [AddComponentMenu("Malbers/Input/Malbers Input")]
    public class MalbersInput : MInput
    {
        #region Variables
        private ICharacterMove mCharacterMove;
        public IInputSystem InputSystem;
       
        public InputAxis Horizontal = new InputAxis("Horizontal", true, true);
        public InputAxis Vertical = new InputAxis("Vertical", true, true);
        public InputAxis UpDown = new InputAxis("UpDown", false, true);
        protected IAIControl AI;  //Referece for AI Input Sources


        private float horizontal;        //Horizontal Right & Left   Axis X
        private float vertical;          //Vertical   Forward & Back Axis Z
        private float upDown;
        #endregion

        protected Vector3 m_InputAxis;

        public virtual void SetMoveCharacter(bool val) => MoveCharacter = val;


        protected override void OnEnable()
        { 
            base.OnEnable(); 

            if (UpDown.active)
            {
                try
                {
                    var UPDown = Input.GetAxis(UpDown.name);
                }
                catch  
                {
                   // Debug.LogError($"<B>[Up Down]</B> input doesn't exist. Please select any Character with the Malbers Input Component and hit <b>UpDown -> [Create]</b>", this);
                   // enabled = false;
                }
            }
        }

        private void CheckUpDown()
        {
            if (UpDown.active)
            {
                //Check if UP Down Exist
#if UNITY_EDITOR
                bool found = false;

                var InputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
                var axesProperty = InputManager.FindProperty("m_Axes");
                for (int i = 0; i < axesProperty.arraySize; ++i)
                {
                    var property = axesProperty.GetArrayElementAtIndex(i);
                    if (property.FindPropertyRelative("m_Name").stringValue.Equals(UpDown.name))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError($"<B>[Up Down]</B> input doesn't exist. Please select any Character with the Malbers Input Component and hit <b>UpDown -> [Create]</b>", this);
                    enabled = false;
                }
#endif
            }
        }



        protected override void OnDisable()
        {
            base.OnDisable();
             mCharacterMove?.Move(Vector3.zero);       //When the Input is Disable make sure the character/animal is not moving.
        }


        void Awake()
        {
            InputSystem = DefaultInput.GetInputSystem(PlayerID);                   //Get Which Input System is being used

            //Update to all the Inputs to the active Input System
            Horizontal.InputSystem = Vertical.InputSystem = UpDown.InputSystem = InputSystem;
            foreach (var i in inputs)
                i.InputSystem = InputSystem;              

            List_to_Dictionary();       //Convert the Inputs to Dic... easier to find
            InitializeCharacter();
            MoveCharacter = true;       //Set that the Character can be moved
             AI = this.FindInterface<IAIControl>();
        }

        protected void InitializeCharacter() => mCharacterMove = GetComponent<ICharacterMove>();


        public virtual void UpAxis(bool input)
        {
            if (upDown == -1) return;        //This means that the Down Button was pressed so ignore the Up button
            upDown = input ? 1 : 0;
        }

        public virtual void DownAxis(bool input) => upDown = input ? -1 : 0;

        void Update() => SetInput();


        /// <summary>Send all the Inputs and Axis to the Animal</summary>
        protected override void SetInput()
        {
            horizontal = Horizontal.GetAxis;
            vertical = Vertical.GetAxis;
            upDown = UpDown.GetAxis;

            m_InputAxis = new Vector3(horizontal, upDown, vertical);

            //Debug.Log("m_InputAxis = " + m_InputAxis);

            if (mCharacterMove != null)
            {
                mCharacterMove.SetInputAxis(MoveCharacter ? m_InputAxis : Vector3.zero);
            }

            base.SetInput();
        }

        public virtual void Horizontal_Enable(bool value) => Horizontal.active = value;
        public virtual void UpDown_Enable(bool value) => UpDown.active = value;
        public virtual void Vertical_Enable(bool value) => Vertical.active = value;

        public void ResetInputAxis() => m_InputAxis = Vector3.zero;

        /// <summary>Convert the List of Inputs into a Dictionary</summary>
        void List_to_Dictionary()
        {
            DInputs = new Dictionary<string, InputRow>();
            foreach (var item in inputs)
                DInputs.Add(item.name, item);
        }
    } 
}