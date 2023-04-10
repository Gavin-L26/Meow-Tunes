using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName platformNote;
    public Melanchall.DryWetMidi.MusicTheory.NoteName fishTreatNote;
    public GameObject platformPrefab;
    public GameObject fishTreatPrefab;
    public GameObject checkpointPrefab;
    public GameObject arrowUpPrefab, arrowLeftPrefab, arrowRightPrefab, arrowDownPrefab;
    public List<Platform> platforms = new List<Platform>();
    public List<GameObject> upArrows = new List<GameObject>(); //List of up arrows spawned in this lane
    public List<GameObject> downArrows = new List<GameObject>(); //List of down arrows spawned in this lane
    public List<GameObject> leftArrows = new List<GameObject>(); //List of left arrows spawned in this lane
    public List<GameObject> rightArrows = new List<GameObject>(); //List of right arrows spawned in this lane
    private float _blinkWaitTime = 0.5f;
    
    public int laneNumber;
    private float _oneEighthofBeat;
    public float spacingSize; //based on the size of the current neutral platform
    
    private const float X = 0F;
    private float _y, _z;
    private int currentUpArrowIndex = 0;
    private int currentDownArrowIndex = 0;
    private int currentLeftArrowIndex = 0;
    private int currentRightArrowIndex = 0;

    public void SpawnPlatformsAndFishTreats(IEnumerable<Note> array, float bpm)
    {
        _oneEighthofBeat = 1 / (bpm / 60f) / 2;
        foreach (var note in array)
        {
            //Octave 1 is for player input
            if (note.Octave == 1) continue;
            
            var metricTimeSpan =
                TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, MusicPlayer.Current.MidiFileTest.GetTempoMap());

            var spawnTime = ((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds +
                             (double)metricTimeSpan.Milliseconds / 1000f);
            if (note.NoteName == platformNote) {
                /* Pre-spawning platforms before game starts */
                SpawnPlatform(note.Octave, note.Velocity, (float)spawnTime, _oneEighthofBeat);
            }
            if (note.NoteName == fishTreatNote)
            {
                ScoreManager.current.maximumFishScore += 1;
                SpawnFishTreat(note.Octave, note.Velocity, (float)spawnTime, _oneEighthofBeat);
            }
        }
    }

    private void SpawnPlatform(int octave, Melanchall.DryWetMidi.Common.SevenBitNumber velocity, float spawnTime,
            float oneEighthofBeat)
    //TODO: Check note velocity to spawn different types of platform
    {
        var newPlatform = Instantiate(platformPrefab, transform, true);
        _y = (octave - 2) * 2F;
        _z = (spawnTime / oneEighthofBeat) * spacingSize;
        var position = new Vector3(X, _y, _z);
        newPlatform.transform.localPosition = position;
        newPlatform.transform.rotation = transform.rotation;

        var alteredColor = new Color
        {
            r = newPlatform.GetComponent<Renderer>().material.color.r,
            g = newPlatform.GetComponent<Renderer>().material.color.g,
            b = newPlatform.GetComponent<Renderer>().material.color.b + (_y/50)
        };

        newPlatform.GetComponent<Renderer>().material.color = alteredColor;

        platforms.Add(newPlatform.GetComponent<Platform>());

        if (velocity == (Melanchall.DryWetMidi.Common.SevenBitNumber)83){
            //Checkpoint
            var newCheckpoint = Instantiate(checkpointPrefab, transform, true);
            _y = (octave - 2) * 2F - 1.8F;
            _z = (spawnTime / oneEighthofBeat) * spacingSize;
            position = new Vector3(0.6F, _y, _z);
            newCheckpoint.transform.localPosition = position;
            newCheckpoint.transform.rotation = transform.rotation;
            newCheckpoint.GetComponent<Checkpoint>().laneNumber = laneNumber;
        }
    }

    private void SpawnFishTreat(int octave, Melanchall.DryWetMidi.Common.SevenBitNumber velocity, float spawnTime,
        float oneEighthofBeat)
    {
        // Debug.Log("spawned");
        var newFishtreat = Instantiate(fishTreatPrefab, transform, true);
        _y = (octave - 2) * 2F + 3;
        if (velocity == (Melanchall.DryWetMidi.Common.SevenBitNumber)120)
        {
            _y += 2f;
        }
        _z = (spawnTime / oneEighthofBeat) * spacingSize - 3.5f;
        var position = new Vector3(X, _y, _z);
        // Debug.Log(spawn_time);
        newFishtreat.transform.localPosition = position;
        newFishtreat.transform.rotation = transform.rotation;
    }

    public void SpawnArrow(float spawnTime, int heightLevel, string direction, float oneEighthofBeat)
    {
        var newArrow = Instantiate(GetArrowPrefab(direction), transform, true);
        var newArrowPosition = newArrow.transform.position;
        var y = (2*heightLevel - 1) + newArrowPosition.y;
        var z = (spawnTime / oneEighthofBeat) * spacingSize - 3.5f;
        var position = new Vector3((X + newArrowPosition.x), y, z);
        newArrow.transform.localPosition = position;
        AddNewArrow(newArrow, direction);
    }

    private GameObject GetArrowPrefab(string direction)
    {
        switch (direction)
        {
            case "up":
                return arrowUpPrefab;
            case "left":
                return arrowLeftPrefab;
            case "right":
                return arrowRightPrefab;
            case "down":
                return arrowDownPrefab;
        }

        return null;
    }

    private void AddNewArrow(GameObject arrow, string direction)
    {
        switch (direction)
        {
            case "up":
                upArrows.Add(arrow);
                break;
            case "down":
                downArrows.Add(arrow);
                break;
            case "left":
                leftArrows.Add(arrow);
                break;
            case "right":
                rightArrows.Add(arrow);
                break;
        }
    }

    public IEnumerator ArrowBlinkDelay(Color blinkColor, string direction, bool increment)
    {
        Color previousColor;
        GameObject arrow;

        arrow= GetArrowWithIndex(direction);
        previousColor = arrow.GetComponent<Renderer>().material.GetColor("_BaseColor");
        arrow.GetComponent<Renderer>().material.SetColor("_BaseColor", blinkColor);
        yield return new WaitForSeconds(_blinkWaitTime);
        arrow.GetComponent<Renderer>().material.SetColor("_BaseColor", previousColor);

        //Increment arrow index only the note is hit at the correct time or after it
        if (increment){
            IncrementArrowIndex(direction);
        }
        yield return null;
    }

    private GameObject GetArrowWithIndex(string direction){
        switch (direction)
        {
            case "up":
                return upArrows[currentUpArrowIndex];
            case "down":
                return downArrows[currentDownArrowIndex];
            case "left":
                return leftArrows[currentLeftArrowIndex];
            case "right":
                return rightArrows[currentRightArrowIndex];
        }
        return null;
    }

    private void IncrementArrowIndex(string direction){
        switch (direction)
        {
            case "up":
                currentUpArrowIndex++;
                break;
            case "down":
                currentDownArrowIndex++;
                break;
            case "left":
                currentLeftArrowIndex++;
                break;
            case "right":
                currentRightArrowIndex++;
                break;
        }
    }

    public int[] GetLaneIndexes(){
        return new int[]{currentUpArrowIndex, currentDownArrowIndex, currentLeftArrowIndex, currentRightArrowIndex};
    }

    public void SetLaneIndexes(int[] indexes){
        currentUpArrowIndex = indexes[0];
        currentDownArrowIndex = indexes[1];
        currentLeftArrowIndex = indexes[2];
        currentRightArrowIndex = indexes[3];
    }
}