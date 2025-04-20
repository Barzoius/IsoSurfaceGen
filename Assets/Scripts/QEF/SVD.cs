using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SVD
{
    //public Mat3 ComputeSVD(Mat3 A)
    //{
    //    Mat3 AT = A.Transpose();

    //    Mat3 ATA = AT * A;




    //}

    public Vector3 ComputeEigenvalues(Mat3 ATA)
    {
        // L^3 - tr(ATA)L^2 + CL - det(ATA)
        
        float trace = ATA.m00 + ATA.m11 + ATA.m22;

        float det = ATA.Determinant();

        Mat3 CM = new Mat3(ATA.m00, ATA.m01, 0, 
                           ATA.m10, ATA.m11, 0,
                           ATA.m20, ATA.m21, 0);

        float C = CM.Determinant();

        return CubicSolver(1, -trace, C, -det);


    }

    public static Vector3 CubicSolver(float a, float b, float c, float d)
    {
        double p = (3 * a * c - b * b) / (3 * a * a);
        double q = (2 * b * b * b - 9 * a * b * c + 27 * a * a * d) / (27 * a * a * a);

        double discriminant = Math.Pow(q/2, 2) + Math.Pow(p/3, 3);

        if (discriminant > 0) // One real root
        {
            double u = Math.Cbrt(-q / 2 + Math.Sqrt(discriminant));
            double v = Math.Cbrt(-q / 2 - Math.Sqrt(discriminant));
            double t1 = u + v;


            double x1 = t1 - (b / (3 * a));

            Debug.Log($"One real root: {x1}");

            return new Vector3((float)x1, (float)x1, (float)x1);
        }
        else if (discriminant == 0) // All roots real and at least two are equal
        {

            double t1 = Math.Cbrt(-q / 2); 

     
            double x1 = t1 - (b / (3 * a));

   
            double x2 = x1; 

           
            double x3 = -(b + a * (x1 + x2)) / a;

           
            Debug.Log($"Two equal roots: {x1}, {x2}");
            Debug.Log($"Third root: {x3}");

            return new Vector3((float)x1, (float)x2, (float)x3);
        }
        else // Three real roots
        {
            
            double r = Math.Sqrt(Math.Pow(q / 2, 2) + Math.Pow(p / 3, 3));
            double theta = Math.Acos(-q / (2 * r));

            double t1 = 2 * Math.Cbrt(r) * Math.Cos(theta / 3);
            double t2 = 2 * Math.Cbrt(r) * Math.Cos((theta + 2 * Math.PI) / 3);
            double t3 = 2 * Math.Cbrt(r) * Math.Cos((theta + 4 * Math.PI) / 3);

            double x1 = t1 - (b / (3 * a));
            double x2 = t2 - (b / (3 * a));
            double x3 = t3 - (b / (3 * a));

            Debug.Log($"Three real roots: {x1}, {x2}, {x3}");

            return new Vector3((float)x1, (float)x2, (float)x3);
        }
    }
}
