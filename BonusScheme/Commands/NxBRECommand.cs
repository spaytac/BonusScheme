using BonusScheme.Repositories;
using BonusScheme.RuleEngine;
using BonusScheme.Helpers;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxBRE.FlowEngine;
using NxBRE.FlowEngine.IO;

namespace BonusScheme.Commands
{
    public class NxBRECommand : IRuleCommand
    {
        private Project Project { get; set; }
        private SPWeb Web { get; set; }

        public NxBRECommand(Project Project)
        {
            this.Project = Project;
        }

        public NxBRECommand(Project Project, SPWeb Web)
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
                var fileUrl = this.Web.Url.AddUriPath(bsFiles.RootFolder.Url.AddUriPath("NxBRE/ProjectRuleSet.xbre"));
                var ruleFile = this.Web.GetFile(fileUrl);

                IFlowEngine bre = new BREImpl();
                bre.Init(new XBusinessRulesStreamDriver(ruleFile.OpenBinaryStream()));
                bre.RuleContext.SetObject("Project", this.Project);
                bre.Process();

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
