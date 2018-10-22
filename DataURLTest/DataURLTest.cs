using UnityEngine;
using System.Collections;

public class DataURLTest : MonoBehaviour
{
    string text = "Some example text\nFeel free to edit and press 'Download text file'\n";

	void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Download random data"))
        {
            byte[] randomData = new byte[Random.Range(200, 500)];
            for (int i = 0; i < randomData.Length; i++)
                randomData[i] = (byte)Random.Range(0, 256);
            WebDownloadHelper.InitiateDownload("ExampleFile.dat",randomData);
        }
        if (GUILayout.Button("Download text file"))
        {
            WebDownloadHelper.InitiateDownload("Textfile.txt", text);
        }
        GUILayout.EndHorizontal();

        text = GUILayout.TextArea(text, GUILayout.ExpandHeight(true));
        GUILayout.EndArea();
    }
}

public class WebDownloadHelper
{
    // Source: http://stackoverflow.com/a/27284736/1607924
    static string scriptTemplate = @"
            var link = document.createElement(""a"");
            link.download = '{0}';
            link.href = 'data:application/octet-stream;charset=utf-8;base64,{1}';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            delete link;
        ";

    public static void InitiateDownload(string aName, byte[] aData)
    {
        string base64 = System.Convert.ToBase64String(aData);
        string script = string.Format(scriptTemplate, aName, base64);
        Application.ExternalEval(script);
    }
    public static void InitiateDownload(string aName, string aData)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(aData);
        InitiateDownload(aName, data);
    }
}