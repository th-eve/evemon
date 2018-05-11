﻿using System;
using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Service;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Extensions;

namespace EVEMon.Common.Models
{
    public sealed class UpcomingCalendarEvent
    {
        #region Fields

        private readonly CCPCharacter m_ccpCharacter;
        private readonly List<CalendarEventAttendee> m_eventAttendees;
        private readonly long m_eventID;
        private string m_ownerName;
        private bool m_queryPending;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor from the API.
        /// </summary>
        /// <param name="ccpCharacter">The CCP character.</param>
        /// <param name="src">The source.</param>
        internal UpcomingCalendarEvent(CCPCharacter ccpCharacter, EsiAPICalendarEvent src)
        {
            m_ccpCharacter = ccpCharacter;

            m_eventID = src.EventID;
            OwnerID = src.OwnerID;
            m_ownerName = EveIDToName.GetIDToName(OwnerID);
            EventTitle = src.EventTitle;
            EventText = src.EventText;
            Duration = src.Duration;
            Importance = src.Importance != 0;
            Response = src.Response;
            EventDate = src.EventDate;
            m_eventAttendees = new List<CalendarEventAttendee>();
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets the owner ID.
        /// </summary>
        public long OwnerID { get; }

        /// <summary>
        /// Gets the name of the owner.
        /// </summary>
        public string OwnerName => m_ownerName.IsEmptyOrUnknown() ? (m_ownerName =
            EveIDToName.GetIDToName(OwnerID)) : m_ownerName;

        /// <summary>
        /// Gets the event title.
        /// </summary>
        public string EventTitle { get; }

        /// <summary>
        /// Gets the event text.
        /// </summary>
        public string EventText { get; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        public int Duration { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="UpcomingCalendarEvent"/> is important.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="UpcomingCalendarEvent"/> is important; otherwise, <c>false</c>.
        /// </value>
        public bool Importance { get; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public string Response { get; }

        /// <summary>
        /// Gets the event date.
        /// </summary>
        public DateTime EventDate { get; }

        /// <summary>
        /// Gets the event attendees.
        /// </summary>
        public IEnumerable<CalendarEventAttendee> Attendees => m_eventAttendees;

        #endregion


        #region Querying

        /// <summary>
        /// Gets the attendees.
        /// </summary>
        public void GetEventAttendees()
        {
            if (!m_queryPending)
            {
                m_queryPending = true;
                ESIKey apiKey = m_ccpCharacter.Identity.FindAPIKeyWithAccess(
                    ESIAPICharacterMethods.CalendarEventAttendees);
                if (apiKey != null)
                {
                    EveMonClient.APIProviders.CurrentProvider.QueryEsiAsync<EsiAPICalendarEventAttendees>(
                       ESIAPICharacterMethods.CalendarEventAttendees, apiKey.AccessToken,
                       m_ccpCharacter.CharacterID, m_eventID, OnCalendarEventAttendeesDownloaded);
                }
            }
        }

        /// <summary>
        /// Processes the queried calendar event attendees.
        /// </summary>
        /// <param name="result">The result.</param>
        private void OnCalendarEventAttendeesDownloaded(EsiResult<EsiAPICalendarEventAttendees> result, object ignore)
        {
            m_queryPending = false;

            // Notify if an error occured
            if (m_ccpCharacter.ShouldNotifyError(result, ESIAPICharacterMethods.CalendarEventAttendees))
            {
                EveMonClient.Notifications.NotifyCharacterCalendarEventAttendeesError(
                   m_ccpCharacter, result);
            }

            if (!result.HasError && result.Result.Count > 0)
            {
                var attendees = result.Result.ToXMLItem().EventAttendees.Select(attendee =>
                    new CalendarEventAttendee(attendee));
                m_eventAttendees.Clear();
                m_eventAttendees.AddRange(attendees);
                EveMonClient.OnCharacterCalendarEventAttendeesDownloaded(m_ccpCharacter);
            }
        }

        #endregion

    }
}
