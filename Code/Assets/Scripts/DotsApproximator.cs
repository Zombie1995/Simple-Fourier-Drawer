using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DotsApproximator : MonoBehaviour
{
    public InputField resDotsNumField;
    
    public GameObject dotObj;
    
    public List<Vector2> startDots = new List<Vector2>();
    public List<Vector2> resultDots = new List<Vector2>();

    public bool visualize = false;

    public void LoadCoords() 
    {
        startDots.Clear();
        resultDots.Clear();

        ReadCoords();
        if (startDots.Count > 0)
        {
            int temp = 0; ;
            int.TryParse(resDotsNumField.text, out temp);
            temp = (temp > 0) ? Math.Max(startDots.Count, temp) : 20000;
            resDotsNumField.text = temp.ToString();
            Approximate(temp);
        }
    }

    public void ReadCoords(string filepath = @"coords.txt") 
    {
        try
        {
            using (StreamReader sr = new StreamReader(filepath))
            {
                while (sr.Peek() >= 0)
                {
                    string[] coords = sr.ReadLine().Split(' ');
                    
                    float x = 0;
                    float y = 0;
                    float.TryParse(coords[0], out x);
                    float.TryParse(coords[1], out y);
                    
                    startDots.Add(new Vector2(x, y));
                }
            }
        }
        catch (Exception e)
        {
            print("The process failed: " + e.ToString());
        }
    }
    public void Approximate(int resDotsNum = 20000) 
    {
        float totalLength = 0;
        for (int i = 0; i < startDots.Count - 1; i++)
        {
            totalLength += (startDots[i + 1] - startDots[i]).magnitude;
        }
        totalLength += (startDots[0] - startDots[startDots.Count - 1]).magnitude;

        float speed = (float)totalLength / (float)resDotsNum;
        Vector2 tempVector = new Vector2(0, 0);
        float difference = 0;
        Vector2 unitVector = new Vector2(0, 0);
        float localLength = 0;
        for (int i = 0; i < startDots.Count - 1; i++)
        {
            unitVector = startDots[i + 1] - startDots[i];
            localLength = unitVector.magnitude;
            tempVector = difference * unitVector.normalized;
            unitVector = unitVector.normalized * speed;

            while (tempVector.magnitude < localLength)
            {
                resultDots.Add(startDots[i] + tempVector);

                tempVector += unitVector;
            }
            difference = tempVector.magnitude - localLength;
        }
        //to connect last and first dot
        ///////////////////
        unitVector = startDots[0] - startDots[startDots.Count - 1];
        localLength = unitVector.magnitude;
        tempVector = difference * unitVector.normalized;
        unitVector = unitVector.normalized * speed;

        while ((tempVector.magnitude < localLength) && (resultDots.Count < resDotsNum))
        {
            resultDots.Add(startDots[startDots.Count - 1] + tempVector);

            tempVector += unitVector;
        }
        ///////////////////

        if (visualize)
        {
            foreach (Vector2 dot in resultDots)
            {
                Instantiate(dotObj, dot, Quaternion.identity);
            }
        }
    }
}
