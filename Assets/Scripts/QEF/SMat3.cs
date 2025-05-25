using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMat3 
{
    public float m00, m01, m02;
    public float      m11, m12;
    public float           m22;

    public SMat3(float m00, float m01, float m02,
                            float m11, float m12,
                                       float m22)
    {
        this.m00 = m00;
        this.m01 = m01;
        this.m02 = m02;

        this.m11 = m11;
        this.m12 = m12;


        this.m22 = m22;
    }

    public SMat3()
    {
        this.m00 = 0;
        this.m01 = 0;
        this.m02 = 0;

        this.m11 = 0;
        this.m12 = 0;


        this.m22 = 0;
    }

    public SMat3(SMat3 matrix)
    {
        this.m00 = matrix.m00;
        this.m01 = matrix.m01;
        this.m02 = matrix.m02;

        this.m11 = matrix.m11;
        this.m12 = matrix.m12;


        this.m22 = matrix.m22;
    }
}
