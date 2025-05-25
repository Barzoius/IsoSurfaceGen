using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SVD
{
    public Mat3 U;
    public Mat3 SIGMA;
    public Mat3 UT;


    public Mat3 ComputeSVD(Mat3 A)
    {
        JacobiSVD(ref A, 15);
        return U * SIGMA * UT;
    }

    public void ResolveSVD(Mat3 A)
    {
        JacobiSVD(ref A, 15);
    }

    public Mat3 ComputeGivensRotation(Mat3 A, int i, int j)
    {
        if (Mathf.Abs(A[i, j]) < 1e-6f)
            return Mat3.Identity();

        float theta = 0.5f * Mathf.Atan2(2f * A[i, j], A[j, j] - A[i, i]);
        float c = Mathf.Cos(theta);
        float s = Mathf.Sin(theta);

        Mat3 J = Mat3.Identity();
        J[i, i] = c;
        J[j, j] = c;
        J[i, j] = s;
        J[j, i] = -s;

        Debug.Log(J);
        return J;
    }

    public void JacobiSVD(ref Mat3 A, int iterations)
    {
        U = Mat3.Identity();

        for (int it = 0; it < iterations; it++)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = i + 1; j < 3; j++)
                {
                    Mat3 J = ComputeGivensRotation(A, i, j);
                    Mat3 JT = J.Transpose();

                    A = JT * A * J;

                    U = U * J;

                }

            }

            if (Mathf.Abs(A[0, 1]) < 1e-5f &&
                Mathf.Abs(A[0, 2]) < 1e-5f &&
                Mathf.Abs(A[1, 2]) < 1e-5f)
                break;
        }

        SIGMA = A;
        UT = U.Transpose();
    }






}
