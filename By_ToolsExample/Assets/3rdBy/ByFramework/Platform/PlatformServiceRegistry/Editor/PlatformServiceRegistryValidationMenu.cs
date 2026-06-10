//=====================================================
// 文件名称: PlatformServiceRegistryValidationMenu.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供 PlatformServiceRegistry 的基础验证菜单。
//=====================================================

namespace _3rdBy.ByFramework.Platform.PlatformServiceRegistry.Editor
{
    using System;
    using Registry = _3rdBy.ByFramework.Platform.PlatformServiceRegistry.PlatformServiceRegistry;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// PlatformServiceRegistry 基础验证菜单。
    /// </summary>
    public static class PlatformServiceRegistryValidationMenu
    {
        private const string LogPrefix = "[PlatformServiceRegistry]";

        [MenuItem("ByFramework/验证/PlatformServiceRegistry 基础验证")]
        private static void RunValidation()
        {
            Registry.Clear();

            try
            {
                VerifyRegister();
                VerifyDuplicateRegisterThrows();
                VerifyTryRegisterDuplicateReturnsFalse();
                VerifyReplace();
                VerifyGet();
                VerifyGetThrowsWhenMissing();
                VerifyTryGetWhenMissing();
                VerifyUnregister();
                VerifyContains();
                VerifyMarkState();
                VerifyDescriptor();
                VerifyClear();

                Debug.Log($"{LogPrefix} 基础验证完成。");
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LogPrefix} 验证失败：{exception.Message}\n{exception}");
                throw;
            }
            finally
            {
                Registry.Clear();
            }
        }

        private static void VerifyRegister()
        {
            Registry.Clear();
            var service = new SamplePlatformService("Register");
            Registry.Register<ISamplePlatformService>(service);
            AssertTrue(Registry.Contains<ISamplePlatformService>(), "Register 后应能检测到服务。");
            Debug.Log($"{LogPrefix} Register 验证通过。");
        }

        private static void VerifyDuplicateRegisterThrows()
        {
            Registry.Clear();
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("A"));

            bool threw = false;
            try
            {
                Registry.Register<ISamplePlatformService>(new SamplePlatformService("B"));
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }

            AssertTrue(threw, "重复 Register 应抛出 InvalidOperationException。");
            Debug.Log($"{LogPrefix} 重复 Register 抛异常验证通过。");
        }

        private static void VerifyTryRegisterDuplicateReturnsFalse()
        {
            Registry.Clear();
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("A"));
            bool result = Registry.TryRegister<ISamplePlatformService>(new SamplePlatformService("B"));
            AssertTrue(result == false, "TryRegister 重复注册应返回 false。");
            Debug.Log($"{LogPrefix} TryRegister 重复返回 false 验证通过。");
        }

        private static void VerifyReplace()
        {
            Registry.Clear();
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("A"));
            Registry.Replace<ISamplePlatformService>(new SamplePlatformService("B"));

            ISamplePlatformService service = Registry.Get<ISamplePlatformService>();
            AssertTrue(service.Name == "B", "Replace 后应返回新实例。");
            Debug.Log($"{LogPrefix} Replace 验证通过。");
        }

        private static void VerifyGet()
        {
            Registry.Clear();
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("Get"));
            ISamplePlatformService service = Registry.Get<ISamplePlatformService>();
            AssertTrue(service != null && service.Name == "Get", "Get 应返回已注册实例。");
            Debug.Log($"{LogPrefix} Get 验证通过。");
        }

        private static void VerifyGetThrowsWhenMissing()
        {
            Registry.Clear();

            bool threw = false;
            try
            {
                Registry.Get<ISamplePlatformService>();
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }

            AssertTrue(threw, "Get 未注册时应抛出 InvalidOperationException。");
            Debug.Log($"{LogPrefix} Get 未注册抛异常验证通过。");
        }

        private static void VerifyTryGetWhenMissing()
        {
            Registry.Clear();
            bool result = Registry.TryGet<ISamplePlatformService>(out ISamplePlatformService service);
            AssertTrue(result == false && service == null, "TryGet 未注册时应返回 false 且输出 default。");
            Debug.Log($"{LogPrefix} TryGet 未注册返回 false 验证通过。");
        }

        private static void VerifyUnregister()
        {
            Registry.Clear();
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("Unregister"));
            bool result = Registry.Unregister<ISamplePlatformService>();
            AssertTrue(result, "Unregister 已注册服务时应返回 true。");
            AssertTrue(Registry.Contains<ISamplePlatformService>() == false, "Unregister 后服务应不存在。");
            Debug.Log($"{LogPrefix} Unregister 验证通过。");
        }

        private static void VerifyContains()
        {
            Registry.Clear();
            AssertTrue(Registry.Contains<ISamplePlatformService>() == false, "空表时 Contains 应返回 false。");
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("Contains"));
            AssertTrue(Registry.Contains<ISamplePlatformService>(), "注册后 Contains 应返回 true。");
            Debug.Log($"{LogPrefix} Contains 验证通过。");
        }

        private static void VerifyMarkState()
        {
            Registry.Clear();
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("Mark"));

            bool updated = Registry.MarkInitialized<ISamplePlatformService>();
            AssertTrue(updated, "MarkInitialized 已注册服务时应返回 true。");

            ServiceDescriptor descriptor = Registry.GetAllDescriptors()[0];
            AssertTrue(descriptor.State == ServiceState.Initialized, "MarkInitialized 后状态应为 Initialized。");
            Debug.Log($"{LogPrefix} MarkState 验证通过。");
        }

        private static void VerifyDescriptor()
        {
            Registry.Clear();
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("Descriptor"));
            Registry.TryRegister<INonMarkedService>(new NonMarkedService("NonMarked"));

            var descriptors = Registry.GetAllDescriptors();
            AssertTrue(descriptors.Count == 2, "ServiceDescriptor 数量应与注册服务一致。");

            bool foundMarked = false;
            bool foundUnmarked = false;
            for (int i = 0; i < descriptors.Count; i++)
            {
                if (descriptors[i].IsPlatformService)
                {
                    foundMarked = true;
                }
                else
                {
                    foundUnmarked = true;
                }
            }

            AssertTrue(foundMarked && foundUnmarked, "ServiceDescriptor 应正确标记 IPlatformService 实现情况。");
            Debug.Log($"{LogPrefix} ServiceDescriptor 验证通过。");
        }

        private static void VerifyClear()
        {
            Registry.Clear();
            Registry.Register<ISamplePlatformService>(new SamplePlatformService("Clear"));
            Registry.Clear();
            AssertTrue(Registry.GetAllDescriptors().Count == 0, "Clear 后服务表应为空。");
            Debug.Log($"{LogPrefix} Clear 验证通过。");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (condition == false)
            {
                throw new InvalidOperationException(message);
            }
        }

        private interface ISamplePlatformService
        {
            string Name { get; }
        }

        private interface INonMarkedService
        {
            string Name { get; }
        }

        private sealed class SamplePlatformService : ISamplePlatformService, IPlatformService
        {
            public SamplePlatformService(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private sealed class NonMarkedService : INonMarkedService
        {
            public NonMarkedService(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}
