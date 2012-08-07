using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jenzabar.Portal.Framework.Web.UI;
using Jenzabar.Portal.Framework.Security.Authorization;
using Jenzabar.Portal.Framework.Facade;

namespace CUS.ICS.ThreeWeekCheck
{
    [PortletOperation("CANACCESS","Can Access Portlet","Whether a user can Access the Portlet",PortletOperationScope.Global)]
    [PortletOperation("CANADMIN", "Can Administer Portlet", "Whether a user can Administrate the Portlet", PortletOperationScope.Global)]
    
    public class ThreeWeekCheck : SecuredPortletBase
    {
      public IPortalUserFacade MyPortalUserFacade { get; set; }

      public ThreeWeekCheck(IPortalUserFacade facade)
      {
            MyPortalUserFacade = facade;
      }
        /// <summary>
        /// Determines which screen/view should be shown based upon 
        /// the string in CurrentPortletScreenName
        /// </summary>
        /// <returns>The View to Show</returns>
        protected override PortletViewBase GetCurrentScreen()
        {
            PortletViewBase screen = null;

            try
            {
                screen = this.LoadPortletView("ICS/ThreeWeekCheckPortlet/" + this.CurrentPortletScreenName.Trim() + ".ascx");
            }
            catch
            {
                screen = this.LoadPortletView("ICS/ThreeWeekCheckPortlet/Default_View.ascx");
            }

            return screen;
        }
    }
}
