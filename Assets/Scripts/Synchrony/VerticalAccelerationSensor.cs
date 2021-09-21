using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Synchrony
{
    public class AccelerationSample
    {
        public float accelerationY; // -0.84 gravity is normal in Amsterdam
        public DateTime dateTimeUtc;

        public double AgeInSeconds()
        {
            return (DateTime.UtcNow - dateTimeUtc).TotalSeconds;
        }
    }

    public class VerticalAccelerationSensor
    {
        public int samplesPerSecond = 20;
        public float pushThresholdInMs2 = 0.2f; // A push is an acceleration of 1 meter per second per second

        List<AccelerationSample> accelerationSamples = new List<AccelerationSample>();

        public void OnEnable()
        {
            if (HasAccelerometer())
                InputSystem.EnableDevice(Accelerometer.current);

            accelerationSamples.Clear();

            "VerticalAccelerationSensor enabled".Log();
        }

        public void OnDisable()
        {
            if (HasAccelerometer())
                InputSystem.DisableDevice(Accelerometer.current);
        }

        internal void OnDestroy()
        {
            OnDisable();
        }

        public void TakeSample()
        {
            if (!HasAccelerometer())
                return;

            var prevSample = MostRecentSample();
            if (prevSample != null && prevSample.AgeInSeconds() < 1 / samplesPerSecond)
                return;

            RemoveOutdatedSample();

            var nextSample = new AccelerationSample
            {
                accelerationY = Accelerometer.current.acceleration.ReadValue().y,
                dateTimeUtc = DateTime.UtcNow
            };

            accelerationSamples.Add(nextSample);
        }

        private void RemoveOutdatedSample()
        {
            var oldestSample = OldestSample();
            if (oldestSample != null && oldestSample.AgeInSeconds() > 1)
                accelerationSamples.RemoveAt(0);
        }

        public AccelerationSample MostRecentSample()
        {
            return accelerationSamples.LastOrDefault();
        }

        public AccelerationSample OldestSample()
        {
            return accelerationSamples.FirstOrDefault();
        }

        private static bool HasAccelerometer()
        {
            var hasAccelerometer = UnityEngine.InputSystem.Accelerometer.current != null;
            return hasAccelerometer;
        }

        private float CurrentNetAcceleration()
        {
            if (!accelerationSamples.Any())
                return 0f;
            if (accelerationSamples.First().AgeInSeconds() < 1.0)
                return 0f;

            var avgAcceleration = accelerationSamples.Average(s => s.accelerationY);
            // Subtract the average which is typically -0.8 (idle Gravity)
            return accelerationSamples.Last().accelerationY - avgAcceleration;
        }

        internal bool IsPushedUp()
        {
            var netAcceleration = CurrentNetAcceleration();

            //if (netAcceleration != 0)
            //    $"IsPushedUp Acceleration: {netAcceleration}".Log();

            return netAcceleration > pushThresholdInMs2;
        }

        internal bool IsPushedDown()
        {
            var netAcceleration = CurrentNetAcceleration();
            return netAcceleration < -pushThresholdInMs2;
        }
    }
}
