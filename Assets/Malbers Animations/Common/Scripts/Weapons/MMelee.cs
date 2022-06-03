using UnityEngine; 
using System.Collections.Generic;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using UnityEngine.Events;
using MalbersAnimations.Scriptables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/Melee Weapon")]
    public class MMelee : MWeapon 
    {
        [RequiredField] public Collider meleeTrigger;  


        public BoolEvent OnCauseDamage = new BoolEvent();
        public Color DebugColor = new Color(1, 0.25f, 0, 0.5f);

        public bool UseCameraSide;
        public bool InvertCameraSide;
        public int Attacks = 1;

        protected bool canCauseDamage;                      //The moment in the Animation the weapon can cause Damage 
        public bool CanCauseDamage
        {
            get =>  canCauseDamage; 
            set
            {
                Debugging($"Can cause Damage [{value}]",this);
                canCauseDamage = value;
                proxy.Active = value;
                meleeTrigger.enabled = value;         //Enable/Disable the Trigger
            }
        }

        protected TriggerProxy proxy { get; private set; }

       
        /// <summary>Damager from the Attack Triger Behaviour</summary>
        public override void ActivateDamager(int value)
        {
            if (value == 0)
            {
                CanCauseDamage = false;
                OnCauseDamage.Invoke(CanCauseDamage);
            }
            else if (value == -1 || value == Index)
            {
                CanCauseDamage = true;
                OnCauseDamage.Invoke(CanCauseDamage);
            }
        }

        public override void DoDamage(bool value)
        {
            ActivateDamager(value ? -1 : 0);
        }

        private void Awake()
        {
            if (animator)
                defaultAnimatorSpeed = animator.speed;

            Initialize();
        }


        public override void Initialize()
        {
            base.Initialize();
            FindTrigger();
        }

        void OnEnable()
        {
            if (proxy)
            {
                proxy.EnterTriggerInteraction += AttackTriggerEnter;
                proxy.ExitTriggerInteraction += AttackTriggerExit;
            }
            CanCauseDamage = false;
        }

        /// <summary>Disable Listeners </summary>
        void OnDisable()
        {
            if (proxy)
            {
                proxy.EnterTriggerInteraction -= AttackTriggerEnter;
                proxy.ExitTriggerInteraction -= AttackTriggerExit;
            }
        }


        #region Main Attack 
        internal override void MainAttack_Start(IMWeaponOwner RC)
        {
            base.MainAttack_Start(RC);
            if (CanAttack)
            {
                ResetCharge();
                var Side = UseCameraSide && (InvertCameraSide ? RC.AimingSide : !RC.AimingSide);
                RiderMeleeAttack(Side);
            }
        }


        internal override void SecondaryAttack_Start(IMWeaponOwner RC)
        {
            MainInput = false;
            SecondInput = true;
            if (CanAttack)
            {
                DoDamage(false);
                ResetCharge();
                if (!UseCameraSide) RiderMeleeAttack(true);                 //Attack with Left Hand
            }
        } 

 

        /// <summary>Set all parameters for Melee Attack </summary>
        /// <param name="rightSide">true = Right Side of the ount.. false = Left Side of the ount</param>
        protected virtual void RiderMeleeAttack(bool rightSide)
        {
            int attackID = Random.Range(1, Attacks + 1) * (rightSide ? 1 : -1);           // Set the Attacks for the RIGHT Side with the 'Right Hand' 1 2
            WeaponAction?.Invoke(attackID); //Convert into Left attack if the weapon is Left Handed
            CanAttack = false;
        }
        #endregion 
 

        void AttackTriggerEnter(GameObject root, Collider other)
        {
           // if (IsInvalid(other)) return;                                               //Check Layers and Don't hit yourself
            if (other.transform.root == IgnoreTransform) return;                        //Check an Extra transform that you cannot hit...e.g your mount

            var damagee = other.GetComponentInParent<IMDamage>();                      //Get the Animal on the Other collider
            var center = meleeTrigger.bounds.center;
            Direction = (other.bounds.center - center).normalized;                      //Calculate the direction of the attack

            TryInteract(other.gameObject);                                              //Get the interactable on the Other collider
            TryPhysics(other.attachedRigidbody, other, center, Direction, Force);       //If the other has a riggid body and it can be pushed
            TryStopAnimator();
            TryHit(other, meleeTrigger.bounds.center);

            var Damage = new StatModifier(statModifier)
            { Value = Mathf.Lerp(MinDamage, MaxDamage, ChargedNormalized) }; //Do the Damage depending the charge

            TryDamage(damagee, Damage); //if the other does'nt have the Damagable Interface dont send the Damagable stuff 
        }


        void AttackTriggerExit(GameObject root, Collider other)
        {
            //???
        }

 
        public override void ResetWeapon()
        {
            meleeTrigger.enabled = false;
            proxy.Active = false;
            base.ResetWeapon();
        }

        private void FindTrigger()
        {
            if (meleeTrigger == null) meleeTrigger = GetComponent<Collider>();

            if (meleeTrigger)
            {
                proxy = TriggerProxy.CheckTriggerProxy(meleeTrigger, Layer, TriggerInteraction, Owner.transform);

                meleeTrigger.enabled = false;
                proxy.Active = meleeTrigger.enabled;
            }
            else
            {
                Debug.LogError($"Weapon [{name}] needs a collider. Please add one. Disabling Weapon", this);
                enabled = false;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (meleeTrigger)
            {
                MTools.DrawTriggers(meleeTrigger.transform, meleeTrigger, DebugColor);
            }
        }


        protected override void Reset()
        {
            base.Reset();

            weaponType = MTools.GetInstance<WeaponID>("Melee");
            m_rate.Value = 0.5f;
            m_Automatic.Value = true;
        }

        //[ContextMenu("Create Cinemachine Impulse")]
        //void CreateCinemachinePulse()
        //{
        //    var cinemachinePulse = GetComponent<Cinemachine.CinemachineImpulseSource>();
        //    if (cinemachinePulse == null)
        //    {
        //        cinemachinePulse = gameObject.AddComponent<Cinemachine.CinemachineImpulseSource>();
        //        MTools.SetDirty(gameObject);
        //    }

        //    cinemachinePulse.m_DefaultVelocity = Vector3.up * 0.1f;
        //    cinemachinePulse.m_ImpulseDefinition.m_ImpulseType = Cinemachine.CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        //    cinemachinePulse.m_ImpulseDefinition.m_ImpulseShape = Cinemachine.CinemachineImpulseDefinition.ImpulseShapes.Bump;
        //    cinemachinePulse.m_ImpulseDefinition.m_ImpulseDuration = 0.2f;
        //    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(OnHit, cinemachinePulse.GenerateImpulse);
        //    MTools.SetDirty(cinemachinePulse);
        //}

#endif
    }


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(MMelee))]
    public class MMeleeEditor : MWeaponEditor
    {
        SerializedProperty meleeCollider, OnCauseDamage, UseCameraSide, InvertCameraSide, Attacks;

        void OnEnable()
        {
            WeaponTab = "Melee";
            SetOnEnable();
            meleeCollider = serializedObject.FindProperty("meleeTrigger");
            OnCauseDamage = serializedObject.FindProperty("OnCauseDamage");
            InvertCameraSide = serializedObject.FindProperty("InvertCameraSide");
            UseCameraSide = serializedObject.FindProperty("UseCameraSide");
            Attacks = serializedObject.FindProperty("Attacks");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDescription("Melee Weapon Properties");
            if (meleeCollider.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Weapon needs a collider. Check [Melee] Tab", MessageType.Error);

            WeaponInspector();
           
            serializedObject.ApplyModifiedProperties();
        }

        protected override void UpdateSoundHelp()
        {
            SoundHelp = "0:Draw   1:Store   2:Swing   3:Hit \n (Leave 3 Empty, add SoundByMaterial and Invoke 'PlayMaterialSound' for custom Hit sounds)";
        }

        protected override void ChildWeaponEvents()
        {
            EditorGUILayout.PropertyField(OnCauseDamage, new GUIContent("On AttackTrigger Active"));
        }

        protected override void DrawAdvancedWeapon()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(meleeCollider, new GUIContent("Melee Trigger", "Gets the reference of where is the Melee Collider of this weapon (Not Always is in the same gameobject level)"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Riding Behaviour", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(Attacks, new GUIContent("Attacks", "Amount of Attack Animations can do on both sides while mounting"));
            EditorGUILayout.PropertyField(UseCameraSide, new GUIContent("Use Camera Side", "The Attacks are Activated by the Main Attack and It uses the Side of the Camera to Attack on the Right or the Left side of the Mount"));
            
            if (UseCameraSide.boolValue)
            EditorGUILayout.PropertyField(InvertCameraSide, new GUIContent("Invert Camera Side", "Inverts the camera side value"));
            EditorGUILayout.EndVertical();
        }
    }
#endif
}