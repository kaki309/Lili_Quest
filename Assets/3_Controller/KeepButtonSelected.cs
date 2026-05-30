using UnityEngine;
using UnityEngine.EventSystems;

public class KeepButtonSelected : MonoBehaviour
{
    GameObject lastSelected;

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (lastSelected && lastSelected.gameObject.activeSelf)
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }
        }
        else
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }
}
