using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Threading.Tasks;
using global::Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace ScadaMoulage.WEB.Models
{
    public class CellDataProd
    {
        public int Counter { get; set; }
        public string CounterAff { get; set; }
        public double Trs { get; set; }
        public double Objective { get; set; }
        public bool ObjOk { get; set; }
        public string Comment { get; set; }
        public bool CommentOk { get; set; }
        public CellDataProd()
        {
        }

    }
    public class TableDataProd
    {

        public CellDataProd[,] ListDataProd { get; set; }
        public String[] DayPlusDate { get; set; }

        public string[] shiftsDay = { "Matin", "Après-Midi", "Nuit", "Total" };

    //    public TableDataProd(List<DataProdModel> listDataProd, DateTime dateStart)
    //    {
    //        string[] daysOfWeek = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche", "Total" };
    //        string[] shiftsDay = { "Matin", "Après-Midi", "Nuit", "Total" };

    //        DayPlusDate = new string[daysOfWeek.Length - 1];

    //        for (int i = 0; i < daysOfWeek.Length - 1; i++) // Pour 7 jours semaine
    //        {
    //            DayPlusDate[i] = $"{daysOfWeek[i]} - {dateStart.AddDays(i).ToString("d")}"; // Tableau Jour + Date 
    //        }

    //        ListDataProd = new CellDataProd[daysOfWeek.Length, shiftsDay.Length];
    //        //Boucle pour les cases
    //        foreach (DataProdModel tDataProd in listDataProd)
    //        {

    //            //int dayOfWeek = (int)tDataProd.DateProd.DayOfWeek - 1;
    //            int dayOfWeek = (tDataProd.DateProd.DayOfWeek != DayOfWeek.Sunday) ? (int)tDataProd.DateProd.DayOfWeek - 1 : 6;
    //            int numDayShift = tDataProd.NumDayShift - 1;

    //            //Case
    //            if (ListDataProd[dayOfWeek, numDayShift] == null) // si objet pas existant
    //            {
    //                ListDataProd[dayOfWeek, numDayShift] = new CellDataProd();
    //            }
    //            ListDataProd[dayOfWeek, numDayShift].Counter = tDataProd.Counter;
    //            ListDataProd[dayOfWeek, numDayShift].Trs = (double)tDataProd.TRS;

    //            ListDataProd[dayOfWeek, numDayShift].ObjOk = tDataProd.ObjOk;
    //            ListDataProd[dayOfWeek, numDayShift].Objective = (double)tDataProd.Objective;

    //            ListDataProd[dayOfWeek, numDayShift].Comment = tDataProd.Comment;
    //        }

    //        int nbColRecord = 0;
    //        double trsCalc = 0;
    //        double obj = 0;

    //        //Calcul totaux Col
    //        for (int i = 0; i < daysOfWeek.Length - 1; i++)
    //        {
    //            nbColRecord = 0;
    //            trsCalc = 0;
    //            obj = 0;
    //            for (int y = 0; y < shiftsDay.Length - 1; y++)
    //            {
    //                if (ListDataProd[i, y] != null)
    //                {
    //                    if (ListDataProd[i, shiftsDay.Length - 1] == null) // si objet non existant
    //                    {
    //                        ListDataProd[i, shiftsDay.Length - 1] = new CellDataProd();
    //                    }

    //                    trsCalc += ListDataProd[i, y].Trs;
    //                    obj += ListDataProd[i, y].Objective;
    //                    ListDataProd[i, shiftsDay.Length - 1].Counter += ListDataProd[i, y].Counter;
    //                    nbColRecord += 1;
    //                }
    //            }

    //            if (ListDataProd[i, shiftsDay.Length - 1] != null)// si objet non existant
    //            {
    //                ListDataProd[i, shiftsDay.Length - 1].Objective = obj / nbColRecord;
    //                ListDataProd[i, shiftsDay.Length - 1].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListDataProd[i, shiftsDay.Length - 1].ObjOk = (ListDataProd[i, shiftsDay.Length - 1].Trs >= ListDataProd[i, shiftsDay.Length - 1].Objective);
    //            }

    //        }

    //        //Calcul totaux ligne
    //        for (int y = 0; y < shiftsDay.Length - 1; y++)
    //        {
    //            nbColRecord = 0;
    //            trsCalc = 0;
    //            obj = 0;
    //            for (int i = 0; i < daysOfWeek.Length - 2; i++)
    //            {
    //                if (ListDataProd[i, y] != null)
    //                {
    //                    if (ListDataProd[daysOfWeek.Length - 1, y] == null) // si objet non existant
    //                    {
    //                        ListDataProd[daysOfWeek.Length - 1, y] = new CellDataProd();
    //                    }

    //                    trsCalc += ListDataProd[i, y].Trs;
    //                    obj += ListDataProd[i, y].Objective;
    //                    ListDataProd[daysOfWeek.Length - 1, y].Counter += ListDataProd[i, y].Counter;
    //                    nbColRecord += 1;
    //                }
    //            }

    //            if (ListDataProd[daysOfWeek.Length - 1, y] != null) // si objet non existant
    //            {
    //                ListDataProd[daysOfWeek.Length - 1, y].Objective = obj / nbColRecord;
    //                ListDataProd[daysOfWeek.Length - 1, y].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListDataProd[daysOfWeek.Length - 1, y].ObjOk = (ListDataProd[daysOfWeek.Length - 1, y].Trs >= ListDataProd[daysOfWeek.Length - 1, y].Objective);
    //            }

    //        }

    //        nbColRecord = 0;
    //        trsCalc = 0;
    //        obj = 0;
    //        //Calcul totaux semaine
    //        for (int y = 0; y < shiftsDay.Length - 1; y++)
    //        {
    //            if (ListDataProd[daysOfWeek.Length - 1, y] != null)
    //            {
    //                if (ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1] == null) // si objet non existant
    //                {
    //                    ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1] = new CellDataProd();
    //                }

    //                trsCalc += ListDataProd[daysOfWeek.Length - 1, y].Trs;
    //                obj += ListDataProd[daysOfWeek.Length - 1, y].Objective;
    //                ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1].Counter += ListDataProd[daysOfWeek.Length - 1, y].Counter;
    //                nbColRecord += 1;
    //            }
    //            if (ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1] != null) // si objet non existant
    //            {
    //                ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1].Objective = obj / nbColRecord;
    //                ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1].ObjOk = (ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1].Trs >= ListDataProd[daysOfWeek.Length - 1, shiftsDay.Length - 1].Objective);
    //            }
    //        }

    //        //Affichage avec séparateur
    //        foreach (var item in ListDataProd)
    //        {
    //            if (item != null) item.CounterAff = item.Counter.ToString("# ### ### ##0");
    //        }
    //    }
    //}

    //public class TableWeekPress
    //{
    //    public CellDataProd[,] ListWeekPressDataProd { get; set; }

    //    public String[] DayPlusDate { get; set; }
    //    public string[] shiftsDay = { "Matin", "Après-Midi", "Nuit", "Total" };
    //    public List<string> allPress { get; set; }


    //    public TableWeekPress(List<DataProd> listDayPressDataProd, List<PressModel> listPress, DateTime dateStart)
    //    {

    //        List<int> listIndexPress = new List<int>();
    //        string[] daysOfWeek = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche", "Total" };

    //        allPress = new List<string>();


    //        DayPlusDate = new string[daysOfWeek.Length - 1];

    //        for (int i = 0; i < daysOfWeek.Length - 1; i++) // Pour 7 jours semaine
    //        {
    //            DayPlusDate[i] = $"{daysOfWeek[i]} - {dateStart.AddDays(i).ToString("d")}"; // Tableau Jour + Date 
    //        }

    //        List<Press> listPressToData = new List<Press>();
    //        foreach (DataProd tDataProd in listDayPressDataProd.OrderBy(d => d.Press.Number))
    //        {
    //            if (!allPress.Contains(tDataProd.Press.Number.ToString()))
    //            {
    //                listPressToData.Add(tDataProd.Press);
    //                allPress.Add(tDataProd.Press.Number.ToString()); // ajout de la presse
    //            }
    //        }
    //        allPress.Add("Total"); // ajout du total (manuel)


    //        ListWeekPressDataProd = new CellDataProd[daysOfWeek.Length, allPress.Count];

    //        int nbColRecord = 0;
    //        double trsCalc = 0;
    //        double obj = 0;

    //        Dictionary<int, int> frequencyShifts = new Dictionary<int, int>();

    //        DateTime checkIfDateChange = dateStart;

    //        // ---------------------------------------------------------- Calcul pour une cellule ------------------------------------------------------------//

    //        foreach (Press press in listPressToData) //Pour toutes les presses trouvées
    //        {
    //            int numPress = listPressToData.IndexOf(press); // numPress = sa position dans la liste : 112= [0] , 208 =[1] ...

    //            for (int i = 0; i < daysOfWeek.Length - 1; i++) // Pour tous les jours de la semaine
    //            {
    //                int dayOfData = (i != 6) ? i + 1 : 0; // Transformation pour vérification jour

    //                List<DataProd> listDataSort = listDayPressDataProd.Where(l => l.Press.PressID == press.PressID)
    //                                                    .Where(l => ((int)l.DateProd.DayOfWeek) == dayOfData)
    //                                                    .ToList();

    //                //Récupération du nombre de shift
    //                int nbShifts = listDataSort.Count();

    //                if (nbShifts > 0) // Si au moins 1 Shift
    //                {
    //                    //Récupération du compteur de la journée
    //                    int counter = listDataSort.Sum(l => l.Counter);

    //                    //Récupération du TRS
    //                    trsCalc = (double)listDataSort.Sum(l => l.TRS);

    //                    //Récupération de l'objectif
    //                    obj = (double)listDataSort.Sum(l => l.Objective);

    //                    //Case
    //                    if (ListWeekPressDataProd[i, numPress] == null) // si case pas existant
    //                    {
    //                        ListWeekPressDataProd[i, numPress] = new CellDataProd();
    //                    }

    //                    if (listDataSort.Any(l => l.Comment != null && l.Comment != ""))
    //                    {
    //                        ListWeekPressDataProd[i, numPress].CommentOk = true;

    //                        foreach (DataProd item in listDataSort.OrderBy(l => l.NumDayShift))
    //                        {
    //                            if (item.Comment != null && item.Comment != "")
    //                            {
    //                                ListWeekPressDataProd[i, numPress].Comment += $"Equipe {shiftsDay[item.NumDayShift - 1]} : {item.Comment}{Environment.NewLine}";
    //                            }
    //                        }
    //                    }

    //                    //Calcul pour affichage
    //                    ListWeekPressDataProd[i, numPress].Trs = Math.Round(trsCalc / nbShifts, 2); // trsCalc
    //                    ListWeekPressDataProd[i, numPress].Objective += obj / nbShifts; // obj
    //                    ListWeekPressDataProd[i, numPress].Counter = counter; //Counter
    //                    ListWeekPressDataProd[i, numPress].ObjOk = (ListWeekPressDataProd[i, numPress].Trs >= ListWeekPressDataProd[i, numPress].Objective); // Bool ObjOK
    //                }
    //            }
    //        }

    //        // ------------------------------------------------- Calcul totaux Col --------------------------------------------------------- //

    //        for (int i = 0; i < daysOfWeek.Length - 1; i++)
    //        {
    //            nbColRecord = 0;
    //            trsCalc = 0;
    //            obj = 0;
    //            for (int y = 0; y < allPress.Count - 1; y++)
    //            {
    //                if (ListWeekPressDataProd[i, y] != null)
    //                {
    //                    if (ListWeekPressDataProd[i, allPress.Count - 1] == null) // si objet non existant
    //                    {
    //                        ListWeekPressDataProd[i, allPress.Count - 1] = new CellDataProd();
    //                    }

    //                    trsCalc += ListWeekPressDataProd[i, y].Trs;
    //                    obj += ListWeekPressDataProd[i, y].Objective;
    //                    ListWeekPressDataProd[i, allPress.Count - 1].Counter += ListWeekPressDataProd[i, y].Counter;
    //                    nbColRecord += 1;
    //                }
    //            }

    //            if (ListWeekPressDataProd[i, allPress.Count - 1] != null)// si objet non existant
    //            {
    //                ListWeekPressDataProd[i, allPress.Count - 1].Objective = obj / nbColRecord;
    //                ListWeekPressDataProd[i, allPress.Count - 1].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListWeekPressDataProd[i, allPress.Count - 1].ObjOk = (ListWeekPressDataProd[i, allPress.Count - 1].Trs >= ListWeekPressDataProd[i, allPress.Count - 1].Objective);
    //            }

    //        }


    //        // ----------------------------------------------- Calcul totaux ligne ----------------------------------------------------------------//

    //        for (int y = 0; y < allPress.Count - 1; y++)
    //        {
    //            nbColRecord = 0;
    //            trsCalc = 0;
    //            obj = 0;
    //            for (int i = 0; i < daysOfWeek.Length - 2; i++)
    //            {
    //                if (ListWeekPressDataProd[i, y] != null)
    //                {
    //                    if (ListWeekPressDataProd[daysOfWeek.Length - 1, y] == null) // si objet non existant
    //                    {
    //                        ListWeekPressDataProd[daysOfWeek.Length - 1, y] = new CellDataProd();
    //                    }

    //                    trsCalc += ListWeekPressDataProd[i, y].Trs;
    //                    obj += ListWeekPressDataProd[i, y].Objective;
    //                    ListWeekPressDataProd[daysOfWeek.Length - 1, y].Counter += ListWeekPressDataProd[i, y].Counter;
    //                    nbColRecord += 1;
    //                }
    //            }

    //            if (ListWeekPressDataProd[daysOfWeek.Length - 1, y] != null) // si objet non existant
    //            {
    //                ListWeekPressDataProd[daysOfWeek.Length - 1, y].Objective = obj / nbColRecord;
    //                ListWeekPressDataProd[daysOfWeek.Length - 1, y].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListWeekPressDataProd[daysOfWeek.Length - 1, y].ObjOk = (ListWeekPressDataProd[daysOfWeek.Length - 1, y].Trs >= ListWeekPressDataProd[daysOfWeek.Length - 1, y].Objective);
    //            }

    //        }

    //        // --------------------------------------------------------------------------------------------------------------------------------------------------/: 

    //        nbColRecord = 0;
    //        trsCalc = 0;
    //        obj = 0;
    //        //Calcul totaux semaine
    //        for (int y = 0; y < allPress.Count - 1; y++)
    //        {
    //            if (ListWeekPressDataProd[daysOfWeek.Length - 1, y] != null)
    //            {
    //                if (ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1] == null) // si objet non existant
    //                {
    //                    ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1] = new CellDataProd();
    //                }

    //                trsCalc += ListWeekPressDataProd[daysOfWeek.Length - 1, y].Trs;
    //                obj += ListWeekPressDataProd[daysOfWeek.Length - 1, y].Objective;
    //                ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1].Counter += ListWeekPressDataProd[daysOfWeek.Length - 1, y].Counter;
    //                nbColRecord += 1;
    //            }
    //            if (ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1] != null) // si objet non existant
    //            {
    //                ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1].Objective = obj / nbColRecord;
    //                ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1].ObjOk = (ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1].Trs >= ListWeekPressDataProd[daysOfWeek.Length - 1, allPress.Count - 1].Objective);
    //            }

    //        }
    //        //Affichage avec séparateur
    //        foreach (var item in ListWeekPressDataProd)
    //        {
    //            if (item != null) item.CounterAff = item.Counter.ToString("# ### ### ##0");
    //        }

    //    }

    //}

    //public class TableDayPress
    //{
    //    public CellDataProd[,] ListDayPressDataProd { get; set; }

    //    public List<string> allPress { get; set; }
    //    public string[] shiftsDay = { "Matin", "Après-Midi", "Nuit", "Total" };

    //    public TableDayPress(List<DataProd> listDayPressDataProd, List<PressModel> listPress)
    //    {
    //        List<int> listIndexPress = new List<int>();
    //        allPress = new List<string>();
    //        string[] shiftsDay = { "Matin", "Après-Midi", "Nuit", "Total" };

    //        //foreach (DataProd tDataProd in listDayPressDataProd)
    //        //{
    //        //    PressModel press = listPress.FirstOrDefault(u => u.PressID == tDataProd.FKPressID); // recupere toutes les presses avec des résultats 
    //        //    if (press != null && !(allPress.Contains(press.Number.ToString())))
    //        //    {
    //        //        allPress.Add(press.Number.ToString()); // ajout de la presse
    //        //    }
    //        //}
    //        //allPress.Add("Total"); // ajout du total (manuel)

    //        List<Press> listPressToData = new List<Press>();
    //        foreach (DataProd tDataProd in listDayPressDataProd.OrderBy(d => d.Press.Number))
    //        {
    //            if (!allPress.Contains(tDataProd.Press.Number.ToString()))
    //            {
    //                listPressToData.Add(tDataProd.Press);
    //                allPress.Add(tDataProd.Press.Number.ToString()); // ajout de la presse
    //            }
    //        }
    //        allPress.Add("Total"); // ajout du total (manuel)

    //        ListDayPressDataProd = new CellDataProd[allPress.Count, shiftsDay.Length];

    //        foreach (DataProd tDataProd in listDayPressDataProd)
    //        {
    //            int numPress = listPressToData.IndexOf(tDataProd.Press); // numPress = sa position dans la liste : 112= [0] , 208 =[1] ...
    //            int numDayShift = tDataProd.NumDayShift - 1;

    //            //Case
    //            if (ListDayPressDataProd[numPress, numDayShift] == null) // si case pas existant
    //            {
    //                ListDayPressDataProd[numPress, numDayShift] = new CellDataProd();
    //            }

    //            ListDayPressDataProd[numPress, numDayShift].Counter = tDataProd.Counter;
    //            ListDayPressDataProd[numPress, numDayShift].Trs = (double)tDataProd.TRS;

    //            ListDayPressDataProd[numPress, numDayShift].ObjOk = tDataProd.ObjOk;
    //            ListDayPressDataProd[numPress, numDayShift].Objective = (double)tDataProd.Objective;

    //            ListDayPressDataProd[numPress, numDayShift].Comment = tDataProd.Comment;

    //        }

    //        int nbColRecord = 0;
    //        double trsCalc = 0;
    //        double obj = 0;

    //        //Calcul totaux Col
    //        for (int i = 0; i < allPress.Count - 1; i++)
    //        {
    //            nbColRecord = 0;
    //            trsCalc = 0;
    //            obj = 0;

    //            for (int y = 0; y < shiftsDay.Length - 1; y++)
    //            {
    //                if (ListDayPressDataProd[i, y] != null)
    //                {
    //                    if (ListDayPressDataProd[i, shiftsDay.Length - 1] == null) // si objet non existant
    //                    {
    //                        ListDayPressDataProd[i, shiftsDay.Length - 1] = new CellDataProd();
    //                    }

    //                    trsCalc += ListDayPressDataProd[i, y].Trs;
    //                    obj += ListDayPressDataProd[i, y].Objective;
    //                    ListDayPressDataProd[i, shiftsDay.Length - 1].Counter += ListDayPressDataProd[i, y].Counter;
    //                    nbColRecord += 1;
    //                }
    //            }

    //            if (ListDayPressDataProd[i, shiftsDay.Length - 1] != null)// si objet non existant
    //            {
    //                ListDayPressDataProd[i, shiftsDay.Length - 1].Objective = obj / nbColRecord;
    //                ListDayPressDataProd[i, shiftsDay.Length - 1].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListDayPressDataProd[i, shiftsDay.Length - 1].ObjOk = (ListDayPressDataProd[i, shiftsDay.Length - 1].Trs >= ListDayPressDataProd[i, shiftsDay.Length - 1].Objective);
    //            }

    //        }

    //        // --------------------------------------/: 

    //        //Calcul totaux ligne
    //        for (int y = 0; y < shiftsDay.Length - 1; y++)
    //        {
    //            nbColRecord = 0;
    //            trsCalc = 0;
    //            obj = 0;

    //            for (int i = 0; i < allPress.Count - 1; i++)
    //            {
    //                if (ListDayPressDataProd[i, y] != null)
    //                {
    //                    if (ListDayPressDataProd[allPress.Count - 1, y] == null) // si objet non existant
    //                    {
    //                        ListDayPressDataProd[allPress.Count - 1, y] = new CellDataProd();
    //                    }

    //                    trsCalc += ListDayPressDataProd[i, y].Trs;
    //                    obj += ListDayPressDataProd[i, y].Objective;
    //                    ListDayPressDataProd[allPress.Count - 1, y].Counter += ListDayPressDataProd[i, y].Counter;
    //                    nbColRecord += 1;
    //                }
    //            }

    //            if (ListDayPressDataProd[allPress.Count - 1, y] != null) // si objet non existant
    //            {
    //                ListDayPressDataProd[allPress.Count - 1, y].Objective = obj / nbColRecord;
    //                ListDayPressDataProd[allPress.Count - 1, y].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListDayPressDataProd[allPress.Count - 1, y].ObjOk = (ListDayPressDataProd[allPress.Count - 1, y].Trs >= ListDayPressDataProd[allPress.Count - 1, y].Objective);
    //            }

    //        }

    //        // -----------------------------------// 

    //        nbColRecord = 0;
    //        trsCalc = 0;
    //        obj = 0;
    //        //Calcul totaux semaine
    //        for (int y = 0; y < shiftsDay.Length - 1; y++)
    //        {
    //            if (ListDayPressDataProd[allPress.Count - 1, y] != null)
    //            {
    //                if (ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1] == null) // si objet non existant
    //                {
    //                    ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1] = new CellDataProd();
    //                }

    //                trsCalc += ListDayPressDataProd[allPress.Count - 1, y].Trs;
    //                obj += ListDayPressDataProd[allPress.Count - 1, y].Objective;
    //                ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1].Counter += ListDayPressDataProd[allPress.Count - 1, y].Counter;
    //                nbColRecord += 1;
    //            }
    //            if (ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1] != null) // si objet non existant
    //            {
    //                ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1].Objective = obj / nbColRecord;
    //                ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1].Trs = Math.Round(trsCalc / nbColRecord, 2);
    //                ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1].ObjOk = (ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1].Trs >= ListDayPressDataProd[allPress.Count - 1, shiftsDay.Length - 1].Objective);
    //            }

    //        }
    //        //Affichage avec séparateur
    //        foreach (var item in ListDayPressDataProd)
    //        {
    //            if (item != null) item.CounterAff = item.Counter.ToString("# ### ### ##0");
    //        }
    //    }
    }
}