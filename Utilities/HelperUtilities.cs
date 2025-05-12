using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    public static bool ValidateCheckEmptyString(Object obj, string fieldName, string stringToCheck)
    {
        if (string.IsNullOrEmpty(stringToCheck))
        {
            Debug.Log(fieldName + "字段为空" + obj.name.ToString());
            return true;
        }

        return false;
    }

    public static bool ValidateCheckEnumerableValues(Object obj, string filedName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;
        if (enumerableObjectToCheck == null)
        {
            Debug.Log(filedName +  "对象为空" + obj.name.ToString());
        }
        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log(filedName + "字段为空" + obj.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log(filedName + "没有值" + obj.name.ToString());
            error = true;
        }

        return error;
    }
    
}
