using CNetworkingSolution;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ServerController : ServerObject
{
    [SerializeField] private float lineLength = 10f;
    [SerializeField] private float lerpSpeed = 25f;
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float laserOffset = 0.25f;
    [SerializeField] private LayerMask laserLayerMask;
    [SerializeField] private float highlightMultiplier = 4f;

    private Quaternion receivedRotation;
    private bool isHolding = false;
    private LineRenderer lineRenderer;

    private GameObject highlightTarget;
    private GameObject grabTarget;
    private Color originalEmission;

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

        if (grabTarget != null)
        {
            Vector3 targetPosition = transform.position + transform.forward * lineLength;

            grabTarget.transform.position = Vector3.Lerp(grabTarget.transform.position, targetPosition, Time.deltaTime * followSpeed);
        }
    }

    private void UpdateLaser()
    {
        Vector3 start = transform.position + -transform.up * laserOffset;
        Vector3 direction = -transform.up;

        lineRenderer.SetPosition(0, start);

        Debug.DrawRay(transform.position, direction * 100f, Color.blue);
        if (lobby.PhysicsScene.Value.Raycast(transform.position, direction, out RaycastHit hit, lineLength, laserLayerMask))
        {
            lineRenderer.SetPosition(1, hit.point);
            HandleHighlight(hit.collider.gameObject);
            Color baseColor = hit.collider.gameObject.GetComponent<Renderer>().material.color;

            lineRenderer.colorGradient = new Gradient
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(baseColor, 0f),
                    new GradientColorKey(baseColor, 1f)
                },
                alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            };

            if (isHolding && grabTarget == null)
            {
                grabTarget = hit.collider.gameObject;
                Rigidbody rb = grabTarget.GetComponent<Rigidbody>();
                rb.isKinematic = true;
            }
            else if (!isHolding && grabTarget != null)
            {
                Rigidbody rb = grabTarget.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                grabTarget = null;
            }
        }
        else
        {
            lineRenderer.SetPosition(1, start + direction * lineLength);
            ClearHighlight();

            lineRenderer.colorGradient = new Gradient
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.red, 1f)
                },
                alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            };
        }
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
