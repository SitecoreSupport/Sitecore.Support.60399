using Sitecore.Data;
using Sitecore.ExperienceEditor.Speak.Server.Contexts;
using Sitecore.ExperienceEditor.Speak.Server.Requests;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.Links;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.Requests.Treecrumb
{
  public class ContextSiteResolver : PipelineProcessorRequest<ItemContext>
  {
    public override PipelineProcessorResponseValue ProcessRequest()
    {
      PipelineProcessorResponseValue responseValue = new PipelineProcessorResponseValue();
      ID id;

      if (!ID.TryParse(base.RequestContext.Argument, out id))
      {
        Sitecore.Diagnostics.Log.Error($"[Sitecore.Support.60399]: item ID from ExperienceEditor.js is invalid.", this);
      }

      try
      {
        var siteContext = LinkManager.GetPreviewSiteContext(
          (Sitecore.Context.Database ?? Sitecore.Context.ContentDatabase).GetItem(id));
        responseValue.Value = siteContext.Name;

      }
      catch (Exception ex)
      {
        Sitecore.Diagnostics.Log.Error($"[Sitecore.Support.60399]: {ex.Message}.", this);
      }
      
      return responseValue;
    }
  }
}