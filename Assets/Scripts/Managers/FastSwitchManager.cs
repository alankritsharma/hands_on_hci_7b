using DefaultNamespace;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class FastSwitchManager : MonoBehaviour
    {
        [SerializeField] private NatNetBridgeReceiver natNetBridgeReceiver;
        [SerializeField] private AbstractModelManager modelManager;
        [SerializeField] private GameObject obstacleCube;
        [SerializeField] private TMP_Text statusIndicator;

        private void Start()
        {
            NatNetBridgeReceiver.PositionChanged += FastSwitch;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle"))
            {
                natNetBridgeReceiver.lookingForFastSwitch = true;
                statusIndicator.color = Color.green;
                modelManager.ToggleModelVisibility(true);
                obstacleCube.SetActive(true);
            }
        }

        private void FastSwitch()
        {
            modelManager.ToggleModelVisibility();
            obstacleCube.SetActive(false);
            natNetBridgeReceiver.lookingForFastSwitch = false;
            natNetBridgeReceiver.ToggleRelayMovement();
            statusIndicator.color = Color.red;
        }
    }
}