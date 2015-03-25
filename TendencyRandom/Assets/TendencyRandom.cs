using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class RandomFunctions
{

    public static bool createNewListPerFunctionCall = false;
    public static bool shuffleList = true;

    private static List<int> _allGeneratedNums { get; set; }
    private static bool _listCreated = false;

    private static int _minRange = 0;
    private static int _maxRange = 100;
    private static int _originMinRange = 0;
    private static int _originMaxRange = 100;
    private static bool _permaChangeMinMaxRange = false;
    private static System.Random _randomVar = new System.Random();
    private static int _cumulatedPercentage = 0;

    private static string _errorMessageMinRange = "One or more percentages ({0}) is inferior to zero";
    private static string _errorMessageMaxRange = "Total of percentages ({0}) inferior or superior to total range ({1}) [maxRange-minRange]";
    private static string _errorMessageTotalRange = "Total range ({0}) [maxRange-minRange] is equal to object range ({1}). No need to use tendency function, the result will always be true";
    private static string _errorMessageMinRangeZero = "Minimum range ({0}) is Inferior to zero, taking positive value of minRange";
    private static string _errorMessageMaxRangeZero = "Maximum range ({0}) is Inferior to zero, taking positive value of minRange";
    private static string _errorMessageMinMaxRangeInferior = "Minimum range ({0}) is superior to maximum range ({1}), inverting values";
    private static string _errorMessageMinMaxRangeEqual = "Minimum range ({0}) is equal to maximum range ({1}), using default values";


    //add tendency with levels of rareness

    #region Tendency Random Functions

    #region Generic Functions
    /// <summary>
    /// Please Note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandom<T>(Dictionary<T, int> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        List<int>[] generatedRandNumsList = new List<int>[objectsPercentages.Count];
        bool singleObject = false;

        //Error Handling Min/Max Range
        HandleErrorMinMaxRange(ref minRange, ref maxRange);
        //Error Handling  Min/Max Range

        CheckIfNewRange(ref minRange, ref maxRange);

        //Error Handling single object
        if (objectsPercentages.Count == 1)
        {
            int totalRange = maxRange - minRange;
            int firstRange = objectsPercentages.First().Value;
            if (firstRange == totalRange || firstRange > totalRange)
            {
                if (firstRange == totalRange)
                    Debug.LogError(string.Format(_errorMessageTotalRange, totalRange, firstRange));
                if (firstRange > totalRange)
                    Debug.LogError(string.Format(_errorMessageMaxRange, firstRange, (maxRange - minRange)));
                if (returnIndex)
                    return 0;
                else
                    return objectsPercentages.Keys.ElementAt(0);
            }
            else
            {
                objectsPercentages.Add(default(T), (totalRange - firstRange));
                //default(T) means you'll return null if T is a reference type (or a nullable value type), 0 for int, '\0' for char etc
                singleObject = true;
            }
        }
        //Error Handling single object

        //Error Handling
        int totalPercentage = 0;
        for (int percentageIndex = 0; percentageIndex < objectsPercentages.Count; percentageIndex++)
        {
            int curPercentage = objectsPercentages.Values.ElementAt(percentageIndex);
            if (curPercentage < 0)
                Debug.LogError(string.Format(_errorMessageMinRange, curPercentage));
            totalPercentage += curPercentage;
        }
        if (totalPercentage < (maxRange - minRange) || totalPercentage > (maxRange - minRange))
            Debug.LogError(string.Format(_errorMessageMaxRange, totalPercentage, (maxRange - minRange)));
        //Error Handling

        CreateShuffle(minRange, maxRange);

        for (int percentageIndex = 0; percentageIndex < objectsPercentages.Count; percentageIndex++)
        {
            int curPercentage = objectsPercentages.Values.ElementAt(percentageIndex);
            _cumulatedPercentage += curPercentage;

            List<int> numsPerPercentage = new List<int>();
            numsPerPercentage.Clear();
            if (percentageIndex != objectsPercentages.Count - 1)
                numsPerPercentage = SpliceList(curPercentage, percentageIndex);
            else
                numsPerPercentage = SpliceList(curPercentage, percentageIndex, true);

            generatedRandNumsList[percentageIndex] = numsPerPercentage;
        }
        _cumulatedPercentage = 0;

        int finalValue = (int)UnityEngine.Random.Range(_minRange, _maxRange);
        int findIndex = -1;
        foreach (List<int> generatedArray in generatedRandNumsList)
        {
            if (generatedArray.Contains(finalValue))
            {
                findIndex = System.Array.IndexOf(generatedRandNumsList, generatedArray);
                break;
            }
        }
        if (findIndex == -1)
        {
            Debug.LogError("An error has accured : Index not found");
        }

        if (!_permaChangeMinMaxRange)
        {
            InitRange();
        }


        if (singleObject && findIndex == 1)
            return false;
        else if (singleObject && findIndex != 1)
            return true;

        if (returnIndex)
            return findIndex;
        else
            return objectsPercentages.Keys.ElementAt(findIndex);
    }

    #region non primary function
    /// <summary>
    /// Please Note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandom<T>(this System.Object _object, Dictionary<T, int> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        return TendencyRandom(objectsPercentages, returnIndex, maxRange, minRange);
    }

    /// <summary>
    /// Please note that the percentages given to the function must not be higher that 1f which means 100%, 0.5f means 50%
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandom<T>(Dictionary<T, float> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        HandleErrorMinMaxRange(ref minRange, ref maxRange);

        Dictionary<T, int> objectsPercentagesDict = new Dictionary<T, int>();
        float totalRange = 0;
        foreach (KeyValuePair<T, float> entry in objectsPercentages)
        {
            totalRange += entry.Value;
            float newVal = entry.Value.Remap(0f, 1f, minRange, maxRange);
            objectsPercentagesDict.Add(entry.Key, (int)newVal);
        }
        if (totalRange > 1f)
            Debug.LogError(string.Format(_errorMessageMaxRange, totalRange, 1f));
        return TendencyRandom(objectsPercentagesDict, returnIndex, maxRange, minRange);
    }

    /// <summary>
    /// Please note that the percentages given to the function must not be higher that 1f which means 100%, 0.5f means 50%
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandom<T>(this System.Object _object, Dictionary<T, float> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        return TendencyRandom(objectsPercentages, returnIndex, maxRange, minRange);
    }

    /// <summary>
    /// Please note that the percentage given will be the only custom one, the rest of the percentages will be devided equally between the remaining objects in the object array
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandom<T>(T[] objects, int firstPercentage, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        HandleErrorMinMaxRange(ref minRange, ref maxRange);

        int totalRange = maxRange - minRange;
        int remainingPercentage = totalRange - firstPercentage;
        int percentageLeft = (int)Mathf.Floor(remainingPercentage / (objects.Count()-1));



        Dictionary<T, int> objectsPercentagesDict = new Dictionary<T, int>();
        int objectCount = 0;
        foreach (T dicObject in objects)
        {
            if (objectCount == 0)
                objectsPercentagesDict.Add(dicObject, firstPercentage);
            else
                objectsPercentagesDict.Add(dicObject, percentageLeft);
            objectCount++;
        }
        return TendencyRandom(objectsPercentagesDict, returnIndex, maxRange, minRange);
    }

    /// <summary>
    /// Please note that the percentage given will be the only custom one, the rest of the percentages will be devided equally between the remaining objects in the object array
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandom<T>(this System.Object _object, T[] objects, int firstPercentage, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        return TendencyRandom(objects, firstPercentage, returnIndex, maxRange, minRange);
    }
    #endregion

    #endregion

    #region System.Object functions

    /// <summary>
    /// Please note that the function will automatically cast all objects in your dictionnary into "System.Object", This means that you will need to do another cast after the function call
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandomObject(Dictionary<System.Object, int> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        HandleErrorMinMaxRange(ref minRange, ref maxRange);

        //objectsPercentages = objectsPercentages.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        Dictionary<System.Object, int> objectsPercentagesDict = new Dictionary<System.Object, int>();
        foreach (KeyValuePair<System.Object, int> entry in objectsPercentages)
        {
            objectsPercentagesDict.Add((System.Object)entry.Key, entry.Value);
        }
        //objectsPercentagesDict = objectsPercentagesDict.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        return TendencyRandomObjectReturnOpts(objectsPercentagesDict, returnIndex, maxRange, minRange);
    }

    /// <summary>
    /// Please note that the function will automatically cast all objects in your dictionnary into "System.Object", This means that you will need to do another cast after the function call
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandomObject(this System.Object _object, Dictionary<System.Object, int> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        return TendencyRandomObject(objectsPercentages, returnIndex, maxRange, minRange);
    }

    /// <summary>
    /// Please note that the percentages given to the function must not be higher that 1f which means 100%, 0.5f means 50%
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandomObject(Dictionary<System.Object, float> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        HandleErrorMinMaxRange(ref minRange, ref maxRange);

        //objectsPercentages = objectsPercentages.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        
        Dictionary<System.Object, int> objectsPercentagesDict = new Dictionary<System.Object, int>();
        float totalRange = 0;
        foreach (KeyValuePair<System.Object, float> entry in objectsPercentages)
        {
            totalRange += entry.Value;
            float newVal = entry.Value.Remap(0f, 1f, 0, (maxRange-minRange));
            objectsPercentagesDict.Add((System.Object)entry.Key, (int)newVal);
        }
        //objectsPercentagesDict = objectsPercentagesDict.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        
        if (totalRange > 1f)
            Debug.LogError(string.Format(_errorMessageMaxRange, totalRange, 1f));

        return TendencyRandomObjectReturnOpts(objectsPercentagesDict, returnIndex, maxRange, minRange);
    }

    /// <summary>
    /// Please note that the total of the percentages given to the function must not be higher that 1f which means 100%, 0.5f means 50%
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandomObject(this System.Object _object, Dictionary<System.Object, float> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        return TendencyRandomObject(objectsPercentages, returnIndex, maxRange, minRange);
    }
     
    /// <summary>
    /// Please note that the percentage given will be the only custom one, the rest of the percentages will be devided equally between the remaining objects in the object array
    /// Also note that the function will automatically cast all objects in your array into "System.Object", This means that you will need to do another cast after the function call
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandomObject(System.Object[] objects, int firstPercentage, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        HandleErrorMinMaxRange(ref minRange, ref maxRange);

        System.Object[] objectsPercentagesArray = new System.Object[objects.Count()];
        for (int i = 0; i < objects.Count(); i++)
        {
            System.Object singleObject = objects[i];
            objectsPercentagesArray[i] = (System.Object)objects[i];
        }

        if (returnIndex || objects.Count() == 1)
            return TendencyRandom(objectsPercentagesArray, firstPercentage, returnIndex, maxRange, minRange);
        else
        {
            int findIndex = (int)TendencyRandom(objectsPercentagesArray, firstPercentage, true, maxRange, minRange);
            return objects[findIndex];
        }
    }

    /// <summary>
    /// Please note that the percentage given will be the only custom one, the rest of the percentages will be devided equally between the remaining objects in the object array
    /// Also note that the function will automatically cast all objects in your array into "System.Object", This means that you will need to do another cast after the function call
    /// Also note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandomObject(this System.Object _object, System.Object[] objects, int firstPercentage, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        return TendencyRandomObject(objects, firstPercentage, returnIndex, maxRange, minRange);
    }

    private static System.Object TendencyRandomObjectReturnOpts(Dictionary<System.Object, int> objectsPercentages, bool returnIndex = false, int maxRange = 100, int minRange = 0)
    {
        if (returnIndex || objectsPercentages.Count() == 1)
            return TendencyRandom(objectsPercentages, returnIndex, maxRange, minRange);
        else
        {
            int findIndex = (int)TendencyRandom(objectsPercentages, true, maxRange, minRange);
            return objectsPercentages.Keys.ElementAt(findIndex);
        }
    } 

    #endregion

    #region other Functions
    /// <summary>
    /// Please Note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandom(int[] objectsPercentages, int maxRange = 100, int minRange = 0)
    {
        HandleErrorMinMaxRange(ref minRange, ref maxRange);

        Dictionary<System.Object, int> objectsPercentagesDict = new Dictionary<System.Object, int>();
        foreach (int percentage in objectsPercentages)
        {
            objectsPercentagesDict.Add(new System.Object(), percentage);
        }
        return TendencyRandom(objectsPercentagesDict, true, maxRange, minRange);
    }

    /// <summary>
    /// Please Note that the maxRange is exclusive while minRange is inclusive
    /// </summary>
    public static System.Object TendencyRandom(this System.Object _object, int[] objectsPercentages, int maxRange = 100, int minRange = 0)
    {
        return TendencyRandom(objectsPercentages, maxRange, minRange);
    }
    #endregion


    #endregion

    #region Miscellaneus Functions for tendency random

    public static void PermaChangeMinMaxRange(int maxRange = 100, int minRange = 0)
    {
        _permaChangeMinMaxRange = true;
        ChangeRange(minRange, maxRange);
    }

    private static void HandleErrorMinMaxRange(ref int minRange, ref int maxRange){
        if (minRange < 0)
        {
            Debug.LogError(string.Format(_errorMessageMinRangeZero, minRange));
            Debug.Log(minRange);
        }
        if (maxRange < 0)
        {
            Debug.LogError(string.Format(_errorMessageMaxRangeZero, maxRange));
            maxRange = Mathf.Abs(maxRange);
        }

        if (maxRange < minRange)
        {
            Debug.LogError(string.Format(_errorMessageMinMaxRangeInferior, minRange, maxRange));
            int tempMaxRange = minRange;
            int tempMinRange = maxRange;
            minRange = tempMinRange;
            maxRange = tempMaxRange;
        }

        if (maxRange == minRange)
        {
            Debug.LogError(string.Format(_errorMessageMinMaxRangeEqual, minRange, maxRange));
            minRange = _originMinRange;
            maxRange = _originMaxRange;
        }
    }

    private static void InitRange()
    {
        _minRange = _originMinRange;
        _maxRange = _originMaxRange;
    }

    private static void CheckIfNewRange(ref int minRange, ref int maxRange)
    {
        if (_permaChangeMinMaxRange)
        {
            minRange = _minRange;
            maxRange = _maxRange;
        }
        else if ((minRange != _minRange || maxRange != _maxRange) && !_permaChangeMinMaxRange)
        {
            ChangeRange(minRange, maxRange);
        }
    }

    private static void ChangeRange(int minRange, int maxRange)
    {
        _minRange = minRange;
        _maxRange = maxRange;

        CreateList();
    }

    private static void CreateList()
    {
        _allGeneratedNums = new List<int>();
        _allGeneratedNums.Clear();
        for (int i = _minRange; i < _maxRange; i++)
        {
            _allGeneratedNums.Add(i);
        }
    }

    private static void ShuffleList<E>(IList<E> list)
    {
        _randomVar = new System.Random();
        if (list.Count > 1)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                E tmp = list[i];
                int randomIndex = _randomVar.Next(i + 1);

                //Swap elements
                list[i] = list[randomIndex];
                list[randomIndex] = tmp;
            }
        }
    }

    private static void CreateShuffle(int minRange, int maxRange)
    {
        if (!_listCreated)
        {
            CreateList();
            _listCreated = true;
        }

        if (createNewListPerFunctionCall)
            CreateList();

        if (shuffleList)
            ShuffleList(_allGeneratedNums);
    }

    private static List<int> SpliceList(int curPercentage, int percentageIndex, bool lastEntry = false)
    {
        List<int> numsPerPercentage = new List<int>();
        numsPerPercentage.Clear();
        numsPerPercentage = new List<int>(_allGeneratedNums);

        int maxIndex = _maxRange - _minRange;

        if (percentageIndex == 0)
        {
            numsPerPercentage.RemoveRange(_cumulatedPercentage, maxIndex - _cumulatedPercentage);
        }
        else if (lastEntry)
        {
            numsPerPercentage.RemoveRange(0, maxIndex - curPercentage);
        }
        else
        {
            numsPerPercentage.RemoveRange(_cumulatedPercentage, maxIndex - _cumulatedPercentage);
            numsPerPercentage.RemoveRange(0, _cumulatedPercentage - curPercentage);
        }

        return numsPerPercentage;
    }

    private static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    private static bool FindNewValue(ref int tempValue)
    {
        tempValue = (int)UnityEngine.Random.Range(_minRange, _maxRange);
        if (!_allGeneratedNums.Contains(tempValue))
            return true;
        else
            return false;
    }

    public static bool TryCast<T>(ref T t, System.Object o)
    {
        //How to use :
        //TryCast(ref ObjectWithTheDesiredType, ObjectToCastToNewType);
        if (o == null || !typeof(T).IsAssignableFrom(o.GetType()))
            return false;
        t = (T)o;
        return true;
    }

    #endregion
}
