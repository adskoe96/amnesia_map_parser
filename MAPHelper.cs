using UnityEngine;
using UnityEditor;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;

internal class MAPHelper : MonoBehaviour
{
    [SerializeField] private string mapFilePath;
    [SerializeField] private string modelsFilePath;
    private Dictionary<int, string> fileIndexToPathMap = new Dictionary<int, string>();

    [CustomEditor(typeof(MAPHelper))]
    private class ImportObjectsFromMapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MAPHelper importScript = (MAPHelper)target;

            if (GUILayout.Button("Import Objects"))
            {
                importScript.ImportObjects();
            }

            DrawDefaultInspector();
        }
    }

    private void ImportObjects()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(mapFilePath);

        XmlNodeList fileList = xmlDoc.SelectNodes("//FileIndex_StaticObjects/File");
        foreach (XmlNode fileNode in fileList)
        {
            int id = int.Parse(fileNode.Attributes["Id"].Value);
            string path = fileNode.Attributes["Path"].Value;
            fileIndexToPathMap[id] = modelsFilePath + path;
        }

        XmlNodeList objectList = xmlDoc.SelectNodes("//StaticObjects/StaticObject");
        foreach (XmlNode objectNode in objectList)
        {
            int fileIndex = int.Parse(objectNode.Attributes["FileIndex"].Value);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fileIndexToPathMap[fileIndex]);
            Vector3 position = ParseVector3(objectNode.Attributes["WorldPos"].Value);
            Vector3 rotation = ParseVector3(objectNode.Attributes["Rotation"].Value);
            Vector3 scale = ParseVector3(objectNode.Attributes["Scale"].Value);

            if (prefab != null)
            {
                print(prefab.name);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = position;
                instance.transform.rotation = Quaternion.Euler(rotation);
                instance.transform.localScale = scale;
            }
        }
    }

    private Vector3 ParseVector3(string vectorString)
    {
        string[] parts = vectorString.Split(' ');
        return new Vector3(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture)
        );
    }
}