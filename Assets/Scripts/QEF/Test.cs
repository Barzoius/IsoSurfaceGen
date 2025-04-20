using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 B = SVD.CubicSolver(4, 2, 4, 4);

        Debug.Log(B);

    }

}
