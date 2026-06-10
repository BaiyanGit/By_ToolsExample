//=====================================================
// 文件名称: InputSystemRuntimeTests.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 覆盖 InputSystem Runtime 的基础行为测试。
//=====================================================

#if UNITY_INCLUDE_TESTS
namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
#endif

    public sealed class InputSystemRuntimeTests
    {
        [Test]
        public void InputContextSnapshot_RemainsImmutableAfterContextRelease()
        {
            MockInputBackend backend = new();
            backend.RegisterAction(new InputActionDescriptor(
                new InputActionId("UI.Confirm"),
                "UI",
                InputValueKind.Button,
                true,
                true));

            InputService service = new(backend);
            InputContextHandle handle = service.ActivateContext(new InputContextRegistration(
                "Dialog",
                "Owner-A",
                100,
                false,
                false,
                new[] { new InputActionId("UI.Confirm") }));

            InputContextSnapshot firstSnapshot = service.GetCurrentContextSnapshot();
            bool released = service.TryReleaseContext(handle);
            InputContextSnapshot secondSnapshot = service.GetCurrentContextSnapshot();

            Assert.That(released, Is.True);
            Assert.That(firstSnapshot.ActiveContexts.Count, Is.EqualTo(1));
            Assert.That(firstSnapshot.ResolvableActions.Count, Is.EqualTo(1));
            Assert.That(secondSnapshot.ActiveContexts.Count, Is.EqualTo(0));
            Assert.That(secondSnapshot.ResolvableActions.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExclusiveContext_BlocksLowerPriorityAction()
        {
            MockInputBackend backend = CreateTwoActionBackend();
            InputService service = new(backend);

            service.ActivateContext(new InputContextRegistration(
                "Top",
                "Owner-A",
                100,
                true,
                false,
                new[] { new InputActionId("UI.Cancel") }));

            service.ActivateContext(new InputContextRegistration(
                "Bottom",
                "Owner-B",
                10,
                false,
                false,
                new[] { new InputActionId("UI.Confirm") }));

            bool isAvailable = service.IsActionAvailable(new InputActionId("UI.Confirm"));
            bool resolved = service.TryResolveActionState(new InputActionId("UI.Confirm"), out InputActionState state);

            Assert.That(isAvailable, Is.False);
            Assert.That(resolved, Is.True);
            Assert.That(state.IsAvailable, Is.False);
        }

        [Test]
        public void PassThroughContext_AllowsLowerPriorityAction()
        {
            MockInputBackend backend = CreateTwoActionBackend();
            InputService service = new(backend);

            service.ActivateContext(new InputContextRegistration(
                "Top",
                "Owner-A",
                100,
                true,
                true,
                new[] { new InputActionId("UI.Cancel") }));

            service.ActivateContext(new InputContextRegistration(
                "Bottom",
                "Owner-B",
                10,
                false,
                false,
                new[] { new InputActionId("UI.Confirm") }));

            bool isAvailable = service.IsActionAvailable(new InputActionId("UI.Confirm"));
            bool resolved = service.TryResolveActionState(new InputActionId("UI.Confirm"), out InputActionState state);

            Assert.That(isAvailable, Is.True);
            Assert.That(resolved, Is.True);
            Assert.That(state.IsAvailable, Is.True);
        }

        [Test]
        public void ActionTriggered_PublishesResolvedFactOnly()
        {
            MockInputBackend backend = new();
            backend.RegisterAction(new InputActionDescriptor(
                new InputActionId("UI.Confirm"),
                "UI",
                InputValueKind.Button,
                true,
                true));

            InputService service = new(backend);
            service.ActivateContext(new InputContextRegistration(
                "Dialog",
                "Owner-A",
                100,
                false,
                false,
                new[] { new InputActionId("UI.Confirm") }));

            List<InputActionEvent> received = new();
            service.ActionTriggered += received.Add;

            bool published = backend.TryPublishAction(new InputActionId("UI.Confirm"), InputStage.Performed);

            Assert.That(published, Is.True);
            Assert.That(received.Count, Is.EqualTo(1));
            Assert.That(received[0].ActionId, Is.EqualTo(new InputActionId("UI.Confirm")));
            Assert.That(received[0].Stage, Is.EqualTo(InputStage.Performed));
            Assert.That(received[0].ValueKind, Is.EqualTo(InputValueKind.Button));
            Assert.That(received[0].Status, Is.EqualTo(InputActionResolveStatus.Success));
            Assert.That(received[0].ContextHandle.IsEmpty, Is.False);
        }

#if ENABLE_INPUT_SYSTEM
        [Test]
        public void UnityInputSystemBackendAdapter_BuildsProfileAndPublishesActionEvent()
        {
            InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
            Keyboard keyboard = null;
            UnityInputSystemBackendAdapter adapter = null;

            try
            {
                InputActionMap map = new("Player");
                map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
                asset.AddActionMap(map);

                keyboard = InputSystem.AddDevice<Keyboard>();
                adapter = new UnityInputSystemBackendAdapter(asset, "UnityProfile", 7, true);
                InputService service = new(adapter);
                service.ActivateContext(new InputContextRegistration(
                    "Gameplay",
                    "Owner-Gameplay",
                    10,
                    false,
                    false,
                    new[] { new InputActionId("Player.Jump") }));

                List<InputActionEvent> received = new();
                service.ActionTriggered += received.Add;

                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
                InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();

                Assert.That(adapter.GetProfileSnapshot().ProfileId, Is.EqualTo("UnityProfile"));
                Assert.That(adapter.GetProfileSnapshot().Version, Is.EqualTo(7));
                Assert.That(adapter.GetProfileSnapshot().Bindings.Count, Is.EqualTo(1));
                Assert.That(adapter.GetProfileSnapshot().Bindings[0].ActionId, Is.EqualTo(new InputActionId("Player.Jump")));
                Assert.That(received.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(received[0].ActionId, Is.EqualTo(new InputActionId("Player.Jump")));
                Assert.That(received[0].Status, Is.EqualTo(InputActionResolveStatus.Success));
            }
            finally
            {
                adapter?.Dispose();
                if (keyboard != null)
                {
                    InputSystem.RemoveDevice(keyboard);
                }

                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }
        }
#endif

        private static MockInputBackend CreateTwoActionBackend()
        {
            MockInputBackend backend = new();
            backend.RegisterAction(new InputActionDescriptor(
                new InputActionId("UI.Confirm"),
                "UI",
                InputValueKind.Button,
                true,
                true));
            backend.RegisterAction(new InputActionDescriptor(
                new InputActionId("UI.Cancel"),
                "UI",
                InputValueKind.Button,
                true,
                true));
            return backend;
        }
    }
}
#endif
