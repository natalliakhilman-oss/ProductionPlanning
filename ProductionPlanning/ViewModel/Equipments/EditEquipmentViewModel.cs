using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Equipments
{
    public class EditEquipmentViewModel
    {
        public Equipment Equipment { get; set; } = new Equipment();

        public EditEquipmentViewModel(Equipment equipment)
        {
            Equipment = equipment;
        }
        public EditEquipmentViewModel() { }
    }
}
