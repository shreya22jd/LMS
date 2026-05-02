using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using LMS_Project.BL;
using LMS_Project.GC;

namespace LMS_Project.Teacher
{
    public partial class MyStudentDetails : System.Web.UI.Page
    {
        TeacherStudentDetailsBL bl = new TeacherStudentDetailsBL();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) { Response.Redirect("../Default.aspx"); return; }
            if (!IsPostBack)
            {
                if (Request.QueryString["UserId"] == null) { Response.Redirect("MyStudents.aspx"); return; }
                LoadStudentDetails(Convert.ToInt32(Request.QueryString["UserId"]));
            }
        }

        private string Val(string s) => string.IsNullOrWhiteSpace(s) ? "—" : s;

        private void LoadStudentDetails(int userId)
        {
            try
            {
                int teacherUserId = Convert.ToInt32(Session["UserId"]);

                // Profile
                TeacherStudentGC profile = bl.GetStudentProfile(userId);
                if (profile != null)
                {
                    string name = profile.FullName;
                    lblFullName.Text = Val(name);
                    lblEmail.Text = Val(profile.Email);
                    lblRoll.Text = Val(profile.RollNumber);
                    lblCourse.Text = Val(profile.CourseName);
                    lblSemester.Text = Val(profile.SemesterName);
                    lblGender.Text = Val(profile.Gender);
                    lblStream.Text = Val(profile.StreamName);
                    lblLevel.Text = Val(profile.LevelName);
                    lblCity.Text = Val(profile.City);
                    lblContact.Text = Val(profile.ContactNo);
                    lblSection.Text = Val(profile.SectionName);
                    lblSession.Text = Val(profile.SessionName);
                    lblAddress.Text = Val(profile.Address);
                    lblSkills.Text = Val(profile.Skills);
                    lblHobbies.Text = Val(profile.Hobbies);
                    lblEmergencyName.Text = Val(profile.EmergencyContactName);
                    lblEmergencyNo.Text = Val(profile.EmergencyContactNo);
                    lblDOB.Text = profile.DOB.HasValue
                                                ? profile.DOB.Value.ToString("dd MMM yyyy")
                                                : "—";
                    lblInitialHero.Text = name.Length > 0
                                                ? name.Substring(0, 1).ToUpper()
                                                : "S";
                }
                else
                {
                    lblError.Text = "Student profile not found.";
                    lblError.Visible = true;
                    return;
                }

                // Attendance
                TeacherStudentGC att = bl.GetAttendanceSummary(userId, teacherUserId);
                lblPresent.Text = att.Present.ToString();
                lblAbsent.Text = att.Absent.ToString();
                int total = att.Present + att.Absent;
                int attPct = total > 0 ? (att.Present * 100 / total) : 0;
                lblAttPct.Text = attPct + "%";
                hfAttPresent.Value = att.Present.ToString();
                hfAttAbsent.Value = att.Absent.ToString();

                // Subjects
                List<TeacherStudentSubjectGC> subjects = bl.GetSubjects(userId);
                rptSubjects.DataSource = subjects;
                rptSubjects.DataBind();

                // Videos chart
                DataTable videosBySubject = bl.GetVideosBySubject(userId, teacherUserId);
                var sbLabels = new StringBuilder("[");
                var sbData = new StringBuilder("[");
                foreach (DataRow r in videosBySubject.Rows)
                {
                    sbLabels.Append($"'{r["SubjectName"]}',");
                    sbData.Append($"{r["WatchedCount"]},");
                }
                hfVideoLabels.Value = sbLabels.ToString().TrimEnd(',') + "]";
                hfVideoData.Value = sbData.ToString().TrimEnd(',') + "]";

                // Assignment chart
                TeacherStudentGC asgn = bl.GetAssignmentStats(userId, teacherUserId);
                hfAsgnSubmitted.Value = asgn.AssignmentsSubmitted.ToString();
                hfAsgnOverdue.Value = asgn.AssignmentsOverdue.ToString();
                hfAsgnPending.Value = asgn.AssignmentsPending.ToString();   // ← new
                // Recent Videos
                List<TeacherStudentActivityGC> videos = bl.GetRecentVideos(userId);
                rptVideos.DataSource = videos;
                rptVideos.DataBind();
                pnlNoVideos.Visible = videos.Count == 0;

                // Recent Assignments
                List<TeacherStudentActivityGC> assignments = bl.GetRecentAssignments(userId);
                rptAssignments.DataSource = assignments;
                rptAssignments.DataBind();
                pnlNoAssignments.Visible = assignments.Count == 0;

                // Progress stats
                TeacherStudentGC stats = bl.GetProgressStats(userId);
                lblVideos.Text = stats.VideosCompleted.ToString();
                lblAssignments.Text = stats.AssignmentsSubmitted.ToString();
            }
            catch (Exception ex)
            {
                lblError.Text = "Could not load student details: " + ex.Message;
                lblError.Visible = true;
            }
        }
    }
}