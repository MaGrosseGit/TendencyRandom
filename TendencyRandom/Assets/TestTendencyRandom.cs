using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TestTendencyRandom : MonoBehaviour
{

    public GameObject obj1;
    public Transform obj2;
    public GameObject obj3;

    private Dictionary<System.Object, int> TendencyTest = new Dictionary<System.Object, int>();
    private Dictionary<System.Object, float> TendencyTestFloat = new Dictionary<System.Object, float>();

    private System.Object[] GOArray = new System.Object[3];
    private int[] percentagesArray = new int[3];

    void Start()
    {
        TendencyTest[obj1] = 30;
        TendencyTest[obj2] = 50;
        TendencyTest[obj3] = 20;

        percentagesArray[0] = 30;
        percentagesArray[1] = 50;
        percentagesArray[2] = 20;

        TendencyTestFloat[obj1] = 0.3f;
        TendencyTestFloat[obj2] = 0.5f;
        TendencyTestFloat[obj3] = 0.2f;

        GOArray[0] = obj1;
        GOArray[1] = obj2;
        GOArray[2] = obj3;
    }

    int i = 0;
    List<System.Object> Results = new List<System.Object>();
    void Update()
    {
        while (i < 100)
        {
            Results.Add(transform.TendencyRandom(percentagesArray));
            i++;
        }

        if (Input.GetKeyUp(KeyCode.AltGr))
        {
            Dictionary<System.Object, int> counts = Results.GroupBy(x => x)
                                            .ToDictionary(g => g.Key,
                                                        g => g.Count());


            foreach (KeyValuePair<System.Object, int> entry in counts)
            {
                Debug.Log("Index (" + entry.Key + ") appeared (" + entry.Value + ") times with an initial frequency of ()");
                //" + (int)TendencyTest[counts.Keys.ElementAt(entry.Key)] + "
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            i = 0;
            Results = new List<System.Object>();
        }

    }
}
