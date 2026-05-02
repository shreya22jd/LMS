using LearningManagementSystem.Admin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;

namespace LearningManagementSystem.BL
{
    public class TeacherDashboardBL
    {
        DataLayer dl = new DataLayer();

        // ════════════════════════════════════════════════════════════
        // WELCOME BANNER
        // ════════════════════════════════════════════════════════════
        public DataTable GetTeacherWelcomeInfo(int teacherId, int instituteId)
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT 
                    up.FullName,
                    ISNULL(s.StreamName, '')   AS Department,
                    ISNULL(td.Designation, '') AS Designation,
                    ISNULL(ac.SessionName, '') AS SessionName
                FROM UserProfile       up
                JOIN TeacherDetails    td  ON td.UserId     = up.UserId
                LEFT JOIN Streams      s   ON s.StreamId    = td.StreamId
                LEFT JOIN AcademicSessions ac
                       ON ac.InstituteId = @InstituteId AND ac.IsCurrent = 1
                WHERE up.UserId = @TeacherId");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // STAT CARDS
        // ════════════════════════════════════════════════════════════
        public DataTable GetDashboardStats(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            (
                SELECT COUNT(DISTINCT SF.SubjectId)
                FROM   SubjectFaculty SF
                WHERE  SF.TeacherId   = @TeacherId
                  AND  SF.InstituteId = @InstituteId
                  AND  SF.SessionId   = @SessionId
                  AND  ISNULL(SF.IsActive, 1) = 1
            ) AS TotalSubjects,

            (
                SELECT COUNT(DISTINCT ass.UserId)
                FROM   AssignStudentSubject ass
                JOIN   SubjectFaculty SF
                       ON  SF.SubjectId = ass.SubjectId
                       AND SF.SessionId = ass.SessionId
                WHERE  SF.TeacherId   = @TeacherId
                  AND  SF.InstituteId = @InstituteId
                  AND  ISNULL(SF.IsActive, 1) = 1
            ) AS TotalStudents,

            (
                SELECT COUNT(DISTINCT a.AssignmentId)
                FROM   Assignments    a
                JOIN   SubjectFaculty SF ON SF.SubjectId = a.SubjectId
                WHERE  SF.TeacherId   = @TeacherId
                  AND  SF.InstituteId = @InstituteId
                  AND  SF.SessionId   = @SessionId
                  AND  ISNULL(SF.IsActive, 1) = 1
                  AND  a.IsActive     = 1
            ) AS TotalAssignments,

            (
                SELECT COUNT(DISTINCT v.VideoId)
                FROM   Videos         v
                JOIN   Chapters       c  ON c.ChapterId  = v.ChapterId
                JOIN   SubjectFaculty SF ON SF.SubjectId = c.SubjectId
                WHERE  SF.TeacherId   = @TeacherId
                  AND  SF.InstituteId = @InstituteId
                  AND  SF.SessionId   = @SessionId
                  AND  ISNULL(SF.IsActive, 1) = 1
                  AND  v.IsActive     = 1
            ) AS TotalVideos
    ");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // FILTER DROPDOWNS
        // ════════════════════════════════════════════════════════════
        public DataTable GetSessionsForFilter(int instituteId, int societyId)
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT SessionId, SessionName
                FROM   AcademicSessions
                WHERE  InstituteId = @InstituteId
                  AND  SocietyId   = @SocietyId
                ORDER  BY StartDate DESC");

            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SocietyId", societyId);
            return dl.GetDataTable(cmd);
        }

