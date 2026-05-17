using UnityEngine;

public class TunnelSegment : Terraformer
{
    public ComputeShader computeEditting;

    public GameObject startCircle;
    float startRadius = 1.0f;

    public GameObject endCircle;
    float endRadius = 1.0f;

    public GameObject lengthBeam;
    float length = 1.0f;

    public override void Edit()
    {
        computeEditting.SetVector("cylinderStart", GetStartPoint());
        computeEditting.SetVector("cylinderEnd", GetEndPoint());

        computeEditting.SetFloat("startRadius", startRadius);
        computeEditting.SetFloat("endRadius", endRadius);

        computeEditting.SetFloat("surfaceLevel", world.GetSurfaceLevel());

        world.EditTunnel(computeEditting, GetStartPoint(), GetEndPoint(), startRadius, endRadius);
    }


    public void SetAllParams(float l, float r1, float r2)
    {
        SetStartRadius(r1);
        SetEndRadius(r2);
        SetLength(l);
    }

    public Vector3 GetEndPoint()
    {
        Vector3 center = lengthBeam.transform.position;
        Vector3 dir = lengthBeam.transform.TransformDirection(Vector3.up).normalized;
        float halfLength = length / 2f;
        Vector3 endPoint = center + dir * halfLength;

        return endPoint;
    }

    public Vector3 GetStartPoint()
    {
        Vector3 center = lengthBeam.transform.position;
        Vector3 dir = lengthBeam.transform.TransformDirection(Vector3.down).normalized;
        float halfLength = length / 2f;
        Vector3 endPoint = center + dir * halfLength;

        return endPoint;
    }

    public void SetLength(float t_newLength)
    {
        length = t_newLength;

        Vector3 newScale = lengthBeam.transform.localScale;
        newScale.y = length / 2.0f;
        lengthBeam.transform.localScale = newScale;

        float halfLength = length / 2f;

        Vector3 center = lengthBeam.transform.position;
        Vector3 dir = lengthBeam.transform.TransformDirection(Vector3.up).normalized;

        Vector3 startPoint = center - dir * halfLength;
        Vector3 endPoint = center + dir * halfLength;

        startCircle.transform.position = startPoint;
        endCircle.transform.position = endPoint;
    }
    public float GetLength() { return length; }


    public void SetStartRadius(float t_newRadius)
    {
        startRadius = t_newRadius;

        Vector3 newScale = startCircle.transform.localScale;
        newScale.x = startRadius * 2;
        newScale.z = startRadius * 2;

        startCircle.transform.localScale = newScale;
    }
    public float GetStartRadius() { return startRadius; }


    public void SetEndRadius(float t_newRadius)
    {
        endRadius = t_newRadius;

        Vector3 newScale = endCircle.transform.localScale;
        newScale.x = endRadius * 2;
        newScale.z = endRadius * 2;

        endCircle.transform.localScale = newScale;
    }
    public float GetEndRadius() { return endRadius; }
}
