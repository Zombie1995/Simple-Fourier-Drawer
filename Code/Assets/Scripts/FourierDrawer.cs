using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;
using System;
using System.IO;

public class FourierDrawer : MonoBehaviour
{
    public Material firstMaterial;
    public Material secondMaterial;

    public Scrollbar zoom;
    public InputField minutesToGrowField;
    public InputField numberOfCircles;
    public InputField percentOfDrawing;
    public InputField cycleTime;
    public float rotationTime = 1f;
    float prRotationTime = 1f;
    public InputField lineSizeField;
    float lineSize = 0.05f;
    float prLineSize = 0.05f;

    public bool draw = false;

    public bool withZeroRotationCircle = true;
    bool prZeroRotationCircleState = true;
    float ZeroRotationCircleMagnitude = 0f;
    int ZeroRotationCircleMagnitudeOrder = 0;

    public float drawPercent = 100f;
    float prDrawPercent = 100f;

    public Transform drawingDot;
    LineRenderer line;
    TrailRenderer trail;

    public List<Complex> constants = new List<Complex>();
    List<Complex> circlePositions = new List<Complex>();

    float t = 0f;

    List<float> magnitudes = new List<float>();
    List<int> magnitudesOrderIndexes = new List<int>();
    float totalMagnitude = 0f;
    int lastCircleIndex = 0;
    float lastCircleRadius = 0f;

    UnityEngine.Vector3 camPosition = new UnityEngine.Vector3(0.1541656f, 0.08832769f, -1.98573f);
    float camDist = -1.98573f;
    float prZoomValue = 0f;

    float minutesToGrow = 0f;
    bool lineGrow = false;

    void Awake()
    {
        constants = startConstants;

        for (int i = 0; i < constants.Count; i++)
        {
            circlePositions.Add(new Complex(1, 0));
        }

        line = drawingDot.GetComponent<LineRenderer>();
        line.positionCount = constants.Count + 1;
        trail = drawingDot.GetComponent<TrailRenderer>();

        drawingDot.position = new UnityEngine.Vector2(-1.060156E-06f, 0.5725658f);
        trail.time = rotationTime;

        for (int i = 0; i < constants.Count; i++)
        {
            magnitudes.Add(Mathf.Sqrt((float)(constants[i].Real * constants[i].Real + constants[i].Imaginary * constants[i].Imaginary)));
            magnitudesOrderIndexes.Add(i);
            totalMagnitude += magnitudes[i];
        }
        for (int i = 0; i < constants.Count; i++)
        {
            for (int j = i + 1; j < constants.Count; j++)
            {
                if (magnitudes[j] > magnitudes[i])
                {
                    float temp = magnitudes[i];
                    magnitudes[i] = magnitudes[j];
                    magnitudes[j] = temp;

                    temp = magnitudesOrderIndexes[i];
                    magnitudesOrderIndexes[i] = magnitudesOrderIndexes[j];
                    magnitudesOrderIndexes[j] = (int)temp;
                }
            }
        }

        for (int i = 0; i < constants.Count; i++)
        {
            if (magnitudesOrderIndexes[i] == (int)(constants.Count / 2))
            {
                ZeroRotationCircleMagnitudeOrder = i;
                ZeroRotationCircleMagnitude = magnitudes[i];
                break;
            }
        }

        lastCircleIndex = constants.Count - 1;
        lastCircleRadius = magnitudes[lastCircleIndex];

        draw = true;
        prZeroRotationCircleState = true;
        withZeroRotationCircle = false;

        StartCoroutine(StartNewTrail());
    }

