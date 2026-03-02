using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Equipments
{
    public class EquipmentsViewModel
    {
        public List<Equipment> Equipments { get; set; } = new();
        
        public EquipmentsViewModel(List<Equipment> equipments)
        {
            Equipments = equipments;
        }
        public EquipmentsViewModel() { }
    }
}
