using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    // Start is called before the first frame update
    private static MatchManager instance;
    public static MatchManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    int level;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
