using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nettle {
public class TimelapseDate {
    public DateTime Date;
    public List<GameObject> Objects;

    public TimelapseDate(DateTime date, List<GameObject> objects) {
        Date = date;
        Objects = objects;
    }

    public TimelapseDate() {
        Date = DateTime.Now;
        Objects = new List<GameObject>();
    }

    public void SetObjectsVisible(bool visible) {
        foreach (var gameObject in Objects) {
            if (gameObject != null) {
                gameObject.SetActive(visible);
            }
        }
    }
}

public class TimelapseObject {
    public string Name;
    public List<TimelapseDate> Dates;

    public TimelapseObject() {
        Name = "";
        Dates = new List<TimelapseDate>();
    }

    public TimelapseObject(string name, List<TimelapseDate> dates) {
        Name = name;
        Dates = dates;
    }

    public bool DateExists(DateTime date, ref int id) {
        for (int i = 0; i < Dates.Count; ++i) {
            if (Dates[i].Date == date) {
                id = i;
                return true;
            }
        }
        return false;
    }

    public void ShowObjectsForTime(DateTime time) {
        for (int i = 0; i < Dates.Count; ++i) {
            if (DateTime.Compare(time, Dates[i].Date) >= 0 &&
                ((i == (Dates.Count-1)) || (DateTime.Compare(time, Dates[i+1].Date) < 0))) {

                Dates[i].SetObjectsVisible(true);
            } else {
                Dates[i].SetObjectsVisible(false);
            }
        }
    }
}

public class TimelapseDateComparer : IComparer<TimelapseDate> {
    public int Compare(TimelapseDate x, TimelapseDate y) {
        DateTime dateX = ((TimelapseDate)x).Date;
        DateTime dateY = ((TimelapseDate)y).Date;

        return DateTime.Compare(dateX, dateY);
    }
}

[Serializable]
public class DateTimeUnityEditor {
    public int Year;
    public int Month;
    public int Day;
    public int Hour;
    public int Minute;
    public int Second;

    public DateTimeUnityEditor() {
        Year = 2000;
        Month = 1;
        Day = 1;
        Hour = 0;
        Minute = 0;
        Second = 0;
    }

    public DateTimeUnityEditor(int year, int month, int day) {
        Year = year;
        Month = month;
        Day = day;
        Hour = 0;
        Minute = 0;
        Second = 0;
    }

    public DateTimeUnityEditor(int year, int month, int day, int hour, int minute, int second) {
        Year = year;
        Month = month;
        Day = day;
        Hour = hour;
        Minute = minute;
        Second = second;
    }

    public DateTime ToDateTime() {
        return new DateTime(Year, Month, Day, Hour, Minute, Second);
    }
}

[Serializable]
public class TimelapseEvent : UnityEvent<long> {
}

public class Timelapse : MonoBehaviour {

    public DateTimeUnityEditor StartDate = new DateTimeUnityEditor();
    public DateTimeUnityEditor EndDate = new DateTimeUnityEditor();
    
    public DateTime CurrenTime = DateTime.Now;

    public List<TimelapseObject> TimelapseObjects = new List<TimelapseObject>();
    public TimelapseEvent OnTimeChange;

    public void Init() {
        // Expected timelapse object format: time_<object_name>_<year>_<month>_<day>
        TimelapseObjects = new List<TimelapseObject>();

        var timelapseObjs = FindObjectsOfType(typeof(GameObject)).Where(v=>v.name.StartsWith("time_")).ToList();
        
        for (int j = 0; j < timelapseObjs.Count; ++j) {
            GameObject timelapseGameObject = (GameObject)timelapseObjs[j];
            var childsCount = timelapseGameObject.transform.childCount;
            GameObject[] childs = new GameObject[childsCount];

            if (childsCount > 0) {
                for (int i = 0; i < childsCount; ++i) {
                    childs[i] = timelapseGameObject.transform.GetChild(i).gameObject;
                }
            }

            var dateSource = timelapseGameObject.name.Split('_');

            int day;
            int month;
            int year;

            if (Int32.TryParse(dateSource[dateSource.Length - 1], out day) &&
                Int32.TryParse(dateSource[dateSource.Length - 2], out month) &&
                Int32.TryParse(dateSource[dateSource.Length - 3], out year)) {

                DateTime objDate = new DateTime(year, month, day);

                string objName = "";
                for (int i = 1; i < dateSource.Length - 3; ++i) {
                    objName += dateSource[i] + "_";
                }
                objName = objName.Remove(objName.Length - 1);

                bool objectExists = false;

                if (TimelapseObjects.Count > 0) {
                    foreach (var timelapseObject in TimelapseObjects) {
                        if (timelapseObject.Name == objName) {
                            //Object with such name exists
                            objectExists = true;
                            int id = 0;
                            if (timelapseObject.DateExists(objDate, ref id)) {
                                //Object already has this date. Strange, but ok, just add new objs
                                timelapseObject.Dates[id].Objects.AddRange(childs);
                            } else {
                                timelapseObject.Dates.Add(new TimelapseDate(objDate, childs.ToList()));
                            }
                        }
                    }
                }
                if (!objectExists) {
                    List<TimelapseDate> newTimelapseDatesList = new List<TimelapseDate>();
                    newTimelapseDatesList.Add(new TimelapseDate(objDate, childs.ToList()));
                    TimelapseObjects.Add(new TimelapseObject(objName, newTimelapseDatesList));
                }
            }
        }
        IComparer<TimelapseDate> tlComparer = new TimelapseDateComparer();
        foreach (var timelapseObject in TimelapseObjects) {
            timelapseObject.Dates.Sort(tlComparer);
        }
    }

    private void SetTimeInternal(DateTime newTime) {
        CurrenTime = newTime < StartDate.ToDateTime() ? StartDate.ToDateTime() : newTime;
        CurrenTime = CurrenTime > EndDate.ToDateTime() ? EndDate.ToDateTime() : CurrenTime;

        foreach (var timelapseObject in TimelapseObjects) {
            timelapseObject.ShowObjectsForTime(CurrenTime);
        }
    }

    public void SetTime(DateTime newTime) {
        SetTimeInternal(newTime);

        if (OnTimeChange != null)
            OnTimeChange.Invoke(newTime.Ticks);
    }

    public void SetTime(long ticks) {
        SetTimeInternal(new DateTime(ticks));

        if (OnTimeChange != null)
            OnTimeChange.Invoke(ticks);
    }

    void Start () {
        Init();
        SetTime(StartDate.ToDateTime());

        print("Start date: " + StartDate.ToDateTime().Ticks);
        print("End date: " + EndDate.ToDateTime().Ticks);
    }

    public void UpdateStartEndDates() {
    //    if (OnChangeStartTime != null)
    //        OnChangeStartTime.Invoke((float)(StartDate.ToDateTime().Ticks));
    //    if (OnChangeEndTime != null)
    //        OnChangeEndTime.Invoke((float)(EndDate.ToDateTime().Ticks));
    }
}
}
