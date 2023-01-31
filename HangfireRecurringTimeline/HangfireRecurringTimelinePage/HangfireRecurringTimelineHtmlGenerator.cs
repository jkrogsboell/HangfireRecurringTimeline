namespace HangfireRecurringTimelinePage;

internal static class HangfireRecurringTimelineHtmlGenerator
{
    internal static string GeneratePageHtml(
        DateTime initialDay, 
        List<string> events,
        RecurringTimelineApplicationBuilderExtensions.ViewType viewType)
    {
        var html = /*language=HTML*/$$"""
<style>
/* The Modal (background) */
.modal {
  display: none; /* Hidden by default */
  position: fixed; /* Stay in place */
  z-index: 1; /* Sit on top */
  padding-top: 200px; /* Location of the box */
  left: 0;
  top: 0;
  width: 100%; /* Full width */
  height: 100%; /* Full height */
  overflow: auto; /* Enable scroll if needed */
  background-color: rgb(0,0,0); /* Fallback color */
  background-color: rgba(0,0,0,0.4); /* Black w/ opacity */
}

/* Modal Content */
.modal-content {
  background-color: #fefefe;
  margin: auto;
  padding: 20px;
  border: 1px solid #888;
  width: 600px;
}

.fc .fc-timegrid-slot-label {
    vertical-align: top !important;
    padding-top: 4px;
}
</style>

<div style="position: absolute; width: 100%; top: 40px; bottom: 60px; overflow-y: scroll">
    <div id='calendar' style="width: 100%;"></div>
</div>

<!-- The Modal -->
<div id="myModal" class="modal">

  <!-- Modal content -->
  <div id="myPopupContent" class="modal-content">
    <p>Some text in the Modal..</p>
  </div>

</div>

<script  type='text/javascript' src='https://cdn.jsdelivr.net/npm/fullcalendar@6.0.2/index.global.min.js'></script>
<script>
    // Get the modal
    var modal = document.getElementById("myModal");
    var popupContent = document.getElementById("myPopupContent");

    // When the user clicks anywhere outside of the modal, close it
    window.onclick = function(event) {
      if (event.target == modal) {
        modal.style.display = "none";
      }
    }

      document.addEventListener('DOMContentLoaded', function() {
        var calendarEl = document.getElementById('calendar');
        var calendar = new FullCalendar.Calendar(calendarEl, {
            locale: '{{Thread.CurrentThread.CurrentUICulture.Name}}',
            initialView: '{{(viewType == RecurringTimelineApplicationBuilderExtensions.ViewType.Week ? "timeGridWeek" : "timeGridDay")}}',
            expandRows: true,
            firstDay: 1,
            initialDate: new Date("{{initialDay:yyyy-MM-dd}}"),
            height: 7200,
            allDaySlot: false,
            headerToolbar: {
                start: '',
                center: '',
                end: ''
            },
            eventClick: function(info) {
                popupContent.innerHTML = info.event.extendedProps.description;
                modal.style.display = "block";               
            },
            events: [{{string.Join(",", events)}}],
            slotLabelFormat: {
                hour: '2-digit',
                minute: '2-digit',
                omitZeroMinute: false,
                meridiem: 'short'
            },
            views: {
                timeGridWeek: {
                    slotDuration: '00:10:00'
                },
                timeGridDay: {
                    slotDuration: '00:10:00'
                }
            }
        });
        
        calendar.render();
    });

    </script>

""";
        return html;
    }

    internal static string GetPopupDescription(RecurringJobDescription recurringJobDescription)
    {
        return /*language=HTML*/$$"""
            <div>
                <table class="table" style="width: 100%">
                    <tr><th>Name</th><td>{{recurringJobDescription.Name}}</td></tr>
                    <tr><th>RecurringId</th><td>{{recurringJobDescription.RecurringJobId}}</td></tr>
                    <tr><th>Cron</th><td>{{recurringJobDescription.CronExpression}}</td></tr>
                    <tr><th></th><td>{{recurringJobDescription.CronExplanation}}</td></tr>
                    <tr><th>Queue</th><td>{{recurringJobDescription.Queue}}</td></tr>
                    <tr><th>Last Job</th><td><a target="_blank" href="jobs/details/{{recurringJobDescription.JobId}}">{{recurringJobDescription.JobId}}</a></td></tr>
                    <tr><th>Last Duration <br>(incl. continuations: {{recurringJobDescription.ContinuationCount}})</th><td>{{
                        (recurringJobDescription.LastDuration.HasValue
                            ? TimeSpan.FromMilliseconds(recurringJobDescription.LastDuration.Value).ToString()
                            : "none")
                    }}</td></tr>
                </table>
            </div>
""";
    }
}