        public int GetCurrentSessionId(int instituteId)
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT TOP 1 SessionId
                FROM   AcademicSessions
                WHERE  InstituteId = @InstituteId
                  AND  IsCurrent   = 1");

            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            DataTable dt = dl.GetDataTable(cmd);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["SessionId"]) : 0;
        }

        public DataTable GetSectionsForFilter(int teacherId, int instituteId)
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT DISTINCT sec.SectionId, sec.SectionName
                FROM   SubjectFaculty SF
                JOIN   Sections       sec ON sec.SectionId = SF.SectionId
                WHERE  SF.TeacherId   = @TeacherId
                  AND  SF.InstituteId = @InstituteId
                  AND  ISNULL(SF.IsActive, 1) = 1
                ORDER  BY sec.SectionName");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            return dl.GetDataTable(cmd);
        }

        public DataTable GetStreamsForFilter(int teacherId, int instituteId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT DISTINCT st.StreamId, st.StreamName
        FROM   SubjectFaculty SF
        JOIN   Subjects       S   ON S.SubjectId  = SF.SubjectId
        JOIN   LevelSemesterSubjects LSS 
                             ON LSS.SubjectId = SF.SubjectId
        JOIN   Streams        st  ON st.StreamId  = LSS.StreamId
        WHERE  SF.TeacherId   = @TeacherId
          AND  SF.InstituteId = @InstituteId
          AND  ISNULL(SF.IsActive, 1) = 1
        ORDER  BY st.StreamName");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // SUBJECTS
        // ════════════════════════════════════════════════════════════
        public DataTable GetTeacherSubjects(
            int teacherId, int instituteId, int societyId,
            int sessionId, int sectionId, int streamId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            S.SubjectId,
            ISNULL(S.SubjectCode, '')    AS SubjectCode,
            S.SubjectName,
            ISNULL(S.Duration, '')       AS Duration,
            ISNULL(SEC.SectionName, '')  AS SectionName,
            ASY.SessionName,
            ''                           AS StreamName,
            ''                           AS CourseName,
            (
                SELECT COUNT(DISTINCT ass.UserId)
                FROM   AssignStudentSubject ass
                WHERE  ass.SubjectId   = SF.SubjectId
                  AND  ass.SessionId   = SF.SessionId
                  AND  ass.InstituteId = SF.InstituteId
            ) AS StudentCount
        FROM   SubjectFaculty       SF
        INNER JOIN Subjects         S   ON S.SubjectId   = SF.SubjectId
        LEFT  JOIN Sections         SEC ON SEC.SectionId = SF.SectionId
        INNER JOIN AcademicSessions ASY ON ASY.SessionId = SF.SessionId
        WHERE  SF.TeacherId   = @TeacherId
          AND  SF.InstituteId = @InstituteId
          AND  SF.SessionId   = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
          AND (@SectionId = 0 OR SF.SectionId = @SectionId)
        ORDER BY S.SubjectName");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId == 0 ? 1 : sessionId);
            cmd.Parameters.AddWithValue("@SectionId", sectionId);
            return dl.GetDataTable(cmd);
        }

        public string GetSubjectChartJson(DataTable dt)
        {
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
                list.Add(new
                {
                    SubjectId = Convert.ToInt32(row["SubjectId"]),
                    SubjectName = row["SubjectName"].ToString(),
                    StudentCount = Convert.ToInt32(row["StudentCount"])
                });

            return new JavaScriptSerializer().Serialize(list);
        }

        // ════════════════════════════════════════════════════════════
        // RECENT STUDENTS
        // ════════════════════════════════════════════════════════════
        public DataTable GetRecentStudents(
            int teacherId, int instituteId,
            int sessionId, int sectionId, int streamId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 6
            up.FullName                  AS StudentName,
            ISNULL(co.CourseName,  '')   AS CourseName,
            ISNULL(sec.SectionName,'')   AS SectionName
        FROM   AssignStudentSubject      ass
        JOIN   SubjectFaculty            SF
               ON  SF.SubjectId = ass.SubjectId
               AND SF.SessionId = ass.SessionId
        JOIN   UserProfile               up  ON up.UserId    = ass.UserId
        LEFT JOIN StudentAcademicDetails sad ON sad.UserId   = ass.UserId
        LEFT JOIN Courses                co  ON co.CourseId  = sad.CourseId
        LEFT JOIN Sections               sec ON sec.SectionId = sad.SectionId
        LEFT JOIN LevelSemesterSubjects  LSS ON LSS.SubjectId = SF.SubjectId
                                            AND LSS.SessionId = SF.SessionId
        WHERE  SF.TeacherId   = @TeacherId
          AND  SF.InstituteId = @InstituteId
          AND  ISNULL(SF.IsActive, 1) = 1
          AND (@SessionId = 0 OR SF.SessionId   = @SessionId)
          AND (@SectionId = 0 OR sad.SectionId  = @SectionId)
          AND (@StreamId  = 0 OR LSS.StreamId   = @StreamId)
        ORDER BY ass.UserId DESC
    ");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@SectionId", sectionId);
            cmd.Parameters.AddWithValue("@StreamId", streamId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // ASSIGNMENT SUBJECT FILTER DROPDOWN
        // ════════════════════════════════════════════════════════════
        public DataTable GetTeacherSubjectsForFilter(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT DISTINCT
            S.SubjectId,
            S.SubjectName
        FROM   SubjectFaculty SF
        JOIN   Subjects       S ON S.SubjectId = SF.SubjectId
        WHERE  SF.TeacherId   = @TeacherId
          AND  SF.InstituteId = @InstituteId
          AND  SF.SessionId   = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
        ORDER  BY S.SubjectName
    ");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // RECENT ASSIGNMENTS WITH SUBJECT FILTER
        // ════════════════════════════════════════════════════════════
        public DataTable GetRecentAssignments(int teacherId, int instituteId, int subjectId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 8
            a.AssignmentId,
            a.Title,
            sub.SubjectName,
            ISNULL(a.MaxMarks, 0)   AS MaxMarks,
            a.DueDate,
            (
                SELECT COUNT(*)
                FROM   AssignmentSubmissions asub
                WHERE  asub.AssignmentId = a.AssignmentId
            ) AS SubmissionCount,
            (
                SELECT COUNT(DISTINCT ass.UserId)
                FROM   AssignStudentSubject ass
                JOIN   SubjectFaculty SF2
                       ON  SF2.SubjectId = ass.SubjectId
                       AND SF2.SessionId = ass.SessionId
                WHERE  SF2.SubjectId   = a.SubjectId
                  AND  SF2.TeacherId   = @TeacherId
                  AND  SF2.InstituteId = @InstituteId
            ) AS TotalStudents
        FROM   Assignments    a
        JOIN   Subjects       sub ON sub.SubjectId = a.SubjectId
        JOIN   SubjectFaculty SF  ON SF.SubjectId  = a.SubjectId
        WHERE  SF.TeacherId   = @TeacherId
          AND  SF.InstituteId = @InstituteId
          AND  ISNULL(SF.IsActive, 1) = 1
          AND  a.IsActive     = 1
          AND (@SubjectId = 0 OR a.SubjectId = @SubjectId)
        ORDER  BY a.CreatedOn DESC
    ");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SubjectId", subjectId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // ASSIGNMENT CHART JSON
        // ════════════════════════════════════════════════════════════
        public string GetAssignmentChartJson(DataTable dt)
        {
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                int submitted = Convert.ToInt32(row["SubmissionCount"]);
                int total = Convert.ToInt32(row["TotalStudents"]);
                int pending = total > submitted ? total - submitted : 0;

                list.Add(new
                {
                    Title = row["Title"].ToString(),
                    SubmissionCount = submitted,
                    Pending = pending,
                    TotalStudents = total
                });
            }
            return new JavaScriptSerializer().Serialize(list);
        }

        // ════════════════════════════════════════════════════════════
        // ASSIGNMENT SUMMARY (progress bars)
        // ════════════════════════════════════════════════════════════
        public DataTable GetAssignmentSummary(DataTable dt)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Title");
            result.Columns.Add("SubmissionCount", typeof(int));
            result.Columns.Add("TotalStudents", typeof(int));
            result.Columns.Add("SubmissionPercent", typeof(int));

            foreach (DataRow row in dt.Rows)
            {
                int submitted = Convert.ToInt32(row["SubmissionCount"]);
                int total = Convert.ToInt32(row["TotalStudents"]);
                int percent = total > 0 ? (int)Math.Round((double)submitted / total * 100) : 0;

                result.Rows.Add(
                    row["Title"].ToString(),
                    submitted,
                    total,
                    percent
                );
            }
            return result;
        }

        public DataTable GetStudentsByDivision(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT 
            ISNULL(sec.SectionName, 'Unassigned') AS Division,
            COUNT(DISTINCT ass.UserId)             AS StudentCount
        FROM   SubjectFaculty            SF
        JOIN   AssignStudentSubject      ass 
               ON  ass.SubjectId   = SF.SubjectId
               AND ass.SessionId   = SF.SessionId
        LEFT JOIN StudentAcademicDetails sad 
               ON  sad.UserId      = ass.UserId
        LEFT JOIN Sections               sec 
               ON  sec.SectionId   = sad.SectionId
        WHERE  SF.TeacherId   = @TeacherId
          AND  SF.InstituteId = @InstituteId
          AND  SF.SessionId   = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
        GROUP BY sec.SectionName
        ORDER BY StudentCount DESC
    ");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        public DataTable GetStudentAnalytics(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            COUNT(DISTINCT ass.UserId) AS TotalStudents,
            COUNT(DISTINCT SF.SubjectId) AS TotalSubjects,
            COUNT(DISTINCT sad.SectionId) AS TotalDivisions,
            ISNULL(
                CAST(ROUND(
                    CAST(COUNT(DISTINCT ass.UserId) AS FLOAT) / 
                    NULLIF(COUNT(DISTINCT SF.SubjectId), 0)
                , 1) AS VARCHAR), '0'
            ) AS AvgStudentsPerSubject
        FROM   SubjectFaculty            SF
        JOIN   AssignStudentSubject      ass 
               ON  ass.SubjectId   = SF.SubjectId
               AND ass.SessionId   = SF.SessionId
        LEFT JOIN StudentAcademicDetails sad 
               ON  sad.UserId      = ass.UserId
        WHERE  SF.TeacherId   = @TeacherId
          AND  SF.InstituteId = @InstituteId
          AND  SF.SessionId   = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
    ");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        public string GetDivisionChartJson(DataTable dt)
        {
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
                list.Add(new
                {
                    Division = row["Division"].ToString(),
                    StudentCount = Convert.ToInt32(row["StudentCount"])
                });
            return new JavaScriptSerializer().Serialize(list);
        }

        // ════════════════════════════════════════════════════════════
        // STUDENT PERFORMANCE — KPI SUMMARY
        // ════════════════════════════════════════════════════════════
        public DataTable GetPerformanceKPIs(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            ISNULL(CAST(ROUND(AVG(CAST(asub.MarksObtained AS FLOAT)), 1) AS VARCHAR), '0') AS AvgMarks,
            ISNULL(MAX(asub.MarksObtained), 0)   AS HighestMarks,
            ISNULL(MIN(asub.MarksObtained), 0)   AS LowestMarks,
            COUNT(asub.SubmissionId)              AS TotalGraded
        FROM   AssignmentSubmissions asub
        JOIN   Assignments           a    ON a.AssignmentId  = asub.AssignmentId
        JOIN   SubjectFaculty        SF   ON SF.SubjectId    = a.SubjectId
        WHERE  SF.TeacherId    = @TeacherId
          AND  SF.InstituteId  = @InstituteId
          AND  SF.SessionId    = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
          AND  asub.MarksObtained IS NOT NULL");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // TOP 5 STUDENTS
        // ════════════════════════════════════════════════════════════
        public DataTable GetTopStudents(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 5
            asub.StudentId,
            up.FullName                                           AS StudentName,
            sub.SubjectName,
            asub.MarksObtained,
            a.MaxMarks,
            ISNULL(
                CAST(ROUND(
                    CAST(asub.MarksObtained AS FLOAT) /
                    NULLIF(a.MaxMarks, 0) * 100, 0)
                AS INT), 0)                                       AS Percentage
        FROM   AssignmentSubmissions asub
        JOIN   Assignments           a    ON a.AssignmentId  = asub.AssignmentId
        JOIN   Subjects              sub  ON sub.SubjectId   = a.SubjectId
        JOIN   SubjectFaculty        SF   ON SF.SubjectId    = a.SubjectId
        JOIN   UserProfile           up   ON up.UserId       = asub.StudentId
        WHERE  SF.TeacherId    = @TeacherId
          AND  SF.InstituteId  = @InstituteId
          AND  SF.SessionId    = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
          AND  asub.MarksObtained IS NOT NULL
          AND  a.MaxMarks > 0
        ORDER BY
            CAST(asub.MarksObtained AS FLOAT) / NULLIF(a.MaxMarks, 0) DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        public DataTable GetLowPerformers(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 5
            asub.StudentId,
            up.FullName                                           AS StudentName,
            sub.SubjectName,
            asub.MarksObtained,
            a.MaxMarks,
            ISNULL(
                CAST(ROUND(
                    CAST(asub.MarksObtained AS FLOAT) /
                    NULLIF(a.MaxMarks, 0) * 100, 0)
                AS INT), 0)                                       AS Percentage
        FROM   AssignmentSubmissions asub
        JOIN   Assignments           a    ON a.AssignmentId  = asub.AssignmentId
        JOIN   Subjects              sub  ON sub.SubjectId   = a.SubjectId
        JOIN   SubjectFaculty        SF   ON SF.SubjectId    = a.SubjectId
        JOIN   UserProfile           up   ON up.UserId       = asub.StudentId
        WHERE  SF.TeacherId    = @TeacherId
          AND  SF.InstituteId  = @InstituteId
          AND  SF.SessionId    = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
          AND  asub.MarksObtained IS NOT NULL
          AND  a.MaxMarks > 0
          AND  (CAST(asub.MarksObtained AS FLOAT) / NULLIF(a.MaxMarks, 0) * 100) < 50
        ORDER BY
            CAST(asub.MarksObtained AS FLOAT) / NULLIF(a.MaxMarks, 0) ASC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // AVG MARKS PER SUBJECT
        // ════════════════════════════════════════════════════════════
        public DataTable GetAvgMarksPerSubject(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            sub.SubjectName,
            ISNULL(a.MaxMarks, 0)                                AS MaxMarks,
            CAST(ROUND(AVG(CAST(asub.MarksObtained AS FLOAT)), 1)
                 AS DECIMAL(5,1))                                AS AvgMarks
        FROM   AssignmentSubmissions asub
        JOIN   Assignments           a    ON a.AssignmentId  = asub.AssignmentId
        JOIN   Subjects              sub  ON sub.SubjectId   = a.SubjectId
        JOIN   SubjectFaculty        SF   ON SF.SubjectId    = a.SubjectId
        WHERE  SF.TeacherId    = @TeacherId
          AND  SF.InstituteId  = @InstituteId
          AND  SF.SessionId    = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
          AND  asub.MarksObtained IS NOT NULL
        GROUP BY sub.SubjectName, a.MaxMarks
        ORDER BY AvgMarks DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            DataTable dt = dl.GetDataTable(cmd);

            string[] colors = {
                "#1565c0","#0288d1","#5e35b1","#2e7d32",
                "#ef6c00","#c62828","#00838f","#4527a0"
            };
            dt.Columns.Add("Color", typeof(string));
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                row["Color"] = colors[i % colors.Length];
                i++;
            }

            return dt;
        }

        // ════════════════════════════════════════════════════════════
        // AVG MARKS CHART JSON (for pie chart)
        // ════════════════════════════════════════════════════════════
        public string GetAvgMarksChartJson(DataTable dt)
        {
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
                list.Add(new
                {
                    SubjectName = row["SubjectName"].ToString(),
                    AvgMarks = Convert.ToDouble(row["AvgMarks"]),
                    MaxMarks = Convert.ToInt32(row["MaxMarks"]),
                    Color = row["Color"].ToString()
                });

            return new JavaScriptSerializer().Serialize(list);
        }

        public DataTable GetStudentMarksDetail(int studentId, int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            asub.SubmissionId,
            a.Title                                               AS AssignmentTitle,
            sub.SubjectName,
            asub.MarksObtained,
            a.MaxMarks,
            ISNULL(
                CAST(ROUND(
                    CAST(asub.MarksObtained AS FLOAT) /
                    NULLIF(a.MaxMarks, 0) * 100, 0)
                AS INT), 0)                                       AS Percentage,
            CONVERT(VARCHAR(11), asub.SubmittedOn, 106)          AS SubmittedOn
        FROM   AssignmentSubmissions asub
        JOIN   Assignments           a    ON a.AssignmentId  = asub.AssignmentId
        JOIN   Subjects              sub  ON sub.SubjectId   = a.SubjectId
        JOIN   SubjectFaculty        SF   ON SF.SubjectId    = a.SubjectId
        WHERE  asub.StudentId  = @StudentId
          AND  SF.TeacherId    = @TeacherId
          AND  SF.InstituteId  = @InstituteId
          AND  SF.SessionId    = @SessionId
          AND  ISNULL(SF.IsActive, 1) = 1
          AND  asub.MarksObtained IS NOT NULL
        ORDER BY asub.SubmittedOn ASC");

            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // SECTION COMPARE DATA (FIXED)
        // ════════════════════════════════════════════════════════════
        public DataTable GetSectionCompareData(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            ISNULL(sec.SectionName, 'No Section') AS SectionName,
            COUNT(DISTINCT ass.UserId) AS TotalStudents,
            ISNULL(
                CAST(ROUND(
                    AVG(CAST(asub.MarksObtained AS FLOAT))
                , 1) AS DECIMAL(5,1))
            , 0) AS AvgMarks,
            ISNULL(
                CAST(ROUND(
                    100.0
                    * SUM(CASE WHEN att.Status = 'Present' THEN 1.0 ELSE 0 END)
                    / NULLIF(COUNT(att.AttendanceId), 0)
                , 1) AS DECIMAL(5,1))
            , 0) AS AttendancePct,
            ISNULL(COUNT(DISTINCT vv.ViewId), 0) AS VideoViews
        FROM SubjectFaculty SF
        JOIN AssignStudentSubject ass
            ON ass.SubjectId = SF.SubjectId
            AND ass.SessionId = SF.SessionId
            AND ass.InstituteId = SF.InstituteId
        LEFT JOIN StudentAcademicDetails sad
            ON sad.UserId = ass.UserId
            AND sad.SessionId = @SessionId
        LEFT JOIN Sections sec
            ON sec.SectionId = sad.SectionId
            AND sec.SessionId = @SessionId
        LEFT JOIN AssignmentSubmissions asub
            ON asub.StudentId = ass.UserId
            AND asub.AssignmentId IN (
                SELECT a.AssignmentId 
                FROM Assignments a 
                WHERE a.SubjectId = SF.SubjectId
                AND a.SessionId = SF.SessionId
                AND a.InstituteId = SF.InstituteId
            )
            AND asub.InstituteId = SF.InstituteId
            AND asub.SessionId = SF.SessionId
            AND asub.MarksObtained IS NOT NULL
        LEFT JOIN Attendance att
            ON att.UserId = ass.UserId
            AND att.SubjectId = SF.SubjectId
            AND att.SessionId = SF.SessionId
            AND att.InstituteId = SF.InstituteId
        LEFT JOIN VideoViews vv
            ON vv.UserId = ass.UserId
            AND vv.InstituteId = SF.InstituteId
            AND vv.SessionId = SF.SessionId
        WHERE SF.TeacherId = @TeacherId
          AND SF.InstituteId = @InstituteId
          AND SF.SessionId = @SessionId
          AND ISNULL(SF.IsActive, 1) = 1
        GROUP BY sec.SectionName
        ORDER BY AvgMarks DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // SUBJECT COMPARE DATA (FIXED)
        // ════════════════════════════════════════════════════════════
        public DataTable GetSubjectCompareData(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            S.SubjectId,
            S.SubjectName,
            COUNT(DISTINCT ass.UserId) AS TotalStudents,
            ISNULL(
                CAST(ROUND(
                    AVG(CAST(asub.MarksObtained AS FLOAT))
                , 1) AS DECIMAL(5,1))
            , 0) AS AvgMarks,
            ISNULL(
                CAST(ROUND(
                    100.0
                    * SUM(CASE WHEN att.Status = 'Present' THEN 1.0 ELSE 0 END)
                    / NULLIF(COUNT(att.AttendanceId), 0)
                , 1) AS DECIMAL(5,1))
            , 0) AS AttendancePct,
            ISNULL(COUNT(DISTINCT vv.ViewId), 0) AS VideoViews
        FROM SubjectFaculty SF
        JOIN Subjects S ON S.SubjectId = SF.SubjectId
        JOIN AssignStudentSubject ass
            ON ass.SubjectId = SF.SubjectId
            AND ass.SessionId = SF.SessionId
            AND ass.InstituteId = SF.InstituteId
        LEFT JOIN AssignmentSubmissions asub
            ON asub.StudentId = ass.UserId
            AND asub.AssignmentId IN (
                SELECT a.AssignmentId 
                FROM Assignments a 
                WHERE a.SubjectId = SF.SubjectId
                AND a.SessionId = SF.SessionId
                AND a.InstituteId = SF.InstituteId
            )
            AND asub.InstituteId = SF.InstituteId
            AND asub.SessionId = SF.SessionId
            AND asub.MarksObtained IS NOT NULL
        LEFT JOIN Attendance att
            ON att.UserId = ass.UserId
            AND att.SubjectId = SF.SubjectId
            AND att.SessionId = SF.SessionId
            AND att.InstituteId = SF.InstituteId
        LEFT JOIN VideoViews vv
            ON vv.UserId = ass.UserId
            AND vv.InstituteId = SF.InstituteId
            AND vv.SessionId = SF.SessionId
        WHERE SF.TeacherId = @TeacherId
          AND SF.InstituteId = @InstituteId
          AND SF.SessionId = @SessionId
          AND ISNULL(SF.IsActive, 1) = 1
        GROUP BY S.SubjectId, S.SubjectName
        ORDER BY AvgMarks DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // JSON SERIALIZER FOR COMPARE DATA
        // ════════════════════════════════════════════════════════════
        public string GetCompareJson(DataTable dt, string nameField)
        {
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                double avgMarks = 0, attendPct = 0;
                int videoViews = 0;
                double.TryParse(row["AvgMarks"]?.ToString(), out avgMarks);
                double.TryParse(row["AttendancePct"]?.ToString(), out attendPct);
                int.TryParse(row["VideoViews"]?.ToString(), out videoViews);

                list.Add(new
                {
                    SectionName = dt.Columns.Contains("SectionName") ? row["SectionName"].ToString() : "",
                    SubjectName = dt.Columns.Contains("SubjectName") ? row["SubjectName"].ToString() : "",
                    AvgMarks = Math.Round(avgMarks, 1),
                    AttendancePct = Math.Round(attendPct, 1),
                    VideoViews = videoViews
                });
            }
            return new JavaScriptSerializer().Serialize(list);
        }

        // ════════════════════════════════════════════════════════════        // AUTO-INSIGHT GENERATORS
        // ════════════════════════════════════════════════════════════
        public DataTable GenerateSectionInsights(DataTable dt)
        {
            DataTable result = BuildInsightTable();
            if (dt == null || dt.Rows.Count < 2) return result;

            double maxM = double.MinValue, minM = double.MaxValue;
            double maxA = double.MinValue, minA = double.MaxValue;
            string bestM = "", worstA = "", bestA = "";

            foreach (DataRow row in dt.Rows)
            {
                double m = 0, a = 0;
                double.TryParse(row["AvgMarks"]?.ToString(), out m);
                double.TryParse(row["AttendancePct"]?.ToString(), out a);
                string name = row["SectionName"].ToString();

                if (!string.IsNullOrEmpty(name))
                {
                    if (m > maxM) { maxM = m; bestM = name; }
                    if (m < minM) { minM = m; }
                    if (a > maxA) { maxA = a; bestA = name; }
                    if (a < minA) { minA = a; worstA = name; }
                }
            }

            if (!string.IsNullOrEmpty(bestM) && maxM > 0)
                result.Rows.Add(bestM + " leads in avg marks (" + maxM + ")", "good", "fa-trophy");

            if (!string.IsNullOrEmpty(worstA) && minA < 60)
                result.Rows.Add(worstA + " needs attendance attention (" + minA + "%)", "warn", "fa-exclamation-triangle");

            if (!string.IsNullOrEmpty(bestA) && maxA > 0)
                result.Rows.Add(bestA + " has the best attendance (" + maxA + "%)", "good", "fa-clipboard-check");

            return result;
        }

        public DataTable GenerateSubjectInsights(DataTable dt)
        {
            DataTable result = BuildInsightTable();
            if (dt == null || dt.Rows.Count < 2) return result;

            double maxM = double.MinValue, minM = double.MaxValue;
            double maxA = double.MinValue;
            int maxV = 0;
            string bestM = "", worstM = "", bestA = "", mostWatched = "";

            foreach (DataRow row in dt.Rows)
            {
                double m = 0, a = 0;
                int v = 0;
                double.TryParse(row["AvgMarks"]?.ToString(), out m);
                double.TryParse(row["AttendancePct"]?.ToString(), out a);
                int.TryParse(row["VideoViews"]?.ToString(), out v);
                string name = row["SubjectName"].ToString();

                if (!string.IsNullOrEmpty(name))
                {
                    if (m > maxM) { maxM = m; bestM = name; }
                    if (m < minM) { minM = m; worstM = name; }
                    if (a > maxA) { maxA = a; bestA = name; }
                    if (v > maxV) { maxV = v; mostWatched = name; }
                }
            }

            if (!string.IsNullOrEmpty(bestM) && maxM > 0)
                result.Rows.Add(bestM + " has highest avg marks (" + maxM + ")", "good", "fa-star");

            if (!string.IsNullOrEmpty(worstM) && worstM != bestM && minM < 40 && minM != double.MaxValue)
                result.Rows.Add(worstM + " avg marks are low (" + minM + ") — review needed", "warn", "fa-exclamation-triangle");

            if (!string.IsNullOrEmpty(bestA) && maxA > 0)
                result.Rows.Add(bestA + " leads in attendance (" + maxA + "%)", "good", "fa-clipboard-check");

            if (!string.IsNullOrEmpty(mostWatched) && maxV > 0)
                result.Rows.Add(mostWatched + " is most engaged (" + maxV + " views)", "good", "fa-play-circle");

            return result;
        }

        private DataTable BuildInsightTable()
        {
            DataTable t = new DataTable();
            t.Columns.Add("Message");
            t.Columns.Add("CssClass");
            t.Columns.Add("Icon");
            return t;
        }

        // ════════════════════════════════════════════════════════════
        // CONTENT ENGAGEMENT METHODS
        // ════════════════════════════════════════════════════════════
        public DataTable GetEngagementKPIs(int teacherId, int instituteId, int sessionId, int subjectId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            ISNULL(COUNT(DISTINCT vv.ViewId), 0) AS TotalViews,
            ISNULL(
                CAST(ROUND(
                    AVG(CAST(vwp.WatchedPercent AS FLOAT))
                , 1) AS DECIMAL(5,1))
            , 0) AS AvgWatchPercent,
            ISNULL((
                SELECT TOP 1 v.Title
                FROM VideoViews vv2
                JOIN Videos v ON v.VideoId = vv2.VideoId
                JOIN Chapters ch ON ch.ChapterId = v.ChapterId
                WHERE vv2.InstituteId = @InstituteId
                  AND vv2.SessionId = @SessionId
                  AND v.IsActive = 1
                  AND (@SubjectId = 0 OR ch.SubjectId = @SubjectId)
                GROUP BY v.Title
                ORDER BY COUNT(*) DESC
            ), '-') AS MostViewedVideo,
            ISNULL((
                SELECT TOP 1 COUNT(*)
                FROM VideoViews vv2
                JOIN Videos v ON v.VideoId = vv2.VideoId
                JOIN Chapters ch ON ch.ChapterId = v.ChapterId
                WHERE vv2.InstituteId = @InstituteId
                  AND vv2.SessionId = @SessionId
                  AND v.IsActive = 1
                  AND (@SubjectId = 0 OR ch.SubjectId = @SubjectId)
                GROUP BY v.Title
                ORDER BY COUNT(*) DESC
            ), 0) AS MostViewedCount,
            COUNT(DISTINCT v.VideoId) AS TotalVideos,
            '+0%' AS ViewsTrend
        FROM Videos v
        JOIN Chapters ch ON ch.ChapterId = v.ChapterId
        LEFT JOIN VideoViews vv ON vv.VideoId = v.VideoId
            AND vv.InstituteId = @InstituteId
            AND vv.SessionId = @SessionId
        LEFT JOIN VideoWatchProgress vwp ON vwp.VideoId = v.VideoId
            AND vwp.InstituteId = @InstituteId
            AND vwp.SessionId = @SessionId
        WHERE v.InstituteId = @InstituteId
          AND v.SessionId = @SessionId
          AND v.IsActive = 1
          AND (@SubjectId = 0 OR ch.SubjectId = @SubjectId)");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@SubjectId", subjectId);
            return dl.GetDataTable(cmd);
        }

        public DataTable GetVideoEngagementData(int teacherId, int instituteId, int sessionId, int subjectId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 10
            v.Title AS VideoName,
            ISNULL(
                CAST(ROUND(
                    AVG(CAST(vwp.WatchedPercent AS FLOAT))
                , 1) AS DECIMAL(5,1))
            , 0) AS WatchPercent,
            ISNULL(COUNT(DISTINCT vv.ViewId), 0) AS ViewCount
        FROM Videos v
        JOIN Chapters ch ON ch.ChapterId = v.ChapterId
        LEFT JOIN VideoViews vv ON vv.VideoId = v.VideoId
            AND vv.InstituteId = @InstituteId
            AND vv.SessionId = @SessionId
        LEFT JOIN VideoWatchProgress vwp ON vwp.VideoId = v.VideoId
            AND vwp.InstituteId = @InstituteId
            AND vwp.SessionId = @SessionId
        WHERE v.InstituteId = @InstituteId
          AND v.SessionId = @SessionId
          AND v.IsActive = 1
          AND (@SubjectId = 0 OR ch.SubjectId = @SubjectId)
        GROUP BY v.Title, v.VideoId
        ORDER BY AVG(CAST(vwp.WatchedPercent AS FLOAT)) DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@SubjectId", subjectId);
            return dl.GetDataTable(cmd);
        }

        public string GetEngagementChartJson(DataTable dt)
        {
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    VideoName = row["VideoName"].ToString(),
                    WatchPercent = Convert.ToDouble(row["WatchPercent"]),
                    ViewCount = Convert.ToInt32(row["ViewCount"])
                });
            }
            return new JavaScriptSerializer().Serialize(list);
        }

        public DataTable GetTopVideos(int teacherId, int instituteId, int sessionId, int subjectId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 5
            v.VideoId,
            v.Title,
            ISNULL(COUNT(DISTINCT vv.ViewId), 0) AS ViewCount,
            ISNULL(
                CAST(ROUND(
                    AVG(CAST(vwp.WatchedPercent AS FLOAT))
                , 1) AS DECIMAL(5,1))
            , 0) AS WatchPercent
        FROM Videos v
        JOIN Chapters ch ON ch.ChapterId = v.ChapterId
        LEFT JOIN VideoViews vv ON vv.VideoId = v.VideoId
            AND vv.InstituteId = @InstituteId
            AND vv.SessionId = @SessionId
        LEFT JOIN VideoWatchProgress vwp ON vwp.VideoId = v.VideoId
            AND vwp.InstituteId = @InstituteId
            AND vwp.SessionId = @SessionId
        WHERE v.InstituteId = @InstituteId
          AND v.SessionId = @SessionId
          AND v.IsActive = 1
          AND (@SubjectId = 0 OR ch.SubjectId = @SubjectId)
        GROUP BY v.VideoId, v.Title
        ORDER BY COUNT(DISTINCT vv.ViewId) DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@SubjectId", subjectId);
            return dl.GetDataTable(cmd);
        }

        public DataTable GetLowPerformingVideos(int teacherId, int instituteId, int sessionId, int subjectId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 5
            v.VideoId,
            v.Title,
            ISNULL(COUNT(DISTINCT vv.ViewId), 0) AS ViewCount,
            ISNULL(
                CAST(ROUND(
                    AVG(CAST(vwp.WatchedPercent AS FLOAT))
                , 1) AS DECIMAL(5,1))
            , 0) AS WatchPercent
        FROM Videos v
        JOIN Chapters ch ON ch.ChapterId = v.ChapterId
        LEFT JOIN VideoViews vv ON vv.VideoId = v.VideoId
            AND vv.InstituteId = @InstituteId
            AND vv.SessionId = @SessionId
        LEFT JOIN VideoWatchProgress vwp ON vwp.VideoId = v.VideoId
            AND vwp.InstituteId = @InstituteId
            AND vwp.SessionId = @SessionId
        WHERE v.InstituteId = @InstituteId
          AND v.SessionId = @SessionId
          AND v.IsActive = 1
          AND (@SubjectId = 0 OR ch.SubjectId = @SubjectId)
        GROUP BY v.VideoId, v.Title
        HAVING COUNT(DISTINCT vv.ViewId) > 0
        ORDER BY AVG(CAST(vwp.WatchedPercent AS FLOAT)) ASC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@SubjectId", subjectId);
            return dl.GetDataTable(cmd);
        }

        public DataTable GetWatchTimeLeaders(int teacherId, int instituteId, int sessionId, int subjectId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 5
            up.FullName AS StudentName,
            ISNULL(SUM(vwp.WatchedSeconds), 0) AS TotalWatchSeconds,
            ISNULL(
                CAST(ROUND(
                    AVG(CAST(vwp.WatchedPercent AS FLOAT))
                , 1) AS DECIMAL(5,1))
            , 0) AS AvgWatchPercent
        FROM VideoWatchProgress vwp
        JOIN Videos v ON v.VideoId = vwp.VideoId
        JOIN Chapters ch ON ch.ChapterId = v.ChapterId
        JOIN UserProfile up ON up.UserId = vwp.UserId
        WHERE vwp.InstituteId = @InstituteId
          AND vwp.SessionId = @SessionId
          AND v.IsActive = 1
          AND (@SubjectId = 0 OR ch.SubjectId = @SubjectId)
        GROUP BY up.FullName
        ORDER BY SUM(vwp.WatchedSeconds) DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@SubjectId", subjectId);
            return dl.GetDataTable(cmd);
        }

        // ════════════════════════════════════════════════════════════
        // LEARNING PATH PROGRESS (CORRECTED)
        // ════════════════════════════════════════════════════════════
        public DataTable GetLearningPathProgress(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
SELECT
    S.SubjectId,
    S.SubjectName,
    COUNT(DISTINCT ch.ChapterId) AS TotalChapters,

    SUM(CASE
        WHEN chapter_views.ViewCount > 0 THEN 1
        ELSE 0
    END) AS ChaptersCovered,

    COUNT(DISTINCT v.VideoId) AS TotalVideos,

    SUM(CASE
        WHEN video_views.ViewCount > 0 THEN 1
        ELSE 0
    END) AS VideosWatched,

    ISNULL(
        CAST(ROUND(
            AVG(CASE
                WHEN vwp.WatchedPercent IS NOT NULL
                THEN CAST(vwp.WatchedPercent AS FLOAT)
                ELSE NULL
            END)
        , 1) AS DECIMAL(5,1))
    , 0) AS AvgWatchPercent,

    ISNULL(
        CASE
            WHEN COUNT(DISTINCT ch.ChapterId) = 0 THEN 0
            ELSE CAST(ROUND(100.0 *
                SUM(CASE WHEN chapter_views.ViewCount > 0 THEN 1 ELSE 0 END)
                / COUNT(DISTINCT ch.ChapterId), 1) AS DECIMAL(5,1))
        END
    , 0) AS SyllabusCompletionPct,

    ISNULL((
        SELECT COUNT(DISTINCT ass2.UserId)
        FROM AssignStudentSubject ass2
        WHERE ass2.SubjectId   = S.SubjectId
          AND ass2.SessionId   = @SessionId
          AND ass2.InstituteId = @InstituteId
    ), 0) AS StudentCount

FROM SubjectFaculty SF
JOIN Subjects S ON S.SubjectId = SF.SubjectId
LEFT JOIN Chapters ch
    ON ch.SubjectId = SF.SubjectId
    AND ch.SessionId = SF.SessionId
LEFT JOIN Videos v
    ON v.ChapterId = ch.ChapterId
    AND v.IsActive = 1
    AND v.SessionId = SF.SessionId
LEFT JOIN (
    SELECT
        ch2.ChapterId,
        COUNT(DISTINCT vv.ViewId) AS ViewCount
    FROM Chapters ch2
    JOIN Videos v2 ON v2.ChapterId = ch2.ChapterId AND v2.IsActive = 1
    LEFT JOIN VideoViews vv
        ON vv.VideoId = v2.VideoId
        AND vv.InstituteId = @InstituteId
        AND vv.SessionId = @SessionId
    WHERE ch2.SessionId = @SessionId
    GROUP BY ch2.ChapterId
) chapter_views ON chapter_views.ChapterId = ch.ChapterId
LEFT JOIN (
    SELECT
        v3.VideoId,
        COUNT(DISTINCT vv2.ViewId) AS ViewCount
    FROM Videos v3
    LEFT JOIN VideoViews vv2
        ON vv2.VideoId = v3.VideoId
        AND vv2.InstituteId = @InstituteId
        AND vv2.SessionId = @SessionId
    WHERE v3.IsActive = 1 AND v3.SessionId = @SessionId
    GROUP BY v3.VideoId
) video_views ON video_views.VideoId = v.VideoId
LEFT JOIN VideoWatchProgress vwp
    ON vwp.VideoId = v.VideoId
    AND vwp.InstituteId = @InstituteId
    AND vwp.SessionId = @SessionId
WHERE SF.TeacherId   = @TeacherId
  AND SF.InstituteId = @InstituteId
  AND SF.SessionId   = @SessionId
  AND ISNULL(SF.IsActive, 1) = 1
GROUP BY S.SubjectId, S.SubjectName
ORDER BY SyllabusCompletionPct DESC
");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }        // ════════════════════════════════════════════════════════════
        // LEARNING PATH SUMMARY (CORRECTED)
        // ════════════════════════════════════════════════════════════
        public DataTable GetLearningPathSummary(int teacherId, int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand(@"
SELECT
    ISNULL((
        SELECT COUNT(DISTINCT ch.ChapterId)
        FROM SubjectFaculty SF
        JOIN Chapters ch ON ch.SubjectId = SF.SubjectId AND ch.SessionId = SF.SessionId
        WHERE SF.TeacherId = @TeacherId
          AND SF.InstituteId = @InstituteId
          AND SF.SessionId = @SessionId
          AND ISNULL(SF.IsActive, 1) = 1
    ), 0) AS TotalChapters,

    ISNULL((
        SELECT COUNT(DISTINCT v.VideoId)
        FROM SubjectFaculty SF
        JOIN Chapters ch ON ch.SubjectId = SF.SubjectId AND ch.SessionId = SF.SessionId
        JOIN Videos v ON v.ChapterId = ch.ChapterId AND v.IsActive = 1 AND v.SessionId = SF.SessionId
        WHERE SF.TeacherId = @TeacherId
          AND SF.InstituteId = @InstituteId
          AND SF.SessionId = @SessionId
          AND ISNULL(SF.IsActive, 1) = 1
    ), 0) AS TotalVideos,

    ISNULL(
        CAST(ROUND((
            SELECT AVG(CAST(vwp.WatchedPercent AS FLOAT))
            FROM SubjectFaculty SF
            JOIN Chapters ch ON ch.SubjectId = SF.SubjectId AND ch.SessionId = SF.SessionId
            JOIN Videos v ON v.ChapterId = ch.ChapterId AND v.IsActive = 1 AND v.SessionId = SF.SessionId
            JOIN VideoWatchProgress vwp ON vwp.VideoId = v.VideoId
                AND vwp.InstituteId = @InstituteId
                AND vwp.SessionId = @SessionId
            WHERE SF.TeacherId = @TeacherId
              AND SF.InstituteId = @InstituteId
              AND SF.SessionId = @SessionId
              AND ISNULL(SF.IsActive, 1) = 1
        ), 1) AS DECIMAL(5,1))
    , 0) AS OverallAvgWatch,

    ISNULL(
        CASE
            WHEN (
                SELECT COUNT(DISTINCT v.VideoId)
                FROM SubjectFaculty SF
                JOIN Chapters ch ON ch.SubjectId = SF.SubjectId AND ch.SessionId = SF.SessionId
                JOIN Videos v ON v.ChapterId = ch.ChapterId AND v.IsActive = 1 AND v.SessionId = SF.SessionId
                WHERE SF.TeacherId = @TeacherId
                  AND SF.InstituteId = @InstituteId
                  AND SF.SessionId = @SessionId
                  AND ISNULL(SF.IsActive, 1) = 1
            ) = 0 THEN 0
            ELSE CAST(ROUND(100.0 * (
                SELECT COUNT(DISTINCT v2.VideoId)
                FROM SubjectFaculty SF2
                JOIN Chapters ch2 ON ch2.SubjectId = SF2.SubjectId AND ch2.SessionId = SF2.SessionId
                JOIN Videos v2 ON v2.ChapterId = ch2.ChapterId AND v2.IsActive = 1 AND v2.SessionId = SF2.SessionId
                JOIN VideoViews vv ON vv.VideoId = v2.VideoId
                    AND vv.InstituteId = @InstituteId
                    AND vv.SessionId = @SessionId
                WHERE SF2.TeacherId = @TeacherId
                  AND SF2.InstituteId = @InstituteId
                  AND SF2.SessionId = @SessionId
                  AND ISNULL(SF2.IsActive, 1) = 1
            ) / (
                SELECT COUNT(DISTINCT v3.VideoId)
                FROM SubjectFaculty SF3
                JOIN Chapters ch3 ON ch3.SubjectId = SF3.SubjectId AND ch3.SessionId = SF3.SessionId
                JOIN Videos v3 ON v3.ChapterId = ch3.ChapterId AND v3.IsActive = 1 AND v3.SessionId = SF3.SessionId
                WHERE SF3.TeacherId = @TeacherId
                  AND SF3.InstituteId = @InstituteId
                  AND SF3.SessionId = @SessionId
                  AND ISNULL(SF3.IsActive, 1) = 1
            ), 1) AS DECIMAL(5,1))
        END
    , 0) AS VideoCompletionPct
");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            return dl.GetDataTable(cmd);
        }
        // ════════════════════════════════════════════════════════════
        // ACTIVITY TRACKING (CORRECTED for your schema - using ActionTime)
        // ════════════════════════════════════════════════════════════

        public DataTable GetActivityKPIs(int teacherId, int instituteId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            (
                SELECT COUNT(*)
                FROM   UserActivityLog
                WHERE  UserId      = @TeacherId
                  AND  InstituteId = @InstituteId
                  AND  CAST(ActionTime AS DATE) = CAST(GETDATE() AS DATE)
            ) AS TodayCount,
            (
                SELECT COUNT(*)
                FROM   UserActivityLog
                WHERE  UserId      = @TeacherId
                  AND  InstituteId = @InstituteId
                  AND  ActionTime >= DATEADD(DAY, -6, CAST(GETDATE() AS DATE))
            ) AS WeekCount,
            ISNULL(
                (
                    SELECT TOP 1
                        CONVERT(VARCHAR(16), ActionTime, 120)
                    FROM   UserActivityLog
                    WHERE  UserId      = @TeacherId
                      AND  InstituteId = @InstituteId
                    ORDER  BY ActionTime DESC
                ), 'No activity yet'
            ) AS LastActive,
            (
                SELECT COUNT(DISTINCT CAST(ActionTime AS DATE))
                FROM   UserActivityLog
                WHERE  UserId      = @TeacherId
                  AND  InstituteId = @InstituteId
                  AND  MONTH(ActionTime) = MONTH(GETDATE())
                  AND  YEAR(ActionTime)  = YEAR(GETDATE())
            ) AS ActiveDaysThisMonth");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            return dl.GetDataTable(cmd);
        }

        public DataTable GetActivityTrend(int teacherId, int instituteId)
        {
            SqlCommand cmd = new SqlCommand(@"
        WITH Days AS (
            SELECT TOP 7
                CAST(DATEADD(DAY, -n, CAST(GETDATE() AS DATE)) AS DATE) AS ActivityDate
            FROM (
                SELECT 0 AS n UNION SELECT 1 UNION SELECT 2 UNION SELECT 3
                UNION SELECT 4 UNION SELECT 5 UNION SELECT 6
            ) nums
        )
        SELECT
            d.ActivityDate,
            DATENAME(WEEKDAY, d.ActivityDate) AS DayLabel,
            ISNULL(COUNT(ual.LogId), 0) AS ActionCount
        FROM Days d
        LEFT JOIN UserActivityLog ual
               ON CAST(ual.ActionTime AS DATE) = d.ActivityDate
              AND ual.UserId = @TeacherId
              AND ual.InstituteId = @InstituteId
        GROUP BY d.ActivityDate
        ORDER BY d.ActivityDate ASC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            return dl.GetDataTable(cmd);
        }

        public string GetActivityTrendJson(DataTable dt)
        {
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                string day = row["DayLabel"].ToString();
                if (day.Length > 3) day = day.Substring(0, 3);
                list.Add(new
                {
                    DayLabel = day,
                    ActionCount = Convert.ToInt32(row["ActionCount"])
                });
            }
            return new JavaScriptSerializer().Serialize(list);
        }

        public DataTable GetRecentActivityLog(int teacherId, int instituteId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 8
            ual.LogId,
            ISNULL(ual.ActivityType, 'General') AS ActionType,
            ISNULL(ual.ActivityType, 'Activity') AS ActionDescription,
            CONVERT(VARCHAR(11), ual.ActionTime, 106) AS ActionDate,
            CASE
                WHEN DATEDIFF(MINUTE, ual.ActionTime, GETDATE()) < 1
                    THEN 'Just now'
                WHEN DATEDIFF(MINUTE, ual.ActionTime, GETDATE()) < 60
                    THEN CAST(DATEDIFF(MINUTE, ual.ActionTime, GETDATE()) AS VARCHAR) + ' min ago'
                WHEN DATEDIFF(HOUR, ual.ActionTime, GETDATE()) < 24
                    THEN CAST(DATEDIFF(HOUR, ual.ActionTime, GETDATE()) AS VARCHAR) + ' hr ago'
                WHEN DATEDIFF(DAY, ual.ActionTime, GETDATE()) < 7
                    THEN CAST(DATEDIFF(DAY, ual.ActionTime, GETDATE()) AS VARCHAR) + ' days ago'
                ELSE CONVERT(VARCHAR(11), ual.ActionTime, 106)
            END AS TimeAgo
        FROM UserActivityLog ual
        WHERE ual.UserId = @TeacherId
          AND ual.InstituteId = @InstituteId
        ORDER BY ual.ActionTime DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            return dl.GetDataTable(cmd);
        }

        public DataTable GetActivityBreakdown(int teacherId, int instituteId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT
            ISNULL(ActivityType, 'General') AS ActionType,
            COUNT(*) AS ActionCount
        FROM UserActivityLog
        WHERE UserId = @TeacherId
          AND InstituteId = @InstituteId
          AND ActionTime >= DATEADD(DAY, -29, GETDATE())
        GROUP BY ActivityType
        ORDER BY ActionCount DESC");

            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            cmd.Parameters.AddWithValue("@InstituteId", instituteId);
            return dl.GetDataTable(cmd);
        }
    }
}