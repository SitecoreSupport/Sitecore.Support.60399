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

      if (base.RequestContext != null && !string.IsNullOrWhiteSpace(base.RequestContext.Argument))
      {
        var uri = new Uri(base.RequestContext.Argument);
        if (!string.IsNullOrWhiteSpace(uri.Query))
        {
          var parts = uri.Query.Split(new char[] { '/', '&' }, StringSplitOptions.RemoveEmptyEntries);
          int scItemIndex = -1;
          for (int i = 0; i < parts.Length; i++)
          {
            if (parts[i].StartsWith("sc_itemid=", StringComparison.InvariantCultureIgnoreCase))
            {
              scItemIndex = i;
              break;
            }
          }
          if (scItemIndex >= 0)
          {
            var itemParts = parts[scItemIndex].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (itemParts.Length == 2 && !string.IsNullOrWhiteSpace(itemParts[1]))
            {
              string itemId = itemParts[1];
              itemId = HttpContext.Current.Server.UrlDecode(itemId);
              if (ID.IsID(itemId))
              {
                var db = Sitecore.Context.Database ?? Sitecore.Context.ContentDatabase;
                if (db != null)
                {
                  var item = db.GetItem(itemId);
                  if (item != null)
                  {
                    var site = GetSiteContext(item.Paths.FullPath);
                    if (site != null)
                    {
                      int scSiteIndex = -1;
                      for (int i = 0; i < parts.Length; i++)
                      {
                        if (parts[i].StartsWith("sc_site=", StringComparison.InvariantCultureIgnoreCase))
                        {
                          scSiteIndex = i;
                          break;
                        }
                      }
                      if (scSiteIndex >= 0)
                      {
                        string newUrl = base.RequestContext.Argument.Replace(parts[scSiteIndex], "sc_site=" + site.Name);
                        result.Value = newUrl;
                      }                      
                    }
                  }
                }
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
