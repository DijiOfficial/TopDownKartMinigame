using UnityEngine;

namespace Battlekart
{
    public class MapActiveToggle : MonoBehaviour
    {
        [SerializeField] private GameObject[] _targets = System.Array.Empty<GameObject>();

        private void Update()
        {
            bool active = MapElements.Active;
            foreach (GameObject target in _targets)
            {
                if (target != null && target.activeSelf != active)
                {
                    target.SetActive(active);
                }
            }
        }
    }
}
