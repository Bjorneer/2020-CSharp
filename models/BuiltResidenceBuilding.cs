namespace DotNet.models
{
    public class BuiltResidenceBuilding : BuiltBuilding
    {
        public int CurrentPop { get; set; }
        public double Temperature { get; set; }
        public double RequestedEnergyIn { get; set; }
        public double HappinessPerTickPerPop { get; set; }
        public int Health { get; set; }

    }
}