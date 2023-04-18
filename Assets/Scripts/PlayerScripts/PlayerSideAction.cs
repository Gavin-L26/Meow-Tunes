using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSideAction : PlayerAction
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestrictionRight;
    public List<double> timeStampsRight = new List<double>();
    public List<int> laneNumsRight = new List<int>();
    public SingleButtonAction jumpAction;
    public int inputIndexRight;
    private double _timeStampRight;
    private double _previousBlinkRight;
    private bool _ableToBlinkRight;
    private Color _blinkColorLeft;
    private Color _blinkColorRight;


    protected override void Start() {
        base.Start();
        _blinkColorLeft = new Color(1f, 0.83f, 0f); //Yellow
        _blinkColorRight = new Color(0.47f, 0.31f, 0.66f); //Purple
    }

    public int GetInputIndex()
    {
        return InputIndex;
    }

    public void SetInputIndex(int inputI)
    {
        InputIndex = inputI;
    }

    public int GetInputIndexRight()
    {
        return inputIndexRight;
    }

    public void SetInputIndexRight(int inputIR)
    {
        inputIndexRight = inputIR;
    }

    public override void SetTimeStamps(IEnumerable<Note> array, Lane[] lanes)
    {
        allLanes = lanes;
        foreach (var note in array)
        {
            timeStamps = AddNoteToTimeStamp(note, noteRestriction, timeStamps, lanes, "left");
            timeStampsRight = AddNoteToTimeStamp(note, noteRestrictionRight, timeStampsRight, lanes, "right");
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

                if (direction == "left"){
                    laneNums.Add(lane);
                }
                else{
                    laneNumsRight.Add(lane);
                }
            }
        }
        return curTimeStamps;
    }

    // Update is called once per frame
    public override void Update()
    {
        if (Time.timeSinceLevelLoad > 5 && !GameManager.Current.IsGamePaused() && !CountdownManager.Current.countingDown)
        {
            AudioTime = MusicPlayer.Current.GetAudioSourceTime() - (MusicPlayer.Current.inputDelayInMilliseconds / 1000.0);
            if (enableBlink)
            {
                (AbleToBlink, PreviousBlink) = CheckBlink(_blinkColorLeft, jumpAction.blinkColor, TimeStamp, jumpAction.TimeStamp, AbleToBlink, PreviousBlink);
                (_ableToBlinkRight, _previousBlinkRight) = CheckBlink(_blinkColorRight, jumpAction.blinkColor, _timeStampRight, jumpAction.TimeStamp,_ableToBlinkRight, _previousBlinkRight);
            }
            
            if (InputIndex < timeStamps.Count){
                TimeStamp = timeStamps[InputIndex];
                InputIndex = CheckMiss(InputIndex, TimeStamp, laneNums, "left");
            }
            if (inputIndexRight < timeStampsRight.Count){
                _timeStampRight = timeStampsRight[inputIndexRight];
                inputIndexRight = CheckMiss(inputIndexRight, _timeStampRight, laneNumsRight, "right");
            }
        }
    }

    private void GetAccuracySide(bool left)
    {
        if (left){
            InputIndex = GetAccuracy( TimeStamp, InputIndex, laneNums, "left");
        }else{
            inputIndexRight = GetAccuracy( _timeStampRight, inputIndexRight, laneNumsRight, "right");
        }
    }
    
    public override void TriggerScoreCalculation(InputAction.CallbackContext context)
    {
        if (Time.timeSinceLevelLoad > 5 && !GameManager.Current.IsGamePaused())
        {
            if (context.performed && context.ReadValue<Vector2>().x == -1 && InputIndex < timeStamps.Count){
                GetAccuracySide(true);
            }
            else if (context.performed && context.ReadValue<Vector2>().x == 1 && inputIndexRight < timeStampsRight.Count){
                GetAccuracySide(false);
            }
        }
    }

}