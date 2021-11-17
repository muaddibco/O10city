using System.ComponentModel;

namespace O10.Client.Common.Dtos
{
    public enum AccountTypeDTO : byte
    {
        [Description("Identity Provider")]
        IdentityProvider = 1,

        [Description("Service Provider")]
        ServiceProvider = 2,

        [Description("Regular User")]
        User = 3
    }
}