using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class DotsChooser : MonoBehaviour
{
    public GameObject Image;

    public GameObject dotObj;
    List<GameObject> dotObjs = new List<GameObject>();

    Camera cam;

    int dotIndex = -1;
    Vector3 prDotPos = new Vector3(0, 0, 0);

    List<Vector3> dots = new List<Vector3>();

    LineRenderer line;

    bool editMode = true;
    bool prEditModeState = true;

    bool cursorOnDot = false;
    bool prCursorOnDotState = false;

    Vector3 prCursorPos = new Vector3(0, 0, 0);

    float maxScale = 1024;
    float scale = 1f;

    void Start()
    {
        //LoadImage();

        cam = GetComponent<Camera>();
        line = GetComponent<LineRenderer>();
        line.positionCount = 1;
    }

    void Update()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            if ((1f / scale) < maxScale)
            {
                transform.position = 0.5f * (transform.position + cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)));
                cam.orthographicSize *= 0.5f;

                foreach (GameObject dot in dotObjs)
                {
                    dot.transform.localScale *= 0.5f;
                }
                line.widthMultiplier *= 0.5f;

                scale *= 0.5f;
            }
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            if (scale < 2)
            {
                transform.position = 2f * transform.position - cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                cam.orthographicSize *= 2f;

                foreach (GameObject dot in dotObjs)
                {
                    dot.transform.localScale *= 2f;
                }
                line.widthMultiplier *= 2f;

                scale *= 2f;
            }
        }

        RaycastHit hit;
        editMode = (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit)) && (CheckUI());

        if (Input.GetMouseButtonDown(1))
        {
            prCursorPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        }
        if (Input.GetMouseButton(1))
        {
            transform.position += prCursorPos - cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            prCursorPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            editMode = false;
        }

        if (editMode != prEditModeState)
        {
            line.positionCount = dots.Count + ((editMode && (!cursorOnDot)) ? 1 : 0);

            foreach (GameObject dot in dotObjs)
            {
                dot.SetActive(editMode);
            }

            prEditModeState = editMode;
        }
        if (!editMode)
        {
            return;
        }

        cursorOnDot = hit.collider.gameObject.CompareTag("Dot") || (dotIndex >= 0);
        if (cursorOnDot != prCursorOnDotState)
        {
            line.positionCount = dots.Count + (cursorOnDot ? 0 : 1);

            prCursorOnDotState = cursorOnDot;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (cursorOnDot)
            {
                for (int i = 0; i < dots.Count; i++)
                {
                    if (dotObjs[i].Equals(hit.collider.gameObject))
                    {
                        dotIndex = i;
                        prDotPos = dots[i];
                        dotObjs[i].GetComponent<MeshCollider>().enabled = false;
                        break;
                    }
                }
            }
            else
            {
                Vector2 pixelUV = hit.textureCoord;
                //int width = hit.collider.gameObject.GetComponent<MeshRenderer>().material.mainTexture.width;
                //int height = hit.collider.gameObject.GetComponent<MeshRenderer>().material.mainTexture.height;
                //print(Mathf.Round(pixelUV.x * width));
                //print(Mathf.Round(height - pixelUV.y * height));

                dots.Add(new Vector3(10f * (hit.collider.transform.localScale.x * (pixelUV.x - 0.5f)), 10f * (pixelUV.y - 0.5f), -1));
                line.positionCount = dots.Count + 1;
                line.SetPosition(line.positionCount - 2, dots[dots.Count - 1]);

                dotObjs.Add(Instantiate(dotObj, new Vector3(dots[dots.Count - 1].x, dots[dots.Count - 1].y, -2), Quaternion.Euler(90, 0, 180)));
                dotObjs[dots.Count - 1].transform.localScale *= scale;
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (dotIndex >= 0)
            {
                if (hit.collider.gameObject.CompareTag("Image"))
                {
                    Vector2 pixelUV = hit.textureCoord;

                    dots[dotIndex] = new Vector3(10f * (hit.collider.transform.localScale.x * (pixelUV.x - 0.5f)), 10f * (pixelUV.y - 0.5f), -1);
                    line.SetPosition(dotIndex, dots[dotIndex]);

                    dotObjs[dotIndex].transform.position = new Vector3(dots[dotIndex].x, dots[dotIndex].y, -2);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (dotIndex >= 0)
            {
                dotObjs[dotIndex].GetComponent<MeshCollider>().enabled = true;
                //float difPosX = Mathf.Abs(prDotPos.x - dots[dotIndex].x);
                //float difPosY = Mathf.Abs(prDotPos.y - dots[dotIndex].y);
                //float halfDotSide = dotObjs[dotIndex].transform.localScale.x * 5f; //10f / 2f
                //if ((difPosX < halfDotSide) && (difPosY < halfDotSide))
                if (prDotPos.Equals(dots[dotIndex]))
                {
                    Destroy(dotObjs[dotIndex]);
                    dotObjs.RemoveAt(dotIndex);
                    dots.RemoveAt(dotIndex);
                    line.positionCount = dots.Count + 1;
                    line.SetPositions(dots.ToArray());
                }
                dotIndex = -1;
            }
        }

        if (!cursorOnDot)
        {
            line.SetPosition(line.positionCount - 1, cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 9)));
        }
    }

    public void ExportData()
    {
        string path = @"coords.txt";

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.CreateNew)))
            {
                foreach (Vector3 dot in dots)
                {
                    sw.WriteLine((dot.x).ToString() + " " + (dot.y).ToString());
                }
            }
        }
        catch (Exception e)
        {
            print("The process failed: " + e.ToString());
        }
    }

    public void LoadImage()
    {
        StartCoroutine(GetTexture());

        //Image.GetComponent<MeshRenderer>().material.mainTexture = Resources.Load<Texture2D>("image");
        //float aspectRatio = 1f;
        //maxScale = 1024;
        //if (Image.GetComponent<MeshRenderer>().material.mainTexture)
        //{
        //    aspectRatio = (float)(Image.GetComponent<MeshRenderer>().material.mainTexture.width) / (float)(Image.GetComponent<MeshRenderer>().material.mainTexture.height);
        //    maxScale = Image.GetComponent<MeshRenderer>().material.mainTexture.height;
        //}
        //Image.transform.localScale = new Vector3(aspectRatio, 1, 1);

        //print(Application.dataPath);
        //print(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        //print(AppDomain.CurrentDomain.BaseDirectory);
        //print(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase));
        //print(System.Environment.CurrentDirectory);
    }
    IEnumerator GetTexture()
    {
        string[] files = Directory.GetFiles(System.Environment.CurrentDirectory, "image.*");
        if (files.Length == 0) yield break;
        
        string path = @"file://" + files[0];
        
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                print(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                Image.GetComponent<MeshRenderer>().material.mainTexture = DownloadHandlerTexture.GetContent(uwr);
                float aspectRatio = 1f;
                maxScale = 1024;
                if (Image.GetComponent<MeshRenderer>().material.mainTexture)
                {
                    aspectRatio = (float)(Image.GetComponent<MeshRenderer>().material.mainTexture.width) / (float)(Image.GetComponent<MeshRenderer>().material.mainTexture.height);
                    maxScale = Image.GetComponent<MeshRenderer>().material.mainTexture.height;
                }
                Image.transform.localScale = new Vector3(aspectRatio, 1, 1);
            }
        }
    }

    public void ZoomPlus()
    {
        if ((1f / scale) < maxScale)
        {
            cam.orthographicSize *= 0.5f;

            foreach (GameObject dot in dotObjs)
            {
                dot.transform.localScale *= 0.5f;
            }
            line.widthMultiplier *= 0.5f;

            scale *= 0.5f;
        }
    }
    public void ZoomMinus()
    {
        if (scale < 2)
        {
            cam.orthographicSize *= 2f;

            foreach (GameObject dot in dotObjs)
            {
                dot.transform.localScale *= 2f;
            }
            line.widthMultiplier *= 2f;

            scale *= 2f;
        }
    }

    public void ClearDots()
    {
        foreach (GameObject dot in dotObjs)
        {
            Destroy(dot);
        }
        dotObjs.Clear();
        dots.Clear();

        line.positionCount = 1;
    }

    bool CheckUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        List<RaycastResult> resultData = new List<RaycastResult>();
        pointerData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointerData, resultData);

        if (resultData.Count > 0)
        {
            return false;
        }

        return true;
    }
}
