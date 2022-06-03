using MalbersAnimations.Events;
using MalbersAnimations.Scriptables; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Effect Manager")]
    public class EffectManager : MonoBehaviour, IAnimatorListener
    {
        [RequiredField, Tooltip("Root Gameobject of the Hierarchy")]
        public Transform Owner;

        public List<Effect> Effects;

        private void Awake()
        {
            foreach (var e in Effects)
            {
                e.Initialize();
            }
        }

        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void PlayEffect(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID && effect.active == true);

            if (effects != null)
                foreach (var effect in effects) Play(effect);
        }


        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void PlayEffect(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name == name && effect.active == true);

            if (effects != null)
                foreach (var effect in effects) Play(effect);
        }

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void StopEffect(int ID) => Effect_Stop(ID);

        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void Effect_Play(int ID) => PlayEffect(ID);
        public virtual void EffectPlay(int ID) => PlayEffect(ID);
        public virtual void Effect_Play(string name) => PlayEffect(name);
        public virtual void EffectPlay(string name) => PlayEffect(name);

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void Effect_Stop(int ID)
        {
            var effects = Effects.FindAll(effect => effect.ID == ID && effect.active == true);

            Stop_Effects(effects);
        }

        private void Stop_Effects(List<Effect> effects)
        {
            if (effects != null)
            {
                foreach (var e in effects)
                {
                    e.Modifier?.StopEffect(e);              //Play Modifier when the effect play
                    e.OnStop.Invoke();

                    if (!e.effect.IsPrefab())
                    {
                      if (e.disableOnStop)  e.Instance?.SetActive(false);
                    }
                    else
                        Destroy(e.Instance);
                }
            }
        }
      

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void Effect_Stop(string name)
        {
            var effects = Effects.FindAll(effect => effect.Name == name && effect.active == true);
            Stop_Effects(effects);
        }

        private IEnumerator Life(Effect e)
        {
            if (e.life > 0)
            {
                yield return new WaitForSeconds(e.life);

                e.Modifier?.StopEffect(e);              //Play Modifier when the effect play
                e.OnStop.Invoke();

                if (e.effect.IsPrefab())
                {
                    Destroy(e.Instance);       //Means the effect is a Prefab destroy the Instance
                }
                else
                {
                  if (e.disableOnStop)  e.effect.SetActive(false);
                }
            }

            yield return null;
        }

        protected virtual void Play(Effect e)
        {
            e.Modifier?.PreStart(e);        //Execute the Method PreStart Effect if it has a modifier

            //Delay an action
            this.Delay_Action(e.delay,
                () =>
                { 
                    //Play Audio
                    if (!e.Clip.NullOrEmpty() && e.audioSource != null)
                    {
                        e.audioSource.clip = e.Clip.GetValue();
                        if (e.audioSource.isPlaying) e.audioSource.Stop();
                        e.audioSource.Play();
                    }

                    if (e.effect != null)
                    {
                        if (e.effect.IsPrefab())                        //If instantiate is active (meaning is a prefab)
                        {
                            e.Instance = Instantiate(e.effect);         //Instantiate!
                            e.Instance.gameObject.SetActive(false);
                        }
                        else
                        {
                            e.Instance = e.effect;                     //Use the effect as the gameobject
                        }

                        if (Owner == null) Owner = transform.root;
                        if (e.Owner == null) e.Owner = Owner;  //Save in all effects that the owner of the effects is this transform


                        if (e.Instance)
                        {
                            e.Instance.gameObject.SetActive(true);
                            e.Instance.transform.localScale = Vector3.Scale(e.Instance.transform.localScale, e.ScaleMultiplier); //Scale the Effect


                            //Apply Offsets
                            if (e.root)
                            {
                                e.Instance.transform.position = e.root.position;

                                if (e.isChild)
                                {
                                    e.Instance.transform.parent = e.root;

                                    e.Instance.transform.localPosition += e.PositionOffset;
                                    e.Instance.transform.localRotation *= Quaternion.Euler(e.RotationOffset);
                                }
                                else
                                {
                                    e.Instance.transform.position = e.root.TransformPoint(e.PositionOffset);
                                }

                                if (e.useRootRotation) e.Instance.transform.rotation = e.root.rotation;     //Orient to the root rotation

                            }


                            if (e.effect.IsPrefab()) //get the trailrenderer and particle system from the Instance instead of the prefab
                            {
                                e.IsTrailRenderer = e.Instance.FindComponent<TrailRenderer>();
                                e.IsParticleSystem = e.Instance.FindComponent<ParticleSystem>();
                            }

                            if (e.IsTrailRenderer) e.IsTrailRenderer.Clear();
                            if (e.IsParticleSystem) e.IsParticleSystem.Play();

                            if (e.Modifier) e.Modifier.StartEffect(e);              //Apply  Modifier when the effect play

                            StartCoroutine(Life(e));
                        }
                    }
                    e.OnPlay.Invoke();                                      //Invoke the Play Event
                }
            ); 
        }


        /// <summary>IAnimatorListener function </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        //─────────────────────────────────CALLBACKS METHODS───────────────────────────────────────────────────────────────────

        /// <summary>Disables all effects using their name </summary>
        public virtual void Effect_Disable(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name.ToUpper() == name.ToUpper());

            if (effects != null)
            {
                foreach (var e in effects) e.active = false;
            }
            else
            {
                Debug.LogWarning("No effect with the name: " + name + " was found");
            }
        }

        /// <summary> Disables all effects using their ID</summary>
        public virtual void Effect_Disable(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID);

            if (effects != null)
            {
                foreach (var e in effects) e.active = false;
            }
            else
            {
                Debug.LogWarning("No effect with the ID: " + ID + " was found");
            }
        }

        /// <summary>Enable all effects using their name</summary>
        public virtual void Effect_Enable(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name.ToUpper() == name.ToUpper());

            if (effects != null)
            {
                foreach (var e in effects) e.active = true;
            }
            else
            {
                Debug.LogWarning("No effect with the name: " + name + " was found");
            }
        }


        /// <summary> Enable all effects using their ID</summary>
        public virtual void Effect_Enable(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID);

            if (effects != null)
            {
                foreach (var e in effects) e.active = true;
            }
            else
            {
                Debug.LogWarning("No effect with the ID: " + ID + " was found");
            }
        }

        private void Reset()
        {
            Owner = transform.root;
        }

