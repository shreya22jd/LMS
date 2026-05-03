<%@ Page Title="Subject Analytics" Language="C#" MasterPageFile="~/Teacher/TeacherMaster.master"
    AutoEventWireup="true" CodeBehind="SubjectAnalytics.aspx.cs"
    Inherits="LMS_Project.Teacher.SubjectAnalytics" %>

<asp:Content ID="bodyContent" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">

<asp:HiddenField ID="hfSubjectId" runat="server" />
<asp:Label ID="lblMsg" runat="server" CssClass="alert d-block mt-3" Visible="false"></asp:Label>

<!-- HEADER -->
<div class="d-flex align-items-center justify-content-between mb-4 flex-wrap gap-2">
    <div class="d-flex align-items-center gap-3">
        <a href='SubjectDetails.aspx?SubjectId=<%= Request.QueryString["SubjectId"] %>' class="btn btn-outline-secondary">
            <i class="fa-solid fa-arrow-left"></i>
        </a>
        <div>
            <small class="text-muted text-uppercase fw-bold" style="letter-spacing:.06em">Subject Analytics</small>
            <h3 class="mb-0 fw-bold">
                <asp:Literal ID="litSubjectName" runat="server" />
            </h3>
        </div>
    </div>
    <div class="d-flex gap-2 align-items-center">
        <span class="badge bg-light text-secondary border px-3 py-2">
            <i class="fa-solid fa-hashtag me-1"></i><asp:Literal ID="litSubjectCode" runat="server" />
        </span>
        <asp:Literal ID="litStatusBadge" runat="server" />
    </div>
</div>

<!-- KPI ROW 1 -->
<div class="row g-3 mb-3">
    <div class="col-6 col-md-3">
        <div class="kpi-card kpi-blue">
            <div class="kpi-icon"><i class="fa-solid fa-users"></i></div>
            <div class="kpi-val"><asp:Literal ID="litTotalStudents" runat="server" /></div>
            <div class="kpi-lbl">Enrolled Students</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="kpi-card kpi-indigo">
            <div class="kpi-icon"><i class="fa-solid fa-book-open"></i></div>
            <div class="kpi-val"><asp:Literal ID="litTotalChapters" runat="server" /></div>
            <div class="kpi-lbl">Chapters</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="kpi-card kpi-red">
            <div class="kpi-icon"><i class="fa-regular fa-circle-play"></i></div>
            <div class="kpi-val"><asp:Literal ID="litTotalVideos" runat="server" /></div>
            <div class="kpi-lbl">Video Lectures</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="kpi-card kpi-teal">
            <div class="kpi-icon"><i class="fa-solid fa-file-lines"></i></div>
            <div class="kpi-val"><asp:Literal ID="litTotalMaterials" runat="server" /></div>
            <div class="kpi-lbl">Materials</div>
        </div>
    </div>
</div>

<!-- KPI ROW 2 -->
<div class="row g-3 mb-4">
    <div class="col-6 col-md-3">
        <div class="kpi-card kpi-orange">
            <div class="kpi-icon"><i class="fa-solid fa-tasks"></i></div>
            <div class="kpi-val"><asp:Literal ID="litTotalAssignments" runat="server" /></div>
            <div class="kpi-lbl">Assignments</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="kpi-card kpi-purple">
            <div class="kpi-icon"><i class="fa-solid fa-circle-check"></i></div>
            <div class="kpi-val"><asp:Literal ID="litTotalSubmissions" runat="server" /></div>
            <div class="kpi-lbl">Total Submissions</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="kpi-card kpi-green">
            <div class="kpi-icon"><i class="fa-solid fa-chart-simple"></i></div>
            <div class="kpi-val"><asp:Literal ID="litAvgMarks" runat="server" /></div>
            <div class="kpi-lbl">Avg Assignment Score</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="kpi-card kpi-pink">
            <div class="kpi-icon"><i class="fa-solid fa-percent"></i></div>
            <div class="kpi-val"><asp:Literal ID="litAvgAttendance" runat="server" /></div>
            <div class="kpi-lbl">Avg Attendance %</div>
        </div>
    </div>
</div>

<!-- CHARTS ROW 1 -->
<div class="row g-4 mb-4">
    <div class="col-md-8">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-white border-bottom fw-semibold py-3">
                <i class="fa-solid fa-calendar-check me-2 text-primary"></i>Attendance by Date (Last 30 Days)
            </div>
            <div class="card-body">
                <canvas id="attendanceChart" height="110"></canvas>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-white border-bottom fw-semibold py-3">
                <i class="fa-solid fa-circle-half-stroke me-2 text-info"></i>Attendance Split
            </div>
            <div class="card-body d-flex flex-column align-items-center justify-content-center">
                <canvas id="attendanceDoughnut" style="max-width:200px;max-height:200px;"></canvas>
                <div id="doughnutLegend" class="d-flex gap-3 mt-3 flex-wrap justify-content-center"></div>
            </div>
        </div>
    </div>
