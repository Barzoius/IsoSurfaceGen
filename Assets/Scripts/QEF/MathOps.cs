
using UnityEngine;

public static class MathOps 
{
    public static float fnorm(Mat3 a)
    {
        return Mathf.Sqrt((a.m00 * a.m00) + (a.m01 * a.m01) + (a.m02 * a.m02)
                    + (a.m10 * a.m10) + (a.m11 * a.m11) + (a.m12 * a.m12)
                    + (a.m20 * a.m20) + (a.m21 * a.m21) + (a.m22 * a.m22));
    }

    public static float fnorm(SMat3 a)
    {
        return Mathf.Sqrt((a.m00 * a.m00) + (a.m01 * a.m01) + (a.m02 * a.m02)
                    + (a.m01 * a.m01) + (a.m11 * a.m11) + (a.m12 * a.m12)
                    + (a.m02 * a.m02) + (a.m12 * a.m12) + (a.m22 * a.m22));
    }

    public static float off(Mat3 a)
    {
        return Mathf.Sqrt((a.m01 * a.m01) + (a.m02 * a.m02) + (a.m10 * a.m10) + (a.m12 * a.m12) + (a.m20 * a.m20) + (a.m21 * a.m21));
    }

    public static float off(SMat3 a)
    {
        return Mathf.Sqrt(2 * ((a.m01 * a.m01) + (a.m02 * a.m02) + (a.m12 * a.m12)));
    }

    public static void mmul(out Mat3 Out, Mat3 a, Mat3 b)
    {
        Out = new Mat3();
        Out.set(a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20,
                a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21,
                a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22,
                a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20,
                a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21,
                a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22,
                a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20,
                a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21,
                a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22);
    }

    public static void mmul_ata(out SMat3 Out, Mat3 a)
    {
        Out = new SMat3();

        Out.setSymmetric(a.m00 * a.m00 + a.m10 * a.m10 + a.m20 * a.m20,
                         a.m00 * a.m01 + a.m10 * a.m11 + a.m20 * a.m21,
                         a.m00 * a.m02 + a.m10 * a.m12 + a.m20 * a.m22,
                         a.m01 * a.m01 + a.m11 * a.m11 + a.m21 * a.m21,
                         a.m01 * a.m02 + a.m11 * a.m12 + a.m21 * a.m22,
                         a.m02 * a.m02 + a.m12 * a.m12 + a.m22 * a.m22);
    }

    public static void transpose(out Mat3 Out, Mat3 a)
    {
        Out = new Mat3();

        Out.set(a.m00, a.m10, a.m20, a.m01, a.m11, a.m21, a.m02, a.m12, a.m22);
    }

    public static void vmul(out Vector3 Out, Mat3 a, Vector3 v)
    {
        Out = new Vector3(
            (a.m00 * v.x) + (a.m01 * v.y) + (a.m02 * v.z),
            (a.m10 * v.x) + (a.m11 * v.y) + (a.m12 * v.z),
            (a.m20 * v.x) + (a.m21 * v.y) + (a.m22 * v.z));
    }

    public static void vmul_symmetric(out Vector3 Out, SMat3 a, Vector3 v)
    {
        Out = new Vector3(
            (a.m00 * v.x) + (a.m01 * v.y) + (a.m02 * v.z),
            (a.m01 * v.x) + (a.m11 * v.y) + (a.m12 * v.z),
            (a.m02 * v.x) + (a.m12 * v.y) + (a.m22 * v.z));
    }
}

public static class Schur2
{
    public static void rot01(SMat3 m, float c, float s)
    {
        SVD.calcSymmetricGivensCoefficients(m.m00, m.m01, m.m11, c, s);
        float cc = c * c;
        float ss = s * s;
        float mix = 2 * c * s * m.m01;
        m.setSymmetric(cc * m.m00 - mix + ss * m.m11, 0, c * m.m02 - s * m.m12,
                       ss * m.m00 + mix + cc * m.m11, s * m.m02 + c * m.m12, m.m22);
    }

    public static void rot02(SMat3 m, float c, float s)
    {
        SVD.calcSymmetricGivensCoefficients(m.m00, m.m02, m.m22, c, s);
        float cc = c * c;
        float ss = s * s;
        float mix = 2 * c * s * m.m02;
        m.setSymmetric(cc * m.m00 - mix + ss * m.m22, c * m.m01 - s * m.m12, 0,
                       m.m11, s * m.m01 + c * m.m12, ss * m.m00 + mix + cc * m.m22);
    }

    public static void rot12(SMat3 m, float c, float s)
    {
        SVD.calcSymmetricGivensCoefficients(m.m11, m.m12, m.m22, c, s);
        float cc = c * c;
        float ss = s * s;
        float mix = 2 * c * s * m.m12;
        m.setSymmetric(m.m00, c * m.m01 - s * m.m02, s * m.m01 + c * m.m02,
                       cc * m.m11 - mix + ss * m.m22, 0, ss * m.m11 + mix + cc * m.m22);
    }
}


public static class Givens
{
    public static void rot01_post(Mat3 m, float c, float s)
    {
        float m00 = m.m00, m01 = m.m01, m10 = m.m10, m11 = m.m11, m20 = m.m20, m21 = m.m21;
        m.set(c * m00 - s * m01, s * m00 + c * m01, m.m02, c * m10 - s * m11,
              s * m10 + c * m11, m.m12, c * m20 - s * m21, s * m20 + c * m21, m.m22);
    }

    public static void rot02_post(Mat3 m, float c, float s)
    {
        float m00 = m.m00, m02 = m.m02, m10 = m.m10, m12 = m.m12, m20 = m.m20, m22 = m.m22;
        m.set(c * m00 - s * m02, m.m01, s * m00 + c * m02, c * m10 - s * m12, m.m11,
              s * m10 + c * m12, c * m20 - s * m22, m.m21, s * m20 + c * m22);
    }

    public static void rot12_post(Mat3 m, float c, float s)
    {
        float m01 = m.m01, m02 = m.m02, m11 = m.m11, m12 = m.m12, m21 = m.m21, m22 = m.m22;
        m.set(m.m00, c * m01 - s * m02, s * m01 + c * m02, m.m10, c * m11 - s * m12,
              s * m11 + c * m12, m.m20, c * m21 - s * m22, s * m21 + c * m22);
    }
}