using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float EngineForce;
    public float BrakeForce;
    public float CDrag;
    public float CRollingResistance;
    public float Mass;
    public float CCenterOfMassHeight = 5; // arbitary constant added to total height of center of mass from ground while resting on ground
    
    [Header("Wheels")]
    public float CWheelFriction = 1;


    public Transform CenterOfMass;
    public WheelController[] RearWheels;
    public WheelController[] FrontWheels;

    public Vector3 Acceleration;
    public Vector3 Velocity;
    public float MaxCenterOfMassHeight; // calculated when vehicle is resting on ground


    // Start is called before the first frame update
    void Start()
    {
        if(Physics.Raycast(CenterOfMass.position, Vector3.down, out RaycastHit hit))
        {
            MaxCenterOfMassHeight = Vector3.Distance(CenterOfMass.position, hit.point) + CCenterOfMassHeight;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var speed = Velocity.magnitude;

        // Fdrag= -C drag * v * |v|
        // Air resistance - aerodynamic drag
        var forceDrag = -CDrag * speed * Velocity;

        // Rolling resistance Frr
        var rollingResistanceForce = -CRollingResistance * Velocity;

        // Logitudnal force
        Vector3 totalLogitudnalForce = Vector3.zero;

        // Accelerate (basically move forward)
        if (Input.GetKey(KeyCode.W))
        {
            Accelerate(forceDrag, rollingResistanceForce, ref totalLogitudnalForce);
        }
        else
        {
            totalLogitudnalForce = forceDrag + rollingResistanceForce;

            // Cease the movement if speed is negligible
            if (speed < 0.3f)
            {
                Velocity = Vector3.zero;
            }
        }

        // Apply Brake
        if (Input.GetKey(KeyCode.Space))
        {
            Brake(speed, forceDrag, rollingResistanceForce, ref totalLogitudnalForce);
        }

        UpdateWheels();

        // Newton's second law
        Acceleration = totalLogitudnalForce / Mass;

        // Euler's method for numerical equation
        Velocity += Time.deltaTime * Acceleration;

        transform.position += Time.deltaTime * Velocity;
    }

    private void Accelerate(Vector3 forceDrag, Vector3 rollingResistanceForce, ref Vector3 totalLogitudnalForce)
    {
        // traction force = u * engine force
        // i have used u = transform.forward where u is a unit vector in the direction of the car's head.
        var tractionForce = transform.forward * EngineForce;

        totalLogitudnalForce = tractionForce + forceDrag + rollingResistanceForce;
    }

    private void Brake(float speed, Vector3 forceDrag, Vector3 rollingResistanceForce, ref Vector3 totalLogitudnalForce)
    {
        // if speed is greater than 10 then apply brake else stop instantly
        if (speed > 1)
        {
            // u * constant
            var brakeForce = transform.forward * BrakeForce;

            totalLogitudnalForce = -brakeForce + forceDrag + rollingResistanceForce;
        }
        else
        {
            // Cease the movement if speed is negligible
            if (speed < 0.3f)
            {
                Velocity = Vector3.zero;
            }
        }
    }

    private void UpdateWheels()
    {
        if(FrontWheels.Length == 0 || RearWheels.Length == 0)
        {
            return;
        }

        Vector3 frontAxel = Vector3.zero;
        Vector3 rearAxel = Vector3.zero;
        var wheelBase = Vector3.Distance(FrontWheels[0].transform.position, RearWheels[0].transform.position);
        float distanceTowardsGround = 0;

        if(Physics.Raycast(CenterOfMass.position, Vector3.down, out RaycastHit hitInfo))
        {
            distanceTowardsGround = Vector3.Distance(CenterOfMass.position, hitInfo.point);

            if(distanceTowardsGround > MaxCenterOfMassHeight)
            {
                distanceTowardsGround = MaxCenterOfMassHeight;
            }
        }
        else
        {
            distanceTowardsGround = MaxCenterOfMassHeight;
        }

        for (int i = 0; i < FrontWheels.Length; i++)
        {
            frontAxel += FrontWheels[i].transform.position;
        }

        for (int i = 0; i < FrontWheels.Length; i++)
        {
            rearAxel += RearWheels[i].transform.position;
        }

        frontAxel /= FrontWheels.Length;
        rearAxel /= RearWheels.Length;

        for (var i = 0; i < FrontWheels.Length; i++)
        {
            var wheel = FrontWheels[i];
            var wheelMass = wheel.Mass;
            var wheelWeight = wheel.Mass * Physics.gravity;
            var frontWheelWeight = (Vector3.Distance(frontAxel, CenterOfMass.position) / wheelBase) * wheelWeight;
            frontWheelWeight -= (distanceTowardsGround / wheelBase) * wheelMass * Acceleration;

            wheel.Weight = frontWheelWeight;
        }

        for (var i = 0; i < RearWheels.Length; i++)
        {
            var wheel = RearWheels[i];
            var wheelMass = wheel.Mass;
            var wheelWeight = wheel.Mass * Physics.gravity;
            var rearWheelWeight = (Vector3.Distance(rearAxel, CenterOfMass.position) / wheelBase) * wheelWeight;
            rearWheelWeight += (distanceTowardsGround / wheelBase) * wheelMass * Acceleration;

            wheel.Weight = rearWheelWeight;
        }
    }
}
