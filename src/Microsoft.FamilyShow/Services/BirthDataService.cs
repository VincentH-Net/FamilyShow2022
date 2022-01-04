using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Services;

public class BirthDataService
{
    public static void ExportBirth(PeopleCollection family, string exportPath)
    {
        var rootTag = new ExportTagPerson("naissances", ExportTagPersonType.Root);
        var naissanceTag = new ExportTagPerson("naissance", ExportTagPersonType.Root);
        rootTag.Childs.Add(naissanceTag);
        naissanceTag.Childs.Add(new ExportTagPerson("prenom", ExportTagPersonType.FirstName));
        naissanceTag.Childs.Add(new ExportTagPerson("nom", ExportTagPersonType.LastName));
        naissanceTag.Childs.Add(new ExportTagPerson("dateNaissance", ExportTagPersonType.BirthDate));
        naissanceTag.Childs.Add(new ExportTagPerson("lieuNaissance", ExportTagPersonType.BirthPlace));
        naissanceTag.Childs.Add(new ExportTagPerson("rangDansFamille", ExportTagPersonType.GenealogicalNumber));
        naissanceTag.Childs.Add(new ExportTagPerson("numero", ExportTagPersonType.OrderIntoSiblings));
        naissanceTag.Childs.Add(new ExportTagPerson("prenomPere", ExportTagPersonType.FatherFirstName));
        naissanceTag.Childs.Add(new ExportTagPerson("prenomMere", ExportTagPersonType.MotherFirstName));
        naissanceTag.Childs.Add(new ExportTagPerson("nomMere", ExportTagPersonType.MotherLastName));

        var xmlDoc = XmlWriter.Create(exportPath + "Naissance_2019b.xml");
        // on écrit la racine du fichier
        xmlDoc.WriteStartElement(rootTag.Name);
        WriteXmlNaissances(xmlDoc, naissanceTag, family.Current, "", 0, 2019, FilterBirth);
        xmlDoc.WriteEndElement();
        xmlDoc.Close();


        var rootTag2 = new ExportTagPerson("mariages", ExportTagPersonType.Root);
        var mariageTag = new ExportTagPerson("mariage", ExportTagPersonType.Root);
        rootTag2.Childs.Add(mariageTag);
        mariageTag.Childs.Add(new ExportTagPerson("prenom", ExportTagPersonType.FirstName));
        mariageTag.Childs.Add(new ExportTagPerson("nom", ExportTagPersonType.LastName));
        mariageTag.Childs.Add(new ExportTagPerson("numeroGenealogique", ExportTagPersonType.GenealogicalNumber));
        mariageTag.Childs.Add(new ExportTagPerson("numeroGenealogique", ExportTagPersonType.GenealogicalNumber));
        mariageTag.Childs.Add(new ExportTagPerson("prenomRapportee", ExportTagPersonType.MariagePartnerFirstName));
        mariageTag.Childs.Add(new ExportTagPerson("nomRapportee", ExportTagPersonType.MariagePartnerLastName));
        mariageTag.Childs.Add(new ExportTagPerson("genreRapportee", ExportTagPersonType.MariagePartnerGenre));
        mariageTag.Childs.Add(new ExportTagPerson("date", ExportTagPersonType.MariageDate));
        mariageTag.Childs.Add(new ExportTagPerson("lieu", ExportTagPersonType.MariagePlace));

        xmlDoc = XmlWriter.Create(exportPath + "Mariages_2014b.xml");
        // on écrit la racine du fichier
        xmlDoc.WriteStartElement(rootTag.Name);
        WriteXmlNaissances(xmlDoc, mariageTag, family.Current, "", 0, 2014, FilterMariage);
        xmlDoc.WriteEndElement();
        xmlDoc.Close();
    }

    private static void WriteXmlNaissances(XmlWriter xmlDoc, IExportTag rootTag, Person rootPers, string parentArbreLevelStr, int levelChild, int dateDepart, Func<Person, int, List<object>> filter)
    {
        var currentArbreLevelStr = parentArbreLevelStr;
        var currentArbreLevel = levelChild;
        if (levelChild > 0)
        {
            if (string.IsNullOrEmpty(currentArbreLevelStr))
                currentArbreLevelStr += currentArbreLevel;
            else
                currentArbreLevelStr += "." + currentArbreLevel;
        }

        var filterObj = filter(rootPers, dateDepart);

        foreach (var item in filterObj) WriteTag(xmlDoc, rootTag, rootPers, item, currentArbreLevelStr, levelChild);

        //int yearOfBirth;
        //if (int.TryParse(rootPers.YearOfBirth, out yearOfBirth))
        //{
        //    if (yearOfBirth >= dateDepart)
        //    {
        //        WriteTag(xmlDoc, rootTag, rootPers, currentArbreLevelStr, levelChild);
        //    }
        //}

        // on ajoute les enfants
        var numberOfChild = 1;
        foreach (var child in rootPers.Children.OrderBy(X => X.BirthDate)) WriteXmlNaissances(xmlDoc, rootTag, child, currentArbreLevelStr, numberOfChild++, dateDepart, filter);
    }

    private static void WriteTag(XmlWriter xmlDoc, IExportTag tag, Person p, object filterObj, string parentArbreLevelStr, int levelChild)
    {
        Console.WriteLine($"RACINE: {tag.Name}");
        //on écrit la racine
        xmlDoc.WriteStartElement(tag.Name);

        //s'il y a des enfants, il faut les écrire
        if (tag.Childs.Count > 0)
        {
            foreach (var subtag in tag.Childs) WriteTag(xmlDoc, subtag, p, filterObj, parentArbreLevelStr, levelChild);
        }
        // sinon, il faut écrire la valeur du tag courant !
        else
        {
            if (filterObj is Person)
            {
                xmlDoc.WriteString(tag.GetValue(p, parentArbreLevelStr, levelChild));
            }
            else if (filterObj is Relationship)
            {
            }
        }

        // dans tous les cas, on ferme!
        xmlDoc.WriteEndElement();
        Console.WriteLine($"RACINE END: {tag.Name}");
    }

    private static List<object> FilterBirth(Person person, int startYear)
    {
        int yearOfBirth;
        var result = new List<object>();
        if (int.TryParse(person.YearOfBirth, out yearOfBirth))
            if (yearOfBirth >= startYear)
                result.Add(person);

        return result;
    }

    private static List<object> FilterMariage(Person person, int startYear)
    {
        var result = new List<object>();
        foreach (var spouseRelationship in person.ListSpousesRelationShip)
            if (spouseRelationship.MarriageDate != null && spouseRelationship.MarriageDate?.Year >= startYear)
                result.Add(spouseRelationship);

        return result;
    }
}