using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Utilities
{
    public static partial class DOTSMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowardsRadians(float current, float target, float speed)
        {
            var delta = DeltaRadians(current, target);
            
            if (-speed < delta &&  delta < speed)
                return target;

            return current + math.clamp(delta, -speed, speed);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DeltaRadians(float current, float target)
        {
            const float tau = math.PI * 2f;
            var delta = Repeat(target - current, tau);
            if (delta >= math.PI)
                delta -= tau;
            return delta;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowardsAngle(float current, float target, float speed)
        {
            var delta = DeltaAngle(current, target);
            
            if (-speed < delta &&  delta < speed)
                return target;

            return current + math.clamp(delta, -speed, speed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DeltaAngle(float current, float target)
        {
            var delta = Repeat(target - current, 360f);
            if (delta >= 180.0)
                delta -= 360f;
            return delta;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Repeat(float t, float length) => math.clamp(t - math.floor(t / length) * length, 0.0f, length);
    }
}