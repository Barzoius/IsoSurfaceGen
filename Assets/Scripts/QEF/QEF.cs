using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QEF
{
    Mat3 ATA;
    Vector3 ATb;
    float bTb;
    Vector3 MassPoint;
    int points;

    public QEF()
    {
        ATA = new Mat3();
        ATb = Vector3.zero;
        MassPoint = Vector3.zero;
    }

    public void AddToQEFData(Vector3 p, Vector3 n)
    {
        ATA.m00 += n.x * n.x;
        ATA.m01 += n.x * n.y;
        ATA.m02 += n.x * n.z;

        ATA.m10 += n.y * n.x;
        ATA.m11 += n.y * n.y;
        ATA.m12 += n.y * n.z;

        ATA.m20 += n.z * n.x;
        ATA.m21 += n.z * n.y;
        ATA.m22 += n.z * n.z;

        // A^T * b (vector)
        float d = Vector3.Dot(n, p);
        ATb += n * d;

        bTb += d;

        MassPoint += p;

        points++;

    }


    public Vector3 SolveQEF(int svd_iterations = 15)
    {
        if (points == 0) return Vector3.zero;

        Vector3 center = MassPoint / points;

        Vector3 shiftedATb = ATb - ATA * center;

        SVD svdResolver = new SVD();
        svdResolver.ResolveSVD(ATA);

        Mat3 U = svdResolver.U;
        Mat3 Sigma = svdResolver.SIGMA;
        float epsilon = 1e-5f;
        Sigma[0, 0] = Sigma[0, 0] > epsilon ? 1 / Sigma[0, 0] : 0;
        Sigma[1, 1] = Sigma[1, 1] > epsilon ? 1 / Sigma[1, 1] : 0;
        Sigma[2, 2] = Sigma[2, 2] > epsilon ? 1 / Sigma[2, 2] : 0;

        Mat3 UT = svdResolver.UT;
        Mat3 invATA = UT * Sigma * U;
        Vector3 vertexOffset = invATA * shiftedATb;

        return vertexOffset + center; 
    }


}
