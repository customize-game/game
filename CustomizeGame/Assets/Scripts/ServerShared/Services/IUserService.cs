using MagicOnion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserService : IService<IUserService>
{

    UnaryResult<string> LoginCheck(string encUserIdOrEmail, string encPassword, int mode);

    UnaryResult<string> AccountRegister(string encUserName, string encEmail, string encPassword);
}