    void Update()
    {
        if (draw)
        {
            if (withZeroRotationCircle != prZeroRotationCircleState)
            {
                if (withZeroRotationCircle)
                {
                    magnitudes.Insert(ZeroRotationCircleMagnitudeOrder, ZeroRotationCircleMagnitude);
                    magnitudesOrderIndexes.Insert(ZeroRotationCircleMagnitudeOrder, (int)(constants.Count / 2));
                    totalMagnitude += ZeroRotationCircleMagnitude;
                }
                else
                {
                    magnitudes.RemoveAt(ZeroRotationCircleMagnitudeOrder);
                    magnitudesOrderIndexes.RemoveAt(ZeroRotationCircleMagnitudeOrder);
                    totalMagnitude -= ZeroRotationCircleMagnitude;
                }

                prDrawPercent = -1f;

                prZeroRotationCircleState = withZeroRotationCircle;
            }

            float.TryParse(cycleTime.text, out rotationTime);
            if (rotationTime != prRotationTime)
            {
                rotationTime = (rotationTime > 0) ? rotationTime : 20f;
                cycleTime.text = rotationTime.ToString();

                trail.time = rotationTime;

                prRotationTime = rotationTime;
            }

            float.TryParse(lineSizeField.text, out lineSize);
            if (lineSize != prLineSize)
            {
                lineSize = (lineSize > 0) ? lineSize : 0.05f;
                lineSizeField.text = lineSize.ToString();

                line.widthMultiplier = lineSize * 0.2f;
                trail.widthMultiplier = lineSize;

                prLineSize = lineSize;
            }

            float.TryParse(percentOfDrawing.text, out drawPercent);
            if (drawPercent != prDrawPercent)
            {
                if ((drawPercent > 100f) || (drawPercent < 0f))
                {
                    drawPercent = 100f;
                }
                percentOfDrawing.text = drawPercent.ToString();

                float curMagnitude = totalMagnitude * (drawPercent / 100f);

                float temp = 0f;
                for (int i = 0; i < magnitudes.Count; i++)
                {
                    if ((temp + magnitudes[i]) > curMagnitude)
                    {
                        lastCircleIndex = i;
                        lastCircleRadius = curMagnitude - temp;
                        break;
                    }
                    temp += magnitudes[i];
                    if (i == magnitudes.Count - 1)
                    {
                        lastCircleIndex = i;
                        lastCircleRadius = magnitudes[i];
                    }
                }

                line.positionCount = lastCircleIndex + 2;

                prDrawPercent = drawPercent;
            }

            for (int i = 0; i <= lastCircleIndex; i++)
            {
                circlePositions[magnitudesOrderIndexes[i]] = Complex.Pow(Mathf.Exp(1), (new Complex(0, (magnitudesOrderIndexes[i] - (int)(constants.Count / 2)) * 2 * Mathf.PI * t)));
            }
            
            Complex c = new Complex(0, 0);
            line.SetPosition(0, new UnityEngine.Vector3((float)c.Real, (float)c.Imaginary, 0));
            for (int i = 0; i < lastCircleIndex; i++)
            {
                c += circlePositions[magnitudesOrderIndexes[i]] * constants[magnitudesOrderIndexes[i]];
                line.SetPosition(i + 1, new UnityEngine.Vector3((float)c.Real, (float)c.Imaginary, 0));
            }
            c += circlePositions[magnitudesOrderIndexes[lastCircleIndex]] * constants[magnitudesOrderIndexes[lastCircleIndex]] * (lastCircleRadius / magnitudes[lastCircleIndex]);
            line.SetPosition(lastCircleIndex + 1, new UnityEngine.Vector3((float)c.Real, (float)c.Imaginary, 0));

            drawingDot.position = new UnityEngine.Vector3((float)c.Real, (float)c.Imaginary, 0);

            t += Time.deltaTime / rotationTime;
            if (t > 1)
            {
                t = t - 1;
            }
        }
        
        if (zoom.value != prZoomValue)
        {
            if (zoom.value == 0)
            {
                line.widthMultiplier = lineSize * 0.2f;
                trail.widthMultiplier = lineSize;

                transform.position = camPosition;
            }
            else
            {
                line.widthMultiplier = (lineSize - lineSize * (-0.015f / camDist)) * 0.2f * (1f - zoom.value) + lineSize * (-0.015f / camDist) * 0.2f;
                trail.widthMultiplier = (lineSize - lineSize * (-0.015f / camDist)) * (1f - zoom.value) + lineSize * (-0.015f / camDist);
            }

            prZoomValue = zoom.value;
        }
        if (zoom.value > 0)
        {
            transform.position = new UnityEngine.Vector3(drawingDot.position.x, drawingDot.position.y, (camDist + 0.015f) * (1f - zoom.value) - 0.015f);
        }

        if (lineGrow) 
        {
            drawPercent += (Time.deltaTime / (minutesToGrow * 60f)) * 100f;
            percentOfDrawing.text = drawPercent.ToString();
            if (drawPercent > 100) 
            {
                drawPercent = 100f;
                percentOfDrawing.text = drawPercent.ToString();

                lineGrow = false;
            }
        }
    }

