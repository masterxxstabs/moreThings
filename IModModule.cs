using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    internal interface IModModule
    {
        void OnModInit();
        void OnSceneLoaded(int buildIndex, string sceneName);
        void OnUpdate();
    }
}
