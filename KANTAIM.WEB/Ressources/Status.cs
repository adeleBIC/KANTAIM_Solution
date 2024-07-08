namespace KANTAIM.WEB.Ressources
{
    public class Status
    {
    }
 
    public class StatusCell
    {
        public const int Undefinded = 0;
        public const int Empty = 1;
        public const int InFill = 2;
        public const int Full = 3;
        public const int Canceled = 99;
 
        public Dictionary<int, String> Status = new Dictionary<int, string>();
 
        public StatusCell()
        {
            Status.Add(Undefinded, "Indéfini");
            Status.Add(Empty, "Vide");
            Status.Add(InFill, "En remplissage");
            Status.Add(Full, "Plein");
            Status.Add(Canceled, "Annulé");
        }
    }
    public class StatusContainer
    {
        public const int Undefinded = 0;
        public const int Empty = 1;
        public const int HalfFull = 2;
        public const int Full = 3;
        public const int Canceled = 99;

        public Dictionary<int, String> Status = new Dictionary<int, string>();

        public StatusContainer()
        {
            Status.Add(Undefinded, "Indéfini");
            Status.Add(Empty, "Vide");
            Status.Add(HalfFull, "Semi-Plein");
            Status.Add(Full, "Plein");
            Status.Add(Canceled, "Annulé");
        }
    }

    public class OperationContainer
    {
        public const int Undefinded = 0;
        public const int Initisalisation = 1;
        public const int Store = 2;
        public const int Shipment = 3;
        public const int Inject = 4;
        public const int Transfer = 5;
        public const int Install = 6;
        public const int Canceled = 99;

        public Dictionary<int, String> Operations = new Dictionary<int, string>();

        public OperationContainer()
        {
            Operations.Add(Undefinded, "Indéfini");
            Operations.Add(Initisalisation, "Initialisation Machine");
            Operations.Add(Store, "Mise en Rack");
            Operations.Add(Shipment, "Sortie de Rack");
            Operations.Add(Inject, "Mise en Machine");
            Operations.Add(Transfer, "Déplacement contenaire");
            Operations.Add(Install, "Mise en contenaire");
            Operations.Add(Canceled, "Annulé");
        }
    }
}