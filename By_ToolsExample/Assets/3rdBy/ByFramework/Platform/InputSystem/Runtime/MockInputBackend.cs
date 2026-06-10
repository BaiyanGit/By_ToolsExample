//=====================================================
// 文件名称: MockInputBackend.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 提供 InputSystem 基础模拟后端。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;
    using System.Collections.Generic;

    internal sealed class MockInputBackend : IInputBackend
    {
        private readonly object _syncRoot = new();
        private readonly Dictionary<string, InputActionDescriptor> _actions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, InputActionState> _states = new(StringComparer.Ordinal);
        private readonly List<InputDeviceDescriptor> _devices = new();
        private InputProfileSnapshot _profile = new("MockProfile", 1, Array.Empty<InputBindingDescriptor>());
        private bool _isAvailable = true;

        public event Action<InputBackendSignal> SignalPublished;

        public string ProviderType => "Mock";

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

        public void RegisterAction(InputActionDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            lock (_syncRoot)
            {
                _actions[descriptor.ActionId.Value] = descriptor;
                _states[descriptor.ActionId.Value] = new InputActionState(
                    descriptor.ActionId,
                    InputStage.Canceled,
                    descriptor.ValueKind,
                    false);
            }
        }

        public void SetProfile(InputProfileSnapshot profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            lock (_syncRoot)
            {
                _profile = profile;
            }
        }

        public void SetDevices(IReadOnlyList<InputDeviceDescriptor> devices)
        {
            lock (_syncRoot)
            {
                _devices.Clear();
                if (devices == null)
                {
                    return;
                }

                for (int index = 0; index < devices.Count; index++)
                {
                    if (devices[index] != null)
                    {
                        _devices.Add(devices[index]);
                    }
                }
            }
        }

        public void SetProviderAvailable(bool isAvailable)
        {
            lock (_syncRoot)
            {
                _isAvailable = isAvailable;
            }
        }

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

        public IReadOnlyList<InputDeviceDescriptor> GetDevices()
        {
            lock (_syncRoot)
            {
                return new List<InputDeviceDescriptor>(_devices).AsReadOnly();
            }
        }

        public InputProfileSnapshot GetProfileSnapshot()
        {
            lock (_syncRoot)
            {
                return _profile;
            }
        }

        public bool HasConnectedDevice()
        {
            lock (_syncRoot)
            {
                if (_devices.Count == 0)
                {
                    return true;
                }

                for (int index = 0; index < _devices.Count; index++)
                {
                    if (_devices[index].IsConnected)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool TryPublishAction(InputActionId actionId, InputStage stage)
        {
            InputBackendSignal signal;

            lock (_syncRoot)
            {
                if (!_actions.TryGetValue(actionId.Value, out InputActionDescriptor descriptor))
                {
                    return false;
                }

                signal = new InputBackendSignal(actionId, stage, descriptor.ValueKind);
                _states[actionId.Value] = new InputActionState(actionId, stage, descriptor.ValueKind, false);
            }

            SignalPublished?.Invoke(signal);
            return true;
        }

        public void SetResolvedState(InputActionId actionId, InputStage stage, InputValueKind valueKind, bool isAvailable)
        {
            lock (_syncRoot)
            {
                _states[actionId.Value] = new InputActionState(actionId, stage, valueKind, isAvailable);
            }
        }
    }

}
