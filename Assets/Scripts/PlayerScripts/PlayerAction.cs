using System.Diagnostics;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerAction : MonoBehaviour
{
    
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    public List<double> timeStamps = new List<double>();
    public List<int> laneNums = new List<int>(); //the lane each note is on
    public double blinkOffset;
    public double blinkCooldown;
    protected double PreviousBlink;
    protected bool AbleToBlink;
    public int InputIndex;
    public int prespawnWarningSeconds;
    public double TimeStamp;
    protected double PerfectMarginOfError;
    protected double NiceMarginOfError;
    protected double AudioTime;
    public bool enableBlink;
    public bool enableArrows;
    protected Lane[] allLanes;
    
    public Color perfectColor  = Color.green;
    public Color niceColor = new Color(1.0f, 1.0f, 0f); //Yellow;
    public Color missColor = Color.red;

    protected virtual void Start() {
        PerfectMarginOfError = MusicPlayer.Current.perfectMarginOfError;
        NiceMarginOfError = MusicPlayer.Current.niceMarginOfError;
        AbleToBlink = true;
    }

    public abstract void SetTimeStamps(IEnumerable<Note> array, Lane[] lanes);

    // Update is called once per frame
    public abstract void Update();

    protected (bool ableToBlink, double previousBlink) CheckBlink(Color blinkColor, Color otherBlinkColor, double timeStamp, double otherTimeStamp, bool ableToBlink, double previousBlink){
        if(!ableToBlink && AudioTime > previousBlink + blinkCooldown){
                ableToBlink = true;
        }

        if (timeStamp - blinkOffset <= AudioTime && timeStamp > AudioTime){
            Blink(otherTimeStamp == timeStamp ? otherBlinkColor : blinkColor);
            ableToBlink = false;
            previousBlink = AudioTime;
        }
        return (ableToBlink, previousBlink);
    }

    protected int GetAccuracy(double timeStamp, int inputIndex, List<int> laneNumsChoice, string direction)
    {
        if (Math.Abs(AudioTime - (timeStamp)) < PerfectMarginOfError)
        {
            //Perfect
            Hit();
            arrowBlink(inputIndex, perfectColor, laneNumsChoice, direction, true);
            print($"Hit on {inputIndex} note - time: {timeStamp} audio time {AudioTime}");
            inputIndex++;
        }
        else if (Math.Abs(AudioTime - (timeStamp)) < NiceMarginOfError)
        {
            //Nice
            Inaccurate();
            arrowBlink(inputIndex, niceColor, laneNumsChoice, direction, true);
            print(
                $"Hit inaccurate on {inputIndex} note with {Math.Abs(AudioTime - timeStamp)} delay - time: {timeStamp} audio time {AudioTime}");
            inputIndex++;
        }
        else{
            //Oops
            Miss();
            print($"Missed {inputIndex} note - time: {timeStamp} audio time {AudioTime}");

            if(AudioTime - timeStamp > NiceMarginOfError){ //After the note has passed
                arrowBlink(inputIndex, missColor, laneNumsChoice, direction, true);
                inputIndex++;
            }else{ //before the note has passed
                arrowBlink(inputIndex, missColor, laneNumsChoice, direction, false);
            }
        }
        return inputIndex;
    }

    protected abstract List<double> AddNoteToTimeStamp(Note curNote, Melanchall.DryWetMidi.MusicTheory.NoteName curNoteRestriction,
        List<double> curTimeStamps, Lane[] lanes, string direction);

    protected int CheckMiss(int inputIndex, double curTimeStamp) {

        if (curTimeStamp + NiceMarginOfError <= AudioTime)
        {
            Miss();
            print($"Missed {inputIndex} note - time: {curTimeStamp} audio time {AudioTime}");
            inputIndex++;
        }
        return inputIndex;
    }

    private void Hit()
    {
        ScoreManager.current.Hit();
    }

    private void Miss()
    {
        ScoreManager.current.Miss();
    }

    private void Inaccurate()
    {
        ScoreManager.current.Inaccurate();
    }

    private void Blink(Color blinkColor)
    {
        PlatformManager.current.InvokeBlink(blinkColor);
    }

    private void arrowBlink(int inputIndex, Color blinkColor, List<int> laneNumsChoice, string direction, bool increment)
    {
        StartCoroutine(allLanes[laneNumsChoice[inputIndex]].ArrowBlinkDelay(blinkColor, direction, increment));
    }

    public abstract void TriggerScoreCalculation(InputAction.CallbackContext context);
}