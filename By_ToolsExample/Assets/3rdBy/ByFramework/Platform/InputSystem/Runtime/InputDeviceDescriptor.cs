//=====================================================
// 文件名称: InputDeviceDescriptor.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 描述输入设备状态。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    public sealed class InputDeviceDescriptor
    {
        public InputDeviceDescriptor(
            string deviceId,
            InputDeviceKind deviceKind,
            string providerType,
            bool isConnected)
        {
            DeviceId = string.IsNullOrWhiteSpace(deviceId) ? string.Empty : deviceId.Trim();
            DeviceKind = deviceKind;
            ProviderType = string.IsNullOrWhiteSpace(providerType) ? string.Empty : providerType.Trim();
            IsConnected = isConnected;
        }

        public string DeviceId { get; }

        public InputDeviceKind DeviceKind { get; }

        public string ProviderType { get; }

        public bool IsConnected { get; }
    }
}
