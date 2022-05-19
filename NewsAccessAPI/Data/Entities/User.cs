using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace NewsAccessAPI.Data.Entities
{
    public class User : IdentityUser
    {
        UserSetting userSetting { get; set; }
    }

    public class UserSetting
    {
        List<string> categoris { get; set; }
        List<string> countries { get; set; }
        List<string> sourceBlackList { get; set; }
    }
}
