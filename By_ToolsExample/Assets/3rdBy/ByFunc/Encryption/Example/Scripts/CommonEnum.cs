namespace _3rdBy.ByFunc.Encryption.Example.Scripts
{
    public class CommonEnum
    {
    }

    /// <summary>
    /// 软件限制类型
    /// </summary>
    public enum SoftLimitType
    {
        None = -1,
        Date,   // 时间限制
        Count,  // 次数限制
        Forever // 永久使用
    }

    /// <summary>
    /// 工程车辆类型
    /// </summary>
    public enum EngineerVehicleType
    {
        None = -1,
        WheelExcavator,   // 轮式挖掘机
        WheelBulldozer,   // 轮式推土机
        WheelLoader,      // 轮式装载机
        WheelGrader,      // 轮式平地机
        WheelRoller,      // 轮式压路机
        WheelCrane,       // 起重机
        WheelPaver,       // 轮式铺路机
        WheelForklift,    // 轮式叉车
        WheelMixerTruck,  // 混凝土搅拌车
        CrawlerExcavator, // 履带挖掘机
        CrawlerBulldozer, // 履带推土机
        WheelDumpTruck,   // 自卸车
    }
}