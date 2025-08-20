namespace GameResources.Features.UISystem
{
    using UnityEngine;

    public abstract class BaseWindow : MonoBehaviour
    {
        public virtual void Show() => gameObject.SetActive(true);

        public virtual void Hide() => gameObject.SetActive(false);
    }
}