</div>

<!-- CHARTS ROW 2 -->
<div class="row g-4 mb-4">
    <div class="col-md-6">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-white border-bottom fw-semibold py-3">
                <i class="fa-solid fa-video me-2 text-danger"></i>Video Views per Chapter
            </div>
            <div class="card-body">
                <canvas id="videoViewsChart" height="160"></canvas>
            </div>
        </div>
    </div>
    <div class="col-md-6">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-white border-bottom fw-semibold py-3">
                <i class="fa-solid fa-tasks me-2 text-warning"></i>Assignment Submissions vs Students
            </div>
            <div class="card-body">
                <canvas id="assignmentChart" height="160"></canvas>
            </div>
        </div>
    </div>
</div>

<!-- CHARTS ROW 3 -->
<div class="row g-4 mb-4">
    <div class="col-md-7">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-white border-bottom fw-semibold py-3">
                <i class="fa-solid fa-graduation-cap me-2 text-success"></i>Quiz Avg Score %
            </div>
            <div class="card-body">
                <canvas id="quizChart" height="150"></canvas>
            </div>
        </div>
    </div>
    <div class="col-md-5">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-white border-bottom fw-semibold py-3">
                <i class="fa-solid fa-layer-group me-2 text-secondary"></i>Content per Chapter
            </div>
            <div class="card-body p-0" style="overflow-y:auto;max-height:280px;">
                <asp:Repeater ID="rptChapterContent" runat="server">
                    <ItemTemplate>
                        <div class="d-flex justify-content-between align-items-center px-3 py-2 border-bottom">
                            <span class="small text-truncate me-2" style="max-width:60%">
                                <strong><%# Container.ItemIndex+1 %>.</strong> <%# Eval("ChapterName") %>
                            </span>
                            <div class="d-flex gap-2">
                                <span class="badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25">
                                    <i class="fa-solid fa-video me-1"></i><%# Eval("VideoCount") %>
                                </span>
                                <span class="badge bg-success bg-opacity-10 text-success border border-success border-opacity-25">
                                    <i class="fa-solid fa-file me-1"></i><%# Eval("MaterialCount") %>
                                </span>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</div>

<!-- STUDENT PROGRESS TABLE -->
<div class="card border-0 shadow-sm mb-4">
    <div class="card-header bg-white border-bottom fw-semibold py-3 d-flex justify-content-between">
        <span><i class="fa-solid fa-table me-2 text-primary"></i>Student Progress</span>
        <span class="badge bg-primary bg-opacity-10 text-primary border">
            <asp:Literal ID="litTotalStudents2" runat="server" /> Students
        </span>
    </div>
    <div class="card-body p-0">
        <div class="table-responsive">
            <table class="table table-hover mb-0 align-middle" style="font-size:.84rem">
                <thead class="table-light">
                    <tr>
                        <th>#</th>
                        <th>Student</th>
                        <th>Attendance %</th>
                        <th>Assignments Done</th>
                        <th>Avg Marks</th>
                        <th>Videos Watched</th>
                        <th>Overall Progress</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptStudentProgress" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><span class="badge bg-secondary"><%# Container.ItemIndex+1 %></span></td>
                                <td>
                                    <div class="d-flex align-items-center gap-2">
<div class="student-avatar"><%# GetInitials(Eval("FullName").ToString()) %></div>
                                        <span class="fw-medium"><%# Eval("FullName") %></span>
                                    </div>
                                </td>
                                <td>
                                    <div class="d-flex align-items-center gap-2">
                                        <div class="mini-bar">
                                            <div class="mini-bar-fill bg-primary" style="width:<%# Eval("AttendancePct") %>%"></div>
                                        </div>
                                        <small class="fw-bold"><%# Eval("AttendancePct") %>%</small>
                                    </div>
                                </td>
                                <td><%# Eval("AssignmentsDone") %> / <%# Eval("TotalAssignments") %></td>
                                <td>
                                    <span class="badge <%# GetScoreClass(Eval("AvgMarks")) %>"><%# Eval("AvgMarks") %></span>
                                </td>
                                <td><%# Eval("VideosWatched") %></td>
                                <td>
                                    <div class="d-flex align-items-center gap-2">
                                        <div class="mini-bar">
                                            <div class="mini-bar-fill bg-success" style="width:<%# Eval("OverallProgress") %>%"></div>
                                        </div>
                                        <small class="fw-bold"><%# Eval("OverallProgress") %>%</small>
                                    </div>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
    </div>
</div>

