using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MassEmail.Models
{
    public class TabItem
    {
        public string Name { get; set; }
        public string ID => $"{Name}-tab";
        public string Href { get; set; }
        public string Text { get; set; }
        public bool Selected { get; set; }
        public bool Visible { get; set; }
        public bool Toggle { get; set; }
        public string CssClass => string.Format("nav-link {0}", Selected ? "active" : string.Empty).Trim();
    }

    public static class TabHelperExtensions
    {
        public static TabItem CreateTab(this HtmlHelper<MassEmailModel> helper, string name, string text, bool visible, bool toggle, string href = null)
        {
            if (string.IsNullOrEmpty(href))
                href = $"#{name}";

            return new TabItem
            {
                Name = name,
                Text = text,
                Href = href,
                Selected = helper.ViewData.Model.IsSelectedTab(name),
                Visible = visible,
                Toggle = toggle
            };
        }

        public static string GetTabPaneCssClass(this HtmlHelper<MassEmailModel> helper, string name)
        {
            var result = "tab-pane ";
            if (helper.ViewData.Model.IsSelectedTab(name))
                result += "show active";
            return result.Trim();
        }

        public static bool IsVisible(this IEnumerable<TabItem> tabs, string name)
        {
            return tabs.Any(x => x.Visible && x.Name == name);
        }
    }
}