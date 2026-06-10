//=====================================================
// 文件名称: InputService.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 实现输入动作解析、上下文管理与快照构建。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;
    using System.Collections.Generic;

    public sealed class InputService : IInputService
    {
        private readonly object _syncRoot = new();
        private readonly IInputBackend _backend;
        private readonly Dictionary<string, ActiveContext> _contexts = new(StringComparer.Ordinal);
        private long _nextContextSequence;

        public InputService()
            : this(new MockInputBackend())
        {
        }

        internal InputService(MockInputBackend backend)
            : this((IInputBackend)(backend ?? new MockInputBackend()))
        {
        }

        internal InputService(IInputBackend backend)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
            _backend.SignalPublished += OnSignalPublished;
        }

        public event Action<InputActionEvent> ActionTriggered;

        public InputContextSnapshot GetCurrentContextSnapshot()
        {
            lock (_syncRoot)
            {
                return BuildSnapshot();
            }
        }

        public IReadOnlyList<InputContextHandle> GetActiveContexts()
        {
            lock (_syncRoot)
            {
                List<ActiveContext> orderedContexts = GetOrderedContexts();
                List<InputContextHandle> handles = new(orderedContexts.Count);
                for (int index = 0; index < orderedContexts.Count; index++)
                {
                    handles.Add(orderedContexts[index].Handle);
                }

                return handles.AsReadOnly();
            }
        }

        public IReadOnlyList<InputActionDescriptor> GetAvailableActions()
        {
            return _backend.GetAvailableActions();
        }

        public bool TryGetActionDescriptor(InputActionId actionId, out InputActionDescriptor descriptor)
        {
            if (actionId.IsEmpty)
            {
                throw new ArgumentException("[InputSystem] ActionId cannot be empty.", nameof(actionId));
            }

            return _backend.TryGetActionDescriptor(actionId, out descriptor);
        }

        public bool IsActionAvailable(InputActionId actionId)
        {
            if (actionId.IsEmpty)
            {
                throw new ArgumentException("[InputSystem] ActionId cannot be empty.", nameof(actionId));
            }

            lock (_syncRoot)
            {
                ResolutionResult resolution = ResolveActionCore(actionId);
                return resolution.Status == InputActionResolveStatus.Success;
            }
        }

        public InputContextHandle ActivateContext(InputContextRegistration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (string.IsNullOrWhiteSpace(registration.ContextId))
            {
                throw new ArgumentException("[InputSystem] ContextId cannot be empty.", nameof(registration));
            }

            if (string.IsNullOrWhiteSpace(registration.OwnerId))
            {
                throw new ArgumentException("[InputSystem] OwnerId cannot be empty.", nameof(registration));
            }

            lock (_syncRoot)
            {
                ValidateAllowedActions(registration.AllowedActions);
                long sequence = _nextContextSequence++;
                string handleValue = string.Concat(registration.OwnerId, ":", registration.ContextId, ":", sequence);
                InputContextHandle handle = new(handleValue);
                ActiveContext context = new(handle, registration, sequence);
                _contexts[handle.Value] = context;
                RefreshActionAvailability();
                return handle;
            }
        }

        public bool TryReleaseContext(InputContextHandle handle)
        {
            if (handle.IsEmpty)
            {
                return false;
            }

            lock (_syncRoot)
            {
                if (!_contexts.Remove(handle.Value))
                {
                    return false;
                }

                RefreshActionAvailability();
                return true;
            }
        }

        public bool TryResolveActionState(InputActionId actionId, out InputActionState state)
        {
            if (actionId.IsEmpty)
            {
                throw new ArgumentException("[InputSystem] ActionId cannot be empty.", nameof(actionId));
            }

            lock (_syncRoot)
            {
                if (!_backend.TryGetActionState(actionId, out InputActionState backendState))
                {
                    state = null;
                    return false;
                }

                ResolutionResult resolution = ResolveActionCore(actionId);
                state = new InputActionState(
                    backendState.ActionId,
                    backendState.Stage,
                    backendState.ValueKind,
                    resolution.Status == InputActionResolveStatus.Success);
                return true;
            }
        }

        private void OnSignalPublished(InputBackendSignal signal)
        {
            InputActionEvent actionEvent;

            lock (_syncRoot)
            {
                ResolutionResult resolution = ResolveActionCore(signal.ActionId);
                _backend.SetResolvedState(
                    signal.ActionId,
                    signal.Stage,
                    signal.ValueKind,
                    resolution.Status == InputActionResolveStatus.Success);

                if (resolution.Status != InputActionResolveStatus.Success)
                {
                    return;
                }

                actionEvent = new InputActionEvent(
                    signal.ActionId,
                    signal.Stage,
                    signal.ValueKind,
                    resolution.ContextHandle,
                    resolution.Status);
            }

            ActionTriggered?.Invoke(actionEvent);
        }

        private InputContextSnapshot BuildSnapshot()
        {
            List<ActiveContext> orderedContexts = GetOrderedContexts();
            List<InputContextHandle> handles = new(orderedContexts.Count);
            Dictionary<string, InputActionId> resolvableActions = new(StringComparer.Ordinal);
            IReadOnlyList<InputActionDescriptor> descriptors = _backend.GetAvailableActions();

            for (int index = 0; index < orderedContexts.Count; index++)
            {
                handles.Add(orderedContexts[index].Handle);
            }

            for (int index = 0; index < descriptors.Count; index++)
            {
                ResolutionResult resolution = ResolveActionCore(descriptors[index].ActionId);
                if (resolution.Status == InputActionResolveStatus.Success)
                {
                    resolvableActions[descriptors[index].ActionId.Value] = descriptors[index].ActionId;
                }
            }

            List<InputActionId> orderedActions = new(resolvableActions.Values);
            orderedActions.Sort((left, right) => string.CompareOrdinal(left.Value, right.Value));
            return new InputContextSnapshot(handles.AsReadOnly(), orderedActions.AsReadOnly());
        }

        private void RefreshActionAvailability()
        {
            IReadOnlyList<InputActionDescriptor> descriptors = _backend.GetAvailableActions();
            for (int index = 0; index < descriptors.Count; index++)
            {
                ResolutionResult resolution = ResolveActionCore(descriptors[index].ActionId);
                if (_backend.TryGetActionState(descriptors[index].ActionId, out InputActionState state))
                {
                    _backend.SetResolvedState(
                        state.ActionId,
                        state.Stage,
                        state.ValueKind,
                        resolution.Status == InputActionResolveStatus.Success);
                }
            }
        }

        private ResolutionResult ResolveActionCore(InputActionId actionId)
        {
            if (!_backend.TryGetActionDescriptor(actionId, out InputActionDescriptor descriptor))
            {
                return ResolutionResult.ActionNotFound(descriptor);
            }

            if (!_backend.IsAvailable)
            {
                return ResolutionResult.ProviderFailure(descriptor);
            }

            if (!_backend.HasConnectedDevice())
            {
                return ResolutionResult.DeviceUnavailable(descriptor);
            }

            List<ActiveContext> orderedContexts = GetOrderedContexts();
            if (orderedContexts.Count == 0)
            {
                return ResolutionResult.ContextNotFound(descriptor);
            }

            for (int index = 0; index < orderedContexts.Count; index++)
            {
                ActiveContext context = orderedContexts[index];
                if (context.Allows(actionId))
                {
                    return ResolutionResult.Success(context.Handle, descriptor);
                }

                if (context.Registration.Exclusive && !context.Registration.PassThrough)
                {
                    return ResolutionResult.ContextBlocked(descriptor);
                }
            }

            return ResolutionResult.ContextNotFound(descriptor);
        }

        private List<ActiveContext> GetOrderedContexts()
        {
            List<ActiveContext> contexts = new(_contexts.Values);
            contexts.Sort(static (left, right) =>
            {
                int priorityCompare = right.Registration.Priority.CompareTo(left.Registration.Priority);
                if (priorityCompare != 0)
                {
                    return priorityCompare;
                }

                return left.Sequence.CompareTo(right.Sequence);
            });
            return contexts;
        }

        private void ValidateAllowedActions(IReadOnlyList<InputActionId> allowedActions)
        {
            if (allowedActions == null)
            {
                return;
            }

            for (int index = 0; index < allowedActions.Count; index++)
            {
                InputActionId actionId = allowedActions[index];
                if (actionId.IsEmpty)
                {
                    throw new ArgumentException("[InputSystem] Allowed action cannot be empty.", nameof(allowedActions));
                }

                if (!_backend.TryGetActionDescriptor(actionId, out _))
                {
                    throw new ArgumentException(
                        $"[InputSystem] Action '{actionId.Value}' has not been registered in backend.",
                        nameof(allowedActions));
                }
            }
        }

        private readonly struct ResolutionResult
        {
            private ResolutionResult(
                InputActionResolveStatus status,
                InputContextHandle contextHandle,
                InputValueKind valueKind)
            {
                Status = status;
                ContextHandle = contextHandle;
                ValueKind = valueKind;
            }

            public InputActionResolveStatus Status { get; }

            public InputContextHandle ContextHandle { get; }

            public InputValueKind ValueKind { get; }

            public static ResolutionResult Success(InputContextHandle handle, InputActionDescriptor descriptor)
            {
                return new ResolutionResult(InputActionResolveStatus.Success, handle, descriptor.ValueKind);
            }

            public static ResolutionResult ActionNotFound(InputActionDescriptor descriptor)
            {
                return new ResolutionResult(
                    InputActionResolveStatus.ActionNotFound,
                    InputContextHandle.Empty,
                    descriptor == null ? InputValueKind.Button : descriptor.ValueKind);
            }

            public static ResolutionResult ContextBlocked(InputActionDescriptor descriptor)
            {
                return new ResolutionResult(InputActionResolveStatus.ContextBlocked, InputContextHandle.Empty, descriptor.ValueKind);
            }

            public static ResolutionResult ContextNotFound(InputActionDescriptor descriptor)
            {
                return new ResolutionResult(InputActionResolveStatus.ContextNotFound, InputContextHandle.Empty, descriptor.ValueKind);
            }

            public static ResolutionResult DeviceUnavailable(InputActionDescriptor descriptor)
            {
                return new ResolutionResult(InputActionResolveStatus.DeviceUnavailable, InputContextHandle.Empty, descriptor.ValueKind);
            }

            public static ResolutionResult ProviderFailure(InputActionDescriptor descriptor)
            {
                return new ResolutionResult(InputActionResolveStatus.ProviderFailure, InputContextHandle.Empty, descriptor.ValueKind);
            }
        }

        private sealed class ActiveContext
        {
            private readonly HashSet<string> _allowedActions;

            public ActiveContext(InputContextHandle handle, InputContextRegistration registration, long sequence)
            {
                Handle = handle;
                Registration = registration;
                Sequence = sequence;
                _allowedActions = new HashSet<string>(StringComparer.Ordinal);
                for (int index = 0; index < registration.AllowedActions.Count; index++)
                {
                    _allowedActions.Add(registration.AllowedActions[index].Value);
                }
            }

            public InputContextHandle Handle { get; }

            public InputContextRegistration Registration { get; }

            public long Sequence { get; }

            public bool Allows(InputActionId actionId)
            {
                if (_allowedActions.Count == 0)
                {
                    return true;
                }

                return _allowedActions.Contains(actionId.Value);
            }
        }
    }
}
