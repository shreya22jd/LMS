<%@ Page Title="Student Details" Language="C#"
    MasterPageFile="~/Teacher/TeacherMaster.Master"
    AutoEventWireup="true"
    CodeBehind="MyStudentDetails.aspx.cs"
    Inherits="LMS_Project.Teacher.MyStudentDetails" %>

<asp:Content ID="cHead" ContentPlaceHolderID="head" runat="server">
<!-- Chart.js -->
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
<style>
/* ── hidden fields ── */
.profile-hero {
    background: linear-gradient(135deg, #1565c0 0%, #1976d2 60%, #42a5f5 100%);
    border-radius: 16px; padding: 28px 32px;
    color: #fff; margin-bottom: 24px; position: relative; overflow: hidden;
}
.profile-hero::after {
    content: "\f007"; font-family: "Font Awesome 6 Free"; font-weight: 900;
    position: absolute; right: 32px; top: 50%; transform: translateY(-50%);
    font-size: 100px; opacity: .08;
}
.avatar-lg {
    width: 72px; height: 72px; border-radius: 50%;
    background: rgba(255,255,255,0.25); color: #fff; font-size: 28px; font-weight: 700;
    display: flex; align-items: center; justify-content: center;
    border: 3px solid rgba(255,255,255,0.4); flex-shrink: 0;
}
.info-card {
    background: #fff; border-radius: 14px;
    box-shadow: 0 2px 8px rgba(0,0,0,.06); margin-bottom: 20px;
}
.info-card .card-header-custom {
    padding: 14px 20px; border-bottom: 1px solid #f0f4f8;
    font-weight: 700; font-size: 14px; color: #1565c0;
    display: flex; align-items: center; gap: 8px;
}
.info-card .card-body-custom { padding: 18px 20px; }
.info-row { display: flex; flex-wrap: wrap; gap: 20px; }
.info-item { min-width: 160px; flex: 1; }
.info-item label {
    font-size: 11px; font-weight: 700; text-transform: uppercase;
    letter-spacing: .6px; color: #90a4ae; display: block; margin-bottom: 4px;
}
.info-item span { font-size: 14px; color: #263238; font-weight: 500; }
.stat-box {
    background: #fff; border-radius: 14px;
    box-shadow: 0 2px 8px rgba(0,0,0,.06); padding: 20px; text-align: center;
}
.stat-box .stat-num { font-size: 32px; font-weight: 800; }
.stat-box .stat-lbl { font-size: 12px; color: #90a4ae; margin-top: 4px; }
.progress-bar-custom {
    height: 8px; border-radius: 4px; background: #e3f2fd; overflow: hidden; margin-top: 6px;
}
.progress-fill { height: 100%; border-radius: 4px; background: #1976d2; transition: width .4s; }
.subject-row {
    display: flex; justify-content: space-between; align-items: center;
    padding: 12px 0; border-bottom: 1px solid #f0f4f8; font-size: 13px; color: #455a64;
}
.subject-row:last-child { border-bottom: none; }
.subject-row .s-name { font-weight: 600; color: #263238; }
.subject-row .s-teacher { font-size: 11px; color: #90a4ae; }
.activity-item {
    display: flex; align-items: flex-start; gap: 10px;
    padding: 9px 0; border-bottom: 1px solid #f0f4f8; font-size: 13px; color: #455a64;
}
.activity-item:last-child { border-bottom: none; }
.activity-dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; margin-top: 4px; }
.dot-blue   { background: #42a5f5; }
.dot-orange { background: #ffa726; }
.tab-btn {
    border: none; background: none; padding: 6px 14px; border-radius: 20px;
    font-size: 12px; font-weight: 600; cursor: pointer; color: #90a4ae;
    transition: all .2s;
}
.tab-btn.active { background: #1565c0; color: #fff; }
.chart-wrap { position: relative; }
canvas { max-width: 100%; }
</style>
</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">

<%-- Hidden fields for chart data --%>
<asp:HiddenField ID="hfAttPresent"    runat="server" />
<asp:HiddenField ID="hfAttAbsent"     runat="server" />
<asp:HiddenField ID="hfVideoLabels"   runat="server" />
<asp:HiddenField ID="hfVideoData"     runat="server" />
<asp:HiddenField ID="hfAsgnSubmitted" runat="server" />
<asp:HiddenField ID="hfAsgnOverdue"   runat="server" />
<asp:HiddenField ID="hfAsgnPending"   runat="server" />
<asp:Label ID="lblError" runat="server" Visible="false" CssClass="alert alert-danger d-block mb-3" />

<%-- Back --%>
<div class="d-flex align-items-center mb-4">
    <a href="MyStudents.aspx" class="btn btn-outline-secondary me-3">
        <i class="fas fa-arrow-left"></i>
    </a>
    <h5 class="mb-0">Student Details</h5>
</div>

<%-- Hero --%>
<div class="profile-hero">
    <div class="d-flex align-items-center gap-4">
        <div class="avatar-lg">
            <asp:Label ID="lblInitialHero" runat="server" Text="S" />
        </div>
        <div>
            <h4 class="mb-1 fw-bold"><asp:Label ID="lblFullName" runat="server" /></h4>
            <div style="opacity:.85;font-size:13px;">
                <i class="fas fa-envelope me-1"></i>
                <asp:Label ID="lblEmail" runat="server" />
            </div>
            <div style="opacity:.85;font-size:13px;margin-top:4px;">
                <i class="fas fa-id-badge me-1"></i>Roll:
                <asp:Label ID="lblRoll" runat="server" />
                &nbsp;|&nbsp;
                <asp:Label ID="lblCourse" runat="server" />
                &nbsp;|&nbsp;
                <asp:Label ID="lblSemester" runat="server" />
                &nbsp;|&nbsp;
                <asp:Label ID="lblAttPct" runat="server" Text="0%" />
                <span style="opacity:.7;font-size:11px;"> attendance</span>
            </div>
        </div>
    </div>
</div>

<%-- Stats row --%>
<div class="row g-3 mb-4">
    <div class="col-6 col-md-3">
        <div class="stat-box">
            <div class="stat-num text-success"><asp:Label ID="lblPresent" runat="server" Text="0" /></div>
            <div class="stat-lbl"><i class="fas fa-check-circle text-success me-1"></i>Present</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="stat-box">
            <div class="stat-num text-danger"><asp:Label ID="lblAbsent" runat="server" Text="0" /></div>
            <div class="stat-lbl"><i class="fas fa-times-circle text-danger me-1"></i>Absent</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="stat-box">
            <div class="stat-num text-primary"><asp:Label ID="lblVideos" runat="server" Text="0" /></div>
            <div class="stat-lbl"><i class="fas fa-video text-primary me-1"></i>Videos Done</div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="stat-box">
            <div class="stat-num text-warning"><asp:Label ID="lblAssignments" runat="server" Text="0" /></div>
            <div class="stat-lbl"><i class="fas fa-tasks text-warning me-1"></i>Assignments</div>
        </div>
    </div>
</div>

<div class="row g-4">
    <%-- LEFT --%>
    <div class="col-md-7">

        <%-- Personal Info (expanded) --%>
        <div class="info-card">
            <div class="card-header-custom">
                <i class="fas fa-user"></i> Personal Information
            </div>
            <div class="card-body-custom">
                <div class="info-row">
                    <div class="info-item">
                        <label>Gender</label>
                        <span><asp:Label ID="lblGender" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item">
                        <label>Date of Birth</label>
                        <span><asp:Label ID="lblDOB" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item">
                        <label>Contact</label>
                        <span><asp:Label ID="lblContact" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item">
                        <label>City</label>
                        <span><asp:Label ID="lblCity" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item">
                        <label>Stream</label>
                        <span><asp:Label ID="lblStream" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item">
                        <label>Level</label>
                        <span><asp:Label ID="lblLevel" runat="server" Text="—" /></span>
                    </div>
                    <%-- NEW fields --%>
                    <div class="info-item">
                        <label>Section</label>
                        <span><asp:Label ID="lblSection" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item">
                        <label>Session</label>
                        <span><asp:Label ID="lblSession" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item" style="flex-basis:100%;">
                        <label>Address</label>
                        <span><asp:Label ID="lblAddress" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item" style="flex-basis:100%;">
                        <label>Skills</label>
                        <span><asp:Label ID="lblSkills" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item" style="flex-basis:100%;">
                        <label>Hobbies</label>
                        <span><asp:Label ID="lblHobbies" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item">
                        <label>Emergency Contact</label>
                        <span><asp:Label ID="lblEmergencyName" runat="server" Text="—" /></span>
                    </div>
                    <div class="info-item">
                        <label>Emergency No.</label>
                        <span><asp:Label ID="lblEmergencyNo" runat="server" Text="—" /></span>
                    </div>
                </div>
            </div>
        </div>

        <%-- Subjects & Progress --%>
        <div class="info-card">
            <div class="card-header-custom">
                <i class="fas fa-book-open"></i> Enrolled Subjects
            </div>
            <div class="card-body-custom">
                <asp:Repeater ID="rptSubjects" runat="server">
                    <ItemTemplate>
                        <div class="subject-row">
                            <div>
                                <div class="s-name"><%# Eval("SubjectName") %></div>
                                <div class="s-teacher">
                                    <i class="fas fa-chalkboard-teacher me-1"></i><%# Eval("TeacherName") %>
                                </div>
                                <div class="progress-bar-custom" style="width:200px;">
                                    <div class="progress-fill" style="width:<%# Eval("Progress") %>%;"></div>
                                </div>
                            </div>
                            <span class="badge bg-primary" style="font-size:12px;">
                                <%# Eval("Progress") %>%
                            </span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <%-- Charts row --%>
        <div class="row g-3">
            <%-- Attendance doughnut --%>
            <div class="col-md-4">
                <div class="info-card" style="height:100%;">
                    <div class="card-header-custom">
                        <i class="fas fa-calendar-check"></i> Attendance
                    </div>
                    <div class="card-body-custom chart-wrap" style="display:flex;align-items:center;justify-content:center;">
                        <canvas id="chartAtt" height="180"></canvas>
                    </div>
                </div>
            </div>
            <%-- Assignment pie --%>
            <div class="col-md-4">
                <div class="info-card" style="height:100%;">
                    <div class="card-header-custom">
                        <i class="fas fa-tasks"></i> Assignments
                    </div>
                    <div class="card-body-custom chart-wrap" style="display:flex;align-items:center;justify-content:center;">
                        <canvas id="chartAsgn" height="180"></canvas>
                    </div>
                </div>
            </div>
            <%-- Videos per subject bar --%>
            <div class="col-md-4">
                <div class="info-card" style="height:100%;">
                    <div class="card-header-custom">
                        <i class="fas fa-video"></i> Videos / Subject
                    </div>
                    <div class="card-body-custom chart-wrap">
                        <canvas id="chartVideos" height="180"></canvas>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <%-- RIGHT: Recent Activity tabs --%>
    <div class="col-md-5">
        <div class="info-card">
            <div class="card-header-custom" style="justify-content:space-between;">
                <span><i class="fas fa-clock"></i> Recent Activity</span>
                <div>
                    <button class="tab-btn active" onclick="switchTab('videos',this)">
                        <i class="fas fa-video me-1"></i>Videos
                    </button>
                    <button class="tab-btn" onclick="switchTab('assignments',this)">
                        <i class="fas fa-tasks me-1"></i>Assignments
                    </button>
                </div>
            </div>
            <div class="card-body-custom">

                <%-- Videos tab --%>
                <div id="tabVideos">
                    <asp:Repeater ID="rptVideos" runat="server">
                        <ItemTemplate>
                            <div class="activity-item">
                                <div class="activity-dot dot-blue"></div>
                                <div>
                                    <div style="font-weight:600;font-size:13px;color:#263238;">
                                        <%# Eval("ActivityType") %>
                                    </div>
                                    <div style="font-size:11px;color:#1976d2;font-weight:500;">
                                        <i class="fas fa-book me-1"></i><%# Eval("SubjectName") %>
                                    </div>
                                    <div style="font-size:11px;color:#90a4ae;">
                                        <%# Eval("ActionTime", "{0:dd MMM yyyy, hh:mm tt}") %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoVideos" runat="server" Visible="false">
                        <div style="text-align:center;padding:30px;color:#90a4ae;font-size:13px;">
                            <i class="fas fa-video" style="font-size:32px;margin-bottom:8px;display:block;"></i>
                            No videos watched yet
                        </div>
                    </asp:Panel>
                </div>

                <%-- Assignments tab --%>
                <div id="tabAssignments" style="display:none;">
                    <asp:Repeater ID="rptAssignments" runat="server">
                        <ItemTemplate>
                            <div class="activity-item">
                                <div class="activity-dot dot-orange"></div>
                                <div>
                                    <div style="font-weight:600;font-size:13px;color:#263238;">
                                        <%# Eval("ActivityType") %>
                                    </div>
                                    <div style="font-size:11px;color:#ffa726;font-weight:500;">
                                        <i class="fas fa-book me-1"></i><%# Eval("SubjectName") %>
                                    </div>
                                    <div style="font-size:11px;color:#90a4ae;">
                                        <%# Eval("ActionTime", "{0:dd MMM yyyy, hh:mm tt}") %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoAssignments" runat="server" Visible="false">
                        <div style="text-align:center;padding:30px;color:#90a4ae;font-size:13px;">
                            <i class="fas fa-tasks" style="font-size:32px;margin-bottom:8px;display:block;"></i>
                            No assignments submitted yet
                        </div>
                    </asp:Panel>
                </div>

            </div>
        </div>
    </div>
</div>

<script>
// ── Tab switching ──────────────────────────────────────────────
function switchTab(tab, btn) {
    document.getElementById('tabVideos').style.display      = tab === 'videos'      ? '' : 'none';
    document.getElementById('tabAssignments').style.display = tab === 'assignments' ? '' : 'none';
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
}

// ── Charts ─────────────────────────────────────────────────────
window.addEventListener('DOMContentLoaded', function () {

    var present = parseInt('<%= hfAttPresent.Value %>') || 0;
    var absent  = parseInt('<%= hfAttAbsent.Value %>')  || 0;
    var submitted = parseInt('<%= hfAsgnSubmitted.Value %>') || 0;
    var overdue   = parseInt('<%= hfAsgnOverdue.Value %>')   || 0;
    var pending = parseInt('<%= hfAsgnPending.Value %>') || 0;
    var videoLabels = <%=(string.IsNullOrEmpty(hfVideoLabels.Value) ? "[]" : hfVideoLabels.Value)%>;
    var videoData   = <%=(string.IsNullOrEmpty(hfVideoData.Value)   ? "[]" : hfVideoData.Value)%>;

    // Attendance doughnut
    new Chart(document.getElementById('chartAtt'), {
        type: 'doughnut',
        data: {
            labels: ['Present', 'Absent'],
            datasets: [{ data: [present, absent],
                backgroundColor: ['#4caf50','#ef5350'],
                borderWidth: 0 }]
        },
        options: {
            cutout: '70%',
            plugins: {
                legend: { position: 'bottom', labels: { font: { size: 11 } } }
            }
        }
    });

  
    // Assignment pie
    var pending = parseInt('<%= hfAsgnPending.Value %>') || 0;

    new Chart(document.getElementById('chartAsgn'), {
        type: 'pie',
        data: {
            labels: ['Submitted', 'Overdue', 'Pending'],
            datasets: [{
                data: [submitted, overdue, pending],
                backgroundColor: ['#4caf50', '#ef5350', '#ffa726'],
                borderWidth: 0
            }]
        },
        options: {
            plugins: {
                legend: { position: 'bottom', labels: { font: { size: 11 } } },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            var total = submitted + overdue + pending;
                            var pct = total > 0 ? Math.round(ctx.parsed / total * 100) : 0;
                            return ctx.label + ': ' + ctx.parsed + ' (' + pct + '%)';
                        }
                    }
                }
            }
        }
    });

    // Videos per subject bar
    new Chart(document.getElementById('chartVideos'), {
        type: 'bar',
        data: {
            labels: videoLabels,
            datasets: [{
                label: 'Videos Watched',
                data: videoData,
                backgroundColor: '#42a5f5',
                borderRadius: 4
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, ticks: { precision: 0, font: { size: 10 } } },
                x: { ticks: { font: { size: 10 } } }
            }
        }
    });
});
</script>

</asp:Content>