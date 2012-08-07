using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Jenzabar.Portal.Framework.Web.UI;
using Jenzabar.Portal.Framework.Facade;

namespace CUS.ICS.ThreeWeekCheck
{
  public partial class Default_View : PortletViewBase
  {

    protected void Page_Load(object sender, EventArgs e)
    {
        SqlDataSource1.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["JenzabarConnectionString"].ConnectionString;

        if (Jenzabar.Portal.Framework.PortalUser.Current.HostID == null ||
            Jenzabar.Portal.Framework.PortalUser.Current.HostID.Equals(String.Empty))
        {
            ParentPortlet.ShowFeedback(FeedbackType.Error, "Error: MISSING ID#");
            btnBack.Visible = false;
            btnCancel.Visible = false;
            btnSave.Visible = false;
            lblTitleCourseName.Visible = false;
            lblTitleSemester.Visible = false;
            ddlCourse.Visible = false;
            return;
        }



        using (System.Data.SqlClient.SqlConnection sqlcon = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["JenzabarConnectionString"].ConnectionString))
        {
            sqlcon.Open();
            if (IsFirstLoad)
            {
                using (System.Data.SqlClient.SqlDataReader sqlread = new System.Data.SqlClient.SqlCommand("SELECT CRS_CDE from SECTION_MASTER where LEAD_INSTRUCTR_ID = " + Jenzabar.Portal.Framework.PortalUser.Current.HostID + " and YR_CDE = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and TRM_CDE = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and crs_cde not like '________L%'", sqlcon).ExecuteReader())
                {
                    if (sqlread.HasRows)
                    {
                        ddlCourse.Items.Add(new ListItem("Select Course ...", " "));
                    }
                    else
                    {
                        ddlCourse.Items.Add("No courses found");
                        ParentPortlet.ShowFeedback(FeedbackType.Message, "No Courses Found in which " + Jenzabar.Portal.Framework.PortalUser.Current.FirstName + " " + Jenzabar.Portal.Framework.PortalUser.Current.LastName + " was the Lead Instructor");
                        ddlCourse.Enabled = false;
                    }

                    while(sqlread.Read())
                    {
                        ddlCourse.Items.Add(sqlread["CRS_CDE"].ToString());
                    }
                }
                using (System.Data.SqlClient.SqlDataReader sqlread = new System.Data.SqlClient.SqlCommand(
                    @"SELECT 
                    (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') as 'YR_CDE', 
                    (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') as 'TRM_CDE',
                    case when SUBSTRING((select CURR_SESS from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR'),5,7) = 'FA' THEN
                    'Fall '
                    when SUBSTRING((select CURR_SESS from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR'),5,7) = 'SP' THEN
                    'Spring '
                    ELSE
                    ' '
                    END
                    + SUBSTRING((select CURR_SESS from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR'),0,5) as 'CUR_SESS'", sqlcon).ExecuteReader())
                {
                    while(sqlread.Read())
                    {
                        lblTitleSemester.Text = "<b>Semester " + sqlread["YR_CDE"] + sqlread["TRM_CDE"] + " - " + sqlread["CUR_SESS"] + "</b>";
                    }
                }
        }
    
        String sqlcmdTextHasRows = @"select distinct a.id_num,a.crs_cde,b.lead_instructr_id 
            from student_crs_hist a
            left outer join section_master b
            on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
            join student_master c
            on a.id_num = c.id_num
            join biograph_master d
            on a.id_num = d.id_num
            join stud_term_sum_div e
            on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
            left outer join sports_tracking f
            on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
            left outer join ATTRIBUTE_TRANS g
            on a.id_num = g.id_num
            where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
                 and (		c.cur_acad_probation in ('PC','PR','SU','AD') --PROBATION STUDENTS
		                or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and d.entrance_cde = 'F') --FIRST TIME FRESHMAN SPRING
		                or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'F') --FIRST TIME FRESHMAN FALL
		                or c.current_class_cde = 'FR' --FTF UNDER 27 HOURS (ALL OTHER FRESHMAN)
		                or c.CURRENT_CLASS_CDE = 'SP' -- SPECIAL NON-DEGREE STUDENTS
		                or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'T') --TRANSERS
		                or c.current_class_cde = 'DP' -- DUAL PARTIC
		                or g.attrib_cde = 'HON' --HONOR STUDENTS
		                or f.sports_cde is not null --ATHLETES
                      )
                 and LEAD_INSTRUCTR_ID = " + Jenzabar.Portal.Framework.PortalUser.Current.HostID
             + " and a.crs_cde = '" + ddlCourse.SelectedValue + "'";

            using (System.Data.SqlClient.SqlDataReader sqlread2 = new System.Data.SqlClient.SqlCommand(sqlcmdTextHasRows,sqlcon).ExecuteReader())
            {
                if (sqlread2.HasRows)
                {
                    btnSave.Visible = true;
                    btnCancel.Visible = true;
                    lblTitleCourseName.Visible = true;
                }
                else
                {
                    btnSave.Visible = false;
                    btnCancel.Visible = false;
                    lblTitleCourseName.Visible = false;
                } 
            }
        }        
    }

    protected void ddlCourse_SelectedIndexChanged(object sender, EventArgs e)
    {
            SqlDataSource1.SelectCommand =
                @"select distinct a.id_num,a.crs_cde,b.lead_instructr_id, 
   case when h.FLG_ABSENCES IS NULL THEN '0' ELSE h.FLG_ABSENCES END as 'FLG_ABSENCES',
   case when h.FLG_COUNSEL IS NULL THEN '0' ELSE h.FLG_COUNSEL END as 'FLG_COUNSEL',
   case when h.FLG_PARTIC IS NULL THEN '0' ELSE h.FLG_PARTIC END as 'FLG_PARTIC',
   case when h.FLG_TUTOR IS NULL THEN '0' ELSE h.FLG_TUTOR END as 'FLG_TUTOR',
   case when h.FLG_WORK IS NULL THEN '0' ELSE h.FLG_WORK END as 'FLG_WORK',
   case when h.COMMENT IS NULL THEN '' ELSE cast(h.COMMENT as varchar(max)) END as 'COMMENT',
   case when f.sports_cde IS NULL THEN 'Athlete: NO' ELSE 'Athlete: YES' END as 'Athlete' 
        from student_crs_hist a
        left outer join section_master b
        on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
        join student_master c
        on a.id_num = c.id_num
        join biograph_master d
        on a.id_num = d.id_num
        join stud_term_sum_div e
        on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
        left outer join sports_tracking f
        on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
        left outer join ATTRIBUTE_TRANS g
        on a.id_num = g.id_num
            left outer join TLU_THREEWEEKCHECK h
            on a.id_num = h.id_num and a.yr_cde = h.yr_cde and a.trm_cde = h.trm_cde and h.LEAD_INSTRUCTR_ID = b.lead_instructr_id and a.crs_cde = h.crs_cde
        where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
             and (		c.cur_acad_probation in ('PC','PR','SU','AD') --PROBATION STUDENTS
			        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and d.entrance_cde = 'F') --FIRST TIME FRESHMAN SPRING
			        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'F') --FIRST TIME FRESHMAN FALL
			        or c.current_class_cde = 'FR' --FTF UNDER 27 HOURS (ALL OTHER FRESHMAN)
			        or c.CURRENT_CLASS_CDE = 'SP' -- SPECIAL NON-DEGREE STUDENTS
			        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'T') --TRANSERS
			        or c.current_class_cde = 'DP' -- DUAL PARTIC
			        or g.attrib_cde = 'HON' --HONOR STUDENTS
			        or f.sports_cde is not null --ATHLETES
                  )
            and b.LEAD_INSTRUCTR_ID = " + Jenzabar.Portal.Framework.PortalUser.Current.HostID
            + " and a.crs_cde = '" + ddlCourse.SelectedValue + "'";

            DataList1.DataBind();
            Page_Load(sender, e);

            if (DataList1.Items.Count == 0)
            {
                ParentPortlet.ShowFeedback(FeedbackType.Message, "No Students in " + ddlCourse.SelectedValue + " Need a Review");
                btnBack.Visible = true;
            }
            ddlCourse.Visible = false;
            lblTitleCourseName.Visible = true;
            lblTitleCourseName.Text = "<b> Course - " + ddlCourse.SelectedValue + "</b>";
    }

    protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
    {
            String strImageUrl = Jenzabar.ICS.UserPhoto.NoPhotoPath;

            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {

                // Retrieve the Image control in the current DataListItem.
                Image imgPhoto = (Image)e.Item.FindControl("imgPhoto");

                // Retrieve the text of the CurrencyColumn from the DataListItem
                // and convert the value to a Double.
                if (imgPhoto != null && imgPhoto.ImageUrl != null)
                {
                    IPortalUserFacade facade = (this.ParentPortlet as ThreeWeekCheck).MyPortalUserFacade;
                    Jenzabar.Portal.Framework.PortalUser pu = facade.FindByHostID(imgPhoto.ImageUrl);
                    if (pu != null)
                    {
                        Jenzabar.ICS.UserPhoto uph = Jenzabar.ICS.UserPhoto.FindByUser(pu);

                        if (uph != null && uph.Path != null && uph.Path != String.Empty)
                        {
                            strImageUrl = uph.Path;
                        }
                    }
                }

                imgPhoto.ImageUrl = strImageUrl;
            }
    }

    protected void btnSave_Click(Object sender, EventArgs args)
    {
        int rows = 0;

        using (System.Data.SqlClient.SqlConnection sqlcon = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["JenzabarConnectionString"].ConnectionString))
        {
            sqlcon.Open();
            String crs_cde = ddlCourse.SelectedValue;
            String lead_instructr_id = Jenzabar.Portal.Framework.PortalUser.Current.HostID;
            String sqlcmdText;

            foreach (DataListItem item in DataList1.Items)
            {
                String StudentID = ((Label)item.FindControl("lblID")).Text;
                String comment = ((Jenzabar.Common.Web.UI.Controls.TextBoxEditor)item.FindControl("txtText")).InnerHtml.Replace("'", "''");
                Byte FLG_ABSENCES = Convert.ToByte(((CheckBox)item.FindControl("FLG_ABSENCES")).Checked);
                Byte FLG_WORK = Convert.ToByte(((CheckBox)item.FindControl("FLG_WORK")).Checked);
                Byte FLG_PARTIC = Convert.ToByte(((CheckBox)item.FindControl("FLG_PARTIC")).Checked);
                Byte FLG_TUTOR = Convert.ToByte(((CheckBox)item.FindControl("FLG_TUTOR")).Checked);
                Byte FLG_COUNSEL = Convert.ToByte(((CheckBox)item.FindControl("FLG_COUNSEL")).Checked);

                String sqlcmdText_existing = "Select * FROM TLU_THREEWEEKCHECK WHERE ID_NUM = " + StudentID + " AND yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR')"
                    + " AND CRS_CDE = " + "'" + crs_cde + "'"
                    + " AND LEAD_INSTRUCTR_ID = " + lead_instructr_id;

                Boolean recordExists;
                using (System.Data.SqlClient.SqlDataReader reader = (new System.Data.SqlClient.SqlCommand(sqlcmdText_existing, sqlcon).ExecuteReader()))
                {
                    recordExists = reader.HasRows;
                }

                if (!recordExists)
                {
                    sqlcmdText = @"INSERT INTO TLU_THREEWEEKCHECK  
                        select distinct 
                            a.id_num,
                            (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR'),
                            (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR'),
                            a.crs_cde,
                            b.lead_instructr_id,"
                            + FLG_ABSENCES + ","
                            + FLG_WORK + ","
                            + FLG_PARTIC + ","
                            + FLG_TUTOR + ","
                            + FLG_COUNSEL + ","
                            + "'" + comment + "'" + ","
//FIRST TIME FRESHMAN FLAG
                        + @"(select cast( case when (Select count(ID_NUM) FROM NAME_MASTER where ID_NUM in (select distinct a.id_num
                            from student_crs_hist a
                            left outer join section_master b
                            on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                            join student_master c
                            on a.id_num = c.id_num
                            join biograph_master d
                            on a.id_num = d.id_num
                            join stud_term_sum_div e
                            on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                            left outer join sports_tracking f
                            on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                            left outer join ATTRIBUTE_TRANS g
                            on a.id_num = g.id_num
                            where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
                                 and a.ID_NUM = " + StudentID
                        //******FIRST TIME FRESHMAN CRITERIA********/
                                 + @" AND ((c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and d.entrance_cde = 'F') --FIRST TIME FRESHMAN SPRING
			or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'F') --FIRST TIME FRESHMAN FALL
			or c.current_class_cde = 'FR')"
                        //*******************************/
                                 + @")) = 0 THEN
                                      '0'
                                      ELSE
                                      '1'
                                      END
                            as bit)),"
//PROBATION
                        + @"(select cast( case when (Select count(ID_NUM) FROM NAME_MASTER where ID_NUM in (select distinct a.id_num
                            from student_crs_hist a
                            left outer join section_master b
                            on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                            join student_master c
                            on a.id_num = c.id_num
                            join biograph_master d
                            on a.id_num = d.id_num
                            join stud_term_sum_div e
                            on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                            left outer join sports_tracking f
                            on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                            left outer join ATTRIBUTE_TRANS g
                            on a.id_num = g.id_num
                            where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
                                 and a.ID_NUM = " + StudentID
                        //******PROBATION CRITERIA********/
                                 + " AND (c.cur_acad_probation in ('PC','PR','SU','AD'))"
                        //*******************************/
                                 + @")) = 0 THEN
                                      '0'
                                      ELSE
                                      '1'
                                      END
                            as bit)),"
//ATHLETES FLAG
                        + @"(select cast( case when (Select count(ID_NUM) FROM NAME_MASTER where ID_NUM in (select distinct a.id_num
                            from student_crs_hist a
                            left outer join section_master b
                            on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                            join student_master c
                            on a.id_num = c.id_num
                            join biograph_master d
                            on a.id_num = d.id_num
                            join stud_term_sum_div e
                            on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                            left outer join sports_tracking f
                            on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                            left outer join ATTRIBUTE_TRANS g
                            on a.id_num = g.id_num
                            where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
                                 and a.ID_NUM = " + StudentID
                                 //******ATHLETES CRITERIA********/
                                 + " AND (f.sports_cde is not null)"
                                 //*******************************/
                                 + @")) = 0 THEN
                                      '0'
                                      ELSE
                                      '1'
                                      END
                            as bit)),"
//DUAL PARTICIPANTS FLAG
                        + @"(select cast( case when (Select count(ID_NUM) FROM NAME_MASTER where ID_NUM in (select distinct a.id_num
                            from student_crs_hist a
                            left outer join section_master b
                            on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                            join student_master c
                            on a.id_num = c.id_num
                            join biograph_master d
                            on a.id_num = d.id_num
                            join stud_term_sum_div e
                            on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                            left outer join sports_tracking f
                            on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                            left outer join ATTRIBUTE_TRANS g
                            on a.id_num = g.id_num
                            where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
                                 and a.ID_NUM = " + StudentID
                        //******DUAL PARTICIPANTS CRITERIA********/
                                 + " AND (c.current_class_cde = 'DP')"
                        //*******************************/
                                 + @")) = 0 THEN
                                      '0'
                                      ELSE
                                      '1'
                                      END
                            as bit)),"
