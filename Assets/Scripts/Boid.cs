using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    #region References

    BoidSettings settings;

    // Cached
    Material material;
    Transform cachedTransform;
    Transform target;

    #endregion

    #region Variables

    // State
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Vector3 forward;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector] public Vector3 avgFlockHeading;
    [HideInInspector] public Vector3 avgAvoidanceHeading;
    [HideInInspector] public Vector3 centreOfFlockmates;
    [HideInInspector] public int numPerceivedFlockmates;
    

    private Vector3 alignmentForce;
    private Vector3 cohesionForce;
    private Vector3 seperationForce;

    #endregion

    #region Awake

    void Awake()
    {
        material = transform.GetComponentInChildren<MeshRenderer>().material;
        cachedTransform = transform;
    }

    #endregion

    #region Function

    public void Initialize(BoidSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    public void SetColour(Color col)
    {
        if (material != null)
        {
            material.color = col;
        }
    }

    public void UpdateBoid()
    {
        Vector3 acceleration = Vector3.zero;

        if (target != null)
        {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

             alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
             cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
             seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;
            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }

    bool IsHeadingForCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst,
                settings.obstacleMask))
        {
            return true;
        }
        else
        {
        }

        return false;
    }

    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask))
            {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }


    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || settings == null) return;

        Vector3 startPos = transform.position;

        if (settings.showCohesionGizmos || cohesionForce == Vector3.zero)
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawRay(startPos,cohesionForce.normalized *3f);
            
        }

        if (settings.showAlignmentGizmos || alignmentForce == Vector3.zero)
        {
            Gizmos.color = Color.green;
            
            Gizmos.DrawRay(startPos , alignmentForce.normalized *3f);
        }

        if (settings.showSeprationGizmos || seperationForce == Vector3.zero)
        {
            Gizmos.color = Color.blue;
            
            Gizmos.DrawRay(startPos, seperationForce.normalized *3f);
        }
        
        Gizmos.DrawWireSphere(startPos, settings.perceptionRadius);
    }

    #endregion
}