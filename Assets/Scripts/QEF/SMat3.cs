using UnityEngine;
public class SMat3
{
    public float m00, m01, m02, m11, m12, m22;

    public SMat3()
    {
        clear();
    }

    public SMat3(float m00, float m01, float m02, float m11, float m12, float m22)
    {
        this.setSymmetric(m00, m01, m02, m11, m12, m22);
    }

    public void clear()
    {
        this.setSymmetric(0, 0, 0, 0, 0, 0);
    }

    public void setSymmetric(float a00, float a01, float a02, float a11, float a12, float a22)
    {
        this.m00 = a00;
        this.m01 = a01;
        this.m02 = a02;
        this.m11 = a11;
        this.m12 = a12;
        this.m22 = a22;
    }

    public void setSymmetric(SMat3 rhs)
    {
        this.setSymmetric(rhs.m00, rhs.m01, rhs.m02, rhs.m11, rhs.m12, rhs.m22);
    }

    private SMat3(SMat3 rhs)
    {
        this.setSymmetric(rhs);
    }

    public void DebugLog(string label = "SMat3")
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