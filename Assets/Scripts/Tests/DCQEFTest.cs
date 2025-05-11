using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DCQEFTest
{

    private static readonly Vector3 VoxelMin = new Vector3(0, 0, 0);
    private static readonly Vector3 VoxelMax = new Vector3(4, 4, 4); // voxelSize = 4

    private void AssertInsideVoxel(Vector3 result)
    {
        Assert.IsTrue(
            result.x >= VoxelMin.x && result.x <= VoxelMax.x &&
            result.y >= VoxelMin.y && result.y <= VoxelMax.y &&
            result.z >= VoxelMin.z && result.z <= VoxelMax.z,
            $"QEF vertex {result} is outside voxel bounds {VoxelMin} - {VoxelMax}"
        );
    }

    [Test]
    public void QEF_CenteredHermitePoints_ShouldStayInside()
    {
        Vector3[] points =
        {
            new Vector3(1, 1, 1),
            new Vector3(3, 1, 1),
            new Vector3(1, 3, 1),
            new Vector3(1, 1, 3)
        };

        Vector3[] normals =
        {
            Vector3.left,
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };

        var qef = new QEF();
        for (int i = 0; i < points.Length; i++)
            qef.AddToQEFData(points[i], normals[i]);

        Vector3 result = qef.SolveQEF();
        AssertInsideVoxel(result);
    }

    [Test]
    public void QEF_PointsOnVoxelEdges_ShouldStayInside()
    {
        Vector3[] points =
        {
            new Vector3(0, 2, 2),
            new Vector3(4, 2, 2),
            new Vector3(2, 0, 2),
            new Vector3(2, 4, 2),
            new Vector3(2, 2, 0),
            new Vector3(2, 2, 4)
        };

        Vector3[] normals =
        {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };

        var qef = new QEF();
        for (int i = 0; i < points.Length; i++)
            qef.AddToQEFData(points[i], normals[i]);

        Vector3 result = qef.SolveQEF();
        AssertInsideVoxel(result);
    }

    [Test]
    public void QEF_AsymmetricPoints_ShouldStayInside()
    {
        Vector3[] points =
        {
            new Vector3(0.5f, 1, 3),
            new Vector3(3.5f, 2, 0.5f),
            new Vector3(1.5f, 3.5f, 1)
        };

        Vector3[] normals =
        {
            new Vector3(0.2f, 0.8f, -0.5f).normalized,
            new Vector3(-0.7f, 0.1f, 0.3f).normalized,
            new Vector3(0.1f, -0.6f, 0.8f).normalized
        };

        var qef = new QEF();
        for (int i = 0; i < points.Length; i++)
            qef.AddToQEFData(points[i], normals[i]);

        Vector3 result = qef.SolveQEF();
        AssertInsideVoxel(result);
    }

    [Test]
    public void QEF_AllPointsOnOneFace_ShouldStayInside()
    {
        Vector3[] points =
        {
            new Vector3(0, 0, 0),
            new Vector3(4, 0, 0),
            new Vector3(4, 4, 0),
            new Vector3(0, 4, 0)
        };

        Vector3[] normals =
        {
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward
        };

        var qef = new QEF();
        for (int i = 0; i < points.Length; i++)
            qef.AddToQEFData(points[i], normals[i]);

        Vector3 result = qef.SolveQEF();
        AssertInsideVoxel(result);
    }

    [Test]
    public void QEF_AllNormalsSameDirection_ShouldStillStayInside()
    {
        Vector3[] points =
        {
            new Vector3(1, 1, 1),
            new Vector3(2, 2, 2),
            new Vector3(3, 3, 3)
        };

        Vector3 sharedNormal = new Vector3(1, 1, 1).normalized;

        var qef = new QEF();
        foreach (Vector3 pt in points)
            qef.AddToQEFData(pt, sharedNormal);

        Vector3 result = qef.SolveQEF();
        AssertInsideVoxel(result);
    }
}