#if UNITY_EDITOR
        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {
            MEventListener listener = gameObject.FindComponent<MEventListener>();

            if (listener == null) listener = gameObject.AddComponent<MEventListener>();
            if (listener.Events == null) listener.Events = new List<MEventItemListener>();

            MEvent effectEnable = MTools.GetInstance<MEvent>("Effect Enable");
            MEvent effectDisable = MTools.GetInstance<MEvent>("Effect Disable");

            if (listener.Events.Find(item => item.Event == effectEnable) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = effectEnable,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Effect_Enable);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, Effect_Enable);
                listener.Events.Add(item);

                Debug.Log("<B>Effect Enable</B> Added to the Event Listeners");
            }

            if (listener.Events.Find(item => item.Event == effectDisable) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = effectDisable,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Effect_Disable);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, Effect_Disable);
                listener.Events.Add(item);

                Debug.Log("<B>Effect Disable</B> Added to the Event Listeners");
            }

            UnityEditor.EditorUtility.SetDirty(listener);
        }
#endif


    }

    [System.Serializable]
    public class Effect
    {
        public string Name = "EffectName";
        public int ID;
        public bool active = true;
        public Transform root;

        public bool isChild;
        public bool disableOnStop = true;
        public bool useRootRotation = true;
        public GameObject effect;
        public Vector3 RotationOffset;
        public Vector3 PositionOffset;
        public Vector3 ScaleMultiplier = Vector3.one;
        public AudioSource audioSource;
        public AudioClipReference Clip;

        /// <summary>Life of the Effect</summary>
        public float life = 10f;

        /// <summary>Delay Time to execute the effect after is called.</summary>
        public float delay;

        /// <summary>Scriptable Object to Modify anything you want before, during or after the effect is invoked</summary>
        public EffectModifier Modifier;


        public UnityEvent OnPlay;
        public UnityEvent OnStop;

        /// <summary>Returns the Owner of the Effect </summary>
        public Transform Owner { get; set; }

        /// <summary>Returns the Instance of the Effect Prefab </summary>
        public GameObject Instance { get => instance; set => instance = value; }

        public TrailRenderer IsTrailRenderer { get; set; }
        public ParticleSystem IsParticleSystem { get; set; }

        [System.NonSerialized]
        private GameObject instance;

        internal void Initialize()
        {
            if (effect != null && !effect.IsPrefab()) //Store if the effect its not a prefab
            {
                effect.gameObject.SetActive(false); //Deactivate at start
                IsTrailRenderer = effect.FindComponent<TrailRenderer>();
                IsParticleSystem = effect.FindComponent<ParticleSystem>();
            }
        }
    }
}