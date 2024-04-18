using ColossalFramework;
using Epic.OnlineServices.Lobby;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static RenderManager;

namespace Lumina.Helpers
{
    public class TimeManager : MonoBehaviour
    {
        private SimulationManager simulationManager = Singleton<SimulationManager>.instance;
        private static TimeManager instance;
        public static string FormatTimeOfDay(bool useTwelweHourConvention, float dayTimeHour)
        {
            float timeOfDay = SimulationManager.Lagrange4(dayTimeHour, 0f, SimulationManager.SUNRISE_HOUR, SimulationManager.SUNSET_HOUR, 24f, 0f, 6f, 18f, 24f);

            int hour = (int)Mathf.Floor(timeOfDay);
            int minute = (int)Mathf.Floor((timeOfDay - hour) * 60.0f);

            DateTime dateTime = DateTime.Parse(string.Format("{0,2:00}:{1,2:00}", hour, minute));

            return useTwelweHourConvention ? dateTime.ToString("hh:mm tt") : dateTime.ToString("HH:mm");
        }

        public static string FormatTimeOfDay(bool useTwelweHourConvention, DateTime dateTime)
        {
            return useTwelweHourConvention ? dateTime.ToString("hh:mm tt") : dateTime.ToString("HH:mm");
        }

      

        public float DayTimeHour
        {
            get
            {
                return simulationManager.m_currentDayTimeHour;
            }
            set
            {
                float newCurrentDayTimeHour = value;
                uint newFrameIndex = (uint)(newCurrentDayTimeHour / 24f * SimulationManager.DAYTIME_FRAMES);
                uint currentFrameIndex = simulationManager.m_currentFrameIndex;
                uint dayTimeOffsetFrames = (newFrameIndex - currentFrameIndex) & (SimulationManager.DAYTIME_FRAMES - 1);
                simulationManager.m_dayTimeOffsetFrames = dayTimeOffsetFrames;
            }

        }
        public static TimeManager Instance
        {
            get
            {
                return instance ?? (instance = new TimeManager());
            }
        }
    }
}
    
