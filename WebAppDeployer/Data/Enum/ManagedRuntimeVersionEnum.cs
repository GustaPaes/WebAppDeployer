using System.ComponentModel;

namespace WebAppDeployer.Data.Enum
{
    public enum ManagedRuntimeVersionEnum : byte
    {
        [Description("")]
        NO_MANAGED_CODE = 0,
        [Description("v2.0")]
        TWO = 1,
        [Description("v4.0")]
        FOUR = 2
    }
}