//TRANSFERS FLAG
                        + @"(select cast( case when (Select count(ID_NUM) FROM NAME_MASTER where ID_NUM in (select distinct a.id_num
                            from student_crs_hist a
                            left outer join section_master b
                            on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                            join student_master c
                            on a.id_num = c.id_num
                            join biograph_master d
                            on a.id_num = d.id_num
                            join stud_term_sum_div e
                            on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                            left outer join sports_tracking f
                            on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                            left outer join ATTRIBUTE_TRANS g
                            on a.id_num = g.id_num
                            where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
                                 and a.ID_NUM = " + StudentID
                        //******TRANSFERS CRITERIA********/
                                 + " AND (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'T')"
                        //*******************************/
                                 + @")) = 0 THEN
                                      '0'
                                      ELSE
                                      '1'
                                      END
                            as bit)),"
//SPECIAL NON-DEGREE FLAG
                        + @"(select cast( case when (Select count(ID_NUM) FROM NAME_MASTER where ID_NUM in (select distinct a.id_num
                            from student_crs_hist a
                            left outer join section_master b
                            on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                            join student_master c
                            on a.id_num = c.id_num
                            join biograph_master d
                            on a.id_num = d.id_num
                            join stud_term_sum_div e
                            on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                            left outer join sports_tracking f
                            on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                            left outer join ATTRIBUTE_TRANS g
                            on a.id_num = g.id_num
                            where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
                                 and a.ID_NUM = " + StudentID
                        //******SPECIAL NON-DEGREE CRITERIA********/
                                 + " AND (c.CURRENT_CLASS_CDE = 'SP')"
                        //*******************************/
                                 + @")) = 0 THEN
                                      '0'
                                      ELSE
                                      '1'
                                      END
                            as bit)),"
