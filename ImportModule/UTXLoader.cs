// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of Assess Extended.

// Assess Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// Assess Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Assess Extended.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    public class UTXLoader : DataLoader
    {
        public UTXLoader() : base()
        {
            _criterionEnumsList = new List<CriterionEnum>();
        }

        private List<CriterionEnum> _criterionEnumsList;

        private void checkEnumValue(string criterionName, string enumID, string enumValue)
        {
            if (enumValue.Equals(""))
                throw new ImproperFileStructureException("Enum type value can not be empty. Criterion " + criterionName + ", EnumID: " +
                                                         enumID + ".");

            double output = 0;
            if (!double.TryParse(enumValue, NumberStyles.Any, CultureInfo.InvariantCulture, out output))
                throw new ImproperFileStructureException("Improper value format '" + enumValue +
                                                         "'. Value has to be floating point. Criterion " + criterionName + ", EnumID: " +
                                                         enumID + ".");
        }

        private Criterion validateCriterion(Criterion criterion, Dictionary<string, string> enumNames,
            Dictionary<string, double> enumValues)
        {
            if (criterion.IsEnum)
            {
                var criterionEnum = new CriterionEnum(criterion.ID);

                foreach (var entry in enumNames)
                {
                    var enumID = entry.Key;
                    var enumName = entry.Value;
                    var enumValue = enumValues[enumID];
                    criterionEnum.EnumDictionary.Add(enumName, enumValue);
                }

                _criterionEnumsList.Add(criterionEnum);
            }

            if (criterion.Name == null || criterion.Name.Equals(""))
                criterion.Name = criterion.ID;

            if (criterion.CriterionDirection == null || criterion.CriterionDirection.Equals(""))
                throw new ImproperFileStructureException("There was no criterion scale ('Cost' or 'Gain') provided for criterion " +
                                                         criterion.Name);

            return criterion;
        }

        protected override void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);
            ValidateFileExtension(filePath, ".utx");

            //load XML
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            //var nameAttributeId = "";
            var descriptionAttributeName = "";

            var attributes = xmlDoc.GetElementsByTagName("ATTRIBUTES");

            var objects = xmlDoc.GetElementsByTagName("OBJECTS");

            // iterate on its nodes
            foreach (XmlNode attribute in attributes[0].ChildNodes)
            {
                var criterion = new Criterion();
                // for UTX ID and Name are the same value
                criterion.Name = criterion.ID = checkCriteriaIdsUniqueness(attribute.Attributes["AttrID"].Value);
                var saveCriterion = true;
                var enumIdsNamesDictionary = new Dictionary<string, string>();
                var enumIdsValuesDictionary = new Dictionary<string, double>();

                foreach (XmlNode attributePart in attribute)
                {
                    var value = attributePart.Attributes["Value"].Value;

                    switch (attributePart.Name)
                    {
                        case "TYPE":
                            if (value == "Enum")
                            {
                                criterion.IsEnum = true;
                                foreach (XmlNode enumName in attributePart)
                                    enumIdsNamesDictionary.Add(enumName.Attributes["EnumID"].Value,
                                        enumName.Attributes["Value"].Value);
                            }

                            break;
                        case "DESCRIPTION":
                            criterion.Description = value;
                            break;
                        case "CRITERION":
                            // if value is enum type
                            if (value == "Rank")
                            {
                                foreach (XmlNode enumValue in attributePart)
                                {
                                    checkEnumValue(criterion.Name, enumValue.Attributes["EnumID"].Value,
                                        enumValue.Attributes["Value"].Value);
                                    enumIdsValuesDictionary.Add(enumValue.Attributes["EnumID"].Value,
                                        double.Parse(enumValue.Attributes["Value"].Value, CultureInfo.InvariantCulture));
                                }

                                criterion.CriterionDirection = "Cost";
                            }
                            else
                            {
                                // "Cost" or "Gain"
                                criterion.CriterionDirection = value == "Cost" ? "Cost" : "Gain";
                            }

                            break;
                        case "ROLE":
                            if (value == "Description")
                            {
                                saveCriterion = false;
                                descriptionAttributeName = criterion.Name;
                            }
                            else
                            {
                                saveCriterion = true;
                            }

                            break;
                        case "SEGMENTS":
                            break;
                        default:
                            throw new Exception("Attribute " + attributePart.Name + " is not compatible with application.");
                    }
                }

                if (saveCriterion)
                {
                    criterion = validateCriterion(criterion, enumIdsNamesDictionary, enumIdsValuesDictionary);
                    criterionList.Add(criterion);
                }
            }


            foreach (XmlNode instance in objects[0].ChildNodes)
            {
                // check if number of all child nodes (except INFO node - which isn't about criterion) is equal to criterionList.Count
                var alternativesCountValidation = 0;
                var alternative = new Alternative {Name = checkAlternativesNamesUniqueness(instance.Attributes["ObjID"].Value)};
                alternative.ID = alternative.Name;

                var criteriaValuesList = new ObservableCollection<CriterionValue>();

                foreach (XmlNode instancePart in instance)
                    //beyond VALUE there exist attributes "RANK" which we do not take into account
                    if (instancePart.Name.Equals("VALUE"))
                    {
                        var value = instancePart.Attributes["Value"].Value;
                        var attributeName = instancePart.Attributes["AttrID"].Value;

                        if (attributeName != descriptionAttributeName)
                        {
                            alternativesCountValidation++;
                            var criterion = criterionList.Find(element => element.Name == attributeName);

                            if (criterion == null)
                                throw new ImproperFileStructureException(
                                    "Error while processing alternative " + alternative.Name + ": criterion named " +
                                    attributeName + " does not exist.");


                            if (criterion.IsEnum)
                            {
                                var enumValue = _criterionEnumsList.Find(o => o.CriterionId == criterion.ID).EnumDictionary[value];
                                criteriaValuesList.Add(new CriterionValue(criterion.Name, enumValue));
                            }
                            else
                            {
                                checkIfValueIsValid(value, criterion.Name, alternative.Name);
                                criteriaValuesList.Add(new CriterionValue(criterion.Name,
                                    double.Parse(value, CultureInfo.InvariantCulture)));
                            }
                        }
                    }

                if (alternativesCountValidation != criterionList.Count)
                    throw new ImproperFileStructureException("Error while processing alternative " + alternative.Name +
                                                             ": there are provided " + alternativesCountValidation +
                                                             " criteria values and required are " + criterionList.Count + ".");

                alternative.CriteriaValuesList = criteriaValuesList;
                alternativeList.Add(alternative);
            }
        }
    }
}