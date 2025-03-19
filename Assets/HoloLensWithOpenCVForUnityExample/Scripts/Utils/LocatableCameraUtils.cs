using UnityEngine;
using Numerics = System.Numerics;

namespace HoloLensCameraStream
{
    public static class LocatableCameraUtils
    {
        public static UnityEngine.Matrix4x4 ConvertFloatArrayToMatrix4x4(float[] floatArray)
        {
            if (floatArray == null || floatArray.Length != 16)
            {
                Debug.LogError("Input float array does not contain 16 elements!");
                return UnityEngine.Matrix4x4.identity;
            }

            UnityEngine.Matrix4x4 matrix = new UnityEngine.Matrix4x4();
            matrix.m00 = floatArray[0];
            matrix.m01 = floatArray[1];
            matrix.m02 = floatArray[2];
            matrix.m03 = floatArray[3];
            matrix.m10 = floatArray[4];
            matrix.m11 = floatArray[5];
            matrix.m12 = floatArray[6];
            matrix.m13 = floatArray[7];
            matrix.m20 = floatArray[8];
            matrix.m21 = floatArray[9];
            matrix.m22 = floatArray[10];
            matrix.m23 = floatArray[11];
            matrix.m30 = floatArray[12];
            matrix.m31 = floatArray[13];
            matrix.m32 = floatArray[14];
            matrix.m33 = floatArray[15];

            return matrix;
        }

        public static UnityEngine.Vector3 ToUnityVector3(this Numerics.Vector3 vector)
        {
            return new UnityEngine.Vector3(vector.X, vector.Y, -vector.Z);
        }

        public static UnityEngine.Matrix4x4 ToUnityMatrix4x4(this Numerics.Matrix4x4 sysMatrix)
        {
            UnityEngine.Matrix4x4 unityMat = new UnityEngine.Matrix4x4();

            unityMat.m00 = sysMatrix.M11;
            unityMat.m01 = sysMatrix.M12;
            unityMat.m02 = sysMatrix.M13;
            unityMat.m03 = sysMatrix.M14;

            unityMat.m10 = sysMatrix.M21;
            unityMat.m11 = sysMatrix.M22;
            unityMat.m12 = sysMatrix.M23;
            unityMat.m13 = sysMatrix.M24;

            unityMat.m20 = -sysMatrix.M31;
            unityMat.m21 = -sysMatrix.M32;
            unityMat.m22 = -sysMatrix.M33;
            unityMat.m23 = -sysMatrix.M34;

            unityMat.m30 = sysMatrix.M41;
            unityMat.m31 = sysMatrix.M42;
            unityMat.m32 = sysMatrix.M43;
            unityMat.m33 = sysMatrix.M44;

            return unityMat;
        }
    }
}
