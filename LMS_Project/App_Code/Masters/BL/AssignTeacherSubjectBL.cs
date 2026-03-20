using LearningManagementSystem.GC;
using System;
using System.Data;
using System.Data.SqlClient;

namespace LearningManagementSystem.BL
{
    public class AssignTeacherSubjectBL
    {
        DataLayer dl = new DataLayer();

        // ================= GET SUBJECTS =================
        public DataTable GetSubjects(int instituteId, int sessionId,
            int streamId, int courseId, int levelId, int semesterId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = @"
            SELECT
            S.SubjectId,
            S.SubjectName,
            LSS.IsMandatory

            FROM LevelSemesterSubjects LSS
            JOIN Subjects S ON LSS.SubjectId = S.SubjectId

            WHERE
            S.IsActive = 1
            AND LSS.InstituteId = @Institute
            AND LSS.SessionId = @Session
            AND LSS.StreamId = @Stream
            AND LSS.LevelId = @Level
            AND (@Course = 0 OR LSS.CourseId = @Course)
            AND (@Semester = 0 OR LSS.SemesterId = @Semester)

            ORDER BY S.SubjectName";

            cmd.Parameters.AddWithValue("@Institute", instituteId);
            cmd.Parameters.AddWithValue("@Session", sessionId);
            cmd.Parameters.AddWithValue("@Stream", streamId);
            cmd.Parameters.AddWithValue("@Course", courseId);
            cmd.Parameters.AddWithValue("@Level", levelId);
            cmd.Parameters.AddWithValue("@Semester", semesterId);

            return dl.GetDataTable(cmd);
        }

        // ================= GET TEACHERS =================
        public DataTable GetTeachers(int instituteId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = @"
            SELECT
            U.UserId,
            P.FullName

            FROM Users U
            JOIN UserProfile P ON U.UserId = P.UserId

            WHERE
            U.InstituteId = @Institute
            AND U.RoleId = 2

            ORDER BY P.FullName";

            cmd.Parameters.AddWithValue("@Institute", instituteId);

            return dl.GetDataTable(cmd);
        }

        // ================= ASSIGN SUBJECT =================
        public void AssignTeacherSubject(AssignTeacherSubjectGC obj)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = @"
            INSERT INTO AssignTeacherSubject
            (
            TeacherId,
            SocietyId,
            InstituteId,
            SessionId,
            StreamId,
            CourseId,
            LevelId,
            SemesterId,
            SectionId,
            SubjectId
            )
            VALUES
            (
            @TeacherId,
            @SocietyId,
            @InstituteId,
            @SessionId,
            @StreamId,
            @CourseId,
            @LevelId,
            @SemesterId,
            @SectionId,
            @SubjectId
            )";

            cmd.Parameters.AddWithValue("@TeacherId", obj.TeacherId);
            cmd.Parameters.AddWithValue("@SocietyId", obj.SocietyId);
            cmd.Parameters.AddWithValue("@InstituteId", obj.InstituteId);
            cmd.Parameters.AddWithValue("@SessionId", obj.SessionId);
            cmd.Parameters.AddWithValue("@StreamId", obj.StreamId);
            cmd.Parameters.AddWithValue("@CourseId", obj.CourseId);
            cmd.Parameters.AddWithValue("@LevelId", obj.LevelId);
            cmd.Parameters.AddWithValue("@SemesterId", obj.SemesterId);
            cmd.Parameters.AddWithValue("@SectionId", obj.SectionId);
            cmd.Parameters.AddWithValue("@SubjectId", obj.SubjectId);

            dl.ExecuteCMD(cmd);
        }

        // ================= SHOW ASSIGNED SUBJECTS =================
        public DataTable GetAssignedTeacherSubjects(int instituteId, int sessionId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = @"
            SELECT
            A.Id,
            P.FullName,
            S.SubjectName,
            SC.SectionName,
            AC.SessionName

            FROM AssignTeacherSubject A
            JOIN Users U ON A.TeacherId = U.UserId
            JOIN UserProfile P ON U.UserId = P.UserId
            JOIN Subjects S ON A.SubjectId = S.SubjectId
            JOIN Sections SC ON A.SectionId = SC.SectionId
            JOIN AcademicSessions AC ON A.SessionId = AC.SessionId

            WHERE
            A.InstituteId = @Institute
            AND A.SessionId = @Session

            ORDER BY P.FullName, S.SubjectName";

            cmd.Parameters.AddWithValue("@Institute", instituteId);
            cmd.Parameters.AddWithValue("@Session", sessionId);

            return dl.GetDataTable(cmd);
        }

        // ================= DELETE ASSIGNMENT =================
        public void DeleteAssignedTeacherSubject(int id)
        {
            SqlCommand cmd = new SqlCommand(
            "DELETE FROM AssignTeacherSubject WHERE Id=@Id");

            cmd.Parameters.AddWithValue("@Id", id);

            dl.ExecuteCMD(cmd);
        }

        // ================= STREAMS =================
        public DataTable GetStreams(int instituteId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText =
            "SELECT StreamId,StreamName FROM Streams WHERE InstituteId=@I AND IsActive=1";

            cmd.Parameters.AddWithValue("@I", instituteId);

            return dl.GetDataTable(cmd);
        }

        // ================= COURSES =================
        public DataTable GetCourses(int streamId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText =
            "SELECT CourseId,CourseName FROM Courses WHERE StreamId=@S AND IsActive=1";

            cmd.Parameters.AddWithValue("@S", streamId);

            return dl.GetDataTable(cmd);
        }

        // ================= LEVELS =================
        public DataTable GetLevels(int instituteId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText =
            "SELECT LevelId,LevelName FROM StudyLevels WHERE InstituteId=@I";

            cmd.Parameters.AddWithValue("@I", instituteId);

            return dl.GetDataTable(cmd);
        }

        // ================= SEMESTERS =================
        public DataTable GetSemesters(int instituteId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText =
            "SELECT SemesterId,SemesterName FROM Semesters WHERE InstituteId=@I";

            cmd.Parameters.AddWithValue("@I", instituteId);

            return dl.GetDataTable(cmd);
        }

        // ================= SECTIONS =================
        public DataTable GetSections(int instituteId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText =
            "SELECT SectionId,SectionName FROM Sections WHERE InstituteId=@I AND IsActive=1";

            cmd.Parameters.AddWithValue("@I", instituteId);

            return dl.GetDataTable(cmd);
        }

        // ================= CURRENT SESSION =================
        public int GetCurrentSession(int instituteId)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText =
            "SELECT SessionId FROM AcademicSessions WHERE InstituteId=@I AND IsCurrent=1";

            cmd.Parameters.AddWithValue("@I", instituteId);

            DataTable dt = dl.GetDataTable(cmd);

            if (dt.Rows.Count > 0)
                return Convert.ToInt32(dt.Rows[0]["SessionId"]);

            return 0;
        }
    }
}