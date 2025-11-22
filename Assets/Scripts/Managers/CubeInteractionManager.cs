using System;
using UnityEngine;

namespace Managers
{
    public class CubeInteractionManager : MonoBehaviour
    {
        public static event Action CubeInteracted;

        private void OnTriggerEnter(Collider other)
        {
            CubeInteracted?.Invoke();
        }
    }
}