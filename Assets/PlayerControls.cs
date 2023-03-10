//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/PlayerControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Playing"",
            ""id"": ""c34a986a-9509-40ed-bab8-b6145bcb4512"",
            ""actions"": [
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""a1250ac1-f2d6-402f-a627-438e2de77668"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Attack"",
                    ""type"": ""Button"",
                    ""id"": ""64bc9741-a9ac-49cb-89fd-9e0073fa171d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Special"",
                    ""type"": ""Button"",
                    ""id"": ""f8857a73-3383-4e31-99b2-e62a686e04bd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Directional"",
                    ""type"": ""Value"",
                    ""id"": ""b7f96922-f047-42d5-a1b4-478684d3eb9b"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Dash"",
                    ""type"": ""Button"",
                    ""id"": ""e120126c-b953-4a4c-990f-a2758291ac7f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Heavy Attack"",
                    ""type"": ""Button"",
                    ""id"": ""e3502153-134c-4efc-be88-a71187d52821"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Movement"",
                    ""type"": ""Button"",
                    ""id"": ""359ab731-0107-4a45-8dae-b3ba007159bc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""50f55929-61b8-405f-a3f8-1e8f45c01bbd"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""69739f24-173b-46b4-bc32-fc1a03b16a8c"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be0777d9-ee78-47eb-8806-fd6258a2a220"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""82601a60-7082-496d-8356-01a5cec5e955"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""41f19a1d-d4d3-4862-982b-d0d62579f904"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Special"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""280db650-7292-4e78-9f17-f65661913fa9"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Special"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""01d4650c-fe12-4278-99ff-95a59d363ef2"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a3e8c3ae-7e19-4afa-bb84-fb0260969b50"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""70714f85-ed04-48a3-bd73-efb1a4753596"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""4fbf2102-5ec5-4f72-bb24-a08ca4cee796"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""246ce2dd-0bd7-4860-8935-1c988b15196f"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""bbadeeea-84da-404f-ad95-1930dd9cd96b"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""3211fe19-dd94-4793-951a-295cb6558779"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""ef939b54-5220-4283-b7a2-88c346895323"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""05d736da-4be9-4fbe-bf05-c32ee5aca9f7"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""05b3f0fe-2ff9-4079-9d33-d5325bf374e4"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Directional"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""5e6f86fd-6a0f-4e7d-8c34-422b757d05b4"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""14e1b5c7-7efe-4331-9607-776a8026142f"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ad262d02-d255-4c0f-b53f-821ea40292df"",
                    ""path"": ""<Keyboard>/u"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Heavy Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""399c41ba-2b04-4e83-9e91-4399d06e9a7f"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Heavy Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""677fafe7-30f5-484b-a63c-1094b99ef6c3"",
                    ""path"": ""<Keyboard>/o"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b12dd2ec-ab78-4746-ba3a-c85a258c3c4a"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""DefaultControls"",
            ""bindingGroup"": ""DefaultControls"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": true,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Playing
        m_Playing = asset.FindActionMap("Playing", throwIfNotFound: true);
        m_Playing_Jump = m_Playing.FindAction("Jump", throwIfNotFound: true);
        m_Playing_Attack = m_Playing.FindAction("Attack", throwIfNotFound: true);
        m_Playing_Special = m_Playing.FindAction("Special", throwIfNotFound: true);
        m_Playing_Directional = m_Playing.FindAction("Directional", throwIfNotFound: true);
        m_Playing_Dash = m_Playing.FindAction("Dash", throwIfNotFound: true);
        m_Playing_HeavyAttack = m_Playing.FindAction("Heavy Attack", throwIfNotFound: true);
        m_Playing_Movement = m_Playing.FindAction("Movement", throwIfNotFound: true);
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
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Playing
    private readonly InputActionMap m_Playing;
    private IPlayingActions m_PlayingActionsCallbackInterface;
    private readonly InputAction m_Playing_Jump;
    private readonly InputAction m_Playing_Attack;
    private readonly InputAction m_Playing_Special;
    private readonly InputAction m_Playing_Directional;
    private readonly InputAction m_Playing_Dash;
    private readonly InputAction m_Playing_HeavyAttack;
    private readonly InputAction m_Playing_Movement;
    public struct PlayingActions
    {
        private @PlayerControls m_Wrapper;
        public PlayingActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Jump => m_Wrapper.m_Playing_Jump;
        public InputAction @Attack => m_Wrapper.m_Playing_Attack;
        public InputAction @Special => m_Wrapper.m_Playing_Special;
        public InputAction @Directional => m_Wrapper.m_Playing_Directional;
        public InputAction @Dash => m_Wrapper.m_Playing_Dash;
        public InputAction @HeavyAttack => m_Wrapper.m_Playing_HeavyAttack;
        public InputAction @Movement => m_Wrapper.m_Playing_Movement;
        public InputActionMap Get() { return m_Wrapper.m_Playing; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayingActions set) { return set.Get(); }
        public void SetCallbacks(IPlayingActions instance)
        {
            if (m_Wrapper.m_PlayingActionsCallbackInterface != null)
            {
                @Jump.started -= m_Wrapper.m_PlayingActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayingActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayingActionsCallbackInterface.OnJump;
                @Attack.started -= m_Wrapper.m_PlayingActionsCallbackInterface.OnAttack;
                @Attack.performed -= m_Wrapper.m_PlayingActionsCallbackInterface.OnAttack;
                @Attack.canceled -= m_Wrapper.m_PlayingActionsCallbackInterface.OnAttack;
                @Special.started -= m_Wrapper.m_PlayingActionsCallbackInterface.OnSpecial;
                @Special.performed -= m_Wrapper.m_PlayingActionsCallbackInterface.OnSpecial;
                @Special.canceled -= m_Wrapper.m_PlayingActionsCallbackInterface.OnSpecial;
                @Directional.started -= m_Wrapper.m_PlayingActionsCallbackInterface.OnDirectional;
                @Directional.performed -= m_Wrapper.m_PlayingActionsCallbackInterface.OnDirectional;
                @Directional.canceled -= m_Wrapper.m_PlayingActionsCallbackInterface.OnDirectional;
                @Dash.started -= m_Wrapper.m_PlayingActionsCallbackInterface.OnDash;
                @Dash.performed -= m_Wrapper.m_PlayingActionsCallbackInterface.OnDash;
                @Dash.canceled -= m_Wrapper.m_PlayingActionsCallbackInterface.OnDash;
                @HeavyAttack.started -= m_Wrapper.m_PlayingActionsCallbackInterface.OnHeavyAttack;
                @HeavyAttack.performed -= m_Wrapper.m_PlayingActionsCallbackInterface.OnHeavyAttack;
                @HeavyAttack.canceled -= m_Wrapper.m_PlayingActionsCallbackInterface.OnHeavyAttack;
                @Movement.started -= m_Wrapper.m_PlayingActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_PlayingActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_PlayingActionsCallbackInterface.OnMovement;
            }
            m_Wrapper.m_PlayingActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Attack.started += instance.OnAttack;
                @Attack.performed += instance.OnAttack;
                @Attack.canceled += instance.OnAttack;
                @Special.started += instance.OnSpecial;
                @Special.performed += instance.OnSpecial;
                @Special.canceled += instance.OnSpecial;
                @Directional.started += instance.OnDirectional;
                @Directional.performed += instance.OnDirectional;
                @Directional.canceled += instance.OnDirectional;
                @Dash.started += instance.OnDash;
                @Dash.performed += instance.OnDash;
                @Dash.canceled += instance.OnDash;
                @HeavyAttack.started += instance.OnHeavyAttack;
                @HeavyAttack.performed += instance.OnHeavyAttack;
                @HeavyAttack.canceled += instance.OnHeavyAttack;
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
            }
        }
    }
    public PlayingActions @Playing => new PlayingActions(this);
    private int m_DefaultControlsSchemeIndex = -1;
    public InputControlScheme DefaultControlsScheme
    {
        get
        {
            if (m_DefaultControlsSchemeIndex == -1) m_DefaultControlsSchemeIndex = asset.FindControlSchemeIndex("DefaultControls");
            return asset.controlSchemes[m_DefaultControlsSchemeIndex];
        }
    }
    public interface IPlayingActions
    {
        void OnJump(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnSpecial(InputAction.CallbackContext context);
        void OnDirectional(InputAction.CallbackContext context);
        void OnDash(InputAction.CallbackContext context);
        void OnHeavyAttack(InputAction.CallbackContext context);
        void OnMovement(InputAction.CallbackContext context);
    }
}
