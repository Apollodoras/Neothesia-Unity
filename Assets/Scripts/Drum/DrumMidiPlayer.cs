using System;
using System.Collections;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// using UnityEngine.InputSystem;
// using Minis;
using MidiJack;
using TMPro;
using System.Reflection;
using Michsky.UI.Shift;
using System.IO;

public class DrumMidiPlayer : MonoBehaviour
{
	[Header("References")]
	public DrumKeyController PianoKeyDetector;
    public static bool playAlong, pass, freeplay;
	public GameObject noteImage, noteUpImage, speedDisplay, timeDisplay, scoreDisplay, timeTexts, noteTexts, levelpass, levelfail, levelendbar, bonusText;
    public AudioSource errorplayer;
    public AudioClip errorplayerClip;

	[Header("Properties")]
	public float GlobalSpeed = 1;
	public RepeatType RepeatType;

	public KeyMode KeyMode;
	public bool ShowMIDIChannelColours;
	public Color[] MIDIChannelColours;
	public TMP_Text currentTimeText, totalTimeText, currentNoteText, totalNoteText, leftNoteText, scoreTexts, percentageProgress;

	[Header("Ensure Song Name is filled for builds")]
	public MidiSong[] MIDISongs;

	[HideInInspector]
	public MidiNote[] MidiNotes;
	public UnityEvent OnPlayTrack { get; set; }

	MidiFileInspector _midi;
    TextAsset _scoretxt;

	string _path, txt_path;
	string[] _keyIndex;
    int[] fingerScore;
	int _noteIndex = 0, leftHandSameIndex = 0, leftHandInterval = 1, leftHandOnceIndex = 0, leftHandOnceInterval = 1, rightHandSameIndex = 0, rightHandInterval = 1, rightHandOnceIndex = 0, rightHandOnceInterval = 1;
	int sameLineNumber, continousFail = 0, midiNoteLength, scoreLength;
	public static int _midiIndex, gamelevel = 1;
    public static int[] alongKeys;
    bool[] alongkeyspressed;
    public static float score = 0;
	double _timer = 0, _sliderTimer = 0, currentTime = 0, startTime;
    float interval, imageInitY, bonus = 1f;
	[SerializeField, HideInInspector]
	bool _preset = false, playended = false;
	Vector2 noteSize;

    GameObject[] u = new GameObject[88];
    float[] initTime = new float[88];
    bool[] pressed = new bool[88];

    Color pinkyColor = new Color(241f / 255f, 86f / 255f, 112f / 255f);//pinky
    Color ringColor = new Color(58f / 255f, 139f / 255f, 241f / 255f);//ring
    Color middleColor = new Color(130f / 255f, 93f / 255f, 245f / 255f);//middle
    Color indexColor = new Color(233f / 255f, 194f / 225f, 35f / 255f);//index
    Color thumbColor = new Color(48f / 255f, 162f / 255f, 109f / 255f);//thumb

    Color _42Color = new Color(255f / 255f, 102f / 255f, 104f / 255f);
    Color _46Color = new Color(254f / 255f, 9f / 255f, 5f / 255f);
    Color _44Color = new Color(255f / 255f, 195f / 255f, 203f / 255f);
    Color _38Color = new Color(0f / 255f, 152f / 255f, 197f / 255f);
    Color _49Color = new Color(255f / 255f, 203f / 255f, 6f / 255f);
    Color _48Color = new Color(0f / 255f, 170f / 255f, 104f / 255f);
    Color _45Color = new Color(4f / 255f, 32f / 255f, 140f / 255f);
    Color _36Color = Color.white;
    Color _51Color = new Color(255f / 255f, 99f / 255f, 17f / 255f);
    Color _43Color = new Color(98f / 255f, 206f / 255f, 96f / 255f);

