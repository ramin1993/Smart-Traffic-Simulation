namespace TrafficSimulation.Models
{// Models/TrafficDirection.cs
    public enum TrafficDirection
    {
        North,
        South,
        East,
        West
    }

    // Models/TrafficLight.cs
    public class TrafficLight
    {
        public TrafficDirection Direction { get; set; }
        public LightColor CurrentColor { get; set; } = LightColor.Red;
        public int VehicleCount { get; set; } = 0;
        public int GreenLightDuration { get; set; } = 10; // saniyə
    }

    // Models/LightColor.cs
    public enum LightColor
    {
        Red,
        Yellow,
        Green
    }
}
