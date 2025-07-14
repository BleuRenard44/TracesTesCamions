namespace TracesTesCamions.Models
{
    public class Entreprise
    {
        public string Nom { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Adresse { get; set; }

        public override string ToString() => Nom;
    }
}
