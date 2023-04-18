using System.Collections.Generic;
using System;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using UnityEngine.InputSystem;
using Note = Melanchall.DryWetMidi.Interaction.Note;
using Melanchall.DryWetMidi.Interaction;

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
        allLanes = lanes;
        foreach (var note in array)
        {
            if (noteRestriction == NoteName.E)
            {
                timeStamps = AddNoteToTimeStamp(note, noteRestriction, timeStamps, lanes, "down");

            }
            else if (noteRestriction == NoteName.D)
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
            
            if (noteRestriction == NoteName.E){
                InputIndex = CheckMiss(InputIndex, TimeStamp, laneNums, "down");
            }
            else if (noteRestriction == NoteName.D){
                InputIndex = CheckMiss(InputIndex, TimeStamp, laneNums, "up");
            }
        }
    }

    protected override List<double> AddNoteToTimeStamp(Note curNote, Melanchall.DryWetMidi.MusicTheory.NoteName curNoteRestriction,
        List<double> curTimeStamps, Lane[] lanes, string direction){
        if (curNote.Octave == 1 && curNote.NoteName == curNoteRestriction)
        {
            var metricTimeSpan =
                TimeConverter.ConvertTo<MetricTimeSpan>(curNote.Time, MusicPlayer.Current.MidiFileTest.GetTempoMap());
            var spawnTime = (double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds +
                                (double)metricTimeSpan.Milliseconds / 1000f;

            curTimeStamps.Add(spawnTime - prespawnWarningSeconds);
            if (enableArrows)
            {
                var velocityAsInt = Convert.ToInt32(curNote.Velocity);
                var lane = velocityAsInt % 10 - 1;
                var heightLevel = velocityAsInt / 10 % 10;
                var oneEighthofBeat = 1 / (MusicPlayer.Current.bpm / 60f) / 2;
                lanes[lane].SpawnArrow((float)spawnTime, heightLevel, direction, oneEighthofBeat);

                laneNums.Add(lane);
            }
        }
        return curTimeStamps;
    }

    public override void TriggerScoreCalculation(InputAction.CallbackContext context)
    {
        if (context.performed && Time.timeSinceLevelLoad > 5 && !GameManager.Current.IsGamePaused() && InputIndex < timeStamps.Count)
        {
            if (context.action.name == "Jump"){
                InputIndex = GetAccuracy(TimeStamp, InputIndex, laneNums, "up");
            }
            else{
                InputIndex = GetAccuracy(TimeStamp, InputIndex, laneNums, "down");
            }
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