﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class JsonManager : MonoBehaviour
{
    public StateManager stateManager;
    public ImageTargetBehaviour TableBase;
    public ImageTargetBehaviour FloorBase;
    public InputField saveField;
    [Space(10)]
    public GameObject bed;
    public GameObject bookshelf, desk, deskChair, gasrange, lightStand, oven, refrigerator, sink, sofa, table, tableChair, television, wardrobe;

    private void Start()
    {
        // AR화면 현재상태 매니저
        stateManager = TrackerManager.Instance.GetStateManager();
    }

    // 저장파일 경로
    public string pathForDocumentsFile(string filename)
    {
        // 아이폰(non-Test)
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5) + "/Resources";
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }
        // 안드로이드
        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath + "/Resources";
            return Path.Combine(path, filename);
        }
        // PC
        else
        {
            string path = Application.dataPath + "/Resources";
            return Path.Combine(path, filename);
        }
    }

    // json을 통한 오브젝트 저장
    public void SaveObjectInJson(string filename = "data")
    {
        if (TableBase == null)
            Debug.LogError("Table Base Target Missing");

        List<SerializedData> list = new List<SerializedData>();

        // 기준 벽
        GameObject go = GameObject.Find("Interior Group");
        if (go != null)
        {
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                SerializedObject so = go.transform.GetChild(i).GetComponent<SerializedObject>();
                if (so == null)
                    continue;
                so.SetCurrentState();
                list.Add(so.sd);
            }
        }

        // 인식된 가구들
        var trackable = stateManager.GetActiveTrackableBehaviours();
        // Warning : 유니티 애디터에서는 아래 포문을 돌지 않지만
        // 안드로이드로 빌드하면 한번을 도는 문제가 있다.
        foreach (var t in trackable)
        {
            if (t == TableBase || t == FloorBase)
                continue;
            SerializedObject so = t.gameObject.GetComponentInChildren<SerializedObject>();
            if (so == null)
            {
                Debug.LogError("Object can't save");
                continue;
            }
            // list에 넣기 전 현상태 불러오기
            GameObject ObjectToSave = t.gameObject.GetComponentInChildren<SerializedObject>().gameObject;

            // TableBase을 중심으로 이동및회전
            Transform tr = ObjectToSave.transform;
            tr.position -= TableBase.transform.position;
            tr.RotateAround(Vector3.zero, Vector3.up, -TableBase.transform.rotation.eulerAngles.y);
            tr.position = new Vector3(tr.position.x, 0, tr.position.z);
            tr.rotation = Quaternion.Euler(new Vector3(0, tr.rotation.eulerAngles.y, 0));

            tr.GetComponent<SerializedObject>().SetCurrentState();
            tr.RotateAround(Vector3.zero, Vector3.up, TableBase.transform.rotation.eulerAngles.y);
            tr.position += TableBase.transform.position;
            list.Add(ObjectToSave.GetComponent<SerializedObject>().sd);
        }
        // 배열의 내용을 json형식 string으로 저장
        string json = JsonHelper.ToJson<SerializedData>(list.ToArray());

        // 저장 디렉토리 존재여부
        if (!Directory.Exists(Application.persistentDataPath + "/Resources"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Resources");
        }
        filename = saveField.text;
        string path = pathForDocumentsFile(filename);

        FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(file);
        sw.WriteLine(json);
        sw.Close();
        file.Close();
    }

    public GameObject LoadObjectFromJson(string filename)
    {
        string path = pathForDocumentsFile(filename);

        if (!File.Exists(path))
        {
            Debug.LogError("Path doesn't Exist : " + path);
            return null;
        }
        // 파일로 부터 string으로 된 json을 가져온다.
        FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
        StreamReader sr = new StreamReader(file);

        string json = sr.ReadLine();

        sr.Close();
        file.Close();

        // 한데 모아둘 빈 객체 생성
        GameObject interiorGroup = GameObject.Find("Interior Group");
        if (interiorGroup == null)
            interiorGroup = new GameObject("Interior Group");
        interiorGroup.transform.position = Vector3.zero;
        interiorGroup.transform.rotation = Quaternion.identity;
        interiorGroup.transform.localScale = Vector3.one;

        // 데이터 배열로 deserialize 하여 그 정보로 오브젝트 생성
        SerializedData[] list = JsonHelper.FromJson<SerializedData>(json);
        foreach (var ObjectToLoad in list)
        {
            string name = ObjectToLoad.mFurniture.Trim();
            
            GameObject madeObject;
            switch (name)
            {
                case "Wall":
                    madeObject = new GameObject("Wall");
                    madeObject.AddComponent<MeshFilter>();
                    MeshRenderer mr = madeObject.AddComponent<MeshRenderer>();
                    madeObject.AddComponent<SerializedObject>();
                    mr.material = new Material(Shader.Find("Standard"));
                    break;
                case "bed":
                    madeObject = GameObject.Instantiate(bed);
                    break;
                case "bookshelf":
                    madeObject = GameObject.Instantiate(bookshelf);
                    break;
                case "desk":
                    madeObject = GameObject.Instantiate(desk);
                    break;
                case "deskChair":
                    madeObject = GameObject.Instantiate(deskChair);
                    break;
                case "gasrange":
                    madeObject = GameObject.Instantiate(gasrange);
                    break;
                case "lightStand":
                    madeObject = GameObject.Instantiate(lightStand);
                    break;
                case "oven":
                    madeObject = GameObject.Instantiate(oven);
                    break;
                case "refrigerator":
                    madeObject = GameObject.Instantiate(refrigerator);
                    break;
                case "sink":
                    madeObject = GameObject.Instantiate(sink);
                    break;
                case "sofa":
                    madeObject = GameObject.Instantiate(sofa);
                    break;
                case "table":
                    madeObject = GameObject.Instantiate(table);
                    break;
                case "tableChair":
                    madeObject = GameObject.Instantiate(tableChair);
                    break;
                case "television":
                    madeObject = GameObject.Instantiate(television);
                    break;
                case "wardrobe":
                    madeObject = GameObject.Instantiate(wardrobe);
                    break;
                default:
                    madeObject = new GameObject("error");
                    Debug.LogError("error 4738672538791389");
                    break;
            }

            madeObject.GetComponent<SerializedObject>().sd = ObjectToLoad;
            madeObject.GetComponent<SerializedObject>().SetSdToObject();
            madeObject.transform.parent = interiorGroup.transform;
        }

        return interiorGroup;
    }
}

// JsonUtility 배열 지원용 Helper 클래스
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }
    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }
    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}