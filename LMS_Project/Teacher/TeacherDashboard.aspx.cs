using LearningManagementSystem.BL;
using LMS_Project.Teacher;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LMS_Project.Teacher
{
    public partial class TeacherDashboard : Page
    {
        TeacherDashboardBL bl = new TeacherDashboardBL();

        private int TeacherId => Convert.ToInt32(Session["UserId"]);
        private int InstituteId => Convert.ToInt32(Session["InstituteId"]);
        private int SocietyId => Convert.ToInt32(Session["SocietyId"]);

        private int SelSessionId
        {
            get => ViewState["SelSession"] != null ? (int)ViewState["SelSession"] : 0;
            set => ViewState["SelSession"] = value;
        }
        private int SelSectionId
        {
            get => ViewState["SelSection"] != null ? (int)ViewState["SelSection"] : 0;
            set => ViewState["SelSection"] = value;
        }
        private int SelStreamId
        {
            get => ViewState["SelStream"] != null ? (int)ViewState["SelStream"] : 0;
            set => ViewState["SelStream"] = value;
        }
        private int SelStudentSessionId
        {
            get => ViewState["SelStudentSession"] != null ? (int)ViewState["SelStudentSession"] : 0;
            set => ViewState["SelStudentSession"] = value;
        }
        private int SelStudentSectionId
        {
            get => ViewState["SelStudentSection"] != null ? (int)ViewState["SelStudentSection"] : 0;
            set => ViewState["SelStudentSection"] = value;
        }
        private int SelStudentStreamId
        {
            get => ViewState["SelStudentStream"] != null ? (int)ViewState["SelStudentStream"] : 0;
            set => ViewState["SelStudentStream"] = value;
        }
        private int SelAsgSubjectId
        {
            get => ViewState["SelAsgSubject"] != null ? (int)ViewState["SelAsgSubject"] : 0;
            set => ViewState["SelAsgSubject"] = value;
        }
        private int SelEngagementSubjectId
        {
            get => ViewState["SelEngagementSubject"] != null ? (int)ViewState["SelEngagementSubject"] : 0;
            set => ViewState["SelEngagementSubject"] = value;
        }
        // ── Rank badge colors ────────────────────────────────────────
        public string GetRankColor(int index)
        {
            string[] colors = { "#f9a825", "#90a4ae", "#a1887f", "#1565c0", "#388e3c" };
            return index < colors.Length ? colors[index] : "#1565c0";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadWelcomeBanner();
                LoadStudentFilterDropdowns();
                LoadStatCards();
                LoadSubjects();
                LoadRecentStudents();
                LoadAssignmentFilterDropdowns();
                LoadRecentAssignments();
                LoadStudentPerformance();
                LoadComparisonAnalytics();
                LoadEngagementFilterDropdowns();
                LoadContentEngagement();
            }
        }

        // ── Welcome Banner ──────────────────────────────────────────
        private void LoadWelcomeBanner()
        {
            DataTable dt = bl.GetTeacherWelcomeInfo(TeacherId, InstituteId);
            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                lblWelcomeName.Text = r["FullName"].ToString();
                lblDeptPill.Text = "🏫 " + r["Department"].ToString();
                lblDesignationPill.Text = "👤 " + r["Designation"].ToString();
                lblSessionPill.Text = "📅 " + r["SessionName"].ToString();
            }
        }

        // ── Stat Cards ──────────────────────────────────────────────
        private void LoadStatCards()
        {
            int sessionId = Session["CurrentSessionId"] != null
                            ? Convert.ToInt32(Session["CurrentSessionId"])
                            : 1;

            DataTable dt = bl.GetDashboardStats(TeacherId, InstituteId, sessionId);
            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                lblTotalSubjects.Text = r["TotalSubjects"].ToString();
                lblTotalStudents.Text = r["TotalStudents"].ToString();
                lblTotalAssignments.Text = r["TotalAssignments"].ToString();
                lblTotalVideos.Text = r["TotalVideos"].ToString();
            }
        }

        // ── Subjects ────────────────────────────────────────────────
        private void LoadSubjects()
        {
            // Use CurrentSessionId from session exactly like TeacherCourses page does
            int sessionId = Session["CurrentSessionId"] != null
                            ? Convert.ToInt32(Session["CurrentSessionId"])
                            : 1;

            DataTable dt = bl.GetTeacherSubjects(
                TeacherId, InstituteId, SocietyId,
                sessionId, SelSectionId, SelStreamId);

            bool hasData = dt.Rows.Count > 0;

            if (hasData)
            {
                int totalSubjects = dt.Rows.Count;
                int totalStudents = 0;
                int maxCount = 0;
                string topSubject = "-";

                foreach (DataRow row in dt.Rows)
                {
                    int count = Convert.ToInt32(row["StudentCount"]);
                    totalStudents += count;
                    if (count > maxCount)
                    {
                        maxCount = count;
                        topSubject = row["SubjectName"].ToString();
                    }
                }

                int avg = totalSubjects > 0
                          ? (int)Math.Round((double)totalStudents / totalSubjects)
                          : 0;

                lblKpiTotalSubjects.Text = totalSubjects.ToString();
                lblKpiTotalStudents.Text = totalStudents.ToString();
                lblKpiAvgStudents.Text = avg.ToString();
                lblKpiTopSubject.Text = topSubject;

                DataTable dtTable = dt.Copy();
                dtTable.Columns.Add("EnrolmentPercent", typeof(int));
                foreach (DataRow row in dtTable.Rows)
                {
                    int count = Convert.ToInt32(row["StudentCount"]);
                    row["EnrolmentPercent"] = totalStudents > 0
                        ? (int)Math.Round((double)count / totalStudents * 100)
                        : 0;
                }

                hfChartData.Value = bl.GetSubjectChartJson(dt);

                rptSubjectTable.DataSource = dtTable;
                rptSubjectTable.DataBind();
            }

            pnlSubjectKPIs.Visible = hasData;
            pnlSubjectsChart.Visible = hasData;
            pnlSubjectTable.Visible = hasData;
            pnlNoSubjects.Visible = !hasData;
        }

        // ── Student Filter Dropdowns ────────────────────────────────
        private void LoadStudentFilterDropdowns()
        {
            ddlStudentSession.Items.Clear();
            ddlStudentSession.Items.Add(new ListItem("All Years", "0"));
            DataTable dtSess = bl.GetSessionsForFilter(InstituteId, SocietyId);
            foreach (DataRow row in dtSess.Rows)
                ddlStudentSession.Items.Add(
                    new ListItem(row["SessionName"].ToString(), row["SessionId"].ToString()));

            int currSession = Session["CurrentSessionId"] != null
                              ? Convert.ToInt32(Session["CurrentSessionId"])
                              : bl.GetCurrentSessionId(InstituteId);
            if (currSession > 0)
            {
                SelStudentSessionId = currSession;
                ddlStudentSession.SelectedValue = currSession.ToString();
            }

            ddlStudentSection.Items.Clear();
            ddlStudentSection.Items.Add(new ListItem("All Sections", "0"));
            DataTable dtSec = bl.GetSectionsForFilter(TeacherId, InstituteId);
            foreach (DataRow row in dtSec.Rows)
                ddlStudentSection.Items.Add(
                    new ListItem(row["SectionName"].ToString(), row["SectionId"].ToString()));

            ddlStudentStream.Items.Clear();
            ddlStudentStream.Items.Add(new ListItem("All Streams", "0"));
            DataTable dtStr = bl.GetStreamsForFilter(TeacherId, InstituteId);
            foreach (DataRow row in dtStr.Rows)
                ddlStudentStream.Items.Add(
                    new ListItem(row["StreamName"].ToString(), row["StreamId"].ToString()));
        }

        // ── Recent Students ─────────────────────────────────────────
        private void LoadRecentStudents()
        {
            int currSession = Session["CurrentSessionId"] != null
                              ? Convert.ToInt32(Session["CurrentSessionId"])
                              : bl.GetCurrentSessionId(InstituteId);

            DataTable dtDiv = bl.GetStudentsByDivision(TeacherId, InstituteId, currSession);
            hfDivisionData.Value = bl.GetDivisionChartJson(dtDiv);

            DataTable dtStats = bl.GetStudentAnalytics(TeacherId, InstituteId, currSession);
            if (dtStats.Rows.Count > 0)
            {
                DataRow r = dtStats.Rows[0];
                lblAnalyticStudents.Text = r["TotalStudents"].ToString();
                lblAnalyticSubjects.Text = r["TotalSubjects"].ToString();
                lblAnalyticDivisions.Text = r["TotalDivisions"].ToString();
                lblAnalyticAvg.Text = r["AvgStudentsPerSubject"].ToString();
            }

            pnlStudents.Visible = true;
            pnlNoStudents.Visible = false;
        }

        // ── Assignment Dropdowns ────────────────────────────────────
        private void LoadAssignmentFilterDropdowns()
        {
            ddlAsgSubject.Items.Clear();
            ddlAsgSubject.Items.Add(new ListItem("All Subjects", "0"));
            DataTable dt = bl.GetTeacherSubjectsForFilter(TeacherId, InstituteId, SelSessionId);
            foreach (DataRow row in dt.Rows)
                ddlAsgSubject.Items.Add(
                    new ListItem(row["SubjectName"].ToString(), row["SubjectId"].ToString()));
        }

        // ── Recent Assignments ──────────────────────────────────────
        private void LoadRecentAssignments()
        {
            DataTable dt = bl.GetRecentAssignments(TeacherId, InstituteId, SelAsgSubjectId);
            bool hasData = dt.Rows.Count > 0;

            rptAssignments.DataSource = dt;
            rptAssignments.DataBind();

            hfAsgChartData.Value = bl.GetAssignmentChartJson(dt);

            DataTable dtSummary = bl.GetAssignmentSummary(dt);
            bool hasSummary = dtSummary.Rows.Count > 0;
            rptAsgSummary.DataSource = dtSummary;
            rptAsgSummary.DataBind();

            pnlAssignments.Visible = hasData;
            pnlAsgChart.Visible = hasData;
            pnlNoAssignments.Visible = !hasData;
            pnlAsgSummary.Visible = hasSummary;
            pnlNoAsgSummary.Visible = !hasSummary;

            if (hasData)
            {
                pnlAssignments.Style["display"] = "";
                pnlAsgChart.Style["display"] = "none";
            }
        }

        // ── Student Performance ─────────────────────────────────────
        private void LoadStudentPerformance()
        {
            int currSession = Session["CurrentSessionId"] != null
                              ? Convert.ToInt32(Session["CurrentSessionId"])
                              : bl.GetCurrentSessionId(InstituteId);

            // KPI summary
            DataTable dtKpi = bl.GetPerformanceKPIs(TeacherId, InstituteId, currSession);
            if (dtKpi.Rows.Count > 0)
            {
                DataRow r = dtKpi.Rows[0];
                lblPerfAvgMarks.Text = r["AvgMarks"].ToString();
                lblPerfHighest.Text = r["HighestMarks"].ToString();
                lblPerfLowest.Text = r["LowestMarks"].ToString();
                lblPerfSubmissions.Text = r["TotalGraded"].ToString();
            }

            // Top 5
            DataTable dtTop = bl.GetTopStudents(TeacherId, InstituteId, currSession);
            bool hasTop = dtTop.Rows.Count > 0;
            rptTopStudents.DataSource = dtTop;
            rptTopStudents.DataBind();
           // pnlTopStudents.Visible = hasTop;
            //pnlNoTopStudents.Visible = !hasTop;

            // Low performers
            DataTable dtLow = bl.GetLowPerformers(TeacherId, InstituteId, currSession);
            bool hasLow = dtLow.Rows.Count > 0;
            rptLowStudents.DataSource = dtLow;
            rptLowStudents.DataBind();
         //   pnlLowStudents.Visible = hasLow;
         //   pnlNoLowStudents.Visible = !hasLow;

            // Avg marks per subject (for pie + legend)
            DataTable dtAvg = bl.GetAvgMarksPerSubject(TeacherId, InstituteId, currSession);
            bool hasAvg = dtAvg.Rows.Count > 0;
            hfAvgMarksData.Value = bl.GetAvgMarksChartJson(dtAvg);
            rptAvgMarks.DataSource = dtAvg;
            rptAvgMarks.DataBind();
            pnlAvgMarksChart.Visible = hasAvg;
            pnlNoAvgMarks.Visible = !hasAvg;

            pnlPerfKPIs.Visible = dtKpi.Rows.Count > 0;
        }
        private void LoadComparisonAnalytics()
        {
            int sessionId = Session["CurrentSessionId"] != null
                            ? Convert.ToInt32(Session["CurrentSessionId"])
                            : bl.GetCurrentSessionId(InstituteId);

            // ── Section vs Section ─────────────────────────────────────
            DataTable dtSec = bl.GetSectionCompareData(TeacherId, InstituteId, sessionId);
            bool hasSec = dtSec != null && dtSec.Rows.Count > 0;

            if (hasSec)
            {
                hfSecCompareData.Value = bl.GetCompareJson(dtSec, "SectionName");

                lblCmpSecCount.Text = dtSec.Rows.Count.ToString();

                // Best by marks
                DataRow bestMarks = GetBestRow(dtSec, "AvgMarks");
                lblCmpSecBest.Text = bestMarks != null
                    ? bestMarks["SectionName"].ToString() + " (" + bestMarks["AvgMarks"] + ")"
                    : "-";

                // Best by attendance
                DataRow bestAttend = GetBestRow(dtSec, "AttendancePct");
                lblCmpSecAttend.Text = bestAttend != null
                    ? bestAttend["SectionName"].ToString() + " (" + bestAttend["AttendancePct"] + "%)"
                    : "-";

                // Best by engagement
                DataRow bestEngage = GetBestRow(dtSec, "VideoViews");
                lblCmpSecEngage.Text = bestEngage != null
                    ? bestEngage["SectionName"].ToString() + " (" + bestEngage["VideoViews"] + " views)"
                    : "-";

                // Auto-insights
                var secInsights = bl.GenerateSectionInsights(dtSec);
                rptSecInsights.DataSource = secInsights;
                rptSecInsights.DataBind();

                pnlSecInsights.Visible = secInsights.Rows.Count > 0;
                pnlNoSecCompare.Visible = false;
            }
            else
            {
                pnlSecInsights.Visible = false;
                pnlNoSecCompare.Visible = true;
            }

            // ── Subject vs Subject ─────────────────────────────────────
            DataTable dtSub = bl.GetSubjectCompareData(TeacherId, InstituteId, sessionId);
            bool hasSub = dtSub != null && dtSub.Rows.Count > 0;

            if (hasSub)
            {
                hfSubCompareData.Value = bl.GetCompareJson(dtSub, "SubjectName");

                lblCmpSubCount.Text = dtSub.Rows.Count.ToString();

                DataRow subBestMarks = GetBestRow(dtSub, "AvgMarks");
                lblCmpSubBest.Text = subBestMarks != null
                    ? TruncateName(subBestMarks["SubjectName"].ToString(), 12) + " (" + subBestMarks["AvgMarks"] + ")"
                    : "-";

                DataRow subBestAttend = GetBestRow(dtSub, "AttendancePct");
                lblCmpSubAttend.Text = subBestAttend != null
                    ? TruncateName(subBestAttend["SubjectName"].ToString(), 12) + " (" + subBestAttend["AttendancePct"] + "%)"
                    : "-";

                DataRow subBestEngage = GetBestRow(dtSub, "VideoViews");
                lblCmpSubEngage.Text = subBestEngage != null
                    ? TruncateName(subBestEngage["SubjectName"].ToString(), 12) + " (" + subBestEngage["VideoViews"] + " views)"
                    : "-";

                var subInsights = bl.GenerateSubjectInsights(dtSub);
                rptSubInsights.DataSource = subInsights;
                rptSubInsights.DataBind();

                pnlSubInsights.Visible = subInsights.Rows.Count > 0;
                pnlNoSubCompare.Visible = false;
            }
            else
            {
                pnlSubInsights.Visible = false;
                pnlNoSubCompare.Visible = true;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────
        private DataRow GetBestRow(DataTable dt, string col)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            DataRow best = null;
            double maxVal = double.MinValue;
            foreach (DataRow row in dt.Rows)
            {
                double val = 0;
                double.TryParse(row[col]?.ToString(), out val);
                if (val > maxVal) { maxVal = val; best = row; }
            }
            return best;
        }
        public string GetRankColorEngagement(int index)
        {
            string[] colors = { "#f9a825", "#90a4ae", "#a1887f", "#1565c0", "#388e3c" };
            return index < colors.Length ? colors[index] : "#1565c0";
        }

        protected string FormatWatchTime(object seconds)
        {
            if (seconds == DBNull.Value || seconds == null) return "0 min";
            int totalSecs = Convert.ToInt32(seconds);
            int hours = totalSecs / 3600;
            int minutes = (totalSecs % 3600) / 60;
            if (hours > 0) return $"{hours}h {minutes}m";
            return $"{minutes} min";
        }

        private void LoadEngagementFilterDropdowns()
        {
            ddlEngagementSubject.Items.Clear();
            ddlEngagementSubject.Items.Add(new ListItem("All Subjects", "0"));
            int sessionId = Session["CurrentSessionId"] != null
                            ? Convert.ToInt32(Session["CurrentSessionId"])
                            : bl.GetCurrentSessionId(InstituteId);
            DataTable dt = bl.GetTeacherSubjectsForFilter(TeacherId, InstituteId, sessionId);
            foreach (DataRow row in dt.Rows)
                ddlEngagementSubject.Items.Add(
                    new ListItem(row["SubjectName"].ToString(), row["SubjectId"].ToString()));
        }

        private void LoadContentEngagement()
        {
            int sessionId = Session["CurrentSessionId"] != null
                            ? Convert.ToInt32(Session["CurrentSessionId"])
                            : bl.GetCurrentSessionId(InstituteId);

            DataTable dtKPIs = bl.GetEngagementKPIs(TeacherId, InstituteId, sessionId, SelEngagementSubjectId);
            if (dtKPIs.Rows.Count > 0)
            {
                DataRow r = dtKPIs.Rows[0];
                lblTotalViews.Text = r["TotalViews"].ToString();
                lblAvgWatchPercent.Text = r["AvgWatchPercent"].ToString();
                lblMostViewedVideo.Text = r["MostViewedVideo"].ToString();
                lblMostViewedCount.Text = r["MostViewedCount"].ToString();
                lblTotalVideosEngaged.Text = r["TotalVideos"].ToString();
                lblViewsTrend.Text = r["ViewsTrend"].ToString();
            }

            DataTable dtChart = bl.GetVideoEngagementData(TeacherId, InstituteId, sessionId, SelEngagementSubjectId);
            hfEngagementData.Value = bl.GetEngagementChartJson(dtChart);

            DataTable dtTop = bl.GetTopVideos(TeacherId, InstituteId, sessionId, SelEngagementSubjectId);
            bool hasTop = dtTop.Rows.Count > 0;
            rptTopVideos.DataSource = dtTop;
            rptTopVideos.DataBind();
            pnlTopVideos.Visible = hasTop;
            pnlNoTopVideos.Visible = !hasTop;

            DataTable dtLow = bl.GetLowPerformingVideos(TeacherId, InstituteId, sessionId, SelEngagementSubjectId);
            bool hasLow = dtLow.Rows.Count > 0;
            rptLowVideos.DataSource = dtLow;
            rptLowVideos.DataBind();
            pnlLowVideos.Visible = hasLow;
            pnlNoLowVideos.Visible = !hasLow;

            DataTable dtLeaders = bl.GetWatchTimeLeaders(TeacherId, InstituteId, sessionId, SelEngagementSubjectId);
            bool hasLeaders = dtLeaders.Rows.Count > 0;
            rptWatchTimeLeaders.DataSource = dtLeaders;
            rptWatchTimeLeaders.DataBind();
            pnlWatchTimeLeaders.Visible = hasLeaders;
            pnlNoWatchTimeLeaders.Visible = !hasLeaders;

            pnlEngagementKPIs.Visible = dtKPIs.Rows.Count > 0;
        }

        private string TruncateName(string name, int maxLen)
        {
            return name.Length <= maxLen ? name : name.Substring(0, maxLen) + "…";
        }
        // ── Postbacks ───────────────────────────────────────────────
        protected void ddlStudentSession_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelStudentSessionId = Convert.ToInt32(ddlStudentSession.SelectedValue);
            LoadRecentStudents();
        }
        protected void ddlStudentSection_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelStudentSectionId = Convert.ToInt32(ddlStudentSection.SelectedValue);
            LoadRecentStudents();
        }
        protected void ddlStudentStream_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelStudentStreamId = Convert.ToInt32(ddlStudentStream.SelectedValue);
            LoadRecentStudents();
        }
        protected void ddlAsgSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelAsgSubjectId = Convert.ToInt32(ddlAsgSubject.SelectedValue);
            LoadRecentAssignments();
        }
        protected void ddlEngagementSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelEngagementSubjectId = Convert.ToInt32(ddlEngagementSubject.SelectedValue);
            LoadContentEngagement();
        }

        protected void ddlEngagementChartType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadContentEngagement();
        }
    }
}