    public void CalcDrawing()
    {
        if (GetComponent<DotsApproximator>().resultDots.Count == 0) return;
        constants.Clear();
        circlePositions.Clear();
        magnitudes.Clear();
        magnitudesOrderIndexes.Clear();
        totalMagnitude = 0f;
        lastCircleIndex = 0;
        lastCircleRadius = 0f;

        int temp = 0;
        int.TryParse(numberOfCircles.text, out temp);
        temp = (temp > 1) ? temp : 2;
        numberOfCircles.text = temp.ToString();
        CalcCircles(temp);
        draw = true;
        prZeroRotationCircleState = true;
        withZeroRotationCircle = false;

        StartCoroutine(StartNewTrail());

        float minX = GetComponent<DotsApproximator>().resultDots[0].x;
        float maxX = GetComponent<DotsApproximator>().resultDots[0].x;
        float minY = GetComponent<DotsApproximator>().resultDots[0].y;
        float maxY = GetComponent<DotsApproximator>().resultDots[0].y;
        foreach (UnityEngine.Vector2 dot in GetComponent<DotsApproximator>().resultDots)
        {
            if (dot.x > maxX)
            {
                maxX = dot.x;
            }
            else if (dot.x < minX)
            {
                minX = dot.x;
            }

            if (dot.y > maxY)
            {
                maxY = dot.y;
            }
            else if (dot.y < minY)
            {
                minY = dot.y;
            }
        }

        float distX = maxX - minX;
        float distY = maxY - minY;
        float neededSize = 0;
        float angle = 0;
        if ((distX / distY) > GetComponent<Camera>().aspect)
        {
            neededSize = distX;
            angle = Camera.VerticalToHorizontalFieldOfView(GetComponent<Camera>().fieldOfView, GetComponent<Camera>().aspect);
        }
        else
        {
            neededSize = distY;
            angle = GetComponent<Camera>().fieldOfView;
        }

        camDist = -1.1f * Mathf.Sqrt(((neededSize * neededSize) / (2 * (1 - Mathf.Cos(Mathf.Deg2Rad * angle)))) - ((neededSize * neededSize) / 4));
        float offsetX = 0;
        float offsetY = 0;
        if (!withZeroRotationCircle)
        {
            offsetX = (float)constants[(int)(constants.Count / 2.0f)].Real;
            offsetY = (float)constants[(int)(constants.Count / 2.0f)].Imaginary;
        }
        camPosition = new UnityEngine.Vector3(((maxX + minX) / 2) - offsetX, ((maxY + minY) / 2) - offsetY, camDist);
        transform.position = camPosition;
        GetComponent<Camera>().farClipPlane = -camDist * 3f;

        //string path = @"temp.txt";
        //try
        //{
        //    if (File.Exists(path))
        //    {
        //        File.Delete(path);
        //    }
        //    using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.CreateNew)))
        //    {
        //        foreach (Complex dot in constants)
        //        {
        //            sw.WriteLine("new UnityEngine.Vector2(" + ((float)dot.Real).ToString() + " " + ((float)dot.Imaginary).ToString() + "),");
        //        }
        //    }
        //}
        //catch (Exception e)
        //{
        //    print("The process failed: " + e.ToString());
        //}
    }

