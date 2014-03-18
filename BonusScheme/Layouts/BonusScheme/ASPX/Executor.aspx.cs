using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.Script.Serialization;
using BonusScheme.Repositories;
using BonusScheme.Commands;
using BonusScheme.RuleEngine;

namespace BonusScheme.Layouts.BonusScheme.ASPX
{
    public partial class Executor : UnsecuredLayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            JavaScriptSerializer jSer = new JavaScriptSerializer();
            var resultValue = new ResultValue() { Succeeded = true };
            try
            {
                bool isPOSTRequest = this.Request.Form.Count > 0;

                var projectAsString = this.Request.Form["project"];
                var project = jSer.Deserialize<Project>(projectAsString);

                IRuleCommand command;
                switch (project.RuleEngine)
                {
                    case Types.SRE:
                        {
                            command = new NotImplementedCommand();
                            break;
                        }
                    case Types.NxBre:
                        {
                            command = new NxBRECommand(project, SPContext.Current.Web);
                            break;
                        }
                    default:
                        {
                            command = new NotImplementedCommand();
                            break;
                        }
                }

                resultValue = command.Process();

            }
            catch (Exception ex)
            {
                resultValue.Succeeded = false;
                resultValue.Message = ex.Message;
            }
            finally
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.Write(jSer.Serialize(resultValue));
                Response.End();
            }
        }
    }
}
