using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float EngineForce = 2500; // engine torque = 448 rpm
    public float BrakeForce = 1000; // Is this also supposed replaced with engine force (in rpm for braking?)
    
    public float CAirDrag = 0.4257f;
    public float CRollingResistance = 12.8f;
    public float Mass = 1500;
    public float CCenterOfMassHeight = 5; // arbitary constant added to total height of center of mass from ground while resting on ground

    public float[] Gears = {2.66f, 1.78f, 1.3f, 1, 0.74f, 0.5f};
    public float ReverseGear = 2.9f;
    public float DifferentialRatio = 3.42f;
    public float TransmissionEfficacy = 0.7f; // torque from engine is converted via gear and differential ratio where energy is lost in form of heat (around 30%) so we get 70% efficacy
    
    [Header("Wheels")]
    public float CWheelFriction = 1;
    public float SteerRotationLimit = 45; // degree


    public Transform CenterOfMass;
    public WheelController[] RearWheels;
    public WheelController[] FrontWheels;

    // Hidden gems for geeks
    public Vector3 Acceleration; // m/s*s
    public Vector3 Velocity; // | m/s |
    public float Speed; // | m/s |
    public float MaxCenterOfMassHeight; // calculated when vehicle is resting on ground
    public float MaxTractionForce;
    public Vector3 DriveForce;

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
        Speed = Velocity.magnitude;

        // Fdrag= -C drag * v * |v|
        // Air resistance - aerodynamic drag
        var airDrag = -CAirDrag * Speed * Velocity;

        // Rolling resistance Frr
        var rollingResistanceForce = -CRollingResistance * Velocity;

        // Logitudnal force
        Vector3 totalLogitudnalForce = Vector3.zero;

        UpdateWheels();

        // Accelerate (basically move forward)
        if (Input.GetKey(KeyCode.W)) // dons't work properly with backward as brake's become unresponsive || Input.GetKey(KeyCode.S))
        {
            var throttleInputSpeed = Input.GetAxis("Vertical");
            Accelerate(throttleInputSpeed, airDrag, rollingResistanceForce, ref totalLogitudnalForce);
        }
        else
        {
            totalLogitudnalForce = airDrag + rollingResistanceForce;

            // Cease the movement if speed is negligible
            if (Speed < 0.3f)
            {
                Velocity = Vector3.zero;
            }
        }

        // Apply Brake
        if (Input.GetKey(KeyCode.Space))
        {
            Brake(Speed, airDrag, rollingResistanceForce, ref totalLogitudnalForce);
        }

        Turn();

        // Newton's second law
        Acceleration = totalLogitudnalForce / Mass;

        // Euler's method for numerical equation
        Velocity += Time.deltaTime * Acceleration;

        transform.position += Time.deltaTime * Velocity;
    }

    private void Accelerate(float throttlePosition, Vector3 forceDrag, Vector3 rollingResistanceForce, ref Vector3 totalLogitudnalForce)
    {
        // traction force = u * engine force
        // i have used u = transform.forward where u is a unit vector in the direction of the car's head.

        // Fdrive = u * Engine Torque * gear ratio * differential ratio * transmission efficacy / wheel radius
        // u is unit vector which reflects the car orientation
        var engineTorque = LookupTorqueCurve(EngineForce) * throttlePosition; // n.m
        var driveTraction = transform.forward * engineTorque * Gears[0] * DifferentialRatio * TransmissionEfficacy;
        var driveForce = driveTraction / RearWheels[0].Radius;

        driveForce = Vector3.ClampMagnitude(driveForce, MaxTractionForce);
        totalLogitudnalForce = driveForce + forceDrag + rollingResistanceForce;
    }

    private void Brake(float speed, Vector3 forceDrag, Vector3 rollingResistanceForce, ref Vector3 totalLogitudnalForce)
    {
        // if speed is greater than 10 then apply brake else stop instantly
        if (speed > 0.1f)
        {
            // u * constant
            //var brakeForce = transform.forward * BrakeForce;
            var brakeForce = transform.forward * LookupTorqueCurve(BrakeForce) * ReverseGear * DifferentialRatio * TransmissionEfficacy / RearWheels[0].Radius;

            totalLogitudnalForce = -brakeForce + forceDrag + rollingResistanceForce;
        }
        else
        {
            // Cease the movement if speed is negligible
            if (speed < 0.1f)
            {
                Velocity = Vector3.zero;
            }
        }
    }

    private void UpdateWheels()
    {
        if(FrontWheels.Length == 0 || RearWheels.Length == 0)
        {
            MaxTractionForce = 0;
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

        Vector3 cumulativeTractionForce = Vector3.zero ;

        for (var i = 0; i < FrontWheels.Length; i++)
        {
            var wheel = FrontWheels[i];
            //var wheelMass = wheel.Mass;
            var wheelWeight = Mass * Physics.gravity;
            var frontWheelWeight = (Vector3.Distance(frontAxel, CenterOfMass.position) / wheelBase) * wheelWeight;
            frontWheelWeight -= (distanceTowardsGround / wheelBase) * Mass * Acceleration;

            wheel.AngularVelocity = Speed / (2 * Mathf.PI * wheel.Radius); // rad/s 
            //wheel.AngularVelocity = Speed / wheel.Radius; // | rad/s | 
            wheel.Weight = frontWheelWeight;

            //cumulativeTractionForce += CWheelFriction * frontWheelWeight;
        }

        for (var i = 0; i < RearWheels.Length; i++)
        {
            var wheel = RearWheels[i];
            //var wheelMass = wheel.Mass;
            var wheelWeight = Mass * Physics.gravity;
            var rearWheelWeight = (Vector3.Distance(rearAxel, CenterOfMass.position) / wheelBase) * wheelWeight;
            rearWheelWeight += (distanceTowardsGround / wheelBase) * Mass * Acceleration;
            wheel.AngularVelocity = Speed / (2 * Mathf.PI * wheel.Radius); // rad/s

            var slipRatio = (wheel.AngularVelocity * wheel.Radius - Speed)/Speed;
            CWheelFriction = slipRatio;
            cumulativeTractionForce += CWheelFriction * rearWheelWeight;

            wheel.Weight = rearWheelWeight;
        }

        MaxTractionForce = cumulativeTractionForce.magnitude / RearWheels.Length; // N
    }

    private float LookupTorqueCurve(float rpm)
    {
        // TODO: implement the graph curve (torque | rpm)
        //public float EngineTorque = 448; // N.m , we get this from engine's force (here its 2500 rpm) // we yield this value through graph
        // horse power = torque * rpm / 5252
        return 448;
    }

    private void Turn()
    {
        var steeringInput = Input.GetAxis("Horizontal");
        var steeringAngle = steeringInput * SteerRotationLimit * Mathf.Deg2Rad;

        for(var i = 0; i < FrontWheels.Length; i++)
        {
            FrontWheels[i].Rotate(steeringAngle);
        }

        Debug.DrawRay(RearWheels[0].transform.position, RearWheels[0].transform.right, Color.green);
        Debug.DrawRay(FrontWheels[0].transform.position, FrontWheels[0].transform.right, Color.yellow);
    }
}
