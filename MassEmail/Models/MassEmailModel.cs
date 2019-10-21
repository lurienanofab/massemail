using LNF.Models;
using LNF.Models.Data;
using System.Collections.Generic;

namespace MassEmail.Models
{
    public abstract class MassEmailModel
    {
        public bool IsAdmin { get; set; }
        public string SelectedTab { get; set; }

        public IEnumerable<IPriv> Privs { get; set; }
        public IEnumerable<ICommunity> Communities { get; set; }
        public IEnumerable<ListItem> Managers { get; set; }
        public IEnumerable<ListItem> Tools { get; set; }
        public IEnumerable<IPassbackRoom> Areas { get; set; }

        public bool IsSelectedTab(string tab)
        {
            return SelectedTab == tab;
        }
    }
}