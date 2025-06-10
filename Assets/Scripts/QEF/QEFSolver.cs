using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QefData
{
    public float ata_00, ata_01, ata_02, ata_11, ata_12, ata_22;
    public float atb_x, atb_y, atb_z;
    public float btb;
    public float massPoint_x, massPoint_y, massPoint_z;
    public int numPoints;

    public QefData()
    {
        clear();
    }

    public QefData(QefData rhs)
    {
        set(rhs);
    }

    public QefData(float ata_00, float ata_01, float ata_02, float ata_11, float ata_12, float ata_22, float atb_x, float atb_y,
            float atb_z, float btb, float massPoint_x, float massPoint_y, float massPoint_z, int numPoints)
    {

        set(ata_00, ata_01, ata_02, ata_11, ata_12, ata_22, atb_x, atb_y, atb_z, btb, massPoint_x, massPoint_y, massPoint_z, numPoints);
    }

    public void add(QefData rhs)
    {
        ata_00 += rhs.ata_00;
        ata_01 += rhs.ata_01;
        ata_02 += rhs.ata_02;
        ata_11 += rhs.ata_11;
        ata_12 += rhs.ata_12;
        ata_22 += rhs.ata_22;
        atb_x += rhs.atb_x;
        atb_y += rhs.atb_y;
        atb_z += rhs.atb_z;
        btb += rhs.btb;
        massPoint_x += rhs.massPoint_x;
        massPoint_y += rhs.massPoint_y;
        massPoint_z += rhs.massPoint_z;
        numPoints += rhs.numPoints;
    }

    public void clear()
    {
        set(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    public void set(float ata_00, float ata_01, float ata_02, float ata_11, float ata_12, float ata_22, float atb_x, float atb_y,
             float atb_z, float btb, float massPoint_x, float massPoint_y, float massPoint_z, int numPoints)
    {
        this.ata_00 = ata_00;
        this.ata_01 = ata_01;
        this.ata_02 = ata_02;
        this.ata_11 = ata_11;
        this.ata_12 = ata_12;
        this.ata_22 = ata_22;
        this.atb_x = atb_x;
        this.atb_y = atb_y;
        this.atb_z = atb_z;
        this.btb = btb;
        this.massPoint_x = massPoint_x;
        this.massPoint_y = massPoint_y;
        this.massPoint_z = massPoint_z;
        this.numPoints = numPoints;
    }

    public void set(QefData rhs)
    {
        set(rhs.ata_00, rhs.ata_01, rhs.ata_02, rhs.ata_11, rhs.ata_12,
              rhs.ata_22, rhs.atb_x, rhs.atb_y, rhs.atb_z, rhs.btb,
              rhs.massPoint_x, rhs.massPoint_y, rhs.massPoint_z,
              rhs.numPoints);
    }
}

public class QefSolver
{
    private QefData data;
    private SMat3 ata;
    private Vector3 atb, massPoint, x;
    private bool hasSolution;


    public QefSolver()
    {
        data = new QefData();
        ata = new SMat3();
        atb = Vector3.zero;
        massPoint = Vector3.zero;
        x = Vector3.zero;
        hasSolution = false;
    }

    private QefSolver(QefSolver rhs)
    { }

    public Vector3 getMassPoint()
    {
        return massPoint;
    }

    public void add(float px, float py, float pz, float nx, float ny, float nz)
    {
        hasSolution = false;


        Vector3 tmpv = new Vector3(nx, ny, nz).normalized;
        nx = tmpv.x;
        ny = tmpv.y;
        nz = tmpv.z;

        data.ata_00 += nx * nx;
        data.ata_01 += nx * ny;
        data.ata_02 += nx * nz;
        data.ata_11 += ny * ny;
        data.ata_12 += ny * nz;
        data.ata_22 += nz * nz;
        float dot = nx * px + ny * py + nz * pz;
        data.atb_x += dot * nx;
        data.atb_y += dot * ny;
        data.atb_z += dot * nz;
        data.btb += dot * dot;
        data.massPoint_x += px;
        data.massPoint_y += py;
        data.massPoint_z += pz;
        data.numPoints += 1;
  
    }

    public void add(Vector3 p, Vector3 n)
    {
        add(p.x, p.y, p.z, n.x, n.y, n.z);
    }

    public void add(QefData rhs)
    {
        hasSolution = false;
        data.add(rhs);
    }

    public QefData getData()
    {
        return data;
    }

    public float getError()
    {
        if (!hasSolution)
        {
            throw new ArgumentException("Qef Solver does not have a solution!");
        }

        return getError(x);
    }

    public float getError(Vector3 pos)
    {
        if (!hasSolution)
        {
            setAta();
            setAtb();
        }

        Vector3 atax;
        MathOps.vmul_symmetric(out atax, ata, pos);
        return Vector3.Dot(pos, atax) - 2 * Vector3.Dot(pos, atb) + data.btb;
    }

    public void reset()
    {
        hasSolution = false;
        data.clear();
    }

    public float solve(out Vector3 outx, float svd_tol, int svd_sweeps, float pinv_tol)
    {
        if (data.numPoints == 0)
        {
            outx = Vector3.zero;
            return 0;
        }

   

        massPoint.Set(data.massPoint_x, data.massPoint_y, data.massPoint_z);
        massPoint *= (1.0f / data.numPoints);
        setAta();
        setAtb();
        Vector3 tmpv;
        MathOps.vmul_symmetric(out tmpv, ata, massPoint);
        //Debug.Log("ATb: " + atb);
        atb = atb - tmpv;
        //Debug.Log("ATb - tmpv: " + atb);
        x = Vector3.zero;

        //ata.DebugLog();
        float result = SVD.solveSymmetric(ata, atb, out x, svd_tol, svd_sweeps, pinv_tol);

        //Debug.Log("x2: " + x);
        if (float.IsNaN(result))
            x = massPoint;   
        else
            x += massPoint;

        setAtb();
        outx = x;
        hasSolution = true;

        //Debug.Log($"DC/Mass Point: {massPoint}, Solved Position: {outx}, Error: {result}");
        return result;
    }

    private void setAta()
    {
        ata.setSymmetric(data.ata_00, data.ata_01, data.ata_02, data.ata_11, data.ata_12, data.ata_22);
    }

    private void setAtb()
    {
        atb.Set(data.atb_x, data.atb_y, data.atb_z);
    }



}