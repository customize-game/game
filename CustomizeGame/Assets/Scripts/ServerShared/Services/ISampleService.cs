using MagicOnion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomizeGame.ServerShared.Services
{

    public interface ISampleService : IService<ISampleService>
    {
        UnaryResult<int> SumAsync(int x, int y);
        UnaryResult<int> ProductAsync(int x, int y);
    }

}