using UnityEngine;

public class Mat3
{
    public float m00, m01, m02, m10, m11, m12, m20, m21, m22;
    public Mat3()
    { }
    public Mat3(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22) { }
    public void clear()
    {
        set(0, 0, 0,
            0, 0, 0,
            0, 0, 0);
    }
    public void set(float m00, float m01, float m02,
                    float m10, float m11, float m12,
                    float m20, float m21, float m22)
    {
        this.m00 = m00; this.m01 = m01; this.m02 = m02;
        this.m10 = m10; this.m11 = m11; this.m12 = m12;
        this.m20 = m20; this.m21 = m21; this.m22 = m22;
    }

    public void set(Mat3 rhs)
    {
        this.m00 = rhs.m00; this.m01 = rhs.m01; this.m02 = rhs.m02;
        this.m10 = rhs.m10; this.m11 = rhs.m11; this.m12 = rhs.m12;
        this.m20 = rhs.m20; this.m21 = rhs.m21; this.m22 = rhs.m22;
    }
    public void setSymmetric(float a00, float a01, float a02,
                             float a11, float a12, float a22)
    {
        this.m00 = a00; this.m01 = a01; this.m02 = a02;
        this.m10 = a01; this.m11 = a11; this.m12 = a12;
        this.m20 = a02; this.m21 = a12; this.m22 = a22;
    }
    public void setSymmetric(SMat3 rhs) { }

    private Mat3(Mat3 rhs) { }

    public void DebugLog(string label = "MMat3")
    {
        float m10 = m01;
        float m20 = m02;
        float m21 = m12;

        string matrixString =
            $"{label}:\n" +
            $"[{m00:0.###}, {m01:0.###}, {m02:0.###}]\n" +
            $"[{m10:0.###}, {m11:0.###}, {m12:0.###}]\n" +
            $"[{m20:0.###}, {m21:0.###}, {m22:0.###}]";

        Debug.Log(matrixString);
    }

}

