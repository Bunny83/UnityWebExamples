using UnityEngine;
using System.Collections;
using B83.ExpressionParser;
using System.Collections.Generic;
using System.Linq;

public class ExtendedParser : ExpressionParser
{
    public ExtendedParser()
    {
        AddFunc("repeat", Repeat);
        AddFunc("pingpong", PingPong);
        AddFunc("clamp", (p) => p[0] < 0 ? 0 : p[0] > 1 ? 1 : p[0]);
        AddFunc("lerp", (p) => p[0] +(p.ElementAtOrDefault(1)-p[0])*p.ElementAtOrDefault(2));
        AddConst("time", () => Time.time);
        AddConst("sinTest", ()=>555);
        AddFunc("effect", (p) => p[0]);
        // pow doesn't work with negative base values and non-integer-power
        // this simply calulates the positive pow and keeps the sign
        AddFunc("pow", (p) => p[0] < 0 ? -System.Math.Pow(-p[0], p.ElementAtOrDefault(1)) : System.Math.Pow(p[0], p.ElementAtOrDefault(1)));
    }
    double Repeat(params double[] aParams)
    {
        if (aParams.Length == 1)
            return aParams[0];
        double v = aParams[0];
        double l = aParams[1];
        return v - System.Math.Floor(v / l) * l;
    }
    double PingPong(params double[] aParams)
    {
        if (aParams.Length == 1)
            return aParams[0];
        double v = aParams[0];
        double l = aParams[1];
        v = Repeat(v, l * 2d);
        return l - System.Math.Abs(v - l);
    }
}

public class ExpressionParserTest : MonoBehaviour
{
    string expression = "sin(x*PI)*(0.6+sin(x*pingpong(time,100)*PI*8)*0.4), cos(x*PI)*(0.6+sin(x*pingpong(time,100)*PI*4)*0.4)";
    string errorText = "";
    Rect winPos = new Rect(10,40,700,300);
    ExpressionParser m_Parser = new ExtendedParser();
    Expression exp = null;
    Material m_Mat;
    public GUIStyle m_TextFieldStyle;
    private void Awake()
    {
        m_TextFieldStyle = null;
        m_Mat = Resources.Load<Material>("Plane_No_zTest");
#if UNITY_EDITOR
        if (m_Mat == null)
        {
            var resDir = new System.IO.DirectoryInfo(System.IO.Path.Combine(Application.dataPath, "Resources"));
            if (!resDir.Exists)
                resDir.Create();
            Shader s = Shader.Find("Plane/No zTest");
            if (s == null)
            {
                string shaderText = "Shader \"Plane/No zTest\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Fog { Mode Off } BindChannels { Bind \"Color\",color } } } }";
                string path = System.IO.Path.Combine(resDir.FullName, "Plane_No_zTest.shader");
                Debug.Log("Shader missing, create asset: " + path);
                System.IO.File.WriteAllText(path, shaderText);
                UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
                UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>("Resources/Plane_No_zTest.shader");
                s = Shader.Find("Plane/No zTest");
            }
            var mat = new Material(s);
            mat.name = "Plane_No_zTest";
            UnityEditor.AssetDatabase.CreateAsset(mat, "Assets/Resources/Plane_No_zTest.mat");
            m_Mat = mat;
        }
#endif
        Parse();
    }
    Vector2 Execute(float aValue)
    {
        if (exp != null)
        {
            foreach (var P in exp.Parameters)
                P.Value.Value = aValue;
            var values = exp.MultiValue;
            if (values.Length == 1)
                return new Vector2(aValue, (float)values[0]);
            return new Vector2((float)values[0], (float)values[1]);
        }
        return new Vector2(aValue,0);
    }
    void Parse()
    {
        try
        {
            exp = m_Parser.EvaluateExpression(expression);
            if (exp.ParsingError != null)
            {
                errorText = "Parsing Error: " + exp.ParsingError;
                exp = null;
            }
            else
                errorText = "Parsing successful:\n" + exp.ToString();
        }
        catch (System.Exception e)
        {
            errorText = "E Parsing Error: " + e.Message;
        }
    }
    void OnGUI()
    {
        winPos = GUI.Window(0, winPos,DrawWindow, "");
        winPos.width = Mathf.Clamp(winPos.width, 100, Screen.width - 20);
        winPos.height = Mathf.Clamp(winPos.height, 100, Screen.height - 20);
        winPos.x = Mathf.Clamp(winPos.x, 20 - winPos.width, Screen.width-20);
        winPos.y = Mathf.Clamp(winPos.y, 20 - winPos.height, Screen.height - 20);

        if (Event.current.type == EventType.Repaint)
        {
            DrawCurve();
        }
	}
    void Sample(string aButtonName, string aExpression)
    {
        if (GUILayout.Button(aButtonName))
        {
            expression = aExpression;
            Parse();
        }
    }

    private void DrawWindow(int id)
    {
        GUI.changed = false;
        GUILayout.BeginHorizontal();
        Sample("parabola", "x,5*x^2 -0.8");
        Sample("sinus", "x,sin(x*PI*5)");
        Sample("circle", "sin(x*PI), cos(x*PI)");
        Sample("sin^3, cos^3", "sin(x*PI)^3*(0.9+rnd(0.2)), cos(x*PI)^3*(0.9+rnd(0.2))");
        Sample("start", "sin(x*PI)*(0.6+sin(x*pingpong(time,100)*PI*8)*0.4), cos(x*PI)*(0.6+sin(x*pingpong(time,100)*PI*4)*0.4)");
        Sample("wobble", "sin(x*PI)*(0.5-sin(x*PI*10+time)*0.05-sin(x*PI*7-time)*0.05), cos(x*PI)*(0.5-sin(x*PI*10+time)*0.05-sin(x*PI*7-time)*0.05)");
        Sample("parabola2", "x*sin(time*PI/2) - (5*x^2 -0.8)*cos(time*PI/2), (5*x^2 -0.8)*sin(time*PI/2) + x*cos(time*PI/2)");
        GUILayout.EndHorizontal();
        if (m_TextFieldStyle == null)
        {
            m_TextFieldStyle = new GUIStyle("TextArea");
            m_TextFieldStyle.font = Font.CreateDynamicFontFromOSFont("Courier New", 18);
            m_TextFieldStyle.fontStyle = FontStyle.Bold;
        }
        expression = GUILayout.TextArea(expression, m_TextFieldStyle, GUILayout.ExpandHeight(true));
        //m_ShowTree = GUILayout.Toggle(m_ShowTree, "Show Tree", "Button");
        GUILayout.Label(errorText, "label");

        if (GUI.changed)
            Parse();

        GUI.DragWindow();
    }


    private void DrawCurve()
    {
        GL.PushMatrix();
        GL.Viewport(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
        GL.LoadPixelMatrix(-1, 1, -1, 1);

        m_Mat.SetPass(0);
        GL.Begin(GL.LINES);
        float start = -1.0f;
        float end   =  1.0f;
        var lastV = Execute(start);
        for(float x = start; x < end; x+= 0.001f)
        {
            var val = Execute(x);
            GL.Color(Color.red);
            GL.Vertex3(lastV.x , lastV.y, -1);
            GL.Vertex3(val.x , val.y, -1);
            lastV = val;
        }
        GL.Color(Color.green);
        GL.Vertex3(0, -1, -1);
        GL.Vertex3(0, 1, -1);
        GL.Vertex3(-1,0, -1);
        GL.Vertex3(1,0, -1);
        GL.End();
        GL.PopMatrix();
    }
}