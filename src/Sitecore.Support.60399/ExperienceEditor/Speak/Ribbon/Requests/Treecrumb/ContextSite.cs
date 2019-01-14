using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.ExperienceEditor.Speak.Server.Contexts;
using Sitecore.ExperienceEditor.Speak.Server.Requests;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.Globalization;
using Sitecore.Sites;
using Sitecore.Web;
using System;
using System.Linq;
using System.Web;

namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.Requests.Treecrumb
{
  public class ContextSite : PipelineProcessorRequest<ItemContext>
  {
    public override PipelineProcessorResponseValue ProcessRequest()
    {
      PipelineProcessorResponseValue result = new PipelineProcessorResponseValue();

			if (RequestContext != null && !string.IsNullOrWhiteSpace(RequestContext.Argument))
			{
				if (!string.IsNullOrEmpty(RequestContext.Argument))
				{
					if (ID.IsID(RequestContext.Argument))
					{
						ID itemId = ID.Parse(RequestContext.Argument);
						var db = Context.Database ?? Context.ContentDatabase;

						if (db != null)
						{
							var item = db.GetItem(itemId);

							if (item != null)
							{
								var site = GetSiteContext(item.Paths.FullPath);
								result.Value = site?.Name;
							}
						}
					}
				}
			}

			return result;
    }

    protected virtual SiteContext GetSiteContext(string fullPath)
    {
      fullPath = fullPath.ToLowerInvariant();
      foreach (var site in SiteManager.GetSites())
      {
        var siteInfo = new SiteInfo(site.Properties);
        if (Matches(siteInfo, fullPath))
        {
          return new SiteContext(siteInfo);
        }
      }
      return null;
    }

    protected virtual bool Matches(SiteInfo siteInfo, string itemPath)
    {
      string rootPath = siteInfo.RootPath;
      rootPath = string.IsNullOrWhiteSpace(rootPath) ? "/" : "/" + rootPath + "/";
      string startItem = siteInfo.StartItem ?? string.Empty;
      string sItemPath = rootPath + startItem;
      while (sItemPath.Contains("//"))
      {
        sItemPath = sItemPath.Replace("//", "/");
      }
      return sItemPath.StartsWith("/sitecore/content", StringComparison.InvariantCultureIgnoreCase) &&
        itemPath.StartsWith(sItemPath, StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