<!-- ASSIGNMENT DETAIL TABLE -->
<div class="card border-0 shadow-sm mb-4">
    <div class="card-header bg-white border-bottom fw-semibold py-3">
        <i class="fa-solid fa-clipboard-list me-2 text-warning"></i>Assignment Details
    </div>
    <div class="card-body p-0">
        <div class="table-responsive">
            <table class="table table-hover mb-0 align-middle" style="font-size:.84rem">
                <thead class="table-light">
                    <tr>
                        <th>Title</th>
                        <th>Due Date</th>
                        <th>Max Marks</th>
                        <th>Submissions</th>
                        <th>Avg Score</th>
                        <th>Submission Rate</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptAssignmentDetail" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td class="fw-semibold"><%# Eval("Title") %></td>
                                <td><small class="text-danger"><%# Eval("DueDate", "{0:dd MMM yyyy}") %></small></td>
                                <td><%# Eval("MaxMarks") %></td>
                                <td><%# Eval("SubmissionCount") %></td>
                                <td>
                                    <span class="badge <%# GetScoreClass(Eval("AvgScore")) %>"><%# Eval("AvgScore") %></span>
                                </td>
                                <td>
                                    <div class="d-flex align-items-center gap-2">
                                        <div class="mini-bar">
                                            <div class="mini-bar-fill bg-warning" style="width:<%# Eval("SubmissionRate") %>%"></div>
                                        </div>
                                        <small><%# Eval("SubmissionRate") %>%</small>
                                    </div>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
    </div>
</div>

<!-- Hidden fields for chart data -->
<asp:HiddenField ID="hfAttendanceLabels"  runat="server" />
<asp:HiddenField ID="hfAttendancePresent" runat="server" />
<asp:HiddenField ID="hfAttendanceAbsent" runat="server" />
<asp:HiddenField ID="hfAttendanceLeave"  runat="server" />
<asp:HiddenField ID="hfPresentTotal"     runat="server" />
<asp:HiddenField ID="hfAbsentTotal"      runat="server" />
<asp:HiddenField ID="hfLeaveTotal"       runat="server" />
<asp:HiddenField ID="hfVideoChapters"    runat="server" />
<asp:HiddenField ID="hfVideoViews"       runat="server" />
<asp:HiddenField ID="hfAssignmentTitles" runat="server" />
<asp:HiddenField ID="hfAssignmentSubs"   runat="server" />
<asp:HiddenField ID="hfAssignmentTotal"  runat="server" />
<asp:HiddenField ID="hfQuizTitles"       runat="server" />
<asp:HiddenField ID="hfQuizAvgPct"       runat="server" />