    public void CalcCircles(int circlesNum = 250)
    {
        float dt = 1.0f / (float)GetComponent<DotsApproximator>().resultDots.Count;

        List<Complex> dots = new List<Complex>();
        foreach (UnityEngine.Vector2 dot in GetComponent<DotsApproximator>().resultDots)
        {
            dots.Add(new Complex(dot.x, dot.y));
        }

        for (int i = 0; i < circlesNum; i++)
        {
            float time = 0;

            constants.Add(new Complex(0, 0));

            foreach (Complex dot in dots)
            {
                constants[i] += dot * (Complex.Pow(Mathf.Exp(1), (new Complex(0, -(i - (int)(circlesNum / 2)) * 2 * Mathf.PI * time)))) * dt;

                time += dt;
            }
        }

        for (int i = 0; i < constants.Count; i++)
        {
            circlePositions.Add(new Complex(1, 0));
        }

        drawingDot.position = GetComponent<DotsApproximator>().resultDots[0];
        trail.time = rotationTime;

        line = drawingDot.GetComponent<LineRenderer>();
        line.positionCount = constants.Count + 1;

        for (int i = 0; i < constants.Count; i++)
        {
            magnitudes.Add(Mathf.Sqrt((float)(constants[i].Real * constants[i].Real + constants[i].Imaginary * constants[i].Imaginary)));
            magnitudesOrderIndexes.Add(i);
            totalMagnitude += magnitudes[i];
        }
        for (int i = 0; i < constants.Count; i++)
        {
            for (int j = i + 1; j < constants.Count; j++)
            {
                if (magnitudes[j] > magnitudes[i])
                {
                    float temp = magnitudes[i];
                    magnitudes[i] = magnitudes[j];
                    magnitudes[j] = temp;

                    temp = magnitudesOrderIndexes[i];
                    magnitudesOrderIndexes[i] = magnitudesOrderIndexes[j];
                    magnitudesOrderIndexes[j] = (int)temp;
                }
            }
        }

        for (int i = 0; i < constants.Count; i++)
        {
            if (magnitudesOrderIndexes[i] == (int)(constants.Count / 2))
            {
                ZeroRotationCircleMagnitudeOrder = i;
                ZeroRotationCircleMagnitude = magnitudes[i];
                break;
            }
        }

        lastCircleIndex = circlesNum - 1;
        lastCircleRadius = magnitudes[lastCircleIndex];

        ExportConstants();
        ExportMagnitudesOrderIndexes();
    }

