using UnityEngine;
using UnityEngine.Events;

public class View : MonoBehaviour
{
    [SerializeField] protected UnityEvent OnActivate;
    [SerializeField] protected UnityEvent OnDeactivate;

    public virtual void Activate()
    {
        this.gameObject.SetActive(true);
        OnActivate.Invoke();
    }

    public virtual void Deactivate(bool useEvent = true)
    {
        this.gameObject.SetActive(false);

        if (useEvent)
        {
            OnDeactivate.Invoke();
        }
    }
}



