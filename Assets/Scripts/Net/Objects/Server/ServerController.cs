using CNetworkingSolution;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ServerController : ServerObject
{
    [Header("Laser")]
    [SerializeField] private float lineLength = 10f;
    [SerializeField] private float lerpSpeed = 25f;
    [SerializeField] private float laserOffset = 0.25f;
    [SerializeField] private LayerMask laserLayerMask;
    [SerializeField] private float highlightMultiplier = 4f;

    [Header("Grabbing")]
    [SerializeField] private float distanceSensitivity = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 15f;

    private Quaternion receivedRotation;
    private bool isHolding = false;
    private bool prevHoldingState = false;
    private float deltaY = 0f;

    private LineRenderer lineRenderer;
    private GameObject highlightTarget;
    private Color originalEmission;

    private GameObject grabTarget;
    private float startingDistanceFromUser = 0f;
    private Vector3 lastPosition;
    private Vector3 throwVelocity;

    public override void Init(ushort id, ServerLobby lobby)
    {
        base.Init(id, lobby);
        lineRenderer = GetComponent<LineRenderer>();
        lobby.GetService<ControllerServerService>().ServerControllers.Add(Owner, this);
    }

    public override void Remove()
    {
        base.Remove();
        lobby.GetService<ControllerServerService>().ServerControllers.Remove(Owner);
    }

    public override void ReceiveData(UserData user, NetPacket packet, ushort commandType, TransportMethod? transportMethod)
    {
        base.ReceiveData(user, packet, commandType, transportMethod);
        switch ((ControllerCommandType)commandType)
        {
            case ControllerCommandType.CONTROLLER_STATE:
                receivedRotation = packet.ReadQuaternion();
                isHolding = packet.ReadBool();
                deltaY = packet.ReadFloat();
                break;
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (OwnerId.HasValue)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, receivedRotation, lerpSpeed * Time.fixedDeltaTime);
        }

        UpdateLaser();
    }

    private void UpdateLaser()
    {
        Vector3 direction = transform.up;
        Vector3 start = transform.position + direction * laserOffset;
        Vector3 end = start + direction * lineLength;

        lineRenderer.SetPosition(0, start);
        Debug.DrawRay(transform.position, direction * 100f, Color.blue);

        if (isHolding && grabTarget != null)
        {
            startingDistanceFromUser += deltaY * distanceSensitivity * Time.fixedDeltaTime;
            startingDistanceFromUser = Mathf.Clamp(startingDistanceFromUser, minDistance, maxDistance);
            Debug.Log($"Holding object at distance: {startingDistanceFromUser}");
            end = start + direction * startingDistanceFromUser;
            lineRenderer.SetPosition(1, end);
            ChangeLaserColor(Color.green);
            grabTarget.transform.position = end;

            Vector3 currentPosition = grabTarget.transform.position;
            throwVelocity = (currentPosition - lastPosition) / Time.fixedDeltaTime;
            lastPosition = currentPosition;
            return;
        }

        if (!isHolding && prevHoldingState)
        {
            prevHoldingState = false;
            Rigidbody rb = grabTarget.GetComponent<Rigidbody>();
            rb.isKinematic = false;

            rb.linearVelocity = throwVelocity;

            grabTarget = null;
            return;
        }

        if (lobby.PhysicsScene.Value.Raycast(transform.position, direction, out RaycastHit hit, lineLength, laserLayerMask))
        {
            end = hit.point;
            lineRenderer.SetPosition(1, end);
            HandleHighlight(hit.collider.gameObject);
            ChangeLaserColor(Color.yellow);

            if (isHolding && !prevHoldingState)
            {
                prevHoldingState = true;
                grabTarget = hit.collider.gameObject;
                Rigidbody rb = grabTarget.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                startingDistanceFromUser = Vector3.Distance(transform.position, grabTarget.transform.position);

                lastPosition = grabTarget.transform.position;
                throwVelocity = Vector3.zero;
            }
        }
        else
        {
            lineRenderer.SetPosition(1, end);
            ClearHighlight();
            ChangeLaserColor(Color.red);
        }
    }

    private void ChangeLaserColor(Color color)
    {
        lineRenderer.colorGradient = new Gradient
        {
            colorKeys = new GradientColorKey[]
            {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
            },
            alphaKeys = new GradientAlphaKey[]
            {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
            }
        };
    }

    private void HandleHighlight(GameObject target)
    {
        if (grabTarget != null || highlightTarget == target) return;

        ClearHighlight();

        highlightTarget = target;
        Renderer r = highlightTarget.GetComponent<Renderer>();
        originalEmission = r.material.GetColor("_EmissionColor");
        r.material.SetColor("_EmissionColor", originalEmission * highlightMultiplier);
    }

    private void ClearHighlight()
    {
        if (highlightTarget == null) return;

        Renderer r = highlightTarget.GetComponent<Renderer>();
        r.material.SetColor("_EmissionColor", originalEmission);
        highlightTarget = null;
    }
}
