using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvgValue {
    private class Element {
        public float Value;
        public float Time;
        public float DeltaTime;
    }

    /// <summary>
    /// Avg value calculated by last "TrackingTime" seconds
    /// </summary>
    public float TrackingTime = 0.5f;

    private LinkedList<Element> Values = new LinkedList<Element>();
    private float _valuePerSec = 0; //Possible float point error accumulate

    public AvgValue(float trackingTime = 0.5f) {
        TrackingTime = trackingTime;
        Element element = new Element() {
            Value = 0,
            Time = Time.time,
            DeltaTime = TrackingTime
        };
        Values.AddFirst(element);
    }

    public void Add(float value) {
        if (Values.Last.Value.Time == Time.time) { //if same frame
            Values.Last.Value.Value += value;
        } else {
            Element element = new Element() {
                Value = value,
                Time = Time.time,
                DeltaTime = Time.time - Values.Last.Value.Time
            };
            Values.AddLast(element);
        }
        _valuePerSec += value / Values.Last.Value.DeltaTime;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Avg value per sec</returns>
    public float Get() {
        Element element = Values.First.Value;
        while (element.Time + TrackingTime < Time.time && Values.Count > 1) {
            _valuePerSec -= element.Value / element.DeltaTime;
            Values.RemoveFirst();
            element = Values.First.Value;
        }
        Debug.Log("AvgValue::" + _valuePerSec + "::" + TrackingTime + "::" + element.Time + "::=" + (_valuePerSec * Mathf.Lerp(TrackingTime, 0, Time.time - (element.Time + TrackingTime))));
        return _valuePerSec * Mathf.Lerp(TrackingTime, 0, Time.time - (element.Time + TrackingTime));

    }

}
