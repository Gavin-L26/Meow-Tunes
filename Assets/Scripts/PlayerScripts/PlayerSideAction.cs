using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSideAction : PlayerAction
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestrictionRight;
    public List<double> timeStampsRight = new List<double>();
    private int _inputIndexRight;
    private double _timeStampRight;

    public override void SetTimeStamps(IEnumerable<Note> array)
    {
        foreach (var note in array)
        {
            timeStamps = AddNoteToTimeStamp(note, noteRestriction, timeStamps);
            timeStampsRight = AddNoteToTimeStamp(note, noteRestrictionRight, timeStampsRight);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        if (Time.time > 5 && !GameManager.current.IsGamePaused())
        {
            _marginOfError = MusicPlayer.current.marginOfError;
            _audioTime = MusicPlayer.GetAudioSourceTime() - (MusicPlayer.current.inputDelayInMilliseconds / 1000.0);
            if (_inputIndex < timeStamps.Count){
                _timeStamp = timeStamps[_inputIndex];
                _inputIndex = CheckMiss(_inputIndex, _timeStamp);
            }
            else if (_inputIndexRight < timeStampsRight.Count){
                _timeStampRight = timeStampsRight[_inputIndexRight];
                _inputIndexRight = CheckMiss(_inputIndexRight, _timeStampRight);
            }
        }
    }
    
    protected void GetAccuracySide(bool left)
    {
        if (left){
            _inputIndex = GetAccuracy( _timeStamp, _inputIndex);
        }else{
            _inputIndexRight = GetAccuracy( _timeStampRight, _inputIndexRight);
        }
    }
    
    public override void TriggerScoreCalculation(InputAction.CallbackContext context)
    {
        if (Time.time > 5 && !GameManager.current.IsGamePaused())
        {
            if (context.ReadValue<Vector2>().x < 0 && _inputIndex < timeStamps.Count){
                GetAccuracySide(true);
            }
            else if (context.ReadValue<Vector2>().x > 0 && _inputIndexRight < timeStampsRight.Count){
                GetAccuracySide(false);
            }
        }
    }

}