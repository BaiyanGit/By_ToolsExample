//=====================================================
// 文件名称: UnityInputSystemBackendAdapter.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 将 Unity Input System 适配为 InputSystem 内部后端。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
#endif

    internal sealed class UnityInputSystemBackendAdapter : IInputBackend, IDisposable
    {
        private readonly object _syncRoot = new();
        private readonly Dictionary<string, InputActionDescriptor> _actions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, InputActionState> _states = new(StringComparer.Ordinal);
        private readonly InputProfileSnapshot _profileSnapshot;
        private readonly string _providerType;
        private bool _isAvailable;

#if ENABLE_INPUT_SYSTEM
        private readonly InputActionAsset _asset;
        private readonly List<InputAction> _subscribedActions = new();
#endif

        public UnityInputSystemBackendAdapter(
#if ENABLE_INPUT_SYSTEM
            InputActionAsset asset,
#else
            ScriptableObject asset,
#endif
            string profileId = "UnityInputSystem",
            int version = 1,
            bool autoEnable = true,
            string providerType = "UnityInputSystem")
        {
            _providerType = string.IsNullOrWhiteSpace(providerType) ? "UnityInputSystem" : providerType.Trim();

#if ENABLE_INPUT_SYSTEM
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            _asset = asset;
            _profileSnapshot = BuildProfileSnapshot(profileId, version, asset);
            RegisterActions(asset);
            if (autoEnable)
            {
                asset.Enable();
            }

            Subscribe(asset);
            _isAvailable = true;
#else
            _profileSnapshot = new InputProfileSnapshot(profileId, version, Array.Empty<InputBindingDescriptor>());
            _isAvailable = false;
#endif
        }

        public event Action<InputBackendSignal> SignalPublished;

        public bool IsAvailable
        {
            get
            {
                lock (_syncRoot)
                {
                    return _isAvailable;
                }
            }
        }

        public string ProviderType => _providerType;

        public IReadOnlyList<InputActionDescriptor> GetAvailableActions()
        {
            lock (_syncRoot)
            {
                List<InputActionDescriptor> actions = new(_actions.Values);
                actions.Sort((left, right) => string.CompareOrdinal(left.ActionId.Value, right.ActionId.Value));
                return actions.AsReadOnly();
            }
        }

        public bool TryGetActionDescriptor(InputActionId actionId, out InputActionDescriptor descriptor)
        {
            lock (_syncRoot)
            {
                return _actions.TryGetValue(actionId.Value, out descriptor);
            }
        }

        public bool TryGetActionState(InputActionId actionId, out InputActionState state)
        {
            lock (_syncRoot)
            {
                return _states.TryGetValue(actionId.Value, out state);
            }
        }

        public void SetResolvedState(InputActionId actionId, InputStage stage, InputValueKind valueKind, bool isAvailable)
        {
            lock (_syncRoot)
            {
                _states[actionId.Value] = new InputActionState(actionId, stage, valueKind, isAvailable);
            }
        }

        public IReadOnlyList<InputDeviceDescriptor> GetDevices()
        {
#if ENABLE_INPUT_SYSTEM
            List<InputDeviceDescriptor> devices = new();
            foreach (InputDevice device in InputSystem.devices)
            {
                devices.Add(new InputDeviceDescriptor(
                    device.deviceId.ToString(),
                    MapDeviceKind(device),
                    _providerType,
                    device.added));
            }

            return devices.AsReadOnly();
#else
            return Array.Empty<InputDeviceDescriptor>();
#endif
        }

        public InputProfileSnapshot GetProfileSnapshot()
        {
            return _profileSnapshot;
        }

        public bool HasConnectedDevice()
        {
#if ENABLE_INPUT_SYSTEM
            if (InputSystem.devices.Count == 0)
            {
                return true;
            }

            foreach (InputDevice device in InputSystem.devices)
            {
                if (device.added)
                {
                    return true;
                }
            }

            return false;
#else
            return false;
#endif
        }

        public void Dispose()
        {
#if ENABLE_INPUT_SYSTEM
            Unsubscribe();
#endif
        }

        internal void SetProviderAvailable(bool isAvailable)
        {
            lock (_syncRoot)
            {
                _isAvailable = isAvailable;
            }
        }

#if ENABLE_INPUT_SYSTEM
        private void RegisterActions(InputActionAsset asset)
        {
            foreach (InputActionMap map in asset.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    InputActionId actionId = BuildActionId(map.name, action.name);
                    InputActionDescriptor descriptor = new(
                        actionId,
                        map.name,
                        MapValueKind(action.expectedControlType),
                        true,
                        true);

                    _actions[actionId.Value] = descriptor;
                    _states[actionId.Value] = new InputActionState(
                        actionId,
                        InputStage.Canceled,
                        descriptor.ValueKind,
                        false);
                }
            }
        }

        private void Subscribe(InputActionAsset asset)
        {
            foreach (InputActionMap map in asset.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    action.started += OnActionStarted;
                    action.performed += OnActionPerformed;
                    action.canceled += OnActionCanceled;
                    _subscribedActions.Add(action);
                }
            }
        }

        private void Unsubscribe()
        {
            for (int index = 0; index < _subscribedActions.Count; index++)
            {
                InputAction action = _subscribedActions[index];
                action.started -= OnActionStarted;
                action.performed -= OnActionPerformed;
                action.canceled -= OnActionCanceled;
            }

            _subscribedActions.Clear();
        }

        private void OnActionStarted(InputAction.CallbackContext context)
        {
            Publish(context, InputStage.Started);
        }

        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            Publish(context, InputStage.Performed);
        }

        private void OnActionCanceled(InputAction.CallbackContext context)
        {
            Publish(context, InputStage.Canceled);
        }

        private void Publish(InputAction.CallbackContext context, InputStage stage)
        {
            if (context.action == null || context.action.actionMap == null)
            {
                return;
            }

            InputActionId actionId = BuildActionId(context.action.actionMap.name, context.action.name);
            if (!_actions.TryGetValue(actionId.Value, out InputActionDescriptor descriptor))
            {
                return;
            }

            InputBackendSignal signal = new(actionId, stage, descriptor.ValueKind);
            lock (_syncRoot)
            {
                _states[actionId.Value] = new InputActionState(actionId, stage, descriptor.ValueKind, false);
            }

            SignalPublished?.Invoke(signal);
        }

        private static InputProfileSnapshot BuildProfileSnapshot(string profileId, int version, InputActionAsset asset)
        {
            List<InputBindingDescriptor> bindings = new();
            foreach (InputActionMap map in asset.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    InputActionId actionId = BuildActionId(map.name, action.name);
                    foreach (InputBinding binding in action.bindings)
                    {
                        bindings.Add(new InputBindingDescriptor(
                            actionId,
                            MapDeviceKind(binding),
                            binding.path,
                            binding.isComposite));
                    }
                }
            }

            return new InputProfileSnapshot(profileId, version, bindings);
        }

        private static InputActionId BuildActionId(string scope, string actionName)
        {
            return new InputActionId(string.Concat(scope?.Trim() ?? string.Empty, ".", actionName?.Trim() ?? string.Empty));
        }

        private static InputValueKind MapValueKind(string expectedControlType)
        {
            if (string.IsNullOrWhiteSpace(expectedControlType))
            {
                return InputValueKind.Button;
            }

            return expectedControlType switch
            {
                "Vector2" => InputValueKind.Axis2D,
                "Axis" => InputValueKind.Axis1D,
                "Button" => InputValueKind.Button,
                "Stick" => InputValueKind.Axis2D,
                "Pointer" => InputValueKind.Pointer,
                "Text" => InputValueKind.Text,
                _ => InputValueKind.Button,
            };
        }

        private static InputDeviceKind MapDeviceKind(InputBinding binding)
        {
            string path = binding.path ?? string.Empty;
            if (path.Contains("<Keyboard>", StringComparison.OrdinalIgnoreCase))
            {
                return InputDeviceKind.Keyboard;
            }

            if (path.Contains("<Mouse>", StringComparison.OrdinalIgnoreCase))
            {
                return InputDeviceKind.Mouse;
            }

            if (path.Contains("<Gamepad>", StringComparison.OrdinalIgnoreCase))
            {
                return InputDeviceKind.Gamepad;
            }

            if (path.Contains("<XRController>", StringComparison.OrdinalIgnoreCase))
            {
                return InputDeviceKind.VRController;
            }

            return InputDeviceKind.CustomDevice;
        }

        private static InputDeviceKind MapDeviceKind(InputDevice device)
        {
            return device switch
            {
                Keyboard => InputDeviceKind.Keyboard,
                Mouse => InputDeviceKind.Mouse,
                Gamepad => InputDeviceKind.Gamepad,
                _ => InputDeviceKind.CustomDevice,
            };
        }
#endif
    }
}
