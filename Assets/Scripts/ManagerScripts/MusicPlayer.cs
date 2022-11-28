using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Current;
    
    public AudioSource audioSource;
    public AudioClip songIntroNormal;

    // public float songBpm = 120;
    // public float secPerBeat;
    // public float songPosition;
    // public float songPositionInBeats;
    // public float dspSongTime;

    public static MidiFile MidiFileTest;
    public float bpm;
    public float spacingSize; //based on the size of the current neutral platform

    public string midiFileName;
    public Lane[] lanes;
    public PlayerAction[] playerActions;
    public double marginOfError = 0.3;
    public int inputDelayInMilliseconds; //Delay Time for when the music starts

    private bool _audioPlayed;
    private void Awake()
    {
        Current = this;
    }
    
    private void Start()
    {
        MidiFileTest = null;
        if (Application.platform is RuntimePlatform.WindowsPlayer or RuntimePlatform.OSXEditor or RuntimePlatform.WindowsEditor)
            MidiFileTest = MidiFile.Read(Application.dataPath + "/StreamingAssets/" + midiFileName);
        if (Application.platform == RuntimePlatform.OSXPlayer)
            MidiFileTest = MidiFile.Read(Application.dataPath + "/Resources/Data/StreamingAssets/" + midiFileName);
        
        var notes = MidiFileTest.GetNotes();
        var array = new Note[notes.Count];
        // Debug.Log(notes.Count);
        notes.CopyTo(array, 0);
        foreach (var lane in lanes){
            lane.SpawnPlatformsAndFishTreats(array, bpm, spacingSize);
            // Debug.Log(lane.timeStamps.Count);
        }
        foreach (var playerAction in playerActions){
            playerAction.SetTimeStamps(array);
        }

        audioSource.clip = songIntroNormal;
        audioSource.loop = false;
        
        // secPerBeat = 60f / songBpm;
        // dspSongTime = (float)AudioSettings.dspTime;
    }

    private void Update()
    {
        // songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        // songPositionInBeats = songPosition / secPerBeat;
        if (Time.timeSinceLevelLoad > 5 && !_audioPlayed)
        {
            Debug.Log("played!");
            audioSource.Play();
            _audioPlayed = true;
        }
    }

    // private void SwitchMusicFromIntroToLoop()
    // {
    //     if (songLoopNormal != null)
    //     {
    //         audioSource.clip = songLoopNormal;
    //     }
    //     audioSource.Play();
    //     audioSource.loop = true;
    // }

    public void Pause()
    {
        audioSource.Pause();
    }

    public void Resume()
    {
        audioSource.Play();
    }

    public double GetAudioSourceTime()
    {
        return (double)Current.audioSource.timeSamples / Current.audioSource.clip.frequency;
    }

    public void ResetAllFishTreats()
    {
        var notes = MidiFileTest.GetNotes();
        var array = new Note[notes.Count];
        // Debug.Log(notes.Count);
        notes.CopyTo(array, 0);
        foreach (var lane in lanes){
            lane.DestroyAllFishTreats();
            lane.RespawnAllFishTreats(array, spacingSize);
        }
    }
}
