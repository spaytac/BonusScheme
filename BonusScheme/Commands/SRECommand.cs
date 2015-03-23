using BonusScheme.Repositories;
using BonusScheme.RuleEngine;
using BonusScheme.Helpers;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using RuleEngine;
using RuleEngine.Compiler;

namespace BonusScheme.Commands
{
    public class SRECommand : IRuleCommand
    {
        private Project Project { get; set; }
        private SPWeb Web { get; set; }

        public SRECommand(Project Project)
        {
            this.Project = Project;
        }

        public SRECommand(Project Project, SPWeb Web)
            : this(Project)
        {
            this.Web = Web;
        }

        public ResultValue Process()
        {
            var returnValue = new ResultValue() { Succeeded = true };
            
            try
            {
                if (Project == null)
                {
                    throw new ArgumentNullException("Project");
                }
                
                if(this.Web == null)
                {
                    if (Project.ListItem != null)
                    {
                        this.Web = Project.ListItem.Web;
                    }
                    else
                    {
                        throw new Exception("Project.ListItem could not be null!");
                    }
                }


                var bsFiles = this.Web.Lists.TryGetList("BSFiles");
                var fileUrl = this.Web.Url.AddUriPath(bsFiles.RootFolder.Url.AddUriPath("SRE/ProjectRuleSet.xml"));
                var ruleFile = this.Web.GetFile(fileUrl);


                var serializedObject = string.Empty;

                var xmlSer = new XmlSerializer(typeof(Project));

                using (var textWriter = new StringWriter())
                {
                    xmlSer.Serialize(textWriter, this.Project);
                    serializedObject = textWriter.ToString();
                }

                XmlDocument rules = new XmlDocument();

                using (var reader = new StreamReader(ruleFile.OpenBinaryStream()))
                {
                    rules.LoadXml(reader.ReadToEnd());
                }

                ROM romXML = Compiler.Compile(rules);

                //models
                XmlDocument model = new XmlDocument();
                model.LoadXml(serializedObject);
                romXML.AddModel("Project", model);
                romXML.AddModel("ResultProject", model);
                romXML.Evaluate();

                using (TextReader reader = new StringReader(model.InnerXml))
                {
                    this.Project = xmlSer.Deserialize(reader) as Project;
                }

                returnValue.Value = this.Project.BonusPerParticipant;
            }
            catch (Exception ex)
            {
                returnValue.Message = ex.Message;
                returnValue.Succeeded = false;
            }

            return returnValue;
        }
    }
}
