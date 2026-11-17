using System;
using UnityEngine;

namespace Battlekart
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static event Action OnInitializedEvent;
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"An instance of {gameObject.name} already exists. Destroying duplicate instance.", this);
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
            OnInitializedEvent?.Invoke();
        }
    }
}