    void Start ()
	{
        //StartCoroutine(WaitAndHalt());
        startTime = Time.realtimeSinceStartup;

        score = 0;
        
        pass = false;
        levelpass.SetActive(false);
        levelfail.SetActive(false);
        levelendbar.SetActive(false);
		imageInitY = -300f + 1.92f * NoteFlow.originSpeed;
		OnPlayTrack = new UnityEvent();
		OnPlayTrack.AddListener(delegate{FindObjectOfType<MusicText>().StartSequence(MIDISongs[_midiIndex].Details);});

        //_midiIndex = 0;

        if (!_preset)
			PlayCurrentMIDI();
		else
		{
#if UNITY_EDITOR
			_path = string.Format("{0}/MIDI/{1}.mid", Application.streamingAssetsPath, MIDISongs[0].MIDIFile.name);
            txt_path = string.Format("{0}/Score/{1}.txt", Application.streamingAssetsPath, MIDISongs[0].MIDIFile.name);
#else
			_path = string.Format("{0}/MIDI/{1}.mid", Application.streamingAssetsPath, MIDISongs[0].SongFileName);
            txt_path = string.Format("{0}/Score/{1}.txt", Application.streamingAssetsPath, MIDISongs[0].SongFileName);
#endif
			_midi = new MidiFileInspector(_path);
            _scoretxt = new TextAsset(txt_path);

            string[] AllWords = File.ReadAllLines(txt_path);
            string[] fingers = AllWords[2].Split(" ");
            int l;
            if (MidiNotes.Length > fingers.Length)
                l = fingers.Length;
            else
                l = MidiNotes.Length;
            fingerScore = new int[l];
            Debug.LogError(l);
            try
            {
                for (int i = 0; i < l; i++)
                {
                    fingerScore[i] = int.Parse(fingers[i]);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            OnPlayTrack.Invoke();
		}

        scoreDisplay.SetActive(false);
        if(freeplay)
        {
            GlobalSpeed = 1f;
            playAlong = false;
            timeTexts.SetActive(false);
            noteTexts.SetActive(false);
            leftNoteText.gameObject.SetActive(false);
            totalNoteText.gameObject.SetActive(false);
            scoreTexts.gameObject.SetActive(false);
            bonusText.SetActive(false);
        }
        else
        {
            //scoreTexts.gameObject.SetActive(true);
            switch (gamelevel)
            {
                case 1:
                    GlobalSpeed = 1f;
                    playAlong = true;
                    //timeTexts.SetActive(true);
                    //noteTexts.SetActive(true);
                    //totalNoteText.gameObject.SetActive(true);
                    leftNoteText.gameObject.SetActive(false);
                    currentNoteText.gameObject.SetActive(false);
                    bonusText.SetActive(false);

                    timeTexts.SetActive(false);
                    noteTexts.SetActive(false);
                    totalNoteText.gameObject.SetActive(false);
                    //scoreTexts.gameObject.SetActive(false);
                    scoreDisplay.SetActive(true);
                    GetComponent<AudioSource>().enabled = false;
                    break;
                case 2:
                    GlobalSpeed = 1f;
                    playAlong = false;
                    timeTexts.SetActive(false);
                    noteTexts.SetActive(true);
                    leftNoteText.gameObject.SetActive(true);
                    totalNoteText.gameObject.SetActive(false);
                    bonusText.SetActive(true);
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                default:
                    GlobalSpeed = 1f;
                    playAlong = false;
                    timeTexts.SetActive(false);
                    noteTexts.SetActive(true);
                    leftNoteText.gameObject.SetActive(false);
                    totalNoteText.gameObject.SetActive(true);
                    bonusText.SetActive(true);
                    break;

            }
        }
        midiNoteLength = MidiNotes.Length;
        interval = _midi.MidiFile.DeltaTicksPerQuarterNote / 4f;
        //noteSize = noteImage.GetComponent<RectTransform>().sizeDelta;
        noteSize = new Vector2(1f, 1f);
        int t = (int)CalcTotalTime();
        totalTimeText.text = "/" + DisplayTotalTime(t);
        if (gamelevel == 1)
            totalTimeText.text = "";
        totalNoteText.text = midiNoteLength.ToString();
        leftNoteText.text = midiNoteLength.ToString();

        //for (int i = 0; i < midiNoteLength; i++)
        //    print(i + 1 + "-" + MidiNotes[i].StartTime + "-" + MidiNotes[i].Note + "-" + (PianoKeyDetector.noteOrder.IndexOf(MidiNotes[i].Note) + 1).ToString());
    }
	int CalcImageIndex(string note)
	{
		int index = 0;
		int num = note[note.Length - 1] - '0';
        if(num == 0)
            index = note[0] - 'A' + 1;
		else
		{
			index = 2 + (num - 2) * 7 + (note[0] - 'C' + 1);
			if (note[0] == 'A' || note[0] == 'B')
				index += 7;
        }
		return index;
	}
    int CalcUpImageIndex(int notenumber)
    {
        if (notenumber <= 2)
            return notenumber / 2 + 1;
        else
        {
            int q = (notenumber-3) / 12;
            int r = (notenumber-3) % 12;
            if (r < 6)
                return q * 7 + 2 + (r+1)/2;
            else
                return q * 7 + 2 + 4 + (r-3)/2;
        }
        
    }
	IEnumerator WaitAndHalt()
    {
        yield return new WaitForSeconds(0.2f);
        Time.timeScale = 0f;
    }

    public void PracticeButton()
    {
        gamelevel = 1;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Drum");
    }

    public void PlayButton()
    {
        gamelevel = 3;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Drum");
    }
    int getNoteNumber(int keyNumber)
    {
        return keyNumber + 21;
    }

    void ScoreMechanics()
    {
        int keynumber;
        for (keynumber = 0; keynumber < 88; keynumber++)
        {
            if (MidiMaster.GetKeyDown(getNoteNumber(keynumber)) || Input.GetKeyUp(KeyCode.E))
            {
                int r = 0;
                for (int i = sameLineNumber - 1; i >= 0; i--)
                {
                    if (alongKeys[i] == getNoteNumber(keynumber) && !alongkeyspressed[i])
                    {
                        alongkeyspressed[i] = true;

                        score++;
                        //score += bonus; //!critical
                        continousFail = 0;
                        scoreTexts.text = ((int)(score * 1000f) / 1000f).ToString();

                        bonus += 0.2f;
                        bonus = ((int)(bonus * 1000)) / 1000f;
                        bonusText.GetComponent<MainButton>().buttonText = "x" + bonus.ToString();
                        bonusText.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TMP_Text>().text = "x" + bonus.ToString();
                        bonusText.transform.GetChild(0).gameObject.GetComponent<CanvasGroup>().alpha = 1f;

                        r++;
                    }
                }
                if (r == 0)
                {
                    errorplayer.PlayOneShot(errorplayerClip);
                    print("error");
                    bonus = 1f;
                    bonusText.GetComponent<MainButton>().buttonText = "x" + bonus.ToString();
                    bonusText.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TMP_Text>().text = "x" + bonus.ToString();
                    bonusText.transform.GetChild(0).gameObject.GetComponent<CanvasGroup>().alpha = 1f;

                    continousFail++;
                }
            }
        }
        if (continousFail >= 10)
        {
            playended = true;
        }
    }
    bool DetectIfDrumNote(string note)
    {
        //note++; //real order since it was calced from 0, not 1
        switch(note)
        {
            case "Bass Drum 1":
            case "Pedal Hi-Hat":
            case "Closed Hi-Hat":
            case "Open Hi-Hat":
            case "Acoustic Snare":
            case "Crash Cymbal 2":
            case "Ride Cymbal 1":
            case "Low Floor Tom":
            case "High Mid Tom":
            case "High Floor Tom":
                return true;
        }
        return false;
    }
    int DetectDrumNote(string note)
    {
        switch (note)
        {
            case "Closed Hi-Hat":
                return 42;
            case "Open Hi-Hat":
                return 46;
            case "Pedal Hi-Hat":
                return 44;
            case "Acoustic Snare":
                return 38;
            case "Crash Cymbal 2":
                return 49;
            case "High Floor Tom":
                return 43;
            case "High Mid Tom":
                return 48;
            case "Bass Drum 1":
                return 36;
            case "Ride Cymbal 1":
                return 51;
            case "Low Floor Tom":
                return 45;
            default:
                return 0;
        }
    }
    int OrderOfDrumNote(string note)
    {
        //note++;
        switch (note)
        {
            case "Closed Hi-Hat":
                return 0;
            case "Open Hi-Hat":
                return 1;
            case "Pedal Hi-Hat":
                return 2;
            case "Acoustic Snare":
                return 3;
            case "Crash Cymbal 2":
                return 4;
            case "High Floor Tom":
                return 5;
            case "High Mid Tom":
                return 6;
            case "Bass Drum 1":
                return 7;
            case "Ride Cymbal 1":
                return 8;
            case "Low Floor Tom":
                return 9;
            default:
                return 0;
        }
    }
    void Update ()
	{
		if (MIDISongs.Length <= 0)
			enabled = false;
        if (_midi != null && midiNoteLength > 0 && _noteIndex < midiNoteLength)
        //if (_midi != null && MidiNotes.Length > 0 && _noteIndex < 20)
        {
            _timer += Time.deltaTime * GlobalSpeed * MidiNotes[_noteIndex].Tempo;
			currentTime += Time.deltaTime * GlobalSpeed;
			currentTimeText.text = DisplayTotalTime((int)currentTime);
            if (gamelevel == 1)
                currentTimeText.text = DisplayTotalTime((int)(Time.realtimeSinceStartup - startTime));

            //Debug.LogError(MidiNotes[_noteIndex].Tempo);
            while (_noteIndex < midiNoteLength && MidiNotes[_noteIndex].StartTime < _timer && !freeplay)
			{
                percentageProgress.text = ((int)((_timer - 800)*100 / MidiNotes[midiNoteLength - 1].StartTime)).ToString()+"%";

                timeDisplay.GetComponent<Slider>().value = (float)((_timer-800) / MidiNotes[midiNoteLength - 1].StartTime);
                //if (PianoKeyDetector.PianoNotes.ContainsKey(MidiNotes[_noteIndex].Note) && DetectIfDrumNote(MidiNotes[_noteIndex].Note))
                bool isDrum = false;
                if (DetectIfDrumNote(MidiNotes[_noteIndex].Note))
                {
                    isDrum = true;
                    GameObject g = Instantiate(noteImage, GameObject.Find("DrumNotes").transform) as GameObject;
                    //if (MidiNotes[0].Length <= 0.1f)
                    //int index = PianoKeyDetector.noteOrder.IndexOf(MidiNotes[_noteIndex].Note);
                    g.transform.localPosition = new Vector3(7.5f + OrderOfDrumNote(MidiNotes[_noteIndex].Note) * 3.4f, 98f, 0f); // disc size 3f, interval 0.4f, left margin 7.5f
                    //else
                    //    g.GetComponent<RectTransform>().localPosition = new Vector2(-950f + (CalcImageIndex(MidiNotes[_noteIndex].Note) - 1) * noteSize.x / 36f * 37.2f, 1080f + (float)MidiNotes[_noteIndex].StartTime / interval * noteSize.y * (int)(MidiNotes[0].Length * 100f) / 10f * 1.1f);
                    //if (MidiNotes[_noteIndex].Note.Length == 3) //#notes
                    {
                        //Vector2 sizeDelta = g.GetComponent<RectTransform>().sizeDelta;
                        //g.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x / 2f, sizeDelta.y);
                        //g.transform.localPosition = new Vector3(g.transform.localPosition.x + noteSize.x / 36f * 18.6f, g.transform.localPosition.y, 0f);
                    }
                    //if (MidiNotes[_noteIndex].Length > 0.1f) // in origin Rosetta, fixing image lengths according to its length
                    {
                        //Vector2 sizeDelta = g.GetComponent<RectTransform>().sizeDelta;
                        //g.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, (int)(MidiNotes[_noteIndex].Length * sizeDelta.y * 100f) / 10f);
                    }
                    //disc coloring
                    //index++;
                    switch(MidiNotes[_noteIndex].Note)
                    {
                        case "Closed Hi-Hat":
                            g.GetComponent<SpriteRenderer>().color = _42Color;
                            break;
                        case "Open Hi-Hat":
                            g.GetComponent<SpriteRenderer>().color = _46Color;
                            break;
                        case "Pedal Hi-Hat":
                            g.GetComponent<SpriteRenderer>().color = _44Color;
                            break;
                        case "Acoustic Snare":
                            g.GetComponent<SpriteRenderer>().color = _38Color;
                            break;
                        case "Crash Cymbal 2":
                            g.GetComponent<SpriteRenderer>().color = _49Color;
                            break;
                        case "High Floor Tom":
                            g.GetComponent<SpriteRenderer>().color = _48Color;
                            break;
                        case "High Mid Tom":
                            g.GetComponent<SpriteRenderer>().color = _45Color;
                            break;
                        case "Bass Drum 1":
                            g.GetComponent<SpriteRenderer>().color = _36Color;
                            break;
                        case "Ride Cymbal 1":
                            g.GetComponent<SpriteRenderer>().color = _51Color;
                            break;
                        case "Low Floor Tom":
                            g.GetComponent<SpriteRenderer>().color = _43Color;
                            break;
                    }

                    //basic coloring
         
                    g.name = MidiNotes[_noteIndex].Channel + "+" + leftHandOnceInterval + "+" + MidiNotes[_noteIndex].StartTime + "+" + MidiNotes[_noteIndex].Note + "+" + (PianoKeyDetector.noteOrder.IndexOf(MidiNotes[_noteIndex].Note)+1).ToString(); // the last number is the 88 midi keyboard's order from left
					
					//StartCoroutine(WaitAndPlay(1.4f, _noteIndex));

				}
                StartCoroutine(WaitAndPlay(1.6f, _noteIndex, isDrum));

                _noteIndex++;
				
			}
            GameObject[] gs = GameObject.FindGameObjectsWithTag("Flow");
            foreach (GameObject gsu in gs)
            {
                if (gsu.transform.localPosition.y < 0f)
                    Destroy(gsu);
            }
        }
		else
		{
            
			SetupNextMIDI();
		}
		if (Input.GetKeyUp(KeyCode.N))
			SetupNextMIDI();
		else if (Input.GetKeyUp(KeyCode.DownArrow))
		{
			if(GlobalSpeed > 0.1f)
				GlobalSpeed -= 0.1f;
			NoteFlow.flowSpeed = NoteFlow.originSpeed * GlobalSpeed;
			DisplaySpeed();
        }
		else if (Input.GetKeyUp(KeyCode.UpArrow))
		{
            GlobalSpeed += 0.1f;
            NoteFlow.flowSpeed = NoteFlow.originSpeed * GlobalSpeed;
            DisplaySpeed();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
			if (Time.timeScale > 0)
				Time.timeScale = 0f;
			else
				Time.timeScale = 1f;
            DisplaySpeed();
        }
        //else if (Input.GetKeyUp(KeyCode.A))
        //{
        //    pass = true;
        //}
        else if (Input.GetKeyUp(KeyCode.Escape))
        {
            OpenMainMenu();
        }

        if (alongKeys != null && !pass && alongkeyspressed != null)
        {
            int keynumber;
            switch (gamelevel)
            {
                case 1:
                //case 3:
                    for(keynumber = 0; keynumber < 88 ; keynumber++) //on piano keyboard number 1~88 vs real midi notes 21~108
                    {
                        if(MidiMaster.GetKeyDown(getNoteNumber(keynumber)) || Input.GetKeyUp(KeyCode.E))
                        {
                            int r = 0;
                            for(int i = alongKeys.Length - 1; i >= 0; i--)
                            {
                                //Here examine with the drum keys
                                if (alongKeys[i] == getNoteNumber(keynumber) && !alongkeyspressed[i])
                                {
                                    alongkeyspressed[i] = true;
                                    score++;
                                    scoreDisplay.GetComponent<Slider>().value = (float)(score / midiNoteLength);
                                    scoreTexts.text = score.ToString();
                                    r++;
                                }                                   
                            }
                            if(r == 0)
                                errorplayer.Play();
                        }
                    }
                    int t;
                    for (t = sameLineNumber - 1; t >= 0 && alongkeyspressed[t]; t--)
                    {
                    }
                    if (t == -1)
                        pass = true;
                    if (playended)
                    {
                        //if (score == MidiNotes.Length)
                        //if (score == 20)
                        {
                            levelpass.SetActive(true);
                            levelendbar.SetActive(true);
                            StartCoroutine(DelayedOpenMenu());
                            Time.timeScale = 1f;
                        }
                        //else
                        //{
                        //    Time.timeScale = 1f;
                        //    levelfail.SetActive(true);
                        //    levelendbar.SetActive(true);
                        //    StartCoroutine(DelayedOpenMenu());
                        //}
                    }
                    break;
                case 2:
                    ScoreMechanics();
                    if (playended)
                    {
                        if (score >= MidiNotes.Length * 0.8f)
                        {
                            levelpass.SetActive(true);
                            levelendbar.SetActive(true);
                            StartCoroutine(DelayedOpenMenu());
                        }
                        else
                        {
                            Time.timeScale = 1f;
                            StartCoroutine(DelayShowResult());
                        }
                    }
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                    ScoreMechanics();
                    if (playended)
                    {
                        if (score >= MidiNotes.Length)
                        {
                            levelpass.SetActive(true);
                            levelendbar.SetActive(true);
                            StartCoroutine(DelayedOpenMenu());
                        }
                        else
                        {
                            Time.timeScale = 1f;
                            StartCoroutine(DelayShowResult());
                        }
                    }
                    break;
            }
        }


        if (pass)
		{
			Time.timeScale = 1f;
			pass = false;
		}
        
        if (freeplay) // not used in the drum mode
        {
            int keynumber;
            for (keynumber = 0; keynumber < 88; keynumber++)
            {
                int tempnumber = keynumber;
                //tempnumber = 12;
                if (!pressed[tempnumber] && (MidiMaster.GetKeyDown(getNoteNumber(keynumber)) || Input.GetKeyDown(KeyCode.E)))
                {
                    u[tempnumber] = Instantiate(noteUpImage, GameObject.Find("Canvas").transform) as GameObject;
                    u[tempnumber].transform.localPosition = new Vector3((CalcImageIndex(PianoKeyDetector.noteOrder[tempnumber].ToString()) - 1) * noteSize.x / 36f * 37.2f, -30f, 0f);
                    if(Menu.hideKeyboard)
                        u[tempnumber].transform.localPosition = new Vector3((CalcImageIndex(PianoKeyDetector.noteOrder[tempnumber].ToString()) - 1) * noteSize.x / 36f * 37.2f, -54f, 0f);
                    if (PianoKeyDetector.noteOrder[tempnumber].Length == 3)
                    {
                        //Vector2 sizeDelta = u[tempnumber].GetComponent<RectTransform>().sizeDelta;
                        //u[tempnumber].GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x / 2f, sizeDelta.y);
                        u[tempnumber].transform.localPosition = new Vector3(u[tempnumber].transform.localPosition.x + noteSize.x / 36f * 18.6f, u[tempnumber].transform.localPosition.y);
                    }
                    initTime[tempnumber] = Time.time;
                    pressed[tempnumber] = true;

                    PianoKeyDetector.PianoNotes[PianoKeyDetector.noteOrder[tempnumber]].Play(10f, 1f, GlobalSpeed);
                }
                if (pressed[tempnumber] && Time.time - initTime[tempnumber] > 0.1f)
                {
                    //Vector2 sizeDelta = u[tempnumber].GetComponent<RectTransform>().sizeDelta;
                    //u[tempnumber].GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, (int)((Time.time - initTime[tempnumber]) * 60f * 100f) / 10f);
                }
                if(MidiMaster.GetKeyUp(getNoteNumber(keynumber)) || Input.GetKeyUp(KeyCode.E))
                    pressed[tempnumber] = false;
            }
        }

        GameObject[] us = GameObject.FindGameObjectsWithTag("Flow");
        foreach(GameObject usu in us)
        {
            if (usu.transform.localPosition.y > 200f)
                Destroy(usu);
        }
	}
    IEnumerator DelayShowResult()
    {
        yield return new WaitForSeconds(1f);
        levelfail.SetActive(true);
        levelendbar.SetActive(true);
        StartCoroutine(DelayedOpenMenu());
    }
    IEnumerator DelayedOpenMenu()
    {
        yield return new WaitForSeconds(6.5f);
        SceneManager.LoadScene("Drum");
    }
	IEnumerator WaitAndPlay(float t, int _index, bool isDrum)
	{
		yield return new WaitForSeconds(t);
		//try
		{
            if (PianoKeyDetector.PianoNotes.ContainsKey(MidiNotes[_index].Note))
            { 
                if(gamelevel == 1)
                {
                    if (ShowMIDIChannelColours)
                    {
                        PianoKeyDetector.PianoNotes[MidiNotes[_index].Note].Play(MIDIChannelColours[MidiNotes[_index].Channel],
                                                                                MidiNotes[_index].Velocity,
                                                                                MidiNotes[_index].Length,
                                                                                PianoKeyDetector.MidiPlayer.GlobalSpeed * MIDISongs[_midiIndex].Speed);
                    }
                    else
                        PianoKeyDetector.PianoNotes[MidiNotes[_index].Note].Play(MidiNotes[_index].Velocity,
                                                                                MidiNotes[_index].Length,
                                                                                PianoKeyDetector.MidiPlayer.GlobalSpeed * MIDISongs[_midiIndex].Speed);
                }
                currentNoteText.text = (_index + 1).ToString();
                leftNoteText.text = (MidiNotes.Length - _index - 1).ToString();
            }

            if(isDrum)
            {
                //sameLineNumber = 1;
                if (_index == 0)
                    sameLineNumber = 1;
                else if (_index > 0 && MidiNotes[_index].StartTime == MidiNotes[_index - 1].StartTime && DetectIfDrumNote(MidiNotes[_index - 1].Note))
                {
                    sameLineNumber++;

                }
                else
                    sameLineNumber = 1;
                print(sameLineNumber);
                if (_index < MidiNotes.Length - 1 && MidiNotes[_index].StartTime != MidiNotes[_index + 1].StartTime)
                {
                    alongKeys = new int[sameLineNumber];
                    for (int i = sameLineNumber - 1; i >= 0; i--)
                    {
                        alongKeys[i] = DetectDrumNote(MidiNotes[_index - sameLineNumber + i + 1].Note);
                        print(MidiNotes[_index - sameLineNumber + i + 1].Note+"+"+alongKeys[i]);
                    }
                }
                else if (_index == MidiNotes.Length - 1)
                {
                    alongKeys = new int[sameLineNumber];
                    for (int i = sameLineNumber - 1; i >= 0; i--)
                    {
                        //alongKeys[i] = PianoKeyDetector.drumOrder.IndexOf(MidiNotes[_index - sameLineNumber + i + 1].Note);
                        alongKeys[i] = DetectDrumNote(MidiNotes[_index - sameLineNumber + i + 1].Note);
                        print(alongKeys[i]);
                    }
                }
                //for (int j = 0; j < alongkeys.length; j++)
                //    print(alongkeys.length + "+" + alongkeys[j] + "+");
            }

            if (!freeplay)
            {
                if (_index == MidiNotes.Length - 1)
                //if(_index == 10)
                {
                    playended = true;
                    print("Finished");
                }

                alongkeyspressed = new bool[sameLineNumber];
            }
            //_sliderTimer += Time.deltaTime * GlobalSpeed * MidiNotes[_index].Tempo;
            //timeDisplay.GetComponent<Slider>().value = (float)(_sliderTimer / MidiNotes[midiNoteLength - 1].StartTime);
        }
        //catch(Exception ex)
        //{
        //	Debug.LogError(MidiNotes.Length);
        //}
        if (playAlong && !pass && DetectIfDrumNote(MidiNotes[_index].Note))
        {
            Time.timeScale = 0f;
        }
    }

    
	void DisplaySpeed()
	{
		speedDisplay.SetActive(true);
        speedDisplay.GetComponent<Text>().text = "Speed: " + ((int)((GlobalSpeed+0.01f)*10f)/10f).ToString();
        StartCoroutine(HideText(0.5f));
	}
    IEnumerator HideText(float t)
	{
		yield return new WaitForSeconds(t);
        speedDisplay.SetActive(false);
    }
    double CalcTotalTime()
    {
        double curTempo = MidiNotes[0].Tempo;
        int curSameTempoNotes = 1;
        double totalSongTime = 0f;
        for (int i = 1; i < MidiNotes.Length; i++)
        {
            if (MidiNotes[i].Tempo != MidiNotes[i - 1].Tempo || i == MidiNotes.Length - 1)
            {
                totalSongTime += (MidiNotes[i].StartTime - MidiNotes[i - curSameTempoNotes].StartTime) / curTempo;
                curTempo = MidiNotes[i].Tempo;
                curSameTempoNotes = 0;
            }
            else
                curSameTempoNotes++;
        }
        return totalSongTime;
    }

	bool CheckChannels() //if Channel 0 or Channel 2? t/f
	{
		for (int i = 0; i < MidiNotes.Length; i++)
		{
			if (MidiNotes[i].Channel == 0)
				return true;
			else if (MidiNotes[i].Channel == 2)
				return false;
        }
		return true;
	}
	void CheckLeftHandNotes()
	{
		int i = 0;
		while(_noteIndex + i + 1 < MidiNotes.Length && MidiNotes[_noteIndex + i].Channel == MidiNotes[_noteIndex + i + 1].Channel && MidiNotes[_noteIndex + i].StartTime == MidiNotes[_noteIndex + i + 1].StartTime)
		{
			i++;
		}
		if (leftHandSameIndex != _noteIndex + i)
		{
			leftHandInterval = i + 1;
			leftHandSameIndex = _noteIndex + i;
		}
        i = 0;
        while (_noteIndex + i + 1 < MidiNotes.Length && MidiNotes[_noteIndex + i].Channel == MidiNotes[_noteIndex + i + 1].Channel && PianoKeyDetector.noteOrder.IndexOf(MidiNotes[_noteIndex + i].Note) <= PianoKeyDetector.noteOrder.IndexOf(MidiNotes[_noteIndex + i + 1].Note))
        {
            i++;
        }
        if (leftHandOnceIndex != _noteIndex + i)
        {
            leftHandOnceInterval = i + 1;
            leftHandOnceIndex = _noteIndex + i;
        }
    }
	void CheckRightHandNotes()
	{
        int i = 0;
        while (_noteIndex + i + 1 < MidiNotes.Length && MidiNotes[_noteIndex + i].Channel == MidiNotes[_noteIndex + i + 1].Channel && MidiNotes[_noteIndex + i].StartTime == MidiNotes[_noteIndex + i + 1].StartTime)
        {
            i++;
        }
        if (rightHandSameIndex != _noteIndex + i)
        {
            rightHandInterval = i + 1;
            rightHandSameIndex = _noteIndex + i;
        }
        i = 0;
        while (_noteIndex + i + 1 < MidiNotes.Length && MidiNotes[_noteIndex + i].Channel == MidiNotes[_noteIndex + i + 1].Channel && PianoKeyDetector.noteOrder.IndexOf(MidiNotes[_noteIndex + i].Note) <= PianoKeyDetector.noteOrder.IndexOf(MidiNotes[_noteIndex + i + 1].Note))
        {
            i++;
        }
        if (rightHandOnceIndex != _noteIndex + i)
        {
            rightHandOnceInterval = i + 1;
            rightHandOnceIndex = _noteIndex + i;
        }
    }
	string DisplayTotalTime(int t)
	{
		string s = "";
		int m = (int)t / 60;
		s = m.ToString();
		if (m < 10)
			s = "0" + s;
        s += ":";

        t = t - m * 60;
		if (t < 10)
			s += "0" + t.ToString();
		else
			s += t.ToString();
        return s;
	}

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("Rosetta");
        Time.timeScale = 1f;
    }
    
    void SetupNextMIDI()
	{
		if(RepeatType == RepeatType.PlayOnlyOne)
		{
			_midi = null;
			return;
		}
		else if (_midiIndex >= MIDISongs.Length - 1)
		{
			if (RepeatType != RepeatType.NoRepeat)
				_midiIndex = 0;
			else
			{
				_midi = null;
				return;
			}
		}
		else
		{
			if (RepeatType != RepeatType.RepeatOne)
				_midiIndex++;
		}

		PlayCurrentMIDI();
	}

	void PlayCurrentMIDI()
	{
		_timer = 0;

#if UNITY_EDITOR
		_path = string.Format("{0}/MIDI/{1}.mid", Application.streamingAssetsPath, MIDISongs[_midiIndex].MIDIFile.name);
        txt_path = string.Format("{0}/Score/{1}.txt", Application.streamingAssetsPath, MIDISongs[_midiIndex].MIDIFile.name);
#else
		_path = string.Format("{0}/MIDI/{1}.mid", Application.streamingAssetsPath, MIDISongs[_midiIndex].SongFileName);
        txt_path = string.Format("{0}/Score/{1}.txt", Application.streamingAssetsPath, MIDISongs[_midiIndex].SongFileName);
#endif
        _midi = new MidiFileInspector(_path);
        MidiNotes = _midi.GetNotes();
        _noteIndex = 0;

        string[] AllWords = File.ReadAllLines(txt_path);
        string[] fingers = AllWords[2].Split(" ");
        int l;
        if (MidiNotes.Length > fingers.Length)
            l = fingers.Length;
        else
            l = MidiNotes.Length;
        fingerScore = new int[l];
        Debug.LogError(l);
        try
        {
            for (int i = 0; i < l; i++)
            {
                fingerScore[i] = int.Parse(fingers[i]);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        OnPlayTrack.Invoke();
	}

	[ContextMenu("Preset MIDI")]
	void PresetFirstMIDI()
	{
#if UNITY_EDITOR
		_path = string.Format("{0}/MIDI/{1}.mid", Application.streamingAssetsPath, MIDISongs[0].MIDIFile.name);
        txt_path = string.Format("{0}/Score/{1}.txt", Application.streamingAssetsPath, MIDISongs[0].MIDIFile.name);
#else
		_path = string.Format("{0}/MIDI/{1}.mid", Application.streamingAssetsPath, MIDISongs[0].SongFileName);
        txt_path = string.Format("{0}/Score/{1}.txt", Application.streamingAssetsPath, MIDISongs[0].SongFileName);
#endif
        _midi = new MidiFileInspector(_path);
		MidiNotes = _midi.GetNotes();

        string[] AllWords = File.ReadAllLines(txt_path);
        string[] fingers = AllWords[2].Split(" ");
        int l;
        if (MidiNotes.Length > fingers.Length)
            l = fingers.Length;
        else
            l = MidiNotes.Length;
        fingerScore = new int[l];
        Debug.LogError(l);
        try
        {
            for (int i = 0; i < l; i++)
            {
                fingerScore[i] = int.Parse(fingers[i]);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        _preset = true;
	}

	[ContextMenu("Clear MIDI")]
	void ClearPresetMIDI()
	{
		MidiNotes = new MidiNote[0];
		_preset = false;
	}

#if UNITY_EDITOR
	[ContextMenu("MIDI to name")]
	public void MIDIToPlaylist()
	{
		for (int i = 0; i < MIDISongs.Length; i++)
		{
			MIDISongs[i].SongFileName = MIDISongs[i].MIDIFile.name;
		}
	}
#endif
}

