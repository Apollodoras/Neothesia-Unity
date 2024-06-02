using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscFlow : MonoBehaviour
{
    public static float originSpeed, flowSpeed;
    // Start is called before the first frame update
    void Start()
    {
        originSpeed = flowSpeed = 50; // disc falls from y 98 to 18(current check position) for 1.6 seconds
        if (Menu.hideKeyboard)
            originSpeed = flowSpeed = 77f; // from 540 to -540 for 1.4 seconds
    }
    
    // Update is called once per frame
    void Update()
    {
        if(!DrumMidiPlayer.playended)
        {
            if (!MidiPlayer.freeplay)
                transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y - Time.deltaTime * flowSpeed);
            else
                transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + Time.deltaTime * flowSpeed);
        }
    }
}
