using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mandelbrot
{
	public static int maxIter = 15000;
	public static double log2 = System.Math.Log(2);
	public static double log4 = System.Math.Log(4);
	public static double startR = 0;
	public static double startI = 0;

	// julia set
	/*public static int Calc(double x, double y)
	{
		double x1 = x;
		double y1 = y;
		double x2 = 0;
		double y2 = 0;
		double mag = 0;
		for (int i = 0; i < maxIter; i++)
		{
			x2 = x1*x1 -y1*y1 + startR;
			y2 = 2*x1*y1 + startI;
			x1 = x2;
			y1 = y2;
			mag = x1*x1 +y1*y1;
			if (mag > 4d)
			{
				return i;// – System.Math.Log(System.Math.Log(mag)/log4)/log2;
			}

		}
		return maxIter;
		
	}*/
	// mandelbrot set
	public static int Calc(double x, double y)
	{
		double x1 = startR;
		double y1 = startI;
		double x2 = 0;
		double y2 = 0;
		double mag = 0;
		for (int i = 0; i < maxIter; i++)
		{
			x2 = x1*x1 -y1*y1 + x;
			y2 = 2*x1*y1 + y;
			x1 = x2;
			y1 = y2;
			mag = x1*x1 +y1*y1;
			if (mag > 4d)
			{
				return i;// – System.Math.Log(System.Math.Log(mag)/log4)/log2;
			}
			
		}
		return maxIter;
		
	}
	public static IEnumerable<Vector2> CalcPoints(double x, double y)
	{
		double x1 = startR;
		double y1 = startI;
		double x2 = 0;
		double y2 = 0;
		double mag = 0;
		yield return new Vector2((float)x,(float)y);
		for (int i = 0; i < maxIter; i++)
		{
			x2 = x1*x1 -y1*y1 + x;
			y2 = 2*x1*y1 + y;
			x1 = x2;
			y1 = y2;
			mag = x1*x1 +y1*y1;
			yield return new Vector2((float)x1,(float)y1);
			if (mag > 4d)
			{
				//yield break;
			}
			
		}
	}
}


public class MandelbrotVisualizer : MonoBehaviour
{
	Texture2D tex;
	Material m;
	public Vector3 test;
	int xSize = 800;
	int	ySize = 800;
	public Color[] colors = new Color[]
	{
		Color.clear,
		Color.grey,
		Color.green,
		Color.red,
		Color.blue,
		Color.yellow,
		Color.cyan,
		Color.white,
		Color.black
	};
	void Start ()
	{
		tex = new Texture2D(xSize,ySize);
		m = new Material("Shader \"LineShader\" { SubShader { Pass { Lighting Off Blend SrcAlpha OneMinusSrcAlpha BindChannels { Bind \"Color\", color} } } }");


	}
	Vector2 pos = Vector2.zero;
	int iterations = 0;
	void OnGUI ()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("render"))
		{
			Color[] cols = new Color[xSize*ySize];
			Mandelbrot.maxIter = 150;
			for(int y = 0; y < ySize; y++)
			{
				int off = y * xSize;
				for(int x = 0; x < xSize; x++)
				{
					float v = (float)Mandelbrot.Calc(-3d+(double)x*4/(xSize),-1.5d+(double)y*3/(ySize))/10;
					var i1 = Mathf.Clamp(Mathf.FloorToInt(v),0,colors.Length-1);
					var i2 = Mathf.Clamp(Mathf.CeilToInt(v),0,colors.Length-1);
					cols[off + x] = Color.Lerp(colors[i1], colors[i2],v - i1);
				}
			}
			tex.SetPixels(cols);
			tex.Apply();
			Mandelbrot.maxIter = 30000;
		}
		GUILayout.Label("point: (" + pos.x.ToString("0.00000") + " + " + pos.y.ToString("0.00000")+"*i)  interations: " + iterations);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		/*
		Mandelbrot.startR = GUILayout.HorizontalSlider((float)Mandelbrot.startR,-1,1);
		Mandelbrot.startI = GUILayout.HorizontalSlider((float)Mandelbrot.startI,-1,1);
		*/
		GUILayout.EndHorizontal();

		Rect r = new Rect(10,50,xSize,ySize);
		GUI.DrawTexture(r,tex);
		GUI.BeginGroup(r);
		Event e = Event.current;
		if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)
		{
			pos = e.mousePosition;
			pos.x = pos.x*4 / xSize - 3;
			pos.y = pos.y*3 / ySize - 1.5f;
			iterations = Mandelbrot.Calc(pos.x, pos.y);
		}
		GUI.EndGroup();
		if (Event.current.type == EventType.Repaint)
		{
			r.y = Screen.height - r.yMax;
			GL.Viewport(r);
			GL.LoadPixelMatrix(-3,1,1.5f,-1.5f);
			Vector2 old = pos;
			GL.Begin(GL.LINES);
			m.SetPass(0);
			GL.Color(Color.blue);
			GL.Vertex3(-3,0,0);
			GL.Vertex3(1,0,0);
			GL.Vertex3(0,-1,0);
			GL.Vertex3(0,1,0);

			GL.Color(Color.red);
			foreach(var p in Mandelbrot.CalcPoints(pos.x, pos.y))
			{
				GL.Vertex(old);
				GL.Vertex(p);
				old = p;
			}
			GL.End();

		}
	}
}
