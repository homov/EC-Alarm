using System;

namespace EasyCaster.Alarm.Exceptions;

public class UserCancelException: Exception
{
    public UserCancelException(): base("User cancel")
    {

    }
}