//HONOR FLAG
                        + @"(select cast( case when (Select count(ID_NUM) FROM NAME_MASTER where ID_NUM in (select distinct a.id_num
                            from student_crs_hist a
                            left outer join section_master b
                            on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                            join student_master c
                            on a.id_num = c.id_num
                            join biograph_master d
                            on a.id_num = d.id_num
                            join stud_term_sum_div e
                            on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                            left outer join sports_tracking f
                            on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                            left outer join ATTRIBUTE_TRANS g
                            on a.id_num = g.id_num
                            where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%'
                                 and a.ID_NUM = " + StudentID
                        //******HONOR CRITERIA********/
                                 + " AND (g.attrib_cde = 'HON')"
                        //*******************************/
                                 + @")) = 0 THEN
                                      '0'
                                      ELSE
                                      '1'
                                      END
                            as bit)),"
                        + @" c.entrance_yr,
                            c.entrance_trm,
                            c.current_class_cde,
                            c.cur_acad_honors,
                            c.cur_acad_probation,
                            d.entrance_cde,
                            e.local_hrs_earned,
                            stuff ( (select ',' + RTRIM(sports_cde) from SPORTS_TRACKING where ID_NUM = a.ID_NUM and YR_CDE = a.YR_CDE and TRM_CDE = a.TRM_CDE FOR xml path('')),1,1,'') as 'sports_cde',
                           (Select gg.attrib_cde from ATTRIBUTE_TRANS gg where gg.attrib_cde = 'HON' and gg.id_num = " + StudentID + @"),
                            GETDATE() 
                        from student_crs_hist a
                        left outer join section_master b
                        on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                        join student_master c
                        on a.id_num = c.id_num
                        join biograph_master d
                        on a.id_num = d.id_num
                        join stud_term_sum_div e
                        on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                        left outer join sports_tracking f
                        on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                        left outer join ATTRIBUTE_TRANS g
                        on a.id_num = g.id_num
                        where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%' 
                         and (		c.cur_acad_probation in ('PC','PR','SU','AD') 
		                        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and d.entrance_cde = 'F') 
		                        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'F') 
		                        or c.current_class_cde = 'FR' 
		                        or c.CURRENT_CLASS_CDE = 'SP' 
		                        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'T') 
		                        or c.current_class_cde = 'DP' 
		                        or g.attrib_cde = 'HON' 
		                        or f.sports_cde is not null 
                              )"
                        + " AND a.ID_NUM = " + StudentID
                        + " AND a.CRS_CDE = '" + crs_cde + "'"
                        + " AND LEAD_INSTRUCTR_ID = " + lead_instructr_id;


                    String sqlcmdTextSports = @"INSERT INTO TLU_THREEWEEKCHECK_SPORTS  
                            SELECT distinct a.ID_NUM,
                            (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR'),
                            (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR'),
                            f.sports_cde
                        from student_crs_hist a
                        left outer join section_master b
                        on a.crs_cde = b.crs_cde and a.yr_cde = b.yr_cde and a.trm_cde = b.trm_cde
                        join student_master c
                        on a.id_num = c.id_num
                        join biograph_master d
                        on a.id_num = d.id_num
                        join stud_term_sum_div e
                        on a.id_num = e.id_num and a.yr_cde = e.yr_cde and a.trm_cde = e.trm_cde
                        left outer join sports_tracking f
                        on a.id_num = f.id_num and a.yr_cde = f.yr_cde and a.trm_cde = f.trm_cde
                        left outer join ATTRIBUTE_TRANS g
                        on a.id_num = g.id_num
                        where a.yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and a.trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and e.hrs_enrolled > '0'  and a.crs_cde not like '________L%' 
                         and (		c.cur_acad_probation in ('PC','PR','SU','AD') 
		                        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'PREV') and d.entrance_cde = 'F') 
		                        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'F') 
		                        or c.current_class_cde = 'FR' 
		                        or c.CURRENT_CLASS_CDE = 'SP' 
		                        or (c.entrance_yr = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and c.entrance_trm = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR') and d.entrance_cde = 'T') 
		                        or c.current_class_cde = 'DP' 
		                        or g.attrib_cde = 'HON' 
		                        or f.sports_cde is not null 
                              )"
                        + " AND a.ID_NUM = " + StudentID
                        + " AND a.CRS_CDE = '" + crs_cde + "'"
                        + " AND LEAD_INSTRUCTR_ID = " + lead_instructr_id;

                    int rowsSports = (new System.Data.SqlClient.SqlCommand(sqlcmdTextSports, sqlcon)).ExecuteNonQuery();
                }
                else
                {
                    sqlcmdText = @"UPDATE TLU_THREEWEEKCHECK SET"
                        + " FLG_ABSENCES = cast(" + FLG_ABSENCES + " as bit)" + ","
                        + " FLG_WORK = cast(" + FLG_WORK + " as bit)" + ","
                        + " FLG_PARTIC = cast(" + FLG_PARTIC + " as bit)" + ","
                        + " FLG_TUTOR = cast(" + FLG_TUTOR + " as bit)" + ","
                        + " FLG_COUNSEL = cast(" + FLG_COUNSEL + " as bit)" + ","
                        + " COMMENT = '" + comment + "'"
                        + " WHERE ID_NUM = " + StudentID
                        + " AND yr_cde = (select CURR_YR from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR')"
                        + " AND trm_cde = (select CURR_TRM from CUST_INTRFC_CNTRL where INTRFC_TYPE = 'CURR')"
                        + " AND CRS_CDE = '" + crs_cde + "'"
                        + " AND LEAD_INSTRUCTR_ID = " + lead_instructr_id;
                }

                rows += (new System.Data.SqlClient.SqlCommand(sqlcmdText, sqlcon)).ExecuteNonQuery();
            }
        }

        if (rows > 0)
        {
            String crs = ddlCourse.SelectedValue;
            ddlCourse.SelectedIndex = 0;
            ddlCourse_SelectedIndexChanged(sender, args);
            ParentPortlet.ShowFeedback(FeedbackType.Message, "Data Saved Successfully for " + crs);
        }
        ddlCourse.Visible = true;
        lblTitleCourseName.Visible = false;
        btnBack.Visible = false;
    }

    protected void btnCancel_Click(Object sender, EventArgs args)
    {

        ddlCourse.SelectedIndex = 0;
        ddlCourse_SelectedIndexChanged(sender, args);
        ddlCourse.Visible = true;
        lblTitleCourseName.Visible = false;
        btnBack.Visible = false;
        ParentPortlet.ShowFeedback(FeedbackType.Message, "");
    }

  }
}