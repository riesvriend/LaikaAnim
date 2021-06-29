// GENERATED AUTOMATICALLY FROM 'Assets/PlayerInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInput : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""DogControls"",
            ""id"": ""7561e4e4-f331-45a6-9f41-a165d1bd6480"",
            ""actions"": [
                {
                    ""name"": ""Up"",
                    ""type"": ""Value"",
                    ""id"": ""82a39427-51f1-46a3-b30a-9dd6a7fd9c10"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Down"",
                    ""type"": ""Value"",
                    ""id"": ""37bb8c37-79c3-455f-8af5-82783b4757af"",
                    ""expectedControlType"": ""Key"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""VerticalAcceleleration"",
                    ""type"": ""Value"",
                    ""id"": ""1c8900d9-90cf-4362-b8e5-ab1bb05f38c8"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""fab5ae0d-ef16-42c4-bc97-ee2f6da6fb0f"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d6f83aea-6274-48e0-9920-276e48f0a0bc"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f6bb8bf8-5fcf-485c-aae5-53d5c338258a"",
                    ""path"": ""<LinearAccelerationSensor>/acceleration/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""VerticalAcceleleration"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // DogControls
        m_DogControls = asset.FindActionMap("DogControls", throwIfNotFound: true);
        m_DogControls_Up = m_DogControls.FindAction("Up", throwIfNotFound: true);
        m_DogControls_Down = m_DogControls.FindAction("Down", throwIfNotFound: true);
        m_DogControls_VerticalAcceleleration = m_DogControls.FindAction("VerticalAcceleleration", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // DogControls
    private readonly InputActionMap m_DogControls;
    private IDogControlsActions m_DogControlsActionsCallbackInterface;
    private readonly InputAction m_DogControls_Up;
    private readonly InputAction m_DogControls_Down;
    private readonly InputAction m_DogControls_VerticalAcceleleration;
    public struct DogControlsActions
    {
        private @PlayerInput m_Wrapper;
        public DogControlsActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Up => m_Wrapper.m_DogControls_Up;
        public InputAction @Down => m_Wrapper.m_DogControls_Down;
        public InputAction @VerticalAcceleleration => m_Wrapper.m_DogControls_VerticalAcceleleration;
        public InputActionMap Get() { return m_Wrapper.m_DogControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DogControlsActions set) { return set.Get(); }
        public void SetCallbacks(IDogControlsActions instance)
        {
            if (m_Wrapper.m_DogControlsActionsCallbackInterface != null)
            {
                @Up.started -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnUp;
                @Up.performed -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnUp;
                @Up.canceled -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnUp;
                @Down.started -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnDown;
                @Down.performed -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnDown;
                @Down.canceled -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnDown;
                @VerticalAcceleleration.started -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnVerticalAcceleleration;
                @VerticalAcceleleration.performed -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnVerticalAcceleleration;
                @VerticalAcceleleration.canceled -= m_Wrapper.m_DogControlsActionsCallbackInterface.OnVerticalAcceleleration;
            }
            m_Wrapper.m_DogControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Up.started += instance.OnUp;
                @Up.performed += instance.OnUp;
                @Up.canceled += instance.OnUp;
                @Down.started += instance.OnDown;
                @Down.performed += instance.OnDown;
                @Down.canceled += instance.OnDown;
                @VerticalAcceleleration.started += instance.OnVerticalAcceleleration;
                @VerticalAcceleleration.performed += instance.OnVerticalAcceleleration;
                @VerticalAcceleleration.canceled += instance.OnVerticalAcceleleration;
            }
        }
    }
    public DogControlsActions @DogControls => new DogControlsActions(this);
    public interface IDogControlsActions
    {
        void OnUp(InputAction.CallbackContext context);
        void OnDown(InputAction.CallbackContext context);
        void OnVerticalAcceleleration(InputAction.CallbackContext context);
    }
}
