using TrafficSimulation.Models;

namespace TrafficSimulation.Services
{
    public class TrafficSimulationService
    {
        public Dictionary<TrafficDirection, TrafficLight> TrafficLights { get; private set; }
        private Timer simulationTimer;
        private Random random = new Random();
        private TrafficDirection currentGreenDirection;
        private bool isRunning = false;
        private int timeSinceLastSwitch = 0;
        private const int MIN_GREEN_TIME = 5;
        private readonly object lockObject = new object();

        public event Action OnStateChanged;

        public TrafficSimulationService()
        {
            InitializeTrafficLights();
        }

        private void InitializeTrafficLights()
        {
            lock (lockObject)
            {
                TrafficLights = new Dictionary<TrafficDirection, TrafficLight>
                {
                    { TrafficDirection.North, new TrafficLight { Direction = TrafficDirection.North, VehicleCount = 3 } },
                    { TrafficDirection.South, new TrafficLight { Direction = TrafficDirection.South, VehicleCount = 2 } },
                    { TrafficDirection.East, new TrafficLight { Direction = TrafficDirection.East, VehicleCount = 4 } },
                    { TrafficDirection.West, new TrafficLight { Direction = TrafficDirection.West, VehicleCount = 1 } }
                };

                currentGreenDirection = GetDirectionWithMostVehicles();
                SetGreenLight(currentGreenDirection);
            }
        }

        public void StartSimulation()
        {
            if (isRunning) return;

            isRunning = true;
            simulationTimer = new Timer(async _ => await UpdateSimulationAsync(), null, 0, 1000);
        }

        public void StopSimulation()
        {
            isRunning = false;
            simulationTimer?.Dispose();
            simulationTimer = null;
        }

        private async Task UpdateSimulationAsync()
        {
            if (!isRunning) return;

            lock (lockObject)
            {
                AddRandomVehicles();

                var currentLight = TrafficLights[currentGreenDirection];
                currentLight.GreenLightDuration--;
                timeSinceLastSwitch++;

                if (currentLight.CurrentColor == LightColor.Green && currentLight.VehicleCount > 0)
                {
                    currentLight.VehicleCount--;
                }

                if (timeSinceLastSwitch >= MIN_GREEN_TIME && currentLight.GreenLightDuration <= 0)
                {
                    SwitchToNextDirection();
                }
            }
            OnStateChanged?.Invoke();
        }

        private void SwitchToNextDirection()
        {
            TrafficLights[currentGreenDirection].CurrentColor = LightColor.Red;

            currentGreenDirection = GetDirectionWithMostVehicles();
            SetGreenLight(currentGreenDirection);

            timeSinceLastSwitch = 0;
        }

        private TrafficDirection GetDirectionWithMostVehicles()
        {
            var redLights = TrafficLights.Values
                .Where(x => x.CurrentColor == LightColor.Red)
                .ToList();

            if (redLights.Count == 0)
                return GetRandomDirectionExceptCurrent();

            var maxVehicles = redLights.Max(x => x.VehicleCount);

            var candidates = redLights
                .Where(x => x.VehicleCount == maxVehicles)
                .ToList();

            return candidates[random.Next(candidates.Count)].Direction;
        }

        private TrafficDirection GetRandomDirectionExceptCurrent()
        {
            var directions = TrafficLights.Keys
                .Where(x => x != currentGreenDirection)
                .ToList();
            return directions[random.Next(directions.Count)];
        }

        private void SetGreenLight(TrafficDirection direction)
        {
            // Bütün işıqları qırmızı et
            foreach (var light in TrafficLights.Values)
            {
                light.CurrentColor = LightColor.Red;
            }

            TrafficLights[direction].CurrentColor = LightColor.Green;
            TrafficLights[direction].GreenLightDuration = 8;
            currentGreenDirection = direction;
        }

        private void AddRandomVehicles()
        {
            foreach (var light in TrafficLights.Values)
            {
                if (light.CurrentColor == LightColor.Red && random.Next(0, 100) < 35)
                {
                    light.VehicleCount += random.Next(1, 3);
                }
            }
        }

        public void AddVehicle(TrafficDirection direction)
        {
            lock (lockObject)
            {
                TrafficLights[direction].VehicleCount++;
            }
            OnStateChanged?.Invoke();
        }

        public TrafficDirection GetCurrentGreenDirection() => currentGreenDirection;

        public void ResetSimulation()
        {
            StopSimulation();
            InitializeTrafficLights();
            OnStateChanged?.Invoke();
        }

        public void Dispose()
        {
            simulationTimer?.Dispose();
        }
    }
}