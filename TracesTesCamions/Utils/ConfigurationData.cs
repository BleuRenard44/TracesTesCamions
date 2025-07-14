using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using TracesTesCamions.Models;

namespace TracesTesCamions.Data
{
    public class ConfigurationData
    {
        public ObservableCollection<string> MarquesVehicule { get; set; } = new();
        public ObservableCollection<string> MarquesPneumatique { get; set; } = new();
        public ObservableCollection<string> EntreprisesMaintenance { get; set; } = new();
        public ObservableCollection<string> TypesDepense { get; set; } = new();
        public string? CurrentDataFolder { get; set; }

        private readonly string folder;

        public ConfigurationData(string dataFolder)
        {
            folder = dataFolder;
            LoadAll();
        }

        private void Load(string filename, ObservableCollection<string> target, string[]? defaultValues = null)
        {
            var path = Path.Combine(folder, filename);
            if (File.Exists(path))
            {
                // Si on charge des entreprises, on récupère juste les noms
                if (filename == "entreprises.json")
                {
                    var entreprises = JsonSerializer.Deserialize<Entreprise[]>(File.ReadAllText(path));
                    target.Clear();
                    if (entreprises != null)
                    {
                        foreach (var entreprise in entreprises)
                            target.Add(entreprise.Nom);
                    }
                }
                else
                {
                    var items = JsonSerializer.Deserialize<string[]>(File.ReadAllText(path));
                    target.Clear();
                    if (items != null)
                    {
                        foreach (var item in items)
                            target.Add(item);
                    }
                }
            }
            else if (defaultValues != null)
            {
                target.Clear();
                foreach (var item in defaultValues)
                    target.Add(item);

                Save(filename, target);
            }
        }


        private void Save(string filename, ObservableCollection<string> source)
        {
            var path = Path.Combine(folder, filename);
            File.WriteAllText(path, JsonSerializer.Serialize(source));
        }

        public void LoadAll()
        {
            Load("marquesVehicule.json", MarquesVehicule, new[] { "Seat", "Renault", "Peugeot", "Dacia", "Citroën", "Opel", "Alfa Romeo", "Škoda", "Chevrolet", "Porsche", "Honda", "Subaru", "Mazda", "Mitsubishi", "Lexus", "Toyota", "BMW", "Volkswagen", "Suzuki", "Mercedes-Benz", "Saab", "Audi", "Kia", "Land Rover", "Dodge", "Chrysler", "Ford", "Hummer", "Hyundai", "Infiniti", "Jaguar", "Jeep", "Nissan", "Volvo", "Daewoo", "Fiat", "MINI", "Rover", "Smart" });
            Load("marquesPneu.json", MarquesPneumatique, new[] { "Michelin", "Goodyear", "Bridgestone", "Continental" });
            Load("entreprises.json", EntreprisesMaintenance);
            Load("typesDepense.json", TypesDepense, new[] { "Pneumatique", "Réparation", "Vidange", "Freins" });
        }

        public void SaveAll()
        {
            Save("marquesVehicule.json", MarquesVehicule);
            Save("marquesPneu.json", MarquesPneumatique);
            Save("entreprises.json", EntreprisesMaintenance);
            Save("typesDepense.json", TypesDepense);
        }
    }
}
