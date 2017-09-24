namespace Physics.Util
{
  public static class VectorExt
  {
    public static UnityEngine.Vector3 ToUnityVector3(Vector3D native)
    {
      return new UnityEngine.Vector3(native.x, native.y, native.z);
    }

    public static Vector3D ToNativeVector3D(UnityEngine.Vector3 vector)
    {
      return new Vector3D(vector.x, vector.y, vector.z);
    }
  }
}
