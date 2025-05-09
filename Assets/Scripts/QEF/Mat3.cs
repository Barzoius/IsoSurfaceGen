using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mat3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;

    public Mat3(float m00, float m01, float m02, 
                float m10, float m11, float m12, 
                float m20, float m21, float m22)
    {
        this.m00 = m00;
        this.m01 = m01;
        this.m02 = m02;
        this.m10 = m10;
        this.m11 = m11;
        this.m12 = m12;
        this.m20 = m20;
        this.m21 = m21;
        this.m22 = m22;
    }

    public Mat3()
    {
        this.m00 = 0;
        this.m01 = 0;
        this.m02 = 0;
        this.m10 = 0;
        this.m11 = 0;
        this.m12 = 0;
        this.m20 = 0;
        this.m21 = 0;
        this.m22 = 0;
    }

    public Mat3(Mat3 matrix)
    {
        this.m00 = matrix.m00; 
        this.m01 = matrix.m01;   
        this.m02 = matrix.m02; 

        this.m10 = matrix.m10;   
        this.m11 = matrix.m11; 
        this.m12 = matrix.m12;

        this.m20 = matrix.m20;
        this.m21 = matrix.m21;
        this.m22 = matrix.m22;
    }

    static public Mat3 Identity()
    {
        return new Mat3(1,0,0,
                        0,1,0,
                        0,0,1);

    }

    public float Determinant()
    {
        return (m00 * m11 * m22 +
                m10 * m21 * m02 +
                m20 * m01 * m12 -
                m10 * m01 * m22 -
                m00 * m21 * m12 -
                m20 * m11 * m02);
    }

    public Mat3 Transpose()
    {
        return new Mat3(m00, m10, m20,
                        m01, m11, m21,
                        m02, m12, m22);
    }


    public Mat3 Cofactor()
    {
        float c00 = m11 * m22 - m12 * m21;
        float c01 = -(m10 * m22 - m12 * m20);
        float c02 = m10 * m21 - m11 * m20;

        float c10 = -(m01 * m22 - m02 * m21);
        float c11 = m00 * m22 - m02 * m20;
        float c12 = -(m00 * m21 - m01 * m20);

        float c20 = m01 * m12 - m02 * m11;
        float c21 = -(m00 * m12 - m02 * m10);
        float c22 = m00 * m11 - m01 * m10;

        return new Mat3(c00, c01, c02,
                        c10, c11, c12,
                        c20, c21, c22);
    }

    public Mat3 Adjoint()
    {
        return this.Cofactor().Transpose();
    }

    public Mat3 Inverse()
    {
        return new Mat3((1 / this.Determinant()) * this.Adjoint());
        
    }

    public static Mat3 operator * (float f, Mat3 matrix)
    {
        return new Mat3(matrix.m00 * f, matrix.m01 * f, matrix.m02 * f,
                        matrix.m10 * f, matrix.m11 * f, matrix.m12 * f,
                        matrix.m20 * f, matrix.m21 * f, matrix.m22 * f);

    }


    public static Vector3 operator * (Vector3 vec3, Mat3 matrix)
    {
       return new Vector3(
       matrix.m00 * vec3.x + matrix.m01 * vec3.y + matrix.m02 * vec3.z,
       matrix.m10 * vec3.x + matrix.m11 * vec3.y + matrix.m12 * vec3.z,
       matrix.m20 * vec3.x + matrix.m21 * vec3.y + matrix.m22 * vec3.z );
    }

    public static Vector3 operator * (Vector3Int vec3, Mat3 matrix)
    {
        return new Vector3(
        matrix.m00 * vec3.x + matrix.m01 * vec3.y + matrix.m02 * vec3.z,
        matrix.m10 * vec3.x + matrix.m11 * vec3.y + matrix.m12 * vec3.z,
        matrix.m20 * vec3.x + matrix.m21 * vec3.y + matrix.m22 * vec3.z);
    }

    public static Mat3 operator * (Mat3 matrix1, Mat3 matrix2)
    {
        float m00 = matrix1.m00 * matrix2.m00 + matrix1.m01 * matrix2.m10 + matrix1.m02 * matrix2.m20;
        float m01 = matrix1.m00 * matrix2.m01 + matrix1.m01 * matrix2.m11 + matrix1.m02 * matrix2.m21;
        float m02 = matrix1.m00 * matrix2.m02 + matrix1.m01 * matrix2.m12 + matrix1.m02 * matrix2.m22;

        float m10 = matrix1.m10 * matrix2.m00 + matrix1.m11 * matrix2.m10 + matrix1.m12 * matrix2.m20;
        float m11 = matrix1.m10 * matrix2.m01 + matrix1.m11 * matrix2.m11 + matrix1.m12 * matrix2.m21;
        float m12 = matrix1.m10 * matrix2.m02 + matrix1.m11 * matrix2.m12 + matrix1.m12 * matrix2.m22;

        float m20 = matrix1.m20 * matrix2.m00 + matrix1.m21 * matrix2.m10 + matrix1.m22 * matrix2.m20;
        float m21 = matrix1.m20 * matrix2.m01 + matrix1.m21 * matrix2.m11 + matrix1.m22 * matrix2.m21;
        float m22 = matrix1.m20 * matrix2.m02 + matrix1.m21 * matrix2.m12 + matrix1.m22 * matrix2.m22;

        return new Mat3( m00, m01, m02,
                         m10, m11, m12,
                         m20, m21, m22 );
    }

    public static Vector3 operator *(Mat3 matrix, Vector3 vec)
    {
        float x = matrix.m00 * vec.x + matrix.m01 * vec.y + matrix.m02 * vec.z;
        float y = matrix.m10 * vec.x + matrix.m11 * vec.y + matrix.m12 * vec.z;
        float z = matrix.m20 * vec.x + matrix.m21 * vec.y + matrix.m22 * vec.z;

        return new Vector3(x, y, z);
    }



    public float Get(int row, int col)
    {
        return this[row, col];
    }

    public void Set(int row, int col, float value)
    {
        this[row, col] = value;
    }

    public float this[int row, int col]
    {
        get
        {
            if (row == 0 && col == 0) return m00;
            if (row == 0 && col == 1) return m01;
            if (row == 0 && col == 2) return m02;
            if (row == 1 && col == 0) return m10;
            if (row == 1 && col == 1) return m11;
            if (row == 1 && col == 2) return m12;
            if (row == 2 && col == 0) return m20;
            if (row == 2 && col == 1) return m21;
            if (row == 2 && col == 2) return m22;
            throw new System.IndexOutOfRangeException();
        }
        set
        {
            if (row == 0 && col == 0) m00 = value;
            else if (row == 0 && col == 1) m01 = value;
            else if (row == 0 && col == 2) m02 = value;
            else if (row == 1 && col == 0) m10 = value;
            else if (row == 1 && col == 1) m11 = value;
            else if (row == 1 && col == 2) m12 = value;
            else if (row == 2 && col == 0) m20 = value;
            else if (row == 2 && col == 1) m21 = value;
            else if (row == 2 && col == 2) m22 = value;
            else throw new System.IndexOutOfRangeException();
        }
    }


    public override string ToString()
    {
        return $"[{m00,6:F2}, {m01,6:F2}, {m02,6:F2}]\n" +
               $"[{m10,6:F2}, {m11,6:F2}, {m12,6:F2}]\n" +
               $"[{m20,6:F2}, {m21,6:F2}, {m22,6:F2}]";


    }


}
