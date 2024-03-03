//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Game/Gui/GameMenu/Controls/GameMenuControls.inputactions
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

namespace Gam.Gui.GameMenu
{
    public partial class @GameMenuControls: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @GameMenuControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""GameMenuControls"",
    ""maps"": [
        {
            ""name"": ""GameMenu"",
            ""id"": ""15e06027-1675-4000-885f-b4e159144c4a"",
            ""actions"": [
                {
                    ""name"": ""Toggle"",
                    ""type"": ""Button"",
                    ""id"": ""0e7d90af-32e0-42f1-91f9-80adb7e890ba"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""df110af0-acea-4794-a304-bf2ec437e7aa"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Toggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // GameMenu
            m_GameMenu = asset.FindActionMap("GameMenu", throwIfNotFound: true);
            m_GameMenu_Toggle = m_GameMenu.FindAction("Toggle", throwIfNotFound: true);
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

        // GameMenu
        private readonly InputActionMap m_GameMenu;
        private List<IGameMenuActions> m_GameMenuActionsCallbackInterfaces = new List<IGameMenuActions>();
        private readonly InputAction m_GameMenu_Toggle;
        public struct GameMenuActions
        {
            private @GameMenuControls m_Wrapper;
            public GameMenuActions(@GameMenuControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Toggle => m_Wrapper.m_GameMenu_Toggle;
            public InputActionMap Get() { return m_Wrapper.m_GameMenu; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(GameMenuActions set) { return set.Get(); }
            public void AddCallbacks(IGameMenuActions instance)
            {
                if (instance == null || m_Wrapper.m_GameMenuActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_GameMenuActionsCallbackInterfaces.Add(instance);
                @Toggle.started += instance.OnToggle;
                @Toggle.performed += instance.OnToggle;
                @Toggle.canceled += instance.OnToggle;
            }

            private void UnregisterCallbacks(IGameMenuActions instance)
            {
                @Toggle.started -= instance.OnToggle;
                @Toggle.performed -= instance.OnToggle;
                @Toggle.canceled -= instance.OnToggle;
            }

            public void RemoveCallbacks(IGameMenuActions instance)
            {
                if (m_Wrapper.m_GameMenuActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IGameMenuActions instance)
            {
                foreach (var item in m_Wrapper.m_GameMenuActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_GameMenuActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public GameMenuActions @GameMenu => new GameMenuActions(this);
        public interface IGameMenuActions
        {
            void OnToggle(InputAction.CallbackContext context);
        }
    }
}
