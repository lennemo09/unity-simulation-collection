using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    public GridGenerator gridGenerator;
    public void OnButtonPress()
    {
        gridGenerator.ToggleRunning();
    }
}