    void ExportConstants()
    {
        string path = @"constants.txt";

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.CreateNew)))
            {
                foreach (Complex constant in constants)
                {
                    sw.WriteLine(constant.Real.ToString() + " " + constant.Imaginary.ToString());
                }
            }
        }
        catch (Exception e)
        {
            print("The process failed: " + e.ToString());
        }
    }

    void ExportMagnitudesOrderIndexes()
    {
        string path = @"magnitudesOrderIndexes.txt";

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.CreateNew)))
            {
                foreach (int orderIndex in magnitudesOrderIndexes)
                {
                    sw.WriteLine(orderIndex);
                }
            }
        }
        catch (Exception e)
        {
            print("The process failed: " + e.ToString());
        }
    }

    IEnumerator StartNewTrail()
    {
        trail.Clear();
        trail.enabled = false;

        yield return true;

        trail.enabled = true;
    }

    public void GrowLine() 
    {
        float.TryParse(minutesToGrowField.text, out minutesToGrow);
        minutesToGrow = (minutesToGrow > 0) ? minutesToGrow : 0f;
        if (minutesToGrow != 0)
        {
            drawPercent = 0f;
            percentOfDrawing.text = drawPercent.ToString();
            
            StartCoroutine(StartNewTrail());

            StartCoroutine(StartGrow());
        }
        //minutesToGrowField.text = mins.ToString();
        minutesToGrowField.text = "";
    }
    IEnumerator StartGrow() 
    {
        yield return new WaitForSeconds(3);

        lineGrow = true;
    }

    public void ChangeMaterial(int num = 0) 
    {
        switch (num) 
        {
            case 0:
                trail.material = firstMaterial;
                break;
            case 1:
                trail.material = secondMaterial;
                break;
        }
    }

    List<Complex> startConstants = new List<Complex>()
    {
        new Complex(-0.0006689925f,0.0008046209f),
        new Complex(-0.0007084318f,-0.0003987662f),
        new Complex(0.0002340332f,0.0004981266f),
        new Complex(0.0005243378f,-0.0001355795f),
        new Complex(-0.0009500204f,0.0004477276f),
        new Complex(-0.0005815658f,0.0009895375f),
        new Complex(-0.0008863991f,0.0008413388f),
        new Complex(0.0006018856f,0.001330937f),
        new Complex(0.000242225f,0.0009845538f),
        new Complex(-0.0004699734f,1.805525E-05f),
        new Complex(0.002230978f,0.000556242f),
        new Complex(-0.000169041f,-0.0009040134f),
        new Complex(0.0006773344f,-0.0005470464f),
        new Complex(0.001276692f,0.001499698f),
        new Complex(-0.0006672974f,0.001679048f),
        new Complex(-0.001640573f,-4.755468E-05f),
        new Complex(-0.0005876312f,-0.002056493f),
        new Complex(0.002031348f,-0.001490297f),
        new Complex(0.000215374f,0.0008813927f),
        new Complex(0.000650277f,-0.001353103f),
        new Complex(0.0005244913f,-0.0009158846f),
        new Complex(-0.002154511f,0.004803017f),
        new Complex(-0.0008714068f,0.0008268426f),
        new Complex(-0.0004564956f,-0.003021567f),
        new Complex(0.001164018f,-0.0008261499f),
        new Complex(0.001056532f,-0.002039906f),
        new Complex(0.000699253f,0.0008942598f),
        new Complex(-0.0002583105f,0.0005820337f),
        new Complex(-0.001637306f,-0.000292589f),
        new Complex(0.0008267953f,-0.0004504391f),
        new Complex(0.0009732424f,-0.0002157097f),
        new Complex(-0.001869118f,-0.001200763f),
        new Complex(0.002339464f,-0.0005345275f),
        new Complex(-0.0007512752f,0.0006600361f),
        new Complex(0.0003217116f,-0.0004580203f),
        new Complex(0.0007766198f,0.001769098f),
        new Complex(-0.003827619f,-0.006841155f),
        new Complex(0.0006952004f,-0.003304145f),
        new Complex(0.006683169f,0.002118684f),
        new Complex(-0.001704974f,0.00118655f),
        new Complex(-0.004016174f,0.0002625245f),
        new Complex(-0.0004231501f,-0.003217648f),
        new Complex(-0.004946117f,0.0006692928f),
        new Complex(-0.0002691469f,-0.00181697f),
        new Complex(-0.003036158f,-0.003342916f),
        new Complex(-0.001243165f,0.005175234f),
        new Complex(0.003383003f,0.003415676f),
        new Complex(-0.003452532f,0.0007376654f),
        new Complex(0.001654916f,0.002425683f),
        new Complex(0.0006521945f,0.002322234f),
        new Complex(0.0007175339f,0.0003669773f),
        new Complex(0.002870749f,0.00276223f),
        new Complex(-0.002836365f,0.0004055264f),
        new Complex(-0.002906563f,-0.001440389f),
        new Complex(0.006143896f,0.00132253f),
        new Complex(0.004350885f,0.001119732f),
        new Complex(-0.003191096f,-0.003833291f),
        new Complex(0.003629303f,-0.001766012f),
        new Complex(-0.0007788744f,0.001581358f),
        new Complex(-0.001105497f,0.002842738f),
        new Complex(-0.00285298f,-0.0008177396f),
        new Complex(-0.00487886f,0.0009365211f),
        new Complex(0.004024848f,-0.00134905f),
        new Complex(0.00100114f,0.0001167735f),
        new Complex(-0.005509123f,-0.0006266788f),
        new Complex(9.945755E-05f,-0.007372021f),
        new Complex(0.004366762f,0.004019766f),
        new Complex(-0.003799149f,-0.0004442082f),
        new Complex(-0.008115124f,-0.0006802423f),
        new Complex(-0.000895627f,0.01354672f),
        new Complex(-0.001001169f,0.00518901f),
        new Complex(0.000194221f,-0.004840425f),
        new Complex(-0.0004999824f,-0.01181081f),
        new Complex(0.003383418f,0.001769389f),
        new Complex(0.002542343f,-0.002474602f),
        new Complex(-0.00768056f,-0.001895028f),
        new Complex(-0.005224839f,0.01675537f),
        new Complex(0.001342696f,0.0008042971f),
        new Complex(-0.007270939f,-0.002202604f),
        new Complex(0.008907677f,-0.002680564f),
        new Complex(0.005177735f,-0.009276768f),
        new Complex(-0.01609983f,0.004350681f),
        new Complex(-0.00207145f,0.01645368f),
        new Complex(-0.002321638f,0.007458152f),
        new Complex(0.008139264f,-0.008318125f),
        new Complex(0.02597629f,0.007291721f),
        new Complex(-0.00180401f,0.003820831f),
        new Complex(-0.003824289f,0.009987293f),
        new Complex(0.006882603f,-0.003018225f),
        new Complex(0.02975634f,-0.03263687f),
        new Complex(-0.002216094f,-0.004574906f),
        new Complex(-0.01084413f,-0.0117768f),
        new Complex(0.003915163f,0.004055214f),
        new Complex(-0.02303034f,0.02014302f),
        new Complex(-0.001273699f,-0.0175567f),
        new Complex(0.02163501f,0.01224983f),
        new Complex(-0.0004282974f,0.005784724f),
        new Complex(0.01915824f,-0.02395128f),
        new Complex(-0.009896051f,0.007501415f),
        new Complex(0.02259621f,-0.01598003f),
        new Complex(-0.00955133f,0.01584088f),
        new Complex(-0.01146957f,-0.01448827f),
        new Complex(-0.008933247f,0.008038493f),
        new Complex(-0.01801439f,-0.002241114f),
        new Complex(0.01071007f,0.01913857f),
        new Complex(0.001335175f,-0.02226258f),
        new Complex(0.03634981f,-0.01038898f),
        new Complex(-0.05130892f,-0.0115962f),
        new Complex(0.04952306f,0.04676692f),
        new Complex(-0.01099189f,0.003794488f),
        new Complex(-0.06837261f,-0.01976394f),
        new Complex(-0.0562919f,-0.01153632f),
        new Complex(-0.0156093f,-0.004455799f),
        new Complex(-0.03860937f,0.05800514f),
        new Complex(-0.001534954f,-0.01520716f),
        new Complex(-0.01289256f,0.06169095f),
        new Complex(0.01460524f,0.0004223116f),
        new Complex(-0.0497998f,0.08004883f),
        new Complex(-0.02769755f,0.07448515f),
        new Complex(-0.1379706f,0.03098647f),
        new Complex(-0.0716851f,0.1064692f),
        new Complex(-0.2587775f,-0.1147805f),
        new Complex(-0.1637371f,0.08338758f),
        new Complex(-0.3487135f,-0.5803977f),
        new Complex(-0.3489365f,0.01024473f),
        new Complex(1.69044f,0.3405909f),
        new Complex(0.1700242f,-0.01908463f),
        new Complex(-0.2991186f,0.5707563f),
        new Complex(-0.002098079f,-0.06701157f),
        new Complex(-0.1777965f,0.1149874f),
        new Complex(0.01793717f,-0.05897713f),
        new Complex(-0.07887571f,-0.002930922f),
        new Complex(0.05953374f,-0.02475729f),
        new Complex(-0.01205618f,-0.02852772f),
        new Complex(0.05680786f,0.001240774f),
        new Complex(0.01205986f,-0.009308977f),
        new Complex(0.04296884f,0.01145986f),
        new Complex(0.03439854f,-0.002126637f),
        new Complex(0.01003794f,-0.007045065f),
        new Complex(-0.01629814f,-0.01534359f),
        new Complex(0.0131308f,-0.07026062f),
        new Complex(-0.009324137f,0.01763888f),
        new Complex(0.01085644f,0.03896494f),
        new Complex(0.006747395f,-0.01091173f),
        new Complex(-0.01281613f,-0.003570561f),
        new Complex(-0.01471352f,0.01616156f),
        new Complex(0.02210618f,-0.01310314f),
        new Complex(0.0171561f,0.008428172f),
        new Complex(-0.005347408f,-0.0008030969f),
        new Complex(-0.003104476f,0.01667883f),
        new Complex(0.01260878f,0.01216896f),
        new Complex(0.008085482f,0.0001669389f),
        new Complex(0.01741342f,0.00296578f),
        new Complex(-0.01068824f,-0.003910339f),
        new Complex(-0.01140704f,-0.02439034f),
        new Complex(0.007056754f,0.002784048f),
        new Complex(-0.004280065f,0.01494705f),
        new Complex(-9.293177E-05f,0.01943799f),
        new Complex(0.0021322f,-0.008311016f),
        new Complex(0.007524319f,0.003051776f),
        new Complex(-0.002011062f,0.005535339f),
        new Complex(0.005270996f,-0.005076986f),
        new Complex(0.001641703f,0.0006512305f),
        new Complex(-0.006518029f,0.006789499f),
        new Complex(0.00388278f,0.001713764f),
        new Complex(0.007074227f,-0.003959407f),
        new Complex(0.004197022f,-0.009970175f),
        new Complex(0.001971865f,-0.002822064f),
        new Complex(-0.01054833f,-0.005958726f),
        new Complex(-0.003918526f,-0.001192374f),
        new Complex(0.006199688f,0.001591779f),
        new Complex(-0.001308399f,-0.001251554f),
        new Complex(-0.01091148f,0.001182159f),
        new Complex(0.001437661f,0.001913494f),
        new Complex(-0.005657973f,-0.003505052f),
        new Complex(0.0003296208f,0.002758329f),
        new Complex(0.004108459f,-0.009118305f),
        new Complex(0.002856243f,-0.004371772f),
        new Complex(-0.004233137f,0.004262404f),
        new Complex(0.0004160806f,0.001755575f),
        new Complex(0.002663231f,0.003629766f),
        new Complex(-0.003215436f,-0.008452018f),
        new Complex(-0.008018971f,0.001659301f),
        new Complex(0.0009925346f,-0.002200589f),
        new Complex(-0.002668453f,-0.006656907f),
        new Complex(0.001501665f,0.001670495f),
        new Complex(0.002781367f,0.003557558f),
        new Complex(-0.001094275f,0.00127735f),
        new Complex(-0.0007420518f,0.001329286f),
        new Complex(-0.004825411f,-0.002318187f),
        new Complex(0.00123003f,0.001093944f),
        new Complex(0.001038999f,0.004634924f),
        new Complex(-0.002469462f,-0.003529362f),
        new Complex(0.004063217f,-0.0001343205f),
        new Complex(0.0007989199f,0.005037321f),
        new Complex(-7.336898E-05f,-0.002866287f),
        new Complex(-0.0003858022f,-0.0002232815f),
        new Complex(0.001672318f,-0.002655221f),
        new Complex(0.001795704f,0.001717842f),
        new Complex(0.001974364f,0.002088689f),
        new Complex(-0.003068721f,1.607524E-05f),
        new Complex(-0.003122133f,-0.002433171f),
        new Complex(0.000429975f,-0.002227446f),
        new Complex(-0.00289407f,0.001771865f),
        new Complex(0.002129803f,0.001143484f),
        new Complex(-0.001272061f,-0.002782546f),
        new Complex(-0.002670151f,0.00251059f),
        new Complex(-0.0003561799f,0.003042259f),
        new Complex(-0.001229343f,0.002648168f),
        new Complex(-0.002461184f,0.003457927f),
        new Complex(9.082288E-05f,0.00155407f),
        new Complex(0.004477845f,-0.0005929651f),
        new Complex(0.0005384829f,0.0001228227f),
        new Complex(-0.001922362f,0.002444493f),
        new Complex(0.0004524767f,0.003448086f),
        new Complex(0.003228871f,-0.002090625f),
        new Complex(0.001851841f,-0.001593256f),
        new Complex(-0.001122048f,0.0008918606f),
        new Complex(0.003413104f,-0.001009391f),
        new Complex(-0.001409068f,-0.0006021092f),
        new Complex(-0.002333113f,0.001941789f),
        new Complex(-0.0002041592f,-0.002078087f),
        new Complex(0.0001915686f,-0.001188733f),
        new Complex(-0.0005497094f,-0.0005938659f),
        new Complex(0.0003505194f,-0.000884279f),
        new Complex(-0.0004037255f,0.0009785488f),
        new Complex(-0.0006491611f,-0.0002787698f),
        new Complex(0.001712673f,0.002021198f),
        new Complex(-0.0007881066f,-0.0001240569f),
        new Complex(-0.003457841f,-0.0027224f),
        new Complex(0.00037138f,0.0007082842f),
        new Complex(-0.0002382812f,0.000744503f),
        new Complex(0.0004718516f,-0.0004591485f),
        new Complex(-0.0004213102f,0.0008851481f),
        new Complex(0.0005928869f,0.001512777f),
        new Complex(-0.0004823238f,-0.001275621f),
        new Complex(-0.0003172903f,8.456132E-05f),
        new Complex(0.0001394886f,0.003273024f),
        new Complex(0.0005559048f,8.328981E-05f),
        new Complex(0.001967344f,0.0004271947f),
        new Complex(0.0003963031f,-0.0006712966f),
        new Complex(-0.0008165816f,6.434455E-05f),
        new Complex(0.00142213f,-0.000611602f),
        new Complex(0.0006010975f,-0.0009006447f),
        new Complex(9.520488E-05f,0.0005884058f),
        new Complex(0.0007302703f,0.0008647722f),
        new Complex(0.0005082805f,0.0001611117f),
        new Complex(-0.0002188214f,9.462227E-05f),
        new Complex(0.001022725f,-0.0008236207f),
        new Complex(5.623507E-05f,-0.0006901436f)
    };
}
