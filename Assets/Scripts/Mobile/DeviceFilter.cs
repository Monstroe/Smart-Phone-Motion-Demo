using UnityEngine;

public abstract class DeviceFilter
{
    public Quaternion Orientation { get; protected set; }

    public abstract void Calibrate(Quaternion referenceOrientation);
    public virtual void Update(Vector3 gyro, Vector3 accel, float dt) { }
    public virtual void Update(Vector3 gyro, Vector3 accel, Vector3 mag, float dt) { }
}

public enum FilterType
{
    Unity,
    Madgwick,
}
