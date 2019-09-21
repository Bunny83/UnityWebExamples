using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Menu,
    Running, 
    EndGame
}

public class Hanoi : MonoBehaviour
{
    Queue<KeyCode> keyQueue = new Queue<KeyCode>();
    IEnumerator ReadNextKey(System.Action<KeyCode> aOnKey)
    {
        while (keyQueue.Count == 0)
            yield return null;
        aOnKey(keyQueue.Dequeue());
    }
    float val = 5;
    GameState state = GameState.Menu;
    private void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            keyQueue.Enqueue(e.keyCode);
        if (state == GameState.Menu)
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 70),"", "box");
            val = GUILayout.HorizontalSlider(val, 1, 20);
            int d = Mathf.RoundToInt(val);
            GUILayout.Label("Discs: " + d + " required moves: " + ((1 << d) - 1));
            if(GUILayout.Button("Start"))
                StartCoroutine(GameLoop(d));
            GUILayout.EndArea();
        }
        else if(state == GameState.Running)
        {
            GUILayout.BeginArea(new Rect(10, 5, 400, 35), "", "box");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Abort", GUILayout.Width(60)))
            {
                StopAllCoroutines();
                state = GameState.Menu;
            }
            GUILayout.Label("Required: " + ((1 << discCount) - 1) + " moves: " + moveCount);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        else
        {
            GUILayout.BeginArea(new Rect(0,0,Screen.width, Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle box = new GUIStyle("box");
            box.fontSize = 20;
            GUI.contentColor = Color.black;
            GUILayout.BeginVertical();
            GUILayout.Label("Game Over! took" + moveCount + " moves. " + (moveCount - (1 << discCount) + 1)+" above optimal solution", box);
            GUILayout.Label("Press <space> to continue",box);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }
    }
    public class Disc : System.IDisposable
    {
        public static float BaseSize = 0.5f;
        public static float Growth = 0.3f;
        public static float Height = 0.2f;
        public static float VerticalSpacing = 0.02f;
        public static Color[] colors = new Color[] {new Color(1,0,0), new Color(0, 1, 0), new Color(0, 0, 1), new Color(1, 1, 0), new Color(1, 0, 1), new Color(0, 1, 1)};

        public int Size;
        public Transform obj;
        public Disc(int aSize)
        {
            Size = aSize;
            obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            obj.name = "Disc_" + Size;
            float s = BaseSize + Size * Growth;
            obj.localScale = new Vector3(s, Height, s);
            obj.GetComponent<Renderer>().material.color = colors[Size % colors.Length];
        }

        public void Dispose()
        {
            Destroy(obj.gameObject);
        }
    }
    public Transform[] poles;
    public AudioClip pickUpSound;
    public AudioClip putDownSound;

    private List<Disc>[] stacks = new List<Disc>[3];
    private int moveCount = 0;
    private int discCount = 0;

    private void Start()
    {
        for (int i = 0; i < stacks.Length; i++)
            stacks[i] = new List<Disc>();
    }

    public void InitGame(int aDiscCount)
    {
        for (int i = 0; i < stacks.Length; i++)
        {
            foreach (var d in stacks[i])
                d.Dispose();
            stacks[i].Clear();
        }
        for (int i = 0; i < aDiscCount; i++)
            stacks[0].Add(new Disc(aDiscCount - i));
        discCount = aDiscCount;
        moveCount = 0;
    }

    void PositionDiscs()
    {
        float h = Disc.Height*2f + Disc.VerticalSpacing;
        for(int i = 0; i < stacks.Length; i++)
        {
            var p = poles[i];
            int count = stacks[i].Count;
            for (int n = 0; n < count; n++)
            {
                stacks[i][n].obj.parent = p;
                stacks[i][n].obj.localPosition = n * h * Vector3.up;
            }
        }
    }
    
    Disc Pop(List<Disc> aStack)
    {
        if (aStack.Count <= 0)
            return null;
        var tmp = aStack[aStack.Count - 1];
        aStack.RemoveAt(aStack.Count - 1);
        tmp.obj.localPosition = Vector3.up * 9;
        AudioSource.PlayClipAtPoint(pickUpSound, Vector3.zero);
        return tmp;
    }

    void Push(List<Disc> aStack, ref Disc aCurrent)
    {
        if (aStack.Count > 0 && aStack[aStack.Count - 1].Size <= aCurrent.Size)
            return;
        aStack.Add(aCurrent);
        aCurrent = null;
        AudioSource.PlayClipAtPoint(putDownSound, Vector3.zero);
        moveCount++;
        PositionDiscs();
    }

    IEnumerator GameLoop(int aDiscCount)
    {
        state = GameState.Running;
        InitGame(aDiscCount);
        PositionDiscs();
        KeyCode key = KeyCode.None;
        while (true)
        {   
            Disc current = null;
            while (current == null)
            {
                yield return ReadNextKey(k => key = k);
                switch (key)
                {
                    case KeyCode.Alpha1:
                    case KeyCode.Keypad1:
                        current = Pop(stacks[0]); break;
                    case KeyCode.Alpha2:
                    case KeyCode.Keypad2:
                        current = Pop(stacks[1]); break;
                    case KeyCode.Alpha3:
                    case KeyCode.Keypad3:
                        current = Pop(stacks[2]); break;
                }
            }
            while (current != null)
            {
                yield return ReadNextKey(k => key = k);
                switch (key)
                {
                    case KeyCode.Alpha1:
                    case KeyCode.Keypad1:
                        Push(stacks[0], ref current); break;
                    case KeyCode.Alpha2:
                    case KeyCode.Keypad2:
                        Push(stacks[1], ref current); break;
                    case KeyCode.Alpha3:
                    case KeyCode.Keypad3:
                        Push(stacks[2], ref current); break;
                }
            }
            if (stacks[0].Count == 0)
            {
                if (stacks[1].Count == 0 || stacks[2].Count == 0)
                {
                    Debug.Log("Game over! Took " + moveCount +" moves. Required moves: " + ((1<<discCount)-1));
                    break;
                }
            }
        }
        state = GameState.EndGame;
        while (key != KeyCode.Space)
            yield return ReadNextKey(k => key = k);
        state = GameState.Menu;
    }
}
