using System.Collections.Generic;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using UnityEngine.InputSystem;
using Note = Melanchall.DryWetMidi.Interaction.Note;

public class SingleButtonAction : PlayerAction
{
    public Color blinkColor; //Set your own Blink colour
    
    public int GetInputIndex()
    {
        return InputIndex;
    }

    public void SetInputIndex(int inputI)
    {
        InputIndex = inputI;
    }
    
    public override void SetTimeStamps(IEnumerable<Note> array, Lane[] lanes)
    {
        foreach (var note in array)
        {
            if (noteRestriction == NoteName.E)
            {
                timeStamps = AddNoteToTimeStamp(note, noteRestriction, timeStamps, lanes, "down");

            }
            else
            {
                timeStamps = AddNoteToTimeStamp(note, noteRestriction, timeStamps, lanes, "up");
            }
        }
    }

    // Update is called once per frame
    public override void Update()
    {   
        if (Time.timeSinceLevelLoad > 5 && !GameManager.Current.IsGamePaused() && InputIndex < timeStamps.Count && !CountdownManager.Current.countingDown)
        {
            AudioTime = MusicPlayer.Current.GetAudioSourceTime() - (MusicPlayer.Current.inputDelayInMilliseconds / 1000.0);
            TimeStamp = timeStamps[InputIndex];
            if (enableBlink)
                (AbleToBlink, PreviousBlink) = CheckBlink(blinkColor, blinkColor, TimeStamp, TimeStamp,  AbleToBlink, PreviousBlink);
            
            InputIndex = CheckMiss(InputIndex, TimeStamp);
        }
    }
    public override void TriggerScoreCalculation(InputAction.CallbackContext context)
    {
        if (context.performed && Time.timeSinceLevelLoad > 5 && !GameManager.Current.IsGamePaused() && InputIndex < timeStamps.Count)
        {
            InputIndex = GetAccuracy(TimeStamp, InputIndex);
        }
    }
    
    public double GetNextTimestamp(int index)
    {
        if (index < timeStamps.Count)
        {
            return timeStamps[index];
        }
        return 999;
    }
}