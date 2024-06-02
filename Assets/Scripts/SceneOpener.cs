using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneOpener : MonoBehaviour
{
    // Start is called before the first frame update

    public void DetectSongNumber()
    {
        MidiPlayer._midiIndex = int.Parse(gameObject.name) - 1;
    }

    public void OpenScene()
    {
        SceneManager.LoadScene("Play");
        MidiPlayer.gamelevel = int.Parse(gameObject.name);
        MidiPlayer.freeplay = false;
    }

    public void OpenFreeplayScene()
    {
        SceneManager.LoadScene("Play");
        MidiPlayer.freeplay = true;
    }
}
