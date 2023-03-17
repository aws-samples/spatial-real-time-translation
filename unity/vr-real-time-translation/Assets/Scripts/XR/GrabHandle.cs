using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabHandle : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable _grabInteractable;
    [SerializeField] private LayerMask _interactionLayerMask;

    private static GrabHandle _activeHandle = null;

    private void Awake()
    {
        _grabInteractable.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_activeHandle == null)
        {
            if (_interactionLayerMask.Contains(other.gameObject.layer))
            {
                _grabInteractable.attachTransform = this.transform;
                _grabInteractable.enabled = true;
                _activeHandle = this;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_activeHandle == this)
        {
            if (_grabInteractable.attachTransform != null)
            {
                _grabInteractable.attachTransform = null;
            }
            _grabInteractable.enabled = false;
            _activeHandle = null;
        }
    }
}