<!-- STYLES -->
<style>
    .kpi-card {
        background: #fff; border-radius: 12px; padding: 18px 16px 14px;
        box-shadow: 0 2px 10px rgba(0,0,0,.07); border-top: 4px solid transparent;
        transition: transform .2s;
    }
    .kpi-card:hover { transform: translateY(-3px); }
    .kpi-icon { font-size: 1.2rem; margin-bottom: 6px; }
    .kpi-val  { font-size: 2rem; font-weight: 800; color: #1a1e2e; line-height: 1; }
    .kpi-lbl  { font-size: .72rem; text-transform: uppercase; letter-spacing: .06em; color: #6b7280; margin-top: 4px; }
    .kpi-blue   { border-top-color: #3b82f6; color: #3b82f6; }
    .kpi-indigo { border-top-color: #6366f1; color: #6366f1; }
    .kpi-red    { border-top-color: #ef4444; color: #ef4444; }
    .kpi-teal   { border-top-color: #14b8a6; color: #14b8a6; }
    .kpi-orange { border-top-color: #f97316; color: #f97316; }
    .kpi-purple { border-top-color: #a855f7; color: #a855f7; }
    .kpi-green  { border-top-color: #22c55e; color: #22c55e; }
    .kpi-pink   { border-top-color: #ec4899; color: #ec4899; }

    .student-avatar {
        width: 32px; height: 32px; border-radius: 50%;
        background: linear-gradient(135deg,#6366f1,#3b82f6);
        color: #fff; font-size: .7rem; font-weight: 700;
        display: grid; place-items: center; flex-shrink: 0;
    }
    .mini-bar { width: 80px; height: 7px; background: #e9ecef; border-radius: 999px; overflow: hidden; }
    .mini-bar-fill { height: 100%; border-radius: 999px; transition: width .6s ease; }
</style>

<!-- Chart.js -->
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
<script>
    document.addEventListener("DOMContentLoaded", function () {

        // ── helpers ──────────────────────────────────────────────
        function parseHF(id) {
            var v = document.getElementById(id)?.value || '';
            try { return JSON.parse(v); } catch { return []; }
        }

        // ── DATA ─────────────────────────────────────────────────
        var attLabels   = parseHF('<%= hfAttendanceLabels.ClientID %>');
        var attPresent  = parseHF('<%= hfAttendancePresent.ClientID %>');
        var attAbsent   = parseHF('<%= hfAttendanceAbsent.ClientID %>');
        var attLeave    = parseHF('<%= hfAttendanceLeave.ClientID %>');

        var pTotal = parseInt(document.getElementById('<%= hfPresentTotal.ClientID %>').value || '0');
        var aTotal = parseInt(document.getElementById('<%= hfAbsentTotal.ClientID %>').value  || '0');
        var lTotal = parseInt(document.getElementById('<%= hfLeaveTotal.ClientID %>').value   || '0');

        var vidChapters = parseHF('<%= hfVideoChapters.ClientID %>');
        var vidViews    = parseHF('<%= hfVideoViews.ClientID %>');

        var asgTitles = parseHF('<%= hfAssignmentTitles.ClientID %>');
        var asgSubs   = parseHF('<%= hfAssignmentSubs.ClientID %>');
        var asgTotal  = parseInt(document.getElementById('<%= hfAssignmentTotal.ClientID %>').value || '0');

        var quizTitles = parseHF('<%= hfQuizTitles.ClientID %>');
        var quizAvg    = parseHF('<%= hfQuizAvgPct.ClientID %>');

        // ── 1. Attendance Bar Chart ───────────────────────────────
        new Chart(document.getElementById('attendanceChart'), {
            type: 'bar',
            data: {
                labels: attLabels,
                datasets: [
                    { label: 'Present', data: attPresent, backgroundColor: '#22c55e', borderRadius: 4 },
                    { label: 'Absent',  data: attAbsent,  backgroundColor: '#ef4444', borderRadius: 4 },
                    { label: 'Leave',   data: attLeave,   backgroundColor: '#f59e0b', borderRadius: 4 }
                ]
            },
            options: {
                responsive: true, plugins: { legend: { position: 'top' } },
                scales: { x: { stacked: false }, y: { beginAtZero: true, ticks: { stepSize: 1 } } }
            }
        });

        // ── 2. Attendance Doughnut ────────────────────────────────
        new Chart(document.getElementById('attendanceDoughnut'), {
            type: 'doughnut',
            data: {
                labels: ['Present', 'Absent', 'Leave'],
                datasets: [{ data: [pTotal, aTotal, lTotal], backgroundColor: ['#22c55e','#ef4444','#f59e0b'], borderWidth: 2 }]
            },
            options: { responsive: true, cutout: '65%', plugins: { legend: { display: false } } }
        });
        // manual legend
        var leg = document.getElementById('doughnutLegend');
        [['Present','#22c55e',pTotal],['Absent','#ef4444',aTotal],['Leave','#f59e0b',lTotal]].forEach(function(d){
            leg.innerHTML += '<span style="font-size:.8rem;display:flex;align-items:center;gap:5px">'
                + '<span style="width:10px;height:10px;border-radius:50%;background:'+d[1]+';display:inline-block"></span>'
                + d[0]+' ('+d[2]+')</span>';
        });

        // ── 3. Video Views Chart ──────────────────────────────────
        new Chart(document.getElementById('videoViewsChart'), {
            type: 'bar',
            data: {
                labels: vidChapters,
                datasets: [{ label: 'Views', data: vidViews, backgroundColor: '#ef4444cc', borderRadius: 6 }]
            },
            options: { responsive: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
        });

        // ── 4. Assignment Submissions Chart ───────────────────────
        var asgTotalArr = asgTitles.map(function() { return asgTotal; });
        new Chart(document.getElementById('assignmentChart'), {
            type: 'bar',
            data: {
                labels: asgTitles,
                datasets: [
                    { label: 'Submitted', data: asgSubs,     backgroundColor: '#3b82f6cc', borderRadius: 4 },
                    { label: 'Enrolled',  data: asgTotalArr, backgroundColor: '#e5e7ebcc', borderRadius: 4 }
                ]
            },
            options: { responsive: true, plugins: { legend: { position: 'top' } }, scales: { y: { beginAtZero: true } } }
        });

        // ── 5. Quiz Chart ─────────────────────────────────────────
        new Chart(document.getElementById('quizChart'), {
            type: 'line',
            data: {
                labels: quizTitles,
                datasets: [{
                    label: 'Avg Score %', data: quizAvg,
                    borderColor: '#22c55e', backgroundColor: '#22c55e22',
                    fill: true, tension: 0.4, pointBackgroundColor: '#22c55e', pointRadius: 5
                }]
            },
            options: { responsive: true, scales: { y: { beginAtZero: true, max: 100 } } }
        });
    });
</script>

<!-- Sidebar active fix -->
<script>
    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll('.sidebar-link').forEach(function(l){ l.classList.remove('active'); });
        var c = document.querySelector('a[href*="TeacherCourses.aspx"]');
        if (c) c.classList.add('active');
    });
</script>

</asp:Content>