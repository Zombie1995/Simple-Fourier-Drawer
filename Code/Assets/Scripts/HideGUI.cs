using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HideGUI : MonoBehaviour
{
    public GameObject GUI;

    bool GUIstate = true;
    bool prGUIstate = true;
    void Update()
    {
        GUIstate = !CheckUI();

        if (GUIstate != prGUIstate) 
        {
            if (GUIstate)
            {
                GUI.SetActive(true);
            }
            else 
            {
                GUI.SetActive(false);
            }

            prGUIstate = GUIstate;
        }
    }

    bool CheckUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        List<RaycastResult> resultData = new List<RaycastResult>();
        pointerData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointerData, resultData);

        if (resultData.Count > 0)
        {
            return false;
        }

        return true;
    }
}
