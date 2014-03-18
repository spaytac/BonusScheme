using BonusScheme.RuleEngine;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BonusScheme.Helpers.Project;
using System.Xml.Serialization;

namespace BonusScheme.Repositories
{
    [Serializable]
    public class Project
    {
        public string Title { get; set; }
        public int ID { get; set; }
        public DateTime DueDate { get; set; }
        public List<string> Participants { get; set; }
        public double Earnings { get; set; }
        public double Expenses { get; set; }
        public Types RuleEngine { get; set; }
        public string RuleGroup { get; set; }
        public string BonusPerParticipant { get; set; }

        [XmlIgnore]
        public SPListItem ListItem { get; private set; }

        public Project() { }

        public Project(SPListItem ProjectItem)
        {
            if (ProjectItem != null)
            {
                this.ListItem = ProjectItem;

                this.Title = ProjectItem.Title;
                this.ID = ProjectItem.ID;

                var dueDate = DateTime.Now;
                if (DateTime.TryParse(Convert.ToString(ProjectItem[ProjectFieldIds.bspDueDate]), out dueDate))
                {
                    this.DueDate = dueDate;
                }
                else
                {
                    throw new Exception("DueDate could not be parsed");
                }

                var participants = new SPFieldUserValueCollection(ProjectItem.Web, Convert.ToString(ProjectItem[ProjectFieldIds.bspParticipants]));
                if (participants != null)
                {
                    this.Participants = participants.Select(x => x.User.Name).ToList();
                }
                else
                {
                    throw new Exception("Participants field must be filled!");
                }

                var ruleEngineField = ProjectItem.Fields.TryGetFieldByStaticName(ProjectFieldNames.bspRuleEngine) as SPFieldChoice;
                var choosenRE = 0;

                if (int.TryParse(GetValueFromMapping(ruleEngineField.Mappings, Convert.ToString(ProjectItem[ruleEngineField.Id])), out choosenRE))
                {
                    this.RuleEngine = (Types)choosenRE;
                }
                else
                {
                    throw new Exception("Could not retrieve choosen RuleEngine");
                }

                var earnings = 0.0;
                if(double.TryParse(Convert.ToString(ProjectItem[ProjectFieldIds.bspEarnings]), out earnings))
                {
                    this.Earnings = earnings;
                }
                else
                {
                    throw new Exception();
                }

                #region Optional Fields

                var expenses = 0.0;
                var expensesFieldValueAsObj = ProjectItem[ProjectFieldIds.bspExpenses];
                if (expensesFieldValueAsObj != null)
                {
                    if (!double.TryParse(Convert.ToString(expensesFieldValueAsObj), out expenses))
                    {
                        // log something if you want ...
                    }
                }
                this.Expenses = expenses;

                var ruleGroupFieldValueAsObj = ProjectItem[ProjectFieldIds.bspRuleGroup];
                if (ruleGroupFieldValueAsObj != null)
                {
                    this.RuleGroup = Convert.ToString(ruleGroupFieldValueAsObj);
                }
                #endregion
            }
            else
            {
                throw new ArgumentNullException("ProjectItem", "is null!");
            }
        }

        private string GetValueFromMapping(string mappingsXml, string fieldValue)
        {
            System.Xml.Linq.XDocument document = System.Xml.Linq.XDocument.Parse(mappingsXml);

            var curMapping = document.Descendants("MAPPING").FirstOrDefault(m => {
                if (m.Value.TrimStart().StartsWith("$"))
                {
                    m.Value = Microsoft.SharePoint.Utilities.SPUtility.GetLocalizedString(m.Value, "core", (uint)System.Threading.Thread.CurrentThread.CurrentUICulture.LCID);
                }

                return m.Value.Equals(fieldValue);
            });

            if (curMapping != null)
            {
                fieldValue = curMapping.Attribute("Value").Value;
            }

            return fieldValue;
        }
    }
}
