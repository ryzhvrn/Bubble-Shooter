#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Helpers/Scriptable Object Initialization")]
    public class ScriptableObjectInitModule : InitModule
    {
        [SerializeField] ScriptableObject[] initObjects;

        public override void CreateComponent(Initialiser Initialiser)
        {
            for (int i = 0; i < initObjects.Length; i++)
            {
                if (initObjects[i] != null)
                {
                    IInitialized objectInterface = initObjects[i] as IInitialized;
                    if (objectInterface != null)
                    {
                        objectInterface.Init();
                    }
                    else
                    {
                        Debug.LogError("[Initialiser]: Object " + initObjects[i].name + " should implement IInitialized interface!");
                    }
                }
                else
                {
                    Debug.LogError("[Initialiser]: Scriptable object can't be null!");
                }
            }
        }

        public ScriptableObjectInitModule()
        {
            moduleName = "Scriptable Object Initialization";
        }
    }
}

// -----------------
// Initialiser v 0.4.1
// -